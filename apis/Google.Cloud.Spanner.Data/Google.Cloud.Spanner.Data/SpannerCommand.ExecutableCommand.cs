﻿// Copyright 2018 Google LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     https://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.Spanner.Admin.Database.V1;
using Google.Cloud.Spanner.Common.V1;
using Google.Cloud.Spanner.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Spanner.Data
{
    public sealed partial class SpannerCommand
    {
        /// <summary>
        /// Class that effectively contains a copy of the parameters of a SpannerCommand, but in a shallow-immutable way.
        /// This means we can validate various things and not worry about them changing. The parameter collection may be modified
        /// externally, along with the SpannerConnection, but other objects should be fine.
        /// 
        /// This class is an implementation detail, used to keep "code required to execute Spanner commands" separate from the ADO
        /// API surface with its mutable properties and many overloads.
        /// </summary>
        private class ExecutableCommand
        {
            private static readonly DatabaseAdminSettings s_databaseAdminSettings = CreateDatabaseAdminSettings();

            private static DatabaseAdminSettings CreateDatabaseAdminSettings()
            {
                var settings = new DatabaseAdminSettings();
                settings.VersionHeaderBuilder.AppendAssemblyVersion("gccl", typeof(SpannerCommand));
                return settings;
            }

            private const string SpannerOptimizerVersionVariable = "SPANNER_OPTIMIZER_VERSION";
            private const string SpannerOptimizerStatisticsPackageVariable = "SPANNER_OPTIMIZER_STATISTICS_PACKAGE";

            internal SpannerConnection Connection { get; }
            internal SpannerCommandTextBuilder CommandTextBuilder { get; }
            internal int CommandTimeout { get; }
            internal SpannerTransaction Transaction { get; }
            internal CommandPartition Partition { get; }
            internal SpannerParameterCollection Parameters { get; }
            internal KeySet KeySet { get; }
            internal QueryOptions QueryOptions { get; }
            internal Priority Priority { get; }
            internal string Tag { get; }
            internal SpannerConversionOptions ConversionOptions => SpannerConversionOptions.ForConnection(Connection);

            public ExecutableCommand(SpannerCommand command)
            {
                GaxPreconditions.CheckState(!(command.KeySet != null && command.Parameters.Count > 0), "Command may not contain both a KeySet and Parameters");
                Connection = command.SpannerConnection;
                CommandTextBuilder = command.SpannerCommandTextBuilder;
                CommandTimeout = command.CommandTimeout;
                Partition = command.Partition;
                Parameters = command.Parameters;
                KeySet = command.KeySet;
                Transaction = command._transaction;
                QueryOptions = command.QueryOptions;
                Priority = command.Priority;
                Tag = command.Tag;
            }

            // ExecuteScalar is simply implemented in terms of ExecuteReader.
            internal async Task<T> ExecuteScalarAsync<T>(CancellationToken cancellationToken)
            {
                // Duplication of later checks, but this means we can report the right method name.
                ValidateConnectionAndCommandTextBuilder();
                if (CommandTextBuilder.SpannerCommandType != SpannerCommandType.Select && CommandTextBuilder.SpannerCommandType != SpannerCommandType.Read)
                {
                    throw new InvalidOperationException("ExecuteScalar functionality is only available for queries and reads.");
                }

                using (var reader = await ExecuteReaderAsync(CommandBehavior.SingleRow, null, cancellationToken).ConfigureAwait(false))
                {
                    bool readValue = await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && reader.FieldCount > 0;
                    return readValue ? reader.GetFieldValue<T>(0) : default;
                }
            }

            // Convenience method for upcasting the from SpannerDataReader to DbDataReader.
            internal async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, TimestampBound singleUseReadSettings, CancellationToken cancellationToken) =>
                await ExecuteReaderAsync(behavior, singleUseReadSettings, cancellationToken).ConfigureAwait(false);

            internal async Task<SpannerDataReader> ExecuteReaderAsync(CommandBehavior behavior, TimestampBound singleUseReadSettings, CancellationToken cancellationToken)
            {
                ValidateConnectionAndCommandTextBuilder();
                ValidateCommandBehavior(behavior);

                if (CommandTextBuilder.SpannerCommandType != SpannerCommandType.Select && CommandTextBuilder.SpannerCommandType != SpannerCommandType.Read)
                {
                    throw new InvalidOperationException("ExecuteReader functionality is only available for queries and reads.");
                }

                await Connection.EnsureIsOpenAsync(cancellationToken).ConfigureAwait(false);

                // Three transaction options:
                // - A single-use transaction. This doesn't go through a BeginTransaction request; instead, the transaction options are in the request.
                // - One specified in the command
                // - The default based on the connection (may be ephemeral, may be implicit via TransactionScope)

                ISpannerTransaction effectiveTransaction = Transaction ?? Connection.AmbientTransaction;
                if (singleUseReadSettings != null && Transaction != null)
                {
                    throw new InvalidOperationException("singleUseReadSettings cannot be used within another transaction.");
                }
                effectiveTransaction = effectiveTransaction ?? new EphemeralTransaction(Connection, Priority);

                var resultSet = await ExecuteReadOrQueryRequestAsync(singleUseReadSettings, effectiveTransaction, cancellationToken)
                        .ConfigureAwait(false);

                var enableGetSchemaTable = Connection.Builder.EnableGetSchemaTable;
                // When the data reader is closed, we may need to dispose of the connection.
                IDisposable resourceToClose = (behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection ? Connection : null;

                return new SpannerDataReader(Connection.Logger, resultSet, Transaction?.ReadTimestamp, resourceToClose, ConversionOptions, enableGetSchemaTable, CommandTimeout);
            }

            private Task<ReliableStreamReader> ExecuteReadOrQueryRequestAsync(TimestampBound singleUseReadSettings, ISpannerTransaction effectiveTransaction, CancellationToken cancellationToken)
            {
                var request = GetReadOrQueryRequest();

                if (singleUseReadSettings != null)
                {
                    request.Transaction = new TransactionSelector { SingleUse = singleUseReadSettings.ToTransactionOptions() };
                }

                if (request.IsQuery)
                {
                    Connection.Logger.SensitiveInfo(() => $"SpannerCommand.ExecuteReader.Query={request.ExecuteSqlRequest.Sql}");
                }

                // Execute the command. Note that the command timeout here is only used for ambient transactions where we need to set a commit timeout.
                return effectiveTransaction.ExecuteReadOrQueryAsync(request, cancellationToken, CommandTimeout);
            }

            internal async Task<IReadOnlyList<CommandPartition>> GetReaderPartitionsAsync(long? partitionSizeBytes, long? maxPartitions, CancellationToken cancellationToken)
            {
                ValidateConnectionAndCommandTextBuilder();

                GaxPreconditions.CheckState(Transaction?.Mode == TransactionMode.ReadOnly,
                    "GetReaderPartitions can only be executed within an explicitly created read-only transaction.");

                await Connection.EnsureIsOpenAsync(cancellationToken).ConfigureAwait(false);
                var readOrQueryRequest = GetReadOrQueryRequest();
                var tokens = await Transaction.GetPartitionTokensAsync(
                        readOrQueryRequest, partitionSizeBytes, maxPartitions, cancellationToken, CommandTimeout)
                    .ConfigureAwait(false);
                return tokens.Select(
                    x =>
                    {
                        var request = readOrQueryRequest.CloneRequest();
                        request.PartitionToken = x;
                        return new CommandPartition(request);
                    }).ToList();
            }

            internal Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
            {
                ValidateConnectionAndCommandTextBuilder();

                switch (CommandTextBuilder.SpannerCommandType)
                {
                    case SpannerCommandType.Ddl:
                        return ExecuteDdlAsync(cancellationToken);
                    case SpannerCommandType.Delete:
                    case SpannerCommandType.Insert:
                    case SpannerCommandType.InsertOrUpdate:
                    case SpannerCommandType.Update:
                        return ExecuteMutationsAsync(cancellationToken);
                    case SpannerCommandType.Dml:
                        return ExecuteDmlAsync(cancellationToken);
                    default:
                        throw new InvalidOperationException("ExecuteNonQuery functionality is only available for DML and DDL commands");
                }
            }

            internal async Task<long> ExecutePartitionedUpdateAsync(CancellationToken cancellationToken)
            {
                ValidateConnectionAndCommandTextBuilder();
                GaxPreconditions.CheckState(Transaction is null && Connection.AmbientTransaction is null, "Partitioned updates cannot be executed within another transaction");
                GaxPreconditions.CheckState(CommandTextBuilder.SpannerCommandType == SpannerCommandType.Dml, "Only general DML commands can be executed in as partitioned updates");
                await Connection.EnsureIsOpenAsync(cancellationToken).ConfigureAwait(false);
                ExecuteSqlRequest request = GetExecuteSqlRequest();

                var transaction = new EphemeralTransaction(Connection, Priority);
                // Note: no commit here. PDML transactions are implicitly committed as they go along.
                return await transaction.ExecutePartitionedDmlAsync(request, cancellationToken, CommandTimeout).ConfigureAwait(false);
            }

            private void ValidateConnectionAndCommandTextBuilder()
            {
                GaxPreconditions.CheckState(Connection != null, "SpannerCommand can only be executed when a connection is assigned.");
                GaxPreconditions.CheckState(CommandTextBuilder != null, "SpannerCommand can only be executed when command text is assigned.");
            }

            private async Task<int> ExecuteDmlAsync(CancellationToken cancellationToken)
            {
                await Connection.EnsureIsOpenAsync(cancellationToken).ConfigureAwait(false);
                var transaction = Transaction ?? Connection.AmbientTransaction ?? new EphemeralTransaction(Connection, Priority);
                ExecuteSqlRequest request = GetExecuteSqlRequest();
                long count = await transaction.ExecuteDmlAsync(request, cancellationToken, CommandTimeout).ConfigureAwait(false);
                // This cannot currently exceed int.MaxValue due to Spanner commit limitations anyway.
                return checked((int)count);
            }

            private async Task<int> ExecuteDdlAsync(CancellationToken cancellationToken)
            {
                string commandText = CommandTextBuilder.CommandText;
                var builder = Connection.Builder;
                var channelOptions = new SpannerClientCreationOptions(builder);
                var credentials = await channelOptions.GetCredentialsAsync().ConfigureAwait(false);

                // Create the builder separately from actually building, so we can note the channel that it created.
                // (This is fairly unpleasant, but we'll try to improve this in the next version of GAX.)
                var databaseAdminClientBuilder = new DatabaseAdminClientBuilder
                {
                    // Note: deliberately not copying EmulatorDetection, as that's handled in SpannerClientCreationOptions
                    Settings = s_databaseAdminSettings,
                    Endpoint = channelOptions.Endpoint,
                    ChannelCredentials = credentials
                };
                var databaseAdminClient = databaseAdminClientBuilder.Build();
                var channel = databaseAdminClientBuilder.LastCreatedChannel;
                try
                {
                    if (CommandTextBuilder.IsCreateDatabaseCommand)
                    {
                        var parent = new InstanceName(Connection.Project, Connection.SpannerInstance);
                        var request = new CreateDatabaseRequest
                        {
                            ParentAsInstanceName = parent,
                            CreateStatement = CommandTextBuilder.CommandText,
                            ExtraStatements = { CommandTextBuilder.ExtraStatements ?? new string[0] }
                        };
                        var response = await databaseAdminClient.CreateDatabaseAsync(request).ConfigureAwait(false);
                        response = await response.PollUntilCompletedAsync().ConfigureAwait(false);
                        if (response.IsFaulted)
                        {
                            throw SpannerException.FromOperationFailedException(response.Exception);
                        }
                    }
                    else if (CommandTextBuilder.IsDropDatabaseCommand)
                    {
                        if (CommandTextBuilder.ExtraStatements?.Count > 0)
                        {
                            throw new InvalidOperationException(
                                "Drop database commands do not support additional ddl statements");
                        }
                        var dbName = new DatabaseName(Connection.Project, Connection.SpannerInstance, CommandTextBuilder.DatabaseToDrop);
                        await databaseAdminClient.DropDatabaseAsync(dbName, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        if (builder.DatabaseName == null)
                        {
                            throw new InvalidOperationException(
                                "DDL commands other than CREATE/DROP DATABASE require a database in the data source");
                        }

                        var request = new UpdateDatabaseDdlRequest
                        {
                            DatabaseAsDatabaseName = builder.DatabaseName,
                            Statements = { commandText, CommandTextBuilder.ExtraStatements ?? Enumerable.Empty<string>() }
                        };

                        var response = await databaseAdminClient.UpdateDatabaseDdlAsync(request).ConfigureAwait(false);
                        response = await response.PollUntilCompletedAsync().ConfigureAwait(false);
                        if (response.IsFaulted)
                        {
                            throw SpannerException.FromOperationFailedException(response.Exception);
                        }
                    }
                }
                catch (RpcException gRpcException)
                {
                    //we translate rpc errors into a spanner exception
                    throw new SpannerException(gRpcException);
                }
                finally
                {
                    channel?.Shutdown();
                }

                return 0;
            }

            private async Task<int> ExecuteMutationsAsync(CancellationToken cancellationToken)
            {
                await Connection.EnsureIsOpenAsync(cancellationToken).ConfigureAwait(false);
                var mutations = GetMutations();
                var transaction = Transaction ?? Connection.AmbientTransaction ?? new EphemeralTransaction(Connection, Priority);
                // Make the request. This will commit immediately or not depending on whether a transaction was explicitly created.
                await transaction.ExecuteMutationsAsync(mutations, cancellationToken, CommandTimeout).ConfigureAwait(false);
                // Return the number of records affected.
                return mutations.Count;
            }

            private List<Mutation> GetMutations()
            {
                // Avoid calling method multiple times in the loop.
                var conversionOptions = ConversionOptions;
                // Whatever we do with the parameters, we'll need them in a ListValue.
                var listValue = new ListValue
                {
                    Values = { Parameters.Select(x => x.GetConfiguredSpannerDbType(conversionOptions).ToProtobufValue(x.GetValidatedValue())) }
                };

                if (CommandTextBuilder.SpannerCommandType != SpannerCommandType.Delete)
                {
                    var w = new Mutation.Types.Write
                    {
                        Table = CommandTextBuilder.TargetTable,
                        Columns = { Parameters.Select(x => x.SourceColumn ?? x.ParameterName) },
                        Values = { listValue }
                    };
                    switch (CommandTextBuilder.SpannerCommandType)
                    {
                        case SpannerCommandType.Update:
                            return new List<Mutation> { new Mutation { Update = w } };
                        case SpannerCommandType.Insert:
                            return new List<Mutation> { new Mutation { Insert = w } };
                        case SpannerCommandType.InsertOrUpdate:
                            return new List<Mutation> { new Mutation { InsertOrUpdate = w } };
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    var w = new Mutation.Types.Delete
                    {
                        Table = CommandTextBuilder.TargetTable,
                        KeySet = new V1.KeySet { Keys = { listValue } }
                    };
                    return new List<Mutation> { new Mutation { Delete = w } };
                }
            }

            // Based on the QueryOptions set at various levels (connection, environment and command),
            // constructs the QueryOptions proto to set in the ExecuteSqlRequest.
            // Options set at the SpannerCommand-level has the highest precedence.
            // Options set at the environment variable level has the next highest precedence.
            // Options set at the connection level has the lowest precedence.
            private V1.ExecuteSqlRequest.Types.QueryOptions GetEffectiveQueryOptions()
            {
                var queryOptionsProto = new V1.ExecuteSqlRequest.Types.QueryOptions();

                // Query options set through the connection have the lowest precedence.
                if (Connection.QueryOptions != null)
                {
                    queryOptionsProto.MergeFrom(Connection.QueryOptions.ToProto());
                }

                // Query options set through an environment variable have the next highest precedence.
                var envQueryOptionsProto = new V1.ExecuteSqlRequest.Types.QueryOptions
                {
                    OptimizerVersion = Environment.GetEnvironmentVariable(SpannerOptimizerVersionVariable)?.Trim() ?? "",
                    OptimizerStatisticsPackage = Environment.GetEnvironmentVariable(SpannerOptimizerStatisticsPackageVariable)?.Trim() ?? ""
                };
                queryOptionsProto.MergeFrom(envQueryOptionsProto);

                // Query options set at the command level have the highest precedence.
                if (QueryOptions != null)
                {
                    queryOptionsProto.MergeFrom(QueryOptions.ToProto());
                }

                return queryOptionsProto;
            }

            private RequestOptions BuildRequestOptions() =>
                new RequestOptions { Priority = PriorityConverter.ToProto(Priority) , RequestTag = Tag ?? "", TransactionTag = Transaction?.Tag ?? "" };

            private ExecuteSqlRequest GetExecuteSqlRequest()
            {
                var request = new ExecuteSqlRequest
                {
                    Sql = CommandTextBuilder.ToString(),
                    QueryOptions = GetEffectiveQueryOptions(),
                    RequestOptions = BuildRequestOptions()
                };

                Parameters.FillSpannerCommandParams(out var parameters, request.ParamTypes, ConversionOptions);
                request.Params = parameters;

                return request;
            }

            private ReadRequest GetReadRequest()
            {
                GaxPreconditions.CheckState(CommandTextBuilder.ReadOptions != null, "Cannot create a ReadRequest without ReadOptions");
                return new ReadRequest
                {
                    Table = CommandTextBuilder.TargetTable,
                    Index = CommandTextBuilder.ReadOptions.IndexName ?? "",
                    Limit = CommandTextBuilder.ReadOptions.Limit ?? 0L,
                    KeySet = KeySet.ToProtobuf(ConversionOptions),
                    Columns = { CommandTextBuilder.ReadOptions.Columns },
                    RequestOptions = BuildRequestOptions()
                };
            }

            private ReadOrQueryRequest GetReadOrQueryRequest()
            {
                if (Partition != null)
                {
                    return Partition.Request;
                }

                ReadOrQueryRequest request;
                switch (CommandTextBuilder.SpannerCommandType)
                {
                    case SpannerCommandType.Select:
                        request = ReadOrQueryRequest.FromQueryRequest(GetExecuteSqlRequest());
                        break;
                    case SpannerCommandType.Read:
                        request = ReadOrQueryRequest.FromReadRequest(GetReadRequest());
                        break;
                    default:
                        throw new InvalidOperationException($"Implementation error: Invalid command type ${CommandTextBuilder.SpannerCommandType} for read or query. This should not happen.");
                }
                return request;
            }

            private static void ValidateCommandBehavior(CommandBehavior behavior)
            {
                if ((behavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo)
                {
                    throw new NotSupportedException(
                        $"{nameof(CommandBehavior.KeyInfo)} is not supported by Cloud Spanner.");
                }
                if ((behavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
                {
                    throw new NotSupportedException(
                        $"{nameof(CommandBehavior.SchemaOnly)} is not supported by Cloud Spanner.");
                }
            }
        }
    }
}

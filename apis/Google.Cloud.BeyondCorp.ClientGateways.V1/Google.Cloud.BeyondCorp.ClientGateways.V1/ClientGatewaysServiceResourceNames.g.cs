// Copyright 2022 Google LLC
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

// Generated code. DO NOT EDIT!

#pragma warning disable CS8981
using gax = Google.Api.Gax;
using gagr = Google.Api.Gax.ResourceNames;
using gcbcv = Google.Cloud.BeyondCorp.ClientGateways.V1;
using sys = System;

namespace Google.Cloud.BeyondCorp.ClientGateways.V1
{
    /// <summary>Resource name for the <c>ClientGateway</c> resource.</summary>
    public sealed partial class ClientGatewayName : gax::IResourceName, sys::IEquatable<ClientGatewayName>
    {
        /// <summary>The possible contents of <see cref="ClientGatewayName"/>.</summary>
        public enum ResourceNameType
        {
            /// <summary>An unparsed resource name.</summary>
            Unparsed = 0,

            /// <summary>
            /// A resource name with pattern <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>
            /// .
            /// </summary>
            ProjectLocationClientGateway = 1,
        }

        private static gax::PathTemplate s_projectLocationClientGateway = new gax::PathTemplate("projects/{project}/locations/{location}/clientGateways/{client_gateway}");

        /// <summary>Creates a <see cref="ClientGatewayName"/> containing an unparsed resource name.</summary>
        /// <param name="unparsedResourceName">The unparsed resource name. Must not be <c>null</c>.</param>
        /// <returns>
        /// A new instance of <see cref="ClientGatewayName"/> containing the provided
        /// <paramref name="unparsedResourceName"/>.
        /// </returns>
        public static ClientGatewayName FromUnparsed(gax::UnparsedResourceName unparsedResourceName) =>
            new ClientGatewayName(ResourceNameType.Unparsed, gax::GaxPreconditions.CheckNotNull(unparsedResourceName, nameof(unparsedResourceName)));

        /// <summary>
        /// Creates a <see cref="ClientGatewayName"/> with the pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>.
        /// </summary>
        /// <param name="projectId">The <c>Project</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="locationId">The <c>Location</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="clientGatewayId">The <c>ClientGateway</c> ID. Must not be <c>null</c> or empty.</param>
        /// <returns>A new instance of <see cref="ClientGatewayName"/> constructed from the provided ids.</returns>
        public static ClientGatewayName FromProjectLocationClientGateway(string projectId, string locationId, string clientGatewayId) =>
            new ClientGatewayName(ResourceNameType.ProjectLocationClientGateway, projectId: gax::GaxPreconditions.CheckNotNullOrEmpty(projectId, nameof(projectId)), locationId: gax::GaxPreconditions.CheckNotNullOrEmpty(locationId, nameof(locationId)), clientGatewayId: gax::GaxPreconditions.CheckNotNullOrEmpty(clientGatewayId, nameof(clientGatewayId)));

        /// <summary>
        /// Formats the IDs into the string representation of this <see cref="ClientGatewayName"/> with pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>.
        /// </summary>
        /// <param name="projectId">The <c>Project</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="locationId">The <c>Location</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="clientGatewayId">The <c>ClientGateway</c> ID. Must not be <c>null</c> or empty.</param>
        /// <returns>
        /// The string representation of this <see cref="ClientGatewayName"/> with pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>.
        /// </returns>
        public static string Format(string projectId, string locationId, string clientGatewayId) =>
            FormatProjectLocationClientGateway(projectId, locationId, clientGatewayId);

        /// <summary>
        /// Formats the IDs into the string representation of this <see cref="ClientGatewayName"/> with pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>.
        /// </summary>
        /// <param name="projectId">The <c>Project</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="locationId">The <c>Location</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="clientGatewayId">The <c>ClientGateway</c> ID. Must not be <c>null</c> or empty.</param>
        /// <returns>
        /// The string representation of this <see cref="ClientGatewayName"/> with pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>.
        /// </returns>
        public static string FormatProjectLocationClientGateway(string projectId, string locationId, string clientGatewayId) =>
            s_projectLocationClientGateway.Expand(gax::GaxPreconditions.CheckNotNullOrEmpty(projectId, nameof(projectId)), gax::GaxPreconditions.CheckNotNullOrEmpty(locationId, nameof(locationId)), gax::GaxPreconditions.CheckNotNullOrEmpty(clientGatewayId, nameof(clientGatewayId)));

        /// <summary>
        /// Parses the given resource name string into a new <see cref="ClientGatewayName"/> instance.
        /// </summary>
        /// <remarks>
        /// To parse successfully, the resource name must be formatted as one of the following:
        /// <list type="bullet">
        /// <item>
        /// <description><c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c></description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="clientGatewayName">The resource name in string form. Must not be <c>null</c>.</param>
        /// <returns>The parsed <see cref="ClientGatewayName"/> if successful.</returns>
        public static ClientGatewayName Parse(string clientGatewayName) => Parse(clientGatewayName, false);

        /// <summary>
        /// Parses the given resource name string into a new <see cref="ClientGatewayName"/> instance; optionally
        /// allowing an unparseable resource name.
        /// </summary>
        /// <remarks>
        /// To parse successfully, the resource name must be formatted as one of the following:
        /// <list type="bullet">
        /// <item>
        /// <description><c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c></description>
        /// </item>
        /// </list>
        /// Or may be in any format if <paramref name="allowUnparsed"/> is <c>true</c>.
        /// </remarks>
        /// <param name="clientGatewayName">The resource name in string form. Must not be <c>null</c>.</param>
        /// <param name="allowUnparsed">
        /// If <c>true</c> will successfully store an unparseable resource name into the <see cref="UnparsedResource"/>
        /// property; otherwise will throw an <see cref="sys::ArgumentException"/> if an unparseable resource name is
        /// specified.
        /// </param>
        /// <returns>The parsed <see cref="ClientGatewayName"/> if successful.</returns>
        public static ClientGatewayName Parse(string clientGatewayName, bool allowUnparsed) =>
            TryParse(clientGatewayName, allowUnparsed, out ClientGatewayName result) ? result : throw new sys::ArgumentException("The given resource-name matches no pattern.");

        /// <summary>
        /// Tries to parse the given resource name string into a new <see cref="ClientGatewayName"/> instance.
        /// </summary>
        /// <remarks>
        /// To parse successfully, the resource name must be formatted as one of the following:
        /// <list type="bullet">
        /// <item>
        /// <description><c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c></description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="clientGatewayName">The resource name in string form. Must not be <c>null</c>.</param>
        /// <param name="result">
        /// When this method returns, the parsed <see cref="ClientGatewayName"/>, or <c>null</c> if parsing failed.
        /// </param>
        /// <returns><c>true</c> if the name was parsed successfully; <c>false</c> otherwise.</returns>
        public static bool TryParse(string clientGatewayName, out ClientGatewayName result) =>
            TryParse(clientGatewayName, false, out result);

        /// <summary>
        /// Tries to parse the given resource name string into a new <see cref="ClientGatewayName"/> instance;
        /// optionally allowing an unparseable resource name.
        /// </summary>
        /// <remarks>
        /// To parse successfully, the resource name must be formatted as one of the following:
        /// <list type="bullet">
        /// <item>
        /// <description><c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c></description>
        /// </item>
        /// </list>
        /// Or may be in any format if <paramref name="allowUnparsed"/> is <c>true</c>.
        /// </remarks>
        /// <param name="clientGatewayName">The resource name in string form. Must not be <c>null</c>.</param>
        /// <param name="allowUnparsed">
        /// If <c>true</c> will successfully store an unparseable resource name into the <see cref="UnparsedResource"/>
        /// property; otherwise will throw an <see cref="sys::ArgumentException"/> if an unparseable resource name is
        /// specified.
        /// </param>
        /// <param name="result">
        /// When this method returns, the parsed <see cref="ClientGatewayName"/>, or <c>null</c> if parsing failed.
        /// </param>
        /// <returns><c>true</c> if the name was parsed successfully; <c>false</c> otherwise.</returns>
        public static bool TryParse(string clientGatewayName, bool allowUnparsed, out ClientGatewayName result)
        {
            gax::GaxPreconditions.CheckNotNull(clientGatewayName, nameof(clientGatewayName));
            gax::TemplatedResourceName resourceName;
            if (s_projectLocationClientGateway.TryParseName(clientGatewayName, out resourceName))
            {
                result = FromProjectLocationClientGateway(resourceName[0], resourceName[1], resourceName[2]);
                return true;
            }
            if (allowUnparsed)
            {
                if (gax::UnparsedResourceName.TryParse(clientGatewayName, out gax::UnparsedResourceName unparsedResourceName))
                {
                    result = FromUnparsed(unparsedResourceName);
                    return true;
                }
            }
            result = null;
            return false;
        }

        private ClientGatewayName(ResourceNameType type, gax::UnparsedResourceName unparsedResourceName = null, string clientGatewayId = null, string locationId = null, string projectId = null)
        {
            Type = type;
            UnparsedResource = unparsedResourceName;
            ClientGatewayId = clientGatewayId;
            LocationId = locationId;
            ProjectId = projectId;
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="ClientGatewayName"/> class from the component parts of pattern
        /// <c>projects/{project}/locations/{location}/clientGateways/{client_gateway}</c>
        /// </summary>
        /// <param name="projectId">The <c>Project</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="locationId">The <c>Location</c> ID. Must not be <c>null</c> or empty.</param>
        /// <param name="clientGatewayId">The <c>ClientGateway</c> ID. Must not be <c>null</c> or empty.</param>
        public ClientGatewayName(string projectId, string locationId, string clientGatewayId) : this(ResourceNameType.ProjectLocationClientGateway, projectId: gax::GaxPreconditions.CheckNotNullOrEmpty(projectId, nameof(projectId)), locationId: gax::GaxPreconditions.CheckNotNullOrEmpty(locationId, nameof(locationId)), clientGatewayId: gax::GaxPreconditions.CheckNotNullOrEmpty(clientGatewayId, nameof(clientGatewayId)))
        {
        }

        /// <summary>The <see cref="ResourceNameType"/> of the contained resource name.</summary>
        public ResourceNameType Type { get; }

        /// <summary>
        /// The contained <see cref="gax::UnparsedResourceName"/>. Only non-<c>null</c> if this instance contains an
        /// unparsed resource name.
        /// </summary>
        public gax::UnparsedResourceName UnparsedResource { get; }

        /// <summary>
        /// The <c>ClientGateway</c> ID. Will not be <c>null</c>, unless this instance contains an unparsed resource
        /// name.
        /// </summary>
        public string ClientGatewayId { get; }

        /// <summary>
        /// The <c>Location</c> ID. Will not be <c>null</c>, unless this instance contains an unparsed resource name.
        /// </summary>
        public string LocationId { get; }

        /// <summary>
        /// The <c>Project</c> ID. Will not be <c>null</c>, unless this instance contains an unparsed resource name.
        /// </summary>
        public string ProjectId { get; }

        /// <summary>Whether this instance contains a resource name with a known pattern.</summary>
        public bool IsKnownPattern => Type != ResourceNameType.Unparsed;

        /// <summary>The string representation of the resource name.</summary>
        /// <returns>The string representation of the resource name.</returns>
        public override string ToString()
        {
            switch (Type)
            {
                case ResourceNameType.Unparsed: return UnparsedResource.ToString();
                case ResourceNameType.ProjectLocationClientGateway: return s_projectLocationClientGateway.Expand(ProjectId, LocationId, ClientGatewayId);
                default: throw new sys::InvalidOperationException("Unrecognized resource-type.");
            }
        }

        /// <summary>Returns a hash code for this resource name.</summary>
        public override int GetHashCode() => ToString().GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as ClientGatewayName);

        /// <inheritdoc/>
        public bool Equals(ClientGatewayName other) => ToString() == other?.ToString();

        /// <inheritdoc/>
        public static bool operator ==(ClientGatewayName a, ClientGatewayName b) => ReferenceEquals(a, b) || (a?.Equals(b) ?? false);

        /// <inheritdoc/>
        public static bool operator !=(ClientGatewayName a, ClientGatewayName b) => !(a == b);
    }

    public partial class ClientGateway
    {
        /// <summary>
        /// <see cref="gcbcv::ClientGatewayName"/>-typed view over the <see cref="Name"/> resource name property.
        /// </summary>
        public gcbcv::ClientGatewayName ClientGatewayName
        {
            get => string.IsNullOrEmpty(Name) ? null : gcbcv::ClientGatewayName.Parse(Name, allowUnparsed: true);
            set => Name = value?.ToString() ?? "";
        }
    }

    public partial class ListClientGatewaysRequest
    {
        /// <summary>
        /// <see cref="gagr::LocationName"/>-typed view over the <see cref="Parent"/> resource name property.
        /// </summary>
        public gagr::LocationName ParentAsLocationName
        {
            get => string.IsNullOrEmpty(Parent) ? null : gagr::LocationName.Parse(Parent, allowUnparsed: true);
            set => Parent = value?.ToString() ?? "";
        }
    }

    public partial class GetClientGatewayRequest
    {
        /// <summary>
        /// <see cref="gcbcv::ClientGatewayName"/>-typed view over the <see cref="Name"/> resource name property.
        /// </summary>
        public gcbcv::ClientGatewayName ClientGatewayName
        {
            get => string.IsNullOrEmpty(Name) ? null : gcbcv::ClientGatewayName.Parse(Name, allowUnparsed: true);
            set => Name = value?.ToString() ?? "";
        }
    }

    public partial class CreateClientGatewayRequest
    {
        /// <summary>
        /// <see cref="gagr::LocationName"/>-typed view over the <see cref="Parent"/> resource name property.
        /// </summary>
        public gagr::LocationName ParentAsLocationName
        {
            get => string.IsNullOrEmpty(Parent) ? null : gagr::LocationName.Parse(Parent, allowUnparsed: true);
            set => Parent = value?.ToString() ?? "";
        }
    }

    public partial class DeleteClientGatewayRequest
    {
        /// <summary>
        /// <see cref="gcbcv::ClientGatewayName"/>-typed view over the <see cref="Name"/> resource name property.
        /// </summary>
        public gcbcv::ClientGatewayName ClientGatewayName
        {
            get => string.IsNullOrEmpty(Name) ? null : gcbcv::ClientGatewayName.Parse(Name, allowUnparsed: true);
            set => Name = value?.ToString() ?? "";
        }
    }
}

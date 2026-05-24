//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace SharpInterop.Core;

/// <summary>
/// Identifies one member of the DCOM activation property array carried by
/// <c>IActivationProperties</c> OBJREFs.
/// </summary>
public enum ActivationPropertyId : uint {
    /// <summary>MS-DCOM SPECIAL_PROPERTIES_DATA.</summary>
    SpecialProperties = 1,

    /// <summary>Requested class and interface activation data.</summary>
    InstanceInfo = 2,

    /// <summary>Client/server location and protocol-sequence data.</summary>
    LocationInfo = 3,

    /// <summary>SCM reply data containing returned interface references.</summary>
    ScmReplyInfo = 4,

    /// <summary>Authentication, impersonation, and capability data.</summary>
    SecurityInfo = 5,
}

/// <summary>DCOM COMVERSION value.</summary>
public readonly record struct ActivationComVersion(ushort Major, ushort Minor) {
    /// <summary>DCOM v5.6, used by modern IRemoteSCMActivator activation.</summary>
    public static ActivationComVersion V5_6 { get; } = new(5, 6);
}

/// <summary>Managed shadow of SPECIAL_PROPERTIES_DATA.</summary>
public sealed record SpecialPropertiesData(
    ActivationComVersion ClientVersion,
    int Mode,
    int ClassContext,
    Guid RequestedIid,
    IReadOnlyList<int> SpecialProperties) {
    /// <summary>An empty v5.6 special-properties set.</summary>
    public static SpecialPropertiesData Empty { get; } = new(
        ActivationComVersion.V5_6,
        0,
        0,
        Guid.Empty,
        Array.Empty<int>());
}

/// <summary>Requested class and interface activation details.</summary>
public sealed record InstanceInfo(Guid Clsid, Guid RequestedIid, int ClassContext, int Mode);

/// <summary>Location and requested RPC protocol-sequence details.</summary>
public sealed record LocationInfo(string? MachineName, int ProcessId, IReadOnlyList<int> ProtocolSequences);

/// <summary>Authentication and impersonation details supplied during activation.</summary>
public sealed record SecurityInfo(int AuthenticationLevel, int ImpersonationLevel, int Capabilities);

/// <summary>SCM activation reply data with the returned OBJREF.</summary>
public sealed record ScmReplyInfo(int Hresult, Guid Oxid, Guid Oid, Guid Ipid, byte[] ObjRef) {
    /// <summary>Creates a reply and defensively copies the OBJREF payload.</summary>
    public ScmReplyInfo(int hresult, Guid oxid, Guid oid, Guid ipid, byte[] objRef, bool copy)
        : this(hresult, oxid, oid, ipid, copy ? Copy(objRef) : objRef) {
    }

    private static byte[] Copy(byte[] value) {
        ArgumentNullException.ThrowIfNull(value);
        return value.Length == 0 ? Array.Empty<byte>() : (byte[])value.Clone();
    }
}

/// <summary>Opaque activation property preserving an unrecognized property payload.</summary>
public sealed class ActivationProperty {
    /// <summary>Creates a property and defensively copies the payload.</summary>
    public ActivationProperty(ActivationPropertyId id, ReadOnlySpan<byte> payload) {
        Id = id;
        Payload = payload.Length == 0 ? Array.Empty<byte>() : payload.ToArray();
    }

    /// <summary>Property identifier.</summary>
    public ActivationPropertyId Id { get; }

    /// <summary>Raw property payload.</summary>
    public byte[] Payload { get; }
}

/// <summary>
/// Managed representation of the versioned activation property array exchanged by
/// IRemoteSCMActivator v5.6.
/// </summary>
public sealed class ActivationProperties {
    /// <summary>Empty v5.6 activation property set.</summary>
    public static ActivationProperties Empty { get; } = new();

    /// <summary>Creates an empty v5.6 activation property set.</summary>
    public ActivationProperties()
        : this(
            SpecialPropertiesData.Empty,
            null,
            null,
            null,
            null,
            Array.Empty<ActivationProperty>()) {
    }

    /// <summary>Creates an activation property set.</summary>
    public ActivationProperties(
        SpecialPropertiesData? specialProperties,
        InstanceInfo? instanceInfo,
        LocationInfo? locationInfo,
        ScmReplyInfo? scmReplyInfo,
        SecurityInfo? securityInfo,
        IReadOnlyList<ActivationProperty>? customProperties = null) {
        SpecialProperties = specialProperties ?? SpecialPropertiesData.Empty;
        InstanceInfo = instanceInfo;
        LocationInfo = locationInfo;
        ScmReplyInfo = scmReplyInfo;
        SecurityInfo = securityInfo;
        CustomProperties = CopyCustomProperties(customProperties);
    }

    /// <summary>SPECIAL_PROPERTIES_DATA.</summary>
    public SpecialPropertiesData SpecialProperties { get; }

    /// <summary>Requested class and interface information.</summary>
    public InstanceInfo? InstanceInfo { get; }

    /// <summary>Location and protocol sequence information.</summary>
    public LocationInfo? LocationInfo { get; }

    /// <summary>SCM reply information returned to the client.</summary>
    public ScmReplyInfo? ScmReplyInfo { get; }

    /// <summary>Security information.</summary>
    public SecurityInfo? SecurityInfo { get; }

    /// <summary>Unrecognized properties preserved for round-trip compatibility.</summary>
    public IReadOnlyList<ActivationProperty> CustomProperties { get; }

    /// <summary>Returns a copy with SCM reply information populated.</summary>
    public ActivationProperties WithScmReplyInfo(ScmReplyInfo reply) => new(
        SpecialProperties,
        InstanceInfo,
        LocationInfo,
        reply,
        SecurityInfo,
        CustomProperties);

    /// <summary>Returns the requested IID in activation-priority order.</summary>
    public Guid GetRequestedIidOr(Guid fallback) {
        if (InstanceInfo is { RequestedIid: var instanceIid } && instanceIid != Guid.Empty) {
            return instanceIid;
        }

        if (SpecialProperties.RequestedIid != Guid.Empty) {
            return SpecialProperties.RequestedIid;
        }

        return fallback;
    }

    private static IReadOnlyList<ActivationProperty> CopyCustomProperties(IReadOnlyList<ActivationProperty>? properties) {
        if (properties is null || properties.Count == 0) {
            return Array.Empty<ActivationProperty>();
        }

        var copy = new ActivationProperty[properties.Count];
        for (int i = 0; i < properties.Count; i++) {
            ActivationProperty property = properties[i];
            copy[i] = new ActivationProperty(property.Id, property.Payload);
        }

        return copy;
    }
}

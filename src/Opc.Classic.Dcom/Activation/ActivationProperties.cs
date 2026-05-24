//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace SharpInterop.Core;

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

    private static ActivationProperty[] CopyCustomProperties(IReadOnlyList<ActivationProperty>? properties) {
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

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Core;

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

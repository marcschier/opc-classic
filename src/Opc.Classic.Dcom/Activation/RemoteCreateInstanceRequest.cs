//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace SharpInterop.Core;

/// <summary>Decoded server-side IRemoteSCMActivator::RemoteCreateInstance request fields.</summary>
public sealed record RemoteCreateInstanceRequest(
    Guid Clsid,
    Guid RequestedIid,
    IReadOnlyList<int> ProtocolSequences) {
    /// <summary>Decoded activation properties supplied by the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Raw activation property payload, when the caller has not decoded it yet.</summary>
    public byte[] RawActivationProperties { get; init; } = Array.Empty<byte>();
}

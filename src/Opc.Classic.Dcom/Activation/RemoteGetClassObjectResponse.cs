//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Core;

/// <summary>Server-side IRemoteSCMActivator::RemoteGetClassObject response.</summary>
public sealed record RemoteGetClassObjectResponse(int Hresult, Guid Oxid, Guid Ipid, byte[] ObjRef) {
    /// <summary>Object identifier allocated for the exported class factory.</summary>
    public Guid Oid { get; init; }

    /// <summary>Activation properties returned to the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Encoded activation properties returned to the client.</summary>
    public byte[] EncodedActivationProperties { get; init; } = Array.Empty<byte>();
}

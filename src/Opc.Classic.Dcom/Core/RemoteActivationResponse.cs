//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Server-side IRemoteActivation::RemoteActivation response scaffold./// </summary>
public sealed record RemoteActivationResponse(
    int Hresult,
    Guid Oxid,
    Guid Ipid,
    byte[] ObjRef);

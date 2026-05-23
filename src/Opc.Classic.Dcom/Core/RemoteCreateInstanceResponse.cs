//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace SharpInterop.Core;

/// <summary>
/// Server-side IRemoteSCMActivator::RemoteCreateInstance response scaffold.
/// </summary>
public sealed record RemoteCreateInstanceResponse(
    int Hresult,
    Guid Oxid,
    Guid Ipid,
    byte[] ObjRef);

//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
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

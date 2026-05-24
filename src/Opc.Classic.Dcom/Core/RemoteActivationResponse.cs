//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hosting;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Server-side IRemoteActivation::RemoteActivation response scaffold./// </summary>
public sealed record RemoteActivationResponse(
    int Hresult,
    Guid Oxid,
    Guid Ipid,
    byte[] ObjRef);

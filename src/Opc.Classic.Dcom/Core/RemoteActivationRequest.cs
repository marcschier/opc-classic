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
/// Decoded server-side IRemoteActivation::RemoteActivation request fields.
/// </summary>
public sealed record RemoteActivationRequest(
    Guid Clsid,
    Guid Iid,
    int DwFlags,
    IReadOnlyList<int> ProtocolSeqs);

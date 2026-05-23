//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace SharpInterop.Core;

/// <summary>
/// Decoded server-side IRemoteSCMActivator::RemoteCreateInstance request fields.
/// </summary>
public sealed record RemoteCreateInstanceRequest(
    Guid Clsid,
    Guid RequestedIid,
    IReadOnlyList<int> ProtocolSequences);

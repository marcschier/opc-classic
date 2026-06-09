//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

/// <summary>Location and requested RPC protocol-sequence details.</summary>
public sealed record LocationInfo(string? MachineName, int ProcessId, IReadOnlyList<int> ProtocolSequences);

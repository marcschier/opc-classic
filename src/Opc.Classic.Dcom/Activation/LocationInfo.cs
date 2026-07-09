// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Location and requested RPC protocol-sequence details.
/// </summary>
public sealed record LocationInfo(string? MachineName, int ProcessId, IReadOnlyList<int> ProtocolSequences);

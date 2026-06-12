//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.InteropServices;

namespace Opc.Classic.Testing;

/// <summary>
/// A single in-memory call-channel invocation captured for test assertions.
/// </summary>
/// <param name="InterfaceId">The destination interface IID.</param>
/// <param name="Opnum">The destination DCE/RPC operation number.</param>
/// <param name="PayloadLength">The request payload length in bytes.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct InMemoryCall(Guid InterfaceId, int Opnum, int PayloadLength);

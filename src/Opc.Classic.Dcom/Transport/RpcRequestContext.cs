// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;

namespace Opc.Classic.Dcom.Transport;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct RpcRequestContext(
    bool IsAuthenticated,
    bool IsEstablished,
    OpcProtectionLevel ProtectionLevel,
    EndPoint RemoteEndpoint);

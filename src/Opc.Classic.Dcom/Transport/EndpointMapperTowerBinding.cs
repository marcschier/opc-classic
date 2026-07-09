// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using System.Runtime.InteropServices;

namespace Opc.Classic.Dcom.Transport;

[StructLayout(LayoutKind.Auto)]
public readonly record struct EndpointMapperTowerBinding(
    Guid InterfaceId,
    ushort InterfaceMajorVersion,
    ushort InterfaceMinorVersion,
    IPAddress Address,
    int Port);

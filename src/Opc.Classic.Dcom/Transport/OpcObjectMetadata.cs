// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Stable DCOM identity metadata assigned to a registered object.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct OpcObjectMetadata(ulong Oxid, ulong Oid);

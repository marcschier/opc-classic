//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;

namespace Opc.Classic.Da.Hosting.Windows;

[StructLayout(LayoutKind.Sequential)]
internal struct OpcConnectData
{
    public OpcConnectData(IntPtr pUnk, uint dwCookie)
    {
        this.pUnk = pUnk;
        this.dwCookie = dwCookie;
    }

    public IntPtr pUnk;
    public uint dwCookie;
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

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

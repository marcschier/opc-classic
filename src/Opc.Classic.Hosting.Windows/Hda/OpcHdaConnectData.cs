// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Opc.Classic.Hda.Hosting.Windows;

[StructLayout(LayoutKind.Sequential)]
internal struct OpcHdaConnectData
{
    public OpcHdaConnectData(IntPtr pUnk, uint dwCookie)
    {
        this.pUnk = pUnk;
        this.dwCookie = dwCookie;
    }

    public IntPtr pUnk;
    public uint dwCookie;
}

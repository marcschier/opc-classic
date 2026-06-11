// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Smb;

public static class SmbSession
{
    public static void Logon(SharpCifs.UniAddress address, NtlmPasswordAuthentication authentication)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(authentication);
    }
}

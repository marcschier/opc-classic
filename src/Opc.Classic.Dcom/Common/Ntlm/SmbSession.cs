// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public static class SmbSession
{
    public static void Logon(Opc.Classic.Dcom.Common.Ntlm.UniAddress address, NtlmPasswordAuthentication authentication)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(authentication);
    }
}

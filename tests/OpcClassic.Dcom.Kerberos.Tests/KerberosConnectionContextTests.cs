//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Dcom.Kerberos;
using TUnit.Core;

namespace OpcClassic.Dcom.Kerberos.Tests;

public sealed class KerberosConnectionContextTests
{
    [Test]
    public async Task KerberosConnectionContext_AcquireApRequest_throws_scaffold_NotImpl()
    {
        var authInfo = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", "alice", null, null, null);
        var context = new KerberosConnectionContext(authInfo);

        NotImplementedException? thrown = null;
        try
        {
            _ = context.AcquireApRequestAsync();
        }
        catch (NotImplementedException ex)
        {
            thrown = ex;
        }

        await Assert.That(thrown).IsNotNull();
        await Assert.That(thrown!.Message).Contains("Phase 3D scaffold");
    }
}

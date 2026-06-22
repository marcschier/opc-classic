// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosConnectionContextTests
{
    [Test]
    public async Task KerberosConnectionContext_AcquireApRequest_requires_password_or_keytab()
    {
        var authInfo = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", "alice", null, null, null);
        var context = new KerberosConnectionContext(authInfo);

        Exception? thrown = null;
        try
        {
            _ = await context.AcquireApRequestAsync();
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        await Assert.That(thrown is InvalidOperationException).IsTrue();
        await Assert.That(thrown!.Message).Contains("Password or KeytabPath");
    }
}

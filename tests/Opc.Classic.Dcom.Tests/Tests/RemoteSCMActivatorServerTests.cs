//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Hosting;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class RemoteSCMActivatorServerTests
{
    private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    private const int E_NOTIMPL = unchecked((int)0x80004001u);

    [Test]
    public async Task RemoteCreateInstance_unknown_clsid_returns_REGDB_E_CLASSNOTREG()
    {
        var server = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());
        var request = new RemoteCreateInstanceRequest(Guid.NewGuid(), Guid.NewGuid(), [7]);

        var response = await server.RemoteCreateInstanceAsync(request);

        await Assert.That(response.Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
        await Assert.That(response.Oxid).IsEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteCreateInstance_known_clsid_returns_E_NOTIMPL_today()
    {
        var clsid = Guid.NewGuid();
        var registry = new InMemoryClsidRegistry();
        registry.Register(new OpcClsidRegistration(
            clsid,
            "Opc.Classic.Test.1",
            "Opc.Classic.TestAssembly",
            "Opc.Classic.Tests.TestServer"));
        var server = new RemoteSCMActivatorServer(registry);
        var request = new RemoteCreateInstanceRequest(clsid, Guid.NewGuid(), [7]);

        var response = await server.RemoteCreateInstanceAsync(request);

        await Assert.That(response.Hresult).IsEqualTo(E_NOTIMPL);
        await Assert.That(response.Oxid).IsEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteCreateInstance_null_request_throws_ArgumentNullException()
    {
        var server = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());

        var thrown = await CaptureExceptionAsync(() => server.RemoteCreateInstanceAsync(null!));

        await Assert.That(thrown is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task RemoteCreateInstance_observes_cancellation_token()
    {
        var server = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());
        var request = new RemoteCreateInstanceRequest(Guid.NewGuid(), Guid.NewGuid(), [7]);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var thrown = await CaptureExceptionAsync(() => server.RemoteCreateInstanceAsync(request, cts.Token));

        await Assert.That(thrown is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task RemoteCreateInstanceRequest_record_equality()
    {
        var clsid = Guid.NewGuid();
        var iid = Guid.NewGuid();
        IReadOnlyList<int> protocolSequences = [7, 9];
        var left = new RemoteCreateInstanceRequest(clsid, iid, protocolSequences);
        var right = new RemoteCreateInstanceRequest(clsid, iid, protocolSequences);

        await Assert.That(left).IsEqualTo(right);
    }

    private static async Task<Exception?> CaptureExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

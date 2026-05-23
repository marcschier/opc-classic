//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Hosting;
using SharpInterop.Core;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class RemoteActivationV54ServerTests
{
    private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    private const int E_NOTIMPL = unchecked((int)0x80004001u);

    [Test]
    public async Task RemoteActivation_unknown_clsid_returns_REGDB_E_CLASSNOTREG()
    {
        var server = new RemoteActivationV54Server(new InMemoryClsidRegistry());
        var request = new RemoteActivationRequest(Guid.NewGuid(), Guid.NewGuid(), 0, [7]);

        var response = await server.RemoteActivationAsync(request);

        await Assert.That(response.Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
        await Assert.That(response.Oxid).IsEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteActivation_known_clsid_returns_E_NOTIMPL_today()
    {
        var clsid = Guid.NewGuid();
        var registry = new InMemoryClsidRegistry();
        registry.Register(new OpcClsidRegistration(
            clsid,
            "Opc.Classic.Test.1",
            "Opc.Classic.TestAssembly",
            "Opc.Classic.Tests.TestServer"));
        var server = new RemoteActivationV54Server(registry);
        var request = new RemoteActivationRequest(clsid, Guid.NewGuid(), 0, [7]);

        var response = await server.RemoteActivationAsync(request);

        await Assert.That(response.Hresult).IsEqualTo(E_NOTIMPL);
        await Assert.That(response.Oxid).IsEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteActivation_null_request_throws_ArgumentNullException()
    {
        var server = new RemoteActivationV54Server(new InMemoryClsidRegistry());

        await Assert.That(() => { _ = server.RemoteActivationAsync(null!); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RemoteActivation_observes_cancellation_token()
    {
        var server = new RemoteActivationV54Server(new InMemoryClsidRegistry());
        var request = new RemoteActivationRequest(Guid.NewGuid(), Guid.NewGuid(), 0, [7]);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.That(() => { _ = server.RemoteActivationAsync(request, cts.Token); })
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task RemoteActivationRequest_record_equality()
    {
        var clsid = Guid.NewGuid();
        var iid = Guid.NewGuid();
        IReadOnlyList<int> protocolSeqs = [7, 9];
        var left = new RemoteActivationRequest(clsid, iid, 1, protocolSeqs);
        var right = new RemoteActivationRequest(clsid, iid, 1, protocolSeqs);

        await Assert.That(left).IsEqualTo(right);
    }
}

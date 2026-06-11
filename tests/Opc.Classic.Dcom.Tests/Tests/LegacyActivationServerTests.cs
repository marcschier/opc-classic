//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Hosting;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class LegacyActivationServerTests
{
    private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154u);
    private const int E_NOTIMPL = unchecked((int)0x80004001u);
    private const int E_NOINTERFACE = unchecked((int)0x80004002u);

    [Test]
    public async Task RemoteActivation_UnknownClsid_ReturnsClassNotRegistered()
    {
        var modern = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());
        var legacy = new Opc.Classic.Dcom.Activation.LegacyActivationServer(modern);

        var response = await legacy.RemoteActivationAsync(new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
            Clsid: Guid.NewGuid(),
            RequestedIids: new[] { Guid.Parse(Opc.Classic.Dcom.Interfaces.IID_IUnknown) },
            ClientImpLevel: 2,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 0x07 }));

        await Assert.That(response.Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
        await Assert.That(response.InterfaceResults.Count).IsEqualTo(1);
        await Assert.That(response.InterfaceResults[0].Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
        await Assert.That(response.ServerVersion.Major).IsEqualTo((ushort)5);
        await Assert.That(response.ServerVersion.Minor).IsEqualTo((ushort)1);
    }

    [Test]
    public async Task RemoteActivation_EmptyRequestedIidList_ReturnsENoInterface()
    {
        var modern = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());
        var legacy = new Opc.Classic.Dcom.Activation.LegacyActivationServer(modern);

        var response = await legacy.RemoteActivationAsync(new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
            Clsid: Guid.NewGuid(),
            RequestedIids: Array.Empty<Guid>(),
            ClientImpLevel: 2,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 0x07 }));

        await Assert.That(response.Hresult).IsEqualTo(E_NOINTERFACE);
        await Assert.That(response.InterfaceResults.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteActivation_KnownClsid_AdditionalIidsBeyondPrimary_ReturnPerIidENoInterface()
    {
        // Register a known CLSID so the modern activator returns E_NOTIMPL (no
        // class factory) for the primary IID; the additional IIDs should each
        // get E_NOINTERFACE since the legacy server does not perform QueryInterface.
        var clsid = Guid.NewGuid();
        var registry = new InMemoryClsidRegistry();
        registry.Register(new OpcClsidRegistration(
            clsid,
            "Opc.Classic.Test.1",
            "Opc.Classic.TestAssembly",
            "Opc.Classic.Tests.TestServer"));
        var modern = new RemoteSCMActivatorServer(registry);
        var legacy = new Opc.Classic.Dcom.Activation.LegacyActivationServer(modern);

        IReadOnlyList<Guid> requested = new[]
        {
            Guid.Parse(Opc.Classic.Dcom.Interfaces.IID_IUnknown),
            Guid.NewGuid(),
            Guid.NewGuid(),
        };

        var response = await legacy.RemoteActivationAsync(new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
            Clsid: clsid,
            RequestedIids: requested,
            ClientImpLevel: 2,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 0x07 }));

        await Assert.That(response.InterfaceResults.Count).IsEqualTo(3);
        await Assert.That(response.InterfaceResults[0].Hresult).IsEqualTo(E_NOTIMPL);
        await Assert.That(response.InterfaceResults[1].Hresult).IsEqualTo(E_NOINTERFACE);
        await Assert.That(response.InterfaceResults[2].Hresult).IsEqualTo(E_NOINTERFACE);
    }

    [Test]
    public async Task RemoteActivation_ModeGetClassObject_RoutesToRemoteGetClassObject()
    {
        var modern = new RemoteSCMActivatorServer(new InMemoryClsidRegistry());
        var legacy = new Opc.Classic.Dcom.Activation.LegacyActivationServer(modern);

        var response = await legacy.RemoteActivationAsync(new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
            Clsid: Guid.NewGuid(),
            RequestedIids: new[] { Guid.Parse("00000001-0000-0000-C000-000000000046") }, // IClassFactory
            ClientImpLevel: 2,
            Mode: 1, // MODE_GET_CLASS_OBJECT
            RequestedProtocolSequences: new ushort[] { 0x07 }));

        // Empty registry → REGDB_E_CLASSNOTREG, same as for RemoteCreateInstance.
        await Assert.That(response.Hresult).IsEqualTo(REGDB_E_CLASSNOTREG);
    }
}

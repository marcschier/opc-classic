//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Net;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Mcp.Tests;

public sealed class OpcSinkObjRefBuilderTests
{
    private static readonly Guid SampleIid = IOPCDataCallback.InterfaceId;
    private static readonly IPEndPoint SampleEndpoint = new(IPAddress.Loopback, 51234);

    [Test]
    public async Task Build_SetsExpectedHeaderFields()
    {
        Guid ipid = Guid.NewGuid();
        IOpcInterfaceRef objref = OpcSinkObjRefBuilder.Build(SampleIid, ipid, SampleEndpoint);

        await Assert.That(objref.Iid).IsEqualTo(SampleIid);
        await Assert.That(objref.Ipid).IsEqualTo(ipid);
        await Assert.That(objref.Flags).IsEqualTo(0u);
        await Assert.That(objref.PublicRefs).IsEqualTo(1u);
        await Assert.That(objref.Oxid).IsNotEqualTo(0UL);
        await Assert.That(objref.Oid).IsNotEqualTo(0UL);
    }

    [Test]
    public async Task Build_GeneratesDistinctOxidAndOidPerCall()
    {
        Guid ipid = Guid.NewGuid();
        IOpcInterfaceRef a = OpcSinkObjRefBuilder.Build(SampleIid, ipid, SampleEndpoint);
        IOpcInterfaceRef b = OpcSinkObjRefBuilder.Build(SampleIid, ipid, SampleEndpoint);

        await Assert.That(a.Oxid).IsNotEqualTo(b.Oxid);
        await Assert.That(a.Oid).IsNotEqualTo(b.Oid);
    }

    [Test]
    public async Task Build_RespectsExplicitOxidAndOid()
    {
        Guid ipid = Guid.NewGuid();
        IOpcInterfaceRef objref = OpcSinkObjRefBuilder.Build(
            SampleIid, ipid, SampleEndpoint, oxid: 0xDEADBEEFCAFEBABEUL, oid: 0x1122334455667788UL);

        await Assert.That(objref.Oxid).IsEqualTo(0xDEADBEEFCAFEBABEUL);
        await Assert.That(objref.Oid).IsEqualTo(0x1122334455667788UL);
    }

    [Test]
    public async Task Build_EncodesTcpStringBindingTowerAndHostPort()
    {
        IOpcInterfaceRef objref = OpcSinkObjRefBuilder.Build(SampleIid, Guid.NewGuid(), SampleEndpoint);

        await Assert.That(objref.ResolverBindings[0]).IsEqualTo((ushort)0x07);

        // Decode the host[port] wchar* between the tower id and the first NUL.
        var sb = new System.Text.StringBuilder();
        for (int i = 1; i < objref.ResolverBindings.Count; i++)
        {
            ushort u = objref.ResolverBindings[i];
            if (u == 0)
            {
                break;
            }
            sb.Append((char)u);
        }

        await Assert.That(sb.ToString()).IsEqualTo("127.0.0.1[51234]");
    }

    [Test]
    public async Task Build_PlacesSecurityBindingAtSecurityOffset()
    {
        IOpcInterfaceRef objref = OpcSinkObjRefBuilder.Build(SampleIid, Guid.NewGuid(), SampleEndpoint);

        // SecurityOffset points into the bindings array at the first ushort of
        // the security binding (auth service id = 0x000A = WinNT NTLM).
        await Assert.That(objref.SecurityOffset).IsGreaterThan((ushort)0);
        await Assert.That(objref.SecurityOffset).IsLessThan((ushort)objref.ResolverBindings.Count);
        await Assert.That(objref.ResolverBindings[objref.SecurityOffset]).IsEqualTo((ushort)0x000A);
    }

    [Test]
    public async Task Build_RoundTripsThroughOpcInterfaceRefCodec()
    {
        Guid ipid = Guid.NewGuid();
        IOpcInterfaceRef built = OpcSinkObjRefBuilder.Build(SampleIid, ipid, SampleEndpoint);

        byte[] buffer = new byte[1024];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(ref writer, built);
        int written = writer.Position;

        var reader = new NdrReader(buffer.AsSpan(0, written));
        IOpcInterfaceRef decoded = OpcInterfaceRefCodec.Read(ref reader);

        await Assert.That(decoded.Iid).IsEqualTo(built.Iid);
        await Assert.That(decoded.Ipid).IsEqualTo(built.Ipid);
        await Assert.That(decoded.Oxid).IsEqualTo(built.Oxid);
        await Assert.That(decoded.Oid).IsEqualTo(built.Oid);
        await Assert.That(decoded.Flags).IsEqualTo(built.Flags);
        await Assert.That(decoded.PublicRefs).IsEqualTo(built.PublicRefs);
        await Assert.That(decoded.SecurityOffset).IsEqualTo(built.SecurityOffset);
        await Assert.That(decoded.ResolverBindings.Count).IsEqualTo(built.ResolverBindings.Count);
        // Element-by-element binding comparison so silent ushort drift is caught.
        for (int i = 0; i < built.ResolverBindings.Count; i++)
        {
            await Assert.That(decoded.ResolverBindings[i]).IsEqualTo(built.ResolverBindings[i]);
        }
    }

    [Test]
    public async Task Build_GeneratesDistinctOxidAndOid_AcrossManyCalls()
    {
        // N=2 sampling masks generators with substantial collision rates.
        // Generate 256 ObjRefs and assert per-field uniqueness via HashSet.Count.
        const int N = 256;
        Guid ipid = Guid.NewGuid();
        var oxids = new HashSet<ulong>(N);
        var oids = new HashSet<ulong>(N);
        for (int i = 0; i < N; i++)
        {
            IOpcInterfaceRef objref = OpcSinkObjRefBuilder.Build(SampleIid, ipid, SampleEndpoint);
            oxids.Add(objref.Oxid);
            oids.Add(objref.Oid);
        }
        await Assert.That(oxids.Count).IsEqualTo(N);
        await Assert.That(oids.Count).IsEqualTo(N);
    }

    [Test]
    public async Task Build_NullEndpoint_Throws()
    {
        await Assert.That(() => { _ = OpcSinkObjRefBuilder.Build(SampleIid, Guid.NewGuid(), null!); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Build_EmptyIid_Throws()
    {
        await Assert.That(() => { _ = OpcSinkObjRefBuilder.Build(Guid.Empty, Guid.NewGuid(), SampleEndpoint); })
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Build_EmptyIpid_Throws()
    {
        await Assert.That(() => { _ = OpcSinkObjRefBuilder.Build(SampleIid, Guid.Empty, SampleEndpoint); })
            .Throws<ArgumentException>();
    }
}

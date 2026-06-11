//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Testing;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class IActivationClientTests
{
    private static readonly Guid TestClsid = new("00112233-4455-6677-8899-AABBCCDDEEFF");
    private static readonly Guid IidIUnknown = new("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task RemoteActivationAsync_serializes_request_and_decodes_response()
    {
        RemoteActivationRequest? received = null;
        byte[] objRef = { 0x4d, 0x45, 0x4f, 0x57, 0x01, 0x00, 0x00, 0x00 };
        byte[] oxidBindings = CreateDualStringArray();
        var ipid = new Guid("11111111-2222-3333-4444-555555555555");
        var channel = new InMemoryCallChannelBuilder()
            .Register(OpcGuids.IID_IActivation, 0, (_, _, payload, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                received = IActivationCodec.DecodeRemoteActivationRequest(payload.Span);
                var response = new RemoteActivationResponse(
                    0,
                    new Guid("08070605-0403-0201-0000-000000000000"),
                    ipid,
                    6,
                    (5, 1),
                    new[] { new RemoteActivationInterfaceResult(0, objRef) })
                {
                    OxidBindings = oxidBindings,
                };
                return Task.FromResult(new NdrCallResult(0, IActivationCodec.EncodeRemoteActivationResponse(response)));
            })
            .Build();
        var client = new ActivationClient(channel);

        RemoteActivationResponse actual = await client.RemoteActivationAsync(
            TestClsid,
            new[] { "ncacn_ip_tcp" },
            "opc-file.moniker",
            new[] { IidIUnknown });

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Clsid).IsEqualTo(TestClsid);
        await Assert.That(received.ObjectName).IsEqualTo("opc-file.moniker");
        await Assert.That(received.RequestedIids.Count).IsEqualTo(1);
        await Assert.That(received.RequestedIids[0]).IsEqualTo(IidIUnknown);
        await Assert.That(received.RequestedProtocolSequences.Count).IsEqualTo(1);
        await Assert.That(received.RequestedProtocolSequences[0]).IsEqualTo((ushort)7);
        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(OpcGuids.IID_IActivation);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(0);
        await Assert.That(actual.Hresult).IsEqualTo(0);
        await Assert.That(actual.IpidRemUnknown).IsEqualTo(ipid);
        await Assert.That(actual.AuthnHint).IsEqualTo(6u);
        await Assert.That(actual.ServerVersion).IsEqualTo(((ushort)5, (ushort)1));
        await Assert.That(Convert.ToHexString(actual.OxidBindings.ToArray())).IsEqualTo(Convert.ToHexString(oxidBindings));
        await Assert.That(actual.InterfaceResults.Count).IsEqualTo(1);
        await Assert.That(Convert.ToHexString(actual.InterfaceResults[0].ObjRef.ToArray())).IsEqualTo(Convert.ToHexString(objRef));
    }

    [Test]
    public async Task Codec_round_trips_request_fields()
    {
        var request = new RemoteActivationRequest(
            TestClsid,
            new[] { IidIUnknown, new Guid("00020400-0000-0000-C000-000000000046") },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 })
        {
            ObjectName = "object-name",
            ObjectStorage = new byte[] { 0x4d, 0x45, 0x4f, 0x57 },
        };

        RemoteActivationRequest decoded = IActivationCodec.DecodeRemoteActivationRequest(
            IActivationCodec.EncodeRemoteActivationRequest(request));

        await Assert.That(decoded.Clsid).IsEqualTo(request.Clsid);
        await Assert.That(decoded.ObjectName).IsEqualTo(request.ObjectName);
        await Assert.That(Convert.ToHexString(decoded.ObjectStorage.ToArray())).IsEqualTo("4D454F57");
        await Assert.That(decoded.ClientImpLevel).IsEqualTo(3u);
        await Assert.That(decoded.Mode).IsEqualTo(0u);
        await Assert.That(decoded.RequestedIids.Count).IsEqualTo(2);
        await Assert.That(decoded.RequestedIids[1]).IsEqualTo(request.RequestedIids[1]);
        await Assert.That(decoded.RequestedProtocolSequences[0]).IsEqualTo((ushort)7);
    }

    [Test]
    public async Task RemoteActivationAsync_empty_iid_array_throws()
    {
        var client = new ActivationClient(new InMemoryCallChannelBuilder().Build());

        await Assert.That(async () =>
        {
            _ = await client.RemoteActivationAsync(TestClsid, new[] { "ncacn_ip_tcp" }, string.Empty, Array.Empty<Guid>());
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task RemoteActivationAsync_malformed_protocol_sequence_throws()
    {
        var client = new ActivationClient(new InMemoryCallChannelBuilder().Build());

        await Assert.That(async () =>
        {
            _ = await client.RemoteActivationAsync(TestClsid, new[] { "ncacn_http" }, string.Empty, new[] { IidIUnknown });
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task EncodeRemoteActivationRequest_known_input_matches_wire_layout()
    {
        var request = new RemoteActivationRequest(
            TestClsid,
            new[] { IidIUnknown },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 7 });

        byte[] payload = IActivationCodec.EncodeRemoteActivationRequest(request);

        await Assert.That(Convert.ToHexString(payload)).IsEqualTo(
            "33221100554477668899AABBCCDDEEFF" +
            "00000000" +
            "00000000" +
            "03000000" +
            "00000000" +
            "01000000" +
            "00000200" +
            "01000000" +
            "0000000000000000C000000000000046" +
            "01000000" +
            "01000000" +
            "0700");
    }

    private static byte[] CreateDualStringArray()
    {
        return new byte[]
        {
            0x02, 0x00,
            0x01, 0x00,
            0x07, 0x00,
            0x00, 0x00,
        };
    }
}

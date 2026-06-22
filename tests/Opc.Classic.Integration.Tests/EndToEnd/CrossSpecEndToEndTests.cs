// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Channels;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Security;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class CrossSpecEndToEndTests
{
    private const ushort MsvAvChannelBindings = 0x000A;

    [Test, Category("EndToEnd")]
    public async Task CausalityPropagation_Then_DaCallInsideAeHandlerUsesSameGuid()
    {
        var daPipeline = new DaEndToEndPipeline();
        var daChannel = new OrpcTrackingInMemoryChannel(daPipeline.Channel.InvokeAsync);
        var daProxy = new IOPCServerClientProxy(daChannel.Channel);
        var aeServer = new NestedAeServer(daProxy);
        var aeDispatcher = new OpcAeServerDispatcher(aeServer);
        var aeChannel = new OrpcTrackingInMemoryChannel(aeDispatcher.DispatchAsync);
        var aeProxy = new IOPCEventServerClientProxy(aeChannel.Channel);

        int[] errors = await aeProxy.AckConditionAsync(
            1,
            "operator.cross",
            "Ack triggers nested DA status",
            ["Plant1.AreaA.Tank7"],
            ["LevelHigh"],
            [new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero).ToFileTime()],
            [0x7711],
            CancellationToken.None);

        ObservedOrpcCall aeCall = aeChannel.Calls.Single();
        ObservedOrpcCall daCall = daChannel.Calls.Single();
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(aeCall.InterfaceId).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(aeCall.Opnum).IsEqualTo(IOPCEventServer.Opnums.AckConditionAsync);
        await Assert.That(daCall.InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(daCall.Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
        await Assert.That(aeCall.OrpcThis.CausalityId).IsNotEqualTo(Guid.Empty);
        await Assert.That(daCall.OrpcThis.CausalityId).IsEqualTo(aeCall.OrpcThis.CausalityId);
        await Assert.That(aeServer.ObservedCausalityId).IsEqualTo(aeCall.OrpcThis.CausalityId);
    }

    [Test, Category("EndToEnd")]
    public async Task OrpcThisAndThatRoundTrip_Then_EnvelopeIsWrittenAndParsed()
    {
        var daPipeline = new DaEndToEndPipeline();
        var channel = new OrpcTrackingInMemoryChannel(daPipeline.Channel.InvokeAsync);
        var proxy = new IOPCServerClientProxy(channel.Channel);

        string error = await proxy.GetErrorStringAsync(OpcResultId.BadRights.Code, 0x0409, CancellationToken.None);

        ObservedOrpcCall call = channel.Calls.Single();
        await Assert.That(error).Contains("0xC0040006");
        await Assert.That(call.OrpcThis.Version).IsEqualTo(OrpcComVersion.Default);
        await Assert.That(call.OrpcThis.Flags).IsEqualTo(0u);
        await Assert.That(call.OrpcThis.CausalityId).IsNotEqualTo(Guid.Empty);
        await Assert.That(call.OrpcThis.Extensions is null).IsTrue();
        await Assert.That(call.OrpcThat.Flags).IsEqualTo(0x1u);
        await Assert.That(call.OrpcThat.Extensions is null).IsTrue();
        await Assert.That(call.RequestPayload.Length).IsGreaterThan(0);
        await Assert.That(call.ResponsePayload.Length).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task NtlmMicPropagation_Then_AuthenticateMicFlowsThrough()
    {
        Type3Message type3 = CreateNtlmType3(channelBindingsHash: null, out NtlmAuthentication server);
        byte[] authenticate = type3.ToByteArray();
        InvokeCreateSecurityWhenServer(server, type3, authenticate);
        var daPipeline = new DaEndToEndPipeline();
        var channel = new OrpcTrackingInMemoryChannel(
            daPipeline.Channel.InvokeAsync,
            new FakeAuthContext(MicRequired: true, type3.GetMic(), ChannelBindingToken: null));
        var proxy = new IOPCServerClientProxy(channel.Channel);

        _ = await proxy.GetStatusAsync(CancellationToken.None);

        ObservedOrpcCall call = channel.Calls.Single();
        byte[] micFromAuthenticate = authenticate.AsSpan(Type3Message.MicOffset, Type3Message.MicLength).ToArray();
        await Assert.That(type3.HasMic).IsTrue();
        await Assert.That(type3.GetMic().SequenceEqual(micFromAuthenticate)).IsTrue();
        await Assert.That(call.Mic is not null).IsTrue();
        await Assert.That(call.Mic!.SequenceEqual(type3.GetMic())).IsTrue();
        await Assert.That(server.Security).IsNotNull();
    }

    [Test, Category("EndToEnd")]
    public async Task ChannelBindingToken_Then_CbtBytesFlowIntoAuthenticateMessage()
    {
        byte[] channelBindingToken = ChannelBindingsHash.Compute(new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: ReadOnlyMemory<byte>.Empty,
            ApplicationData: "tls-server-end-point:e2e-certificate"u8.ToArray()));
        Type3Message type3 = CreateNtlmType3(channelBindingToken, out _);
        var daPipeline = new DaEndToEndPipeline();
        var channel = new OrpcTrackingInMemoryChannel(
            daPipeline.Channel.InvokeAsync,
            new FakeAuthContext(MicRequired: false, Mic: null, channelBindingToken));
        var proxy = new IOPCServerClientProxy(channel.Channel);

        _ = await proxy.GetStatusAsync(CancellationToken.None);

        bool found = TryGetNtlmV2AvPair(type3.GetNTResponse(), MsvAvChannelBindings, out byte[] actualToken);
        ObservedOrpcCall call = channel.Calls.Single();
        await Assert.That(found).IsTrue();
        await Assert.That(actualToken.SequenceEqual(channelBindingToken)).IsTrue();
        await Assert.That(call.ChannelBindingToken is not null).IsTrue();
        await Assert.That(call.ChannelBindingToken!.SequenceEqual(channelBindingToken)).IsTrue();
        await Assert.That(call.InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(call.Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
    }

    private static Type3Message CreateNtlmType3(byte[]? channelBindingsHash, out NtlmAuthentication server)
    {
        var client = CreateAuthentication(channelBindingsHash);
        server = CreateAuthentication(channelBindingsHash: null);
        Type1Message type1 = client.CreateType1();
        Type2Message type2 = server.CreateType2(type1);
        return client.CreateType3(type2);
    }

    private static NtlmAuthentication CreateAuthentication(byte[]? channelBindingsHash)
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "true");
        properties.SetProperty("rpc.ntlm.seal", "true");
        properties.SetProperty("rpc.ntlm.keyExchange", "true");
        properties.SetProperty("rpc.ntlm.keyLength", "128");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.allowV1", "false");
        properties.SetProperty("rpc.ntlm.sso", "false");
        properties.SetProperty("rpc.ntlm.domain", "DOMAIN");
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, "User");
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, "Password");
        if (channelBindingsHash is not null)
        {
            properties.SetProperty("rpc.ntlm.channelBindingsHash", channelBindingsHash);
        }

        return new NtlmAuthentication(properties);
    }

    private static bool TryGetNtlmV2AvPair(byte[] ntResponse, ushort avId, out byte[] value)
    {
        const int ntProofLength = 16;
        const int avPairsOffsetInBlob = 28;
        int offset = ntProofLength + avPairsOffsetInBlob;
        while (offset + 4 <= ntResponse.Length)
        {
            ushort currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(ntResponse.AsSpan(offset, sizeof(ushort)));
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(ntResponse.AsSpan(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > ntResponse.Length - offset)
            {
                break;
            }

            if (currentAvId == 0)
            {
                break;
            }

            if (currentAvId == avId)
            {
                value = ntResponse.AsSpan(offset, length).ToArray();
                return true;
            }

            offset += length;
        }

        value = [];
        return false;
    }

    private static void InvokeCreateSecurityWhenServer(NtlmAuthentication authentication, Type3Message type3, byte[] authenticate)
    {
        MethodInfo method = typeof(NtlmAuthentication).GetMethod(
            "CreateSecurityWhenServerWithMic",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(object), typeof(byte[])],
            modifiers: null)!;
        try
        {
            method.Invoke(authentication, [type3, authenticate]);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private sealed class NestedAeServer : IOpcAeServer
    {
        private readonly IOPCServer _daServer;

        public NestedAeServer(IOPCServer daServer) => _daServer = daServer;

        public Guid ObservedCausalityId { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) => Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
            LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Nested AE server",
        });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0x1F);

        public async Task<int[]> AckConditionAsync(
            int dwCount,
            string acknowledgerId,
            string comment,
            string[] sources,
            string[] conditionNames,
            long[] activeTimes,
            int[] cookies,
            CancellationToken cancellationToken = default)
        {
            _ = dwCount;
            _ = acknowledgerId;
            _ = comment;
            _ = activeTimes;
            _ = sources;
            _ = conditionNames;
            ObservedCausalityId = CausalityContext.Current.Value.GetValueOrDefault();
            _ = await _daServer.GetStatusAsync(cancellationToken).ConfigureAwait(false);
            return cookies.Select(_ => OpcResultId.Ok.Code).ToArray();
        }
    }
}

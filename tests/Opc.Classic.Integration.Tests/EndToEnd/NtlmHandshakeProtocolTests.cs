// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Protocol tests assert exact NTLM wire bytes.

using System.Buffers.Binary;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Kerberos;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Security;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class NtlmHandshakeProtocolTests
{
    private const string TestDomain = "DOMAIN";
    private const string TestUser = "User";
    private const string TestPassword = "Password";
    private const string WrongPassword = "WrongPassword";
    private const string NtlmAuthContextTypeName = "Opc.Classic.Dcom.Rpc.Auth.ntlm.NtlmAuthentication+NtlmAuthContext";
    private const int NtResponseSecurityBufferOffset = 20;
    private const int NtlmV2ProofLength = 16;
    private const int NtlmV2AvPairsOffsetInBlob = 28;
    private static readonly byte[] ExpectedServerChallenge = [1, 2, 3, 4, 5, 6, 7, 8];

    [Test, Category("EndToEnd")]
    public async Task Type1Negotiate_GeneratesExpectedNtlmSspHeaderAndFlags()
    {
        var client = CreateAuthentication(password: TestPassword, channelBindingsHash: null);

        Type1Message type1 = client.CreateType1();
        byte[] negotiate = type1.ToByteArray();
        NtlmFlags flags = ReadType1Flags(negotiate);

        await AssertNtlmHeaderAsync(negotiate, expectedMessageType: 1);
        await Assert.That(type1.GetSuppliedDomain()).IsEqualTo(TestDomain);
        await Assert.That(type1.GetSuppliedWorkstation()).IsEqualTo(Type1Message.GetDefaultWorkstation());
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateUnicode)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspRequestTarget)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateNtlm)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateAlwaysSign)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateNtlm2)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateSign)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateSeal)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateKeyExch)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateVersion)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiate128)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiate56)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateOemDomainSupplied)).IsTrue();
        await Assert.That(HasFlag(flags, NtlmFlags.NtlmsspNegotiateOemWorkstationSupplied)).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task Type2Challenge_FromAuthenticationSourceCarriesChallengeTargetInfoAndMicRequest()
    {
        HandshakeTokens handshake = BuildHandshake(clientPassword: TestPassword);
        Type2Message type2 = handshake.Type2;
        byte[] targetInformation = type2.GetTargetInformation();
        bool hasMicFlags = TryGetAvPair(targetInformation, NtlmAvPairs.MsvAvFlags, out byte[] avFlags);
        uint avFlagsValue = hasMicFlags && avFlags.Length >= sizeof(uint)
            ? BinaryPrimitives.ReadUInt32LittleEndian(avFlags.AsSpan(0, sizeof(uint)))
            : 0;

        await AssertNtlmHeaderAsync(handshake.Type2Token, expectedMessageType: 2);
        await Assert.That(type2.MessageType).IsEqualTo(2);
        await Assert.That(type2.GetTarget()).IsEqualTo(TestDomain);
        await Assert.That(type2.GetChallenge().SequenceEqual(ExpectedServerChallenge)).IsTrue();
        await Assert.That(type2.GetChallenge().Length).IsEqualTo(8);
        await Assert.That(targetInformation.Length).IsGreaterThan(0);
        await Assert.That(HasFlag(type2.Flags, NtlmFlags.NtlmsspTargetTypeServer)).IsTrue();
        await Assert.That(HasFlag(type2.Flags, NtlmFlags.NtlmsspNegotiateTargetInfo)).IsTrue();
        await Assert.That(HasFlag(type2.Flags, NtlmFlags.NtlmsspNegotiateNtlm2)).IsTrue();
        await Assert.That(hasMicFlags).IsTrue();
        await Assert.That((avFlagsValue & NtlmAvPairs.MsvAvFlagsMic) != 0).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task Type3Authenticate_CarriesNtlmV2ResponseIdentityMicAndSessionKeys()
    {
        HandshakeTokens handshake = BuildHandshake(clientPassword: TestPassword);
        Type3Message type3 = handshake.Type3;
        byte[] authenticate = handshake.Type3Token;
        byte[] ntResponse = type3.GetNTResponse();
        byte[] ntProof = ntResponse[..NtlmV2ProofLength];
        byte[] blob = ntResponse[NtlmV2ProofLength..];
        byte[] blobSignature = blob[..4];
        byte[] blobReserved = blob[4..8];
        long timestamp = BinaryPrimitives.ReadInt64LittleEndian(blob.AsSpan(8, sizeof(long)));
        byte[] clientNonce = blob[16..24];
        byte[] blobReserved2 = blob[24..28];

        await AssertNtlmHeaderAsync(authenticate, expectedMessageType: 3);
        await Assert.That(type3.MessageType).IsEqualTo(3);
        await Assert.That(type3.GetDomain()).IsEqualTo(TestDomain);
        await Assert.That(type3.GetUser()).IsEqualTo(TestUser);
        await Assert.That(type3.GetWorkstation()).IsEqualTo(Type3Message.GetDefaultWorkstation());
        await Assert.That(ntResponse.Length).IsGreaterThan(NtlmV2ProofLength + NtlmV2AvPairsOffsetInBlob);
        await Assert.That(ntProof.Any(static b => b != 0)).IsTrue();
        await Assert.That(blobSignature.SequenceEqual(new byte[] { 0x01, 0x01, 0x00, 0x00 })).IsTrue();
        await Assert.That(blobReserved.SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00 })).IsTrue();
        await Assert.That(timestamp).IsGreaterThan(0);
        await Assert.That(clientNonce.Length).IsEqualTo(8);
        await Assert.That(clientNonce.Any(static b => b != 0)).IsTrue();
        await Assert.That(blobReserved2.SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00 })).IsTrue();
        await Assert.That(HasFlag(type3.Flags, NtlmFlags.NtlmsspNegotiateNtlm2)).IsTrue();
        await Assert.That(HasFlag(type3.Flags, NtlmFlags.NtlmsspNegotiateKeyExch)).IsTrue();
        await Assert.That(type3.GetSessionKey().Length).IsEqualTo(16);
        await Assert.That(type3.HasMic).IsTrue();
        await Assert.That(type3.GetMic().Length).IsEqualTo(Type3Message.MicLength);
        await Assert.That(authenticate.AsSpan(Type3Message.MicOffset, Type3Message.MicLength).ToArray()
            .SequenceEqual(type3.GetMic())).IsTrue();

        sbyte[] serverSessionKey = handshake.Source.Authenticate(new PropertyBag(), handshake.Type2, type3);
        byte[] serverSessionKeyBytes = FromSByteArray(serverSessionKey);
        ReadOnlyMemory<byte>? clientSessionKey = handshake.Client.EstablishedSessionKey;
        await Assert.That(clientSessionKey.HasValue).IsTrue();
        byte[] clientSessionKeyBytes = clientSessionKey.GetValueOrDefault().ToArray();
        await Assert.That(clientSessionKeyBytes.Length).IsEqualTo(16);
        await Assert.That(serverSessionKeyBytes.SequenceEqual(clientSessionKeyBytes)).IsTrue();
        await Assert.That(handshake.Source.SecurityEstablished).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task Authenticate_RejectsTamperedType3MicAndWrongPassword()
    {
        HandshakeTokens handshake = BuildHandshake(clientPassword: TestPassword);
        byte[] tamperedAuthenticate = handshake.Type3Token.ToArray();
        (ushort ntResponseLength, uint ntResponseOffset) = ReadSecurityBuffer(tamperedAuthenticate, NtResponseSecurityBufferOffset);
        tamperedAuthenticate[checked((int)ntResponseOffset)] ^= 0x40;
        Type3Message tamperedType3 = new(tamperedAuthenticate);

        await Assert.That((int)ntResponseLength).IsGreaterThan(0);
        await Assert.That(() => handshake.Source.Authenticate(new PropertyBag(), handshake.Type2, tamperedType3))
            .Throws<SecurityException>();

        HandshakeTokens wrongPassword = BuildHandshake(clientPassword: WrongPassword);
        await Assert.That(() => wrongPassword.Source.Authenticate(new PropertyBag(), wrongPassword.Type2, wrongPassword.Type3))
            .Throws<SecurityException>();
    }

    [Test, Category("EndToEnd")]
    public async Task ChannelBindingToken_IsEmbeddedInNtlmV2BlobAndMismatchesAreRejected()
    {
        byte[] channelBindingsHash = CreateChannelBindingsHash("tls-server-end-point:ntlm-protocol-test");
        HandshakeTokens handshake = BuildHandshake(
            clientPassword: TestPassword,
            clientChannelBindingsHash: channelBindingsHash,
            serverChannelBindingsHash: channelBindingsHash);
        bool foundChannelBindings = TryGetNtlmV2AvPair(
            handshake.Type3.GetNTResponse(),
            NtlmAvPairs.MsvAvChannelBindings,
            out byte[] actualChannelBindings);

        sbyte[] serverSessionKey = handshake.Source.Authenticate(new PropertyBag(), handshake.Type2, handshake.Type3);

        await Assert.That(foundChannelBindings).IsTrue();
        await Assert.That(actualChannelBindings.SequenceEqual(channelBindingsHash)).IsTrue();
        await Assert.That(serverSessionKey.Length).IsEqualTo(16);

        byte[] differentChannelBindingsHash = CreateChannelBindingsHash("tls-server-end-point:different-listener");
        HandshakeTokens mismatched = BuildHandshake(
            clientPassword: TestPassword,
            clientChannelBindingsHash: channelBindingsHash,
            serverChannelBindingsHash: differentChannelBindingsHash);
        await Assert.That(() => mismatched.Source.Authenticate(new PropertyBag(), mismatched.Type2, mismatched.Type3))
            .Throws<SecurityException>();
    }

    [Test, Category("EndToEnd")]
    public async Task CreateAuthContext_MapsModesAndNtlmContextTokenMethodsDriveHandshake()
    {
        OpcUrl url = OpcUrl.Parse("opcda://opc-host/Test.Server");
        var credentials = new NetworkCredential(TestUser, TestPassword, TestDomain);

        IAuthContext anonymous = NtlmAuthentication.CreateAuthContext(OpcConnectData.Anonymous(url));
        IAuthContext ntlm = NtlmAuthentication.CreateAuthContext(
            OpcConnectData.WithNtlmV2(url, credentials, OpcProtectionLevel.Privacy));
        IAuthContext kerberos = NtlmAuthentication.CreateAuthContext(
            OpcConnectData.WithKerberos(url, new NetworkCredential("user@DOMAIN.example", TestPassword, TestDomain)));

        await Assert.That(ReferenceEquals(anonymous, NoOpAuthContext.Instance)).IsTrue();
        await Assert.That(ntlm.GetType().FullName).IsEqualTo(NtlmAuthContextTypeName);
        await Assert.That(ntlm.AuthenticationServiceCode).IsEqualTo((byte)0x0A);
        await Assert.That(ntlm.ProtectionLevel).IsEqualTo(OpcProtectionLevel.Privacy);
        await Assert.That(kerberos).IsTypeOf<KerberosAuthContext>();
        await Assert.That(kerberos.AuthenticationServiceCode).IsEqualTo((byte)0x09);

        if (OperatingSystem.IsWindows())
        {
            IAuthContext windowsSso = NtlmAuthentication.CreateAuthContext(OpcConnectData.WithWindowsSso(url));
            await Assert.That(windowsSso).IsTypeOf<WindowsSsoAuthContext>();
            await Assert.That(windowsSso.AuthenticationServiceCode).IsEqualTo((byte)0x0A);
        }
        else
        {
            await Assert.That(() => NtlmAuthentication.CreateAuthContext(OpcConnectData.WithWindowsSso(url)))
                .Throws<PlatformNotSupportedException>();
        }

        byte[] initialToken = ntlm.BuildInitialToken();
        var type1 = new Type1Message(initialToken);
        var source = new TestAuthenticationSource(channelBindingsHash: null);
        byte[] challengeToken = source.CreateChallenge(new PropertyBag(), type1);
        byte[] authenticateToken = ntlm.ProcessChallengeToken(challengeToken);
        var type2 = new Type2Message(challengeToken);
        var type3 = new Type3Message(authenticateToken);
        sbyte[] serverSessionKey = source.Authenticate(new PropertyBag(), type2, type3);
        var ntlmSessionKeyProvider = (IAuthSessionKeyProvider)ntlm;
        ReadOnlyMemory<byte>? clientSessionKey = ntlmSessionKeyProvider.GetSessionKey();

        await AssertNtlmHeaderAsync(initialToken, expectedMessageType: 1);
        await AssertNtlmHeaderAsync(challengeToken, expectedMessageType: 2);
        await AssertNtlmHeaderAsync(authenticateToken, expectedMessageType: 3);
        await Assert.That(serverSessionKey.Length).IsEqualTo(16);
        await Assert.That(clientSessionKey.HasValue).IsTrue();
        await Assert.That(FromSByteArray(serverSessionKey).SequenceEqual(clientSessionKey.GetValueOrDefault().ToArray())).IsTrue();
    }

    private static HandshakeTokens BuildHandshake(
        string clientPassword,
        byte[]? clientChannelBindingsHash = null,
        byte[]? serverChannelBindingsHash = null)
    {
        var client = CreateAuthentication(clientPassword, clientChannelBindingsHash);
        var source = new TestAuthenticationSource(serverChannelBindingsHash);
        Type1Message type1 = client.CreateType1();
        byte[] type1Token = type1.ToByteArray();
        byte[] type2Token = source.CreateChallenge(new PropertyBag(), type1);
        var type2 = new Type2Message(type2Token);
        Type3Message type3 = client.CreateType3(type2);
        byte[] type3Token = type3.ToByteArray();
        return new HandshakeTokens(client, source, type1, type2, type3, type1Token, type2Token, type3Token);
    }

    private static NtlmAuthentication CreateAuthentication(string password, byte[]? channelBindingsHash)
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
        properties.SetProperty("rpc.ntlm.domain", TestDomain);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, TestUser);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, password);
        if (channelBindingsHash is not null)
        {
            properties.SetProperty("rpc.ntlm.channelBindingsHash", channelBindingsHash);
        }

        return new NtlmAuthentication(properties);
    }

    private static async Task AssertNtlmHeaderAsync(byte[] token, int expectedMessageType)
    {
        bool hasSignature = HasNtlmSignature(token);
        int messageType = ReadNtlmMessageType(token);
        await Assert.That(hasSignature).IsTrue();
        await Assert.That(messageType).IsEqualTo(expectedMessageType);
    }

    private static bool HasNtlmSignature(byte[] token) =>
        token.Length >= 8 && token.AsSpan(0, 8).SequenceEqual("NTLMSSP\0"u8);

    private static int ReadNtlmMessageType(byte[] token) =>
        BinaryPrimitives.ReadInt32LittleEndian(token.AsSpan(8, sizeof(int)));

    private static NtlmFlags ReadType1Flags(byte[] negotiate) =>
        (NtlmFlags)BinaryPrimitives.ReadUInt32LittleEndian(negotiate.AsSpan(12, sizeof(uint)));

    private static (ushort Length, uint Offset) ReadSecurityBuffer(byte[] token, int descriptorOffset) =>
    (
        BinaryPrimitives.ReadUInt16LittleEndian(token.AsSpan(descriptorOffset, sizeof(ushort))),
        BinaryPrimitives.ReadUInt32LittleEndian(token.AsSpan(descriptorOffset + 4, sizeof(uint)))
    );

    private static bool HasFlag(NtlmFlags flags, NtlmFlags flag) => (flags & flag) == flag;

    private static bool TryGetNtlmV2AvPair(byte[] ntResponse, ushort avId, out byte[] value)
    {
        if (ntResponse.Length <= NtlmV2ProofLength + NtlmV2AvPairsOffsetInBlob)
        {
            value = [];
            return false;
        }

        return TryGetAvPair(ntResponse.AsSpan(NtlmV2ProofLength + NtlmV2AvPairsOffsetInBlob), avId, out value);
    }

    private static bool TryGetAvPair(ReadOnlySpan<byte> targetInformation, ushort avId, out byte[] value)
    {
        int offset = 0;
        while (offset + 4 <= targetInformation.Length)
        {
            ushort currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset, sizeof(ushort)));
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > targetInformation.Length - offset)
            {
                break;
            }

            if (currentAvId == avId)
            {
                value = targetInformation.Slice(offset, length).ToArray();
                return true;
            }

            if (currentAvId == NtlmAvPairs.MsvAvEol)
            {
                break;
            }

            offset += length;
        }

        value = [];
        return false;
    }

    private static byte[] CreateChannelBindingsHash(string applicationData) =>
        ChannelBindingsHash.Compute(new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: ReadOnlyMemory<byte>.Empty,
            ApplicationData: System.Text.Encoding.ASCII.GetBytes(applicationData)));

    private static byte[] FromSByteArray(sbyte[] bytes)
    {
        var unsigned = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            unsigned[i] = unchecked((byte)bytes[i]);
        }

        return unsigned;
    }

    private static sbyte[] ToSByteArray(ReadOnlyMemory<byte>? bytes)
    {
        byte[] unsigned = bytes.GetValueOrDefault().ToArray();
        var signed = new sbyte[unsigned.Length];
        for (int i = 0; i < unsigned.Length; i++)
        {
            signed[i] = unchecked((sbyte)unsigned[i]);
        }

        return signed;
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

    private sealed record HandshakeTokens(
        NtlmAuthentication Client,
        TestAuthenticationSource Source,
        Type1Message Type1,
        Type2Message Type2,
        Type3Message Type3,
        byte[] Type1Token,
        byte[] Type2Token,
        byte[] Type3Token);

    private sealed class TestAuthenticationSource : AuthenticationSource
    {
        private readonly byte[]? _channelBindingsHash;
        private NtlmAuthentication? _server;

        public TestAuthenticationSource(byte[]? channelBindingsHash) =>
            _channelBindingsHash = channelBindingsHash is null ? null : (byte[])channelBindingsHash.Clone();

        public bool SecurityEstablished => _server?.Security is not null;

        public override byte[] CreateChallenge(PropertyBag properties, Type1Message type1)
        {
            _ = properties;
            _server = CreateAuthentication(TestPassword, _channelBindingsHash);
            return _server.CreateType2(type1).ToByteArray();
        }

        public override sbyte[] Authenticate(PropertyBag properties, Type2Message type2, Type3Message type3)
        {
            _ = properties;
            _ = type2;
            if (_server is null)
            {
                throw new InvalidOperationException("CreateChallenge must be called before Authenticate.");
            }

            InvokeCreateSecurityWhenServer(_server, type3, type3.ToByteArray());
            return ToSByteArray(_server.EstablishedSessionKey);
        }
    }
}

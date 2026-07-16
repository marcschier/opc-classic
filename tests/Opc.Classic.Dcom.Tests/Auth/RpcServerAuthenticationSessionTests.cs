// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Principal;
using Opc.Classic.Dcom.Rpc.Auth;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Auth;

public sealed class RpcServerAuthenticationSessionTests
{
    [Test]
    [Arguments(OpcProtectionLevel.Integrity)]
    [Arguments(OpcProtectionLevel.Privacy)]
    public async Task Protected_session_requires_packet_protection_context(
        OpcProtectionLevel protectionLevel)
    {
        await Assert.That(() => CreateSession(protectionLevel, protectionContext: null))
            .Throws<ArgumentException>();
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(65536)]
    public async Task Protected_session_requires_nonempty_verifier(int verifierLength)
    {
        var protectionContext = new StubProtectionContext(
            OpcProtectionLevel.Integrity,
            verifierLength);

        await Assert.That(() => CreateSession(
                OpcProtectionLevel.Integrity,
                protectionContext))
            .Throws<ArgumentException>();
    }

    [Test]
    [Arguments(OpcProtectionLevel.None)]
    [Arguments(OpcProtectionLevel.Connect)]
    public async Task Anonymous_or_connect_session_may_omit_protection_context(
        OpcProtectionLevel protectionLevel)
    {
        RpcServerAuthenticationSession session =
            CreateSession(protectionLevel, protectionContext: null);

        await Assert.That(session.ProtectionContext).IsNull();
        await Assert.That(session.ProtectionLevel).IsEqualTo(protectionLevel);
    }

    [Test]
    public async Task Protected_session_accepts_matching_context_with_verifier()
    {
        var protectionContext = new StubProtectionContext(
            OpcProtectionLevel.Privacy,
            verifierLength: 16);

        RpcServerAuthenticationSession session =
            CreateSession(OpcProtectionLevel.Privacy, protectionContext);

        await Assert.That(session.ProtectionContext).IsSameReferenceAs(protectionContext);
    }

    private static RpcServerAuthenticationSession CreateSession(
        OpcProtectionLevel protectionLevel,
        IRpcServerProtectionContext? protectionContext)
    {
        var principal = new GenericPrincipal(
            new GenericIdentity("test-user", "TEST"),
            []);
        return new RpcServerAuthenticationSession(
            authenticationService: 42,
            principal,
            protectionLevel,
            protectionContext);
    }

    private sealed class StubProtectionContext : IRpcServerProtectionContext
    {
        public StubProtectionContext(
            OpcProtectionLevel protectionLevel,
            int verifierLength)
        {
            ProtectionLevel = protectionLevel;
            VerifierLength = verifierLength;
        }

        public int AuthenticationService => 42;

        public OpcProtectionLevel ProtectionLevel { get; }

        public int VerifierLength { get; }

        public void Protect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            out byte[] verifier) =>
            verifier = new byte[VerifierLength];

        public bool Unprotect(
            Span<byte> signedRegion,
            int confidentialOffset,
            int confidentialLength,
            ReadOnlyMemory<byte> verifier) =>
            verifier.Length == VerifierLength;
    }
}

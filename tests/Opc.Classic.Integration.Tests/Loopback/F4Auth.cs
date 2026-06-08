//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Core;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F4Auth
{
    private const string Ntlmv2SkipReason =
        "Authenticated calls over the managed TCP listener are not yet supported: RpcServerConnectionProcessor rejects authenticated binds unless the dispatcher is IRpcRequestContextDispatcher, and NtlmConnectionContext.Accept throws 'Server-side NTLM bind challenge handling is not implemented' for BindPdu/AlterContextPdu. NTLMv2 protocol-level handshake is covered by NtlmHandshakeProtocolTests instead.";

    private const string KerberosSkipReason =
        "Authenticated calls over the managed TCP listener are not yet supported: Kerberos requires the KDC fixture covered by KerberosKdcFixtureTests plus server-side Kerberos acceptor wiring on the listener.";

    private const string SpnegoSkipReason =
        "Authenticated calls over the managed TCP listener are not yet supported: SPNEGO requires server-side negotiation wiring on the listener before it can select NTLMv2 or Kerberos.";

    // See NtlmHandshakeProtocolTests for protocol-layer NTLMv2 coverage.
    [Test, Skip(Ntlmv2SkipReason)]
    public void Ntlmv2_authenticates_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — bind NTLMv2 credentials to the managed call channel and verify authenticated proxy calls.
    }

    [Test, Skip(KerberosSkipReason)]
    public void Kerberos_authenticates_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — use the Testcontainers KDC fixture to issue tickets and authenticate the loopback channel.
    }

    [Test, Skip(SpnegoSkipReason)]
    public void Spnego_negotiates_ntlmv2_or_kerberos_for_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — exercise SPNEGO negotiation and assert the selected NTLMv2/Kerberos mechanism is enforced.
    }
}

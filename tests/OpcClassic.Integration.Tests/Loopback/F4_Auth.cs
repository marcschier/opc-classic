//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using TUnit.Core;

namespace OpcClassic.Integration.Tests.Loopback;

public sealed class F4_Auth
{
    [Test, Skip("Phase 13-followup: NTLMv2 managed loopback auth needs the Phase 3 auth flow wired into the in-memory call path.")]
    public void Ntlmv2_authenticates_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — bind NTLMv2 credentials to the managed call channel and verify authenticated proxy calls.
    }

    [Test, Skip("Phase 13-followup: Kerberos managed loopback auth needs Phase 3D KDC integration and ticket acquisition.")]
    public void Kerberos_authenticates_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — use the Testcontainers KDC fixture to issue tickets and authenticate the loopback channel.
    }

    [Test, Skip("Phase 13-followup: SPNEGO managed loopback auth needs Phase 3E negotiation wiring into the auth flow.")]
    public void Spnego_negotiates_ntlmv2_or_kerberos_for_the_managed_loopback_call_path()
    {
        // TODO: Phase 13-followup — exercise SPNEGO negotiation and assert the selected NTLMv2/Kerberos mechanism is enforced.
    }
}

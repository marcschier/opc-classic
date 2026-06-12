//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Smb.Tests.Pcap;

public sealed class Smb2NegotiateReplayTests : PcapFixtureBase
{
    [Test]
    public async Task Negotiate_smb2_1_replays_placeholder_fixture()
    {
        _ = await ReplayNegotiateFixtureAsync(
            "negotiate-smb2-1.txt",
            Smb2Dialect.Smb202,
            expectedSigningRequired: false,
            expectedEncryptionSupported: false);
    }
}

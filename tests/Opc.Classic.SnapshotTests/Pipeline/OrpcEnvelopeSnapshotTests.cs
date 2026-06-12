//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Ndr;
using Opc.Classic.SnapshotTests.Support;

namespace Opc.Classic.SnapshotTests.Pipeline;

public sealed class OrpcEnvelopeSnapshotTests
{
    [Test]
    public async Task OrpcThis_with_zero_causality_guid_encodes_to_stable_bytes()
    {
        var value = new OrpcThis
        {
            CausalityId = Guid.Empty,
        };

        await SnapshotVerifier.VerifyBytes(
            "ORPC_THIS",
            "COMVERSION 5.7, flags=0, causality=00000000-0000-0000-0000-000000000000, no extensions",
            NdrSnapshotWriter.Write((ref NdrWriter writer) => value.Write(ref writer), capacity: OrpcThis.NullExtensionsWireSize));
    }

    [Test]
    public async Task OrpcThat_with_flags_zero_and_no_extensions_encodes_to_stable_bytes()
    {
        var value = new OrpcThat
        {
            Flags = 0,
            Extensions = null,
        };

        await SnapshotVerifier.VerifyBytes(
            "ORPC_THAT",
            "flags=0, no extensions",
            NdrSnapshotWriter.Write((ref NdrWriter writer) => value.Write(ref writer), capacity: OrpcThat.NullExtensionsWireSize));
    }
}

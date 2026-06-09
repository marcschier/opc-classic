//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using Opc.Classic.Dcom.Registry;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests.Fixtures.Winreg;

[NotInParallel]
public sealed class WinregFixtureReplayTests {
    private static readonly byte[] s_expectedHandle = Enumerable.Range(1, 20).Select(static value => (byte)value).ToArray();

    [Test]
    public async Task OpenLocalMachine_request_marshals_to_canonical_bytes() {
        var expectedRequest = MockWinregServer.ReadFixture("openlocalmachine_request.bin");
        var (client, server) = MockWinregServer.CreateClient(
            "openlocalmachine_request.bin",
            "openlocalmachine_response.bin");

        _ = client.OpenHKLM();

        await Assert.That(server.GetLastCanonicalRequest()).IsEquivalentTo(expectedRequest);
        server.AssertCompleted();
    }

    [Test]
    public async Task OpenLocalMachine_response_unmarshals_to_valid_policy_handle() {
        var (client, server) = MockWinregServer.CreateClient(
            "openlocalmachine_request.bin",
            "openlocalmachine_response.bin");

        var handle = client.OpenHKLM();

        await Assert.That(handle.Handle).IsEquivalentTo(s_expectedHandle);
        await Assert.That(handle.Handle.Any(static value => value != 0)).IsTrue();
        server.AssertCompleted();
    }

    [Test]
    public async Task BaseRegEnumKey_response_unmarshals_subkey_name() {
        var (client, server) = MockWinregServer.CreateClient(
            "enumkey_request.bin",
            "enumkey_response.bin");
        var handle = CreatePolicyHandle();

        var result = client.EnumKey(handle, 0);

        await Assert.That(result[0]).IsEqualTo("Software");
        await Assert.That(result[1]).IsEqualTo(string.Empty);
        server.AssertCompleted();
    }

    private static PolicyHandle CreatePolicyHandle() {
        var handle = new PolicyHandle(false);
        Array.Copy(s_expectedHandle, 0, handle.Handle, 0, s_expectedHandle.Length);
        return handle;
    }
}

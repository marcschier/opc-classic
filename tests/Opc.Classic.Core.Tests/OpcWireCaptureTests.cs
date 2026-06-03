//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Exercises Track AK2: WireCapturingCallChannel decorator + OpcWireCapture
// static gate. The decorator writes per-call hex dumps under the configured
// directory; failures during write must NEVER alter call semantics.
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Diagnostics;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcWireCaptureTests
{
    private static readonly Guid SampleIid = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");

    [Test]
    public async Task Wrap_WhenEnvVarUnset_ReturnsChannelUnchanged()
    {
        Environment.SetEnvironmentVariable("OPCCLASSIC_WIRE_CAPTURE_DIR", null);
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));

        ICallChannel wrapped = OpcWireCapture.Wrap(inner, "tag");

        await Assert.That(ReferenceEquals(wrapped, inner)).IsTrue();
    }

    [Test]
    public async Task WireCapturingCallChannel_WritesHexDumpPerCall()
    {
        string dir = Path.Combine(Path.GetTempPath(), "opc-wire-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var requestBytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var responseBytes = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };
            var inner = new InMemoryCallChannel((iid, opnum, payload, _) =>
                Task.FromResult(new NdrCallResult(0, responseBytes)));
            var capturing = new WireCapturingCallChannel(inner, dir, "test-tag");

            NdrCallResult result = await capturing.InvokeAsync(SampleIid, 3, requestBytes, CancellationToken.None);

            await Assert.That(result.Hresult).IsEqualTo(0);
            string[] files = Directory.GetFiles(dir, "*.hex");
            await Assert.That(files.Length).IsEqualTo(1);
            string contents = await File.ReadAllTextAsync(files[0]);
            await Assert.That(contents.Contains("# context: test-tag", StringComparison.Ordinal)).IsTrue();
            await Assert.That(contents.Contains("# iid:     39c13a4d-011e-11d0-9675-0020afd8adb3", StringComparison.Ordinal)).IsTrue();
            await Assert.That(contents.Contains("## request (4 bytes)", StringComparison.Ordinal)).IsTrue();
            await Assert.That(contents.Contains("de ad be ef", StringComparison.Ordinal)).IsTrue();
            await Assert.That(contents.Contains("ca fe ba be", StringComparison.Ordinal)).IsTrue();
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }

    [Test]
    public async Task WireCapturingCallChannel_WriteFailureDoesNotPropagate()
    {
        // Construct a path that cannot be created (illegal characters on Windows).
        string badDir = Path.Combine(Path.GetTempPath(), "wire-test-" + Guid.NewGuid().ToString("N"), "bad|path*name");
        var responseBytes = new byte[] { 0x01 };
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, responseBytes)));
        var capturing = new WireCapturingCallChannel(inner, badDir, "test-tag");

        NdrCallResult result = await capturing.InvokeAsync(SampleIid, 3, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(0);
    }
}

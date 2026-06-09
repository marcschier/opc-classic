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
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

[NotInParallel(nameof(OpcWireCaptureTests))]
public sealed class OpcWireCaptureTests {
    private static readonly Guid SampleIid = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");

    private const string EnvVarName = "OPCCLASSIC_WIRE_CAPTURE_DIR";

    /// <summary>
    /// Saves the current value of OPCCLASSIC_WIRE_CAPTURE_DIR, sets it to
    /// <paramref name="value"/>, and returns an IDisposable that restores
    /// the prior value on disposal. Prevents test-isolation leaks where one
    /// test's env-var mutation bleeds into the rest of the suite.
    /// </summary>
    private static IDisposable WithEnvVar(string? value) {
        string? prior = Environment.GetEnvironmentVariable(EnvVarName);
        Environment.SetEnvironmentVariable(EnvVarName, value);
        return new RestoreEnvVarOnDispose(prior);
    }

    private sealed class RestoreEnvVarOnDispose : IDisposable {
        private readonly string? _prior;
        public RestoreEnvVarOnDispose(string? prior) => _prior = prior;
        public void Dispose() => Environment.SetEnvironmentVariable(EnvVarName, _prior);
    }

    [Test]
    public async Task Wrap_WhenEnvVarUnset_ReturnsChannelUnchanged() {
        using var _ = WithEnvVar(null);
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));

        ICallChannel wrapped = OpcWireCapture.Wrap(inner, "tag");

        await Assert.That(ReferenceEquals(wrapped, inner)).IsTrue();
        await Assert.That(OpcWireCapture.IsEnabled).IsFalse();
    }

    [Test]
    public async Task WireCapturingCallChannel_WritesHexDumpPerCall() {
        string dir = Path.Combine(Path.GetTempPath(), "opc-wire-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try {
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
        finally {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }

    [Test]
    public async Task WireCapturingCallChannel_WriteFailureDoesNotPropagate() {
        // Force a guaranteed-invalid directory path on every OS by using a regular
        // FILE as the parent of the intended capture dir. Path.Combine(file, "child")
        // creates a logical sub-path of an actual file → Directory.CreateDirectory()
        // throws on every platform. The pipe|asterisk approach from the original
        // test was Windows-only and silently succeeded on Linux/macOS.
        string tmpFile = Path.GetTempFileName();
        string badDir = Path.Combine(tmpFile, "wire-test-" + Guid.NewGuid().ToString("N"));
        try {
            var responseBytes = new byte[] { 0x01 };
            var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, responseBytes)));
            var capturing = new WireCapturingCallChannel(inner, badDir, "test-tag");

            NdrCallResult result = await capturing.InvokeAsync(SampleIid, 3, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

            await Assert.That(result.Hresult).IsEqualTo(0);
        }
        finally {
            try { File.Delete(tmpFile); } catch { /* best-effort */ }
        }
    }

    [Test]
    public async Task Wrap_WhenEnvVarSet_ReturnsWireCapturingDecorator() {
        string dir = Path.Combine(Path.GetTempPath(), "opc-wire-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        using var _ = WithEnvVar(dir);
        try {
            var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));

            ICallChannel wrapped = OpcWireCapture.Wrap(inner, "tag");

            await Assert.That(ReferenceEquals(wrapped, inner)).IsFalse();
            await Assert.That(wrapped).IsTypeOf<WireCapturingCallChannel>();
            await Assert.That(OpcWireCapture.IsEnabled).IsTrue();
            await Assert.That(OpcWireCapture.CaptureDirectory).IsEqualTo(dir);
        }
        finally {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }

    [Test]
    public async Task Wrap_WhenEnvVarWhitespaceOnly_TreatsAsDisabled() {
        using var _ = WithEnvVar("   ");
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));

        ICallChannel wrapped = OpcWireCapture.Wrap(inner, "tag");

        await Assert.That(ReferenceEquals(wrapped, inner)).IsTrue();
        await Assert.That(OpcWireCapture.IsEnabled).IsFalse();
    }

    [Test]
    public async Task Wrap_NullChannel_Throws() {
        await Assert.That(() => { _ = OpcWireCapture.Wrap(null!, "tag"); }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Wrap_NullContextTag_Throws() {
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        await Assert.That(() => { _ = OpcWireCapture.Wrap(inner, null!); }).Throws<ArgumentNullException>();
    }
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Verifies the round-trip between WireCapturingCallChannel (the writer) and
// WireCaptureFile (the loader): a payload written through the decorator must
// be loadable byte-for-byte by the replay helper.
//

using Opc.Classic.Da.Tests.Wire.Replay;
using Opc.Classic.Diagnostics;
using Opc.Classic.Testing;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class WireCaptureFileTests
{
    private static readonly Guid SampleIid = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");

    [Test]
    public async Task LoadsRequestAndResponseBytesEmittedByWireCapturingCallChannel()
    {
        string dir = Path.Combine(Path.GetTempPath(), "wire-replay-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var requestBytes = new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                0x11, 0x12, 0x13, 0x14,
            };
            var responseBytes = new byte[]
            {
                0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00, 0x11,
                0x22, 0x33,
            };
            var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0x00040003, responseBytes)));
            var capturing = new WireCapturingCallChannel(inner, dir, "replay-test");

            await capturing.InvokeAsync(SampleIid, 7, requestBytes, CancellationToken.None);

            string[] files = Directory.GetFiles(dir, "*.hex");
            await Assert.That(files.Length).IsEqualTo(1);

            WireCaptureFile capture = WireCaptureFile.Load(files[0]);

            await Assert.That(capture.RequestPayload.Length).IsEqualTo(requestBytes.Length);
            await Assert.That(capture.RequestPayload).IsEquivalentTo(requestBytes);
            await Assert.That(capture.ResponsePayload.Length).IsEqualTo(responseBytes.Length);
            await Assert.That(capture.ResponsePayload).IsEquivalentTo(responseBytes);
            await Assert.That(capture.Iid).IsEqualTo(SampleIid);
            await Assert.That(capture.Opnum).IsEqualTo(7);
            await Assert.That(capture.Metadata["hresult"]).IsEqualTo("0x00040003");
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }

    [Test]
    public async Task FormatResponseContext_RendersHexWindowAroundOffset()
    {
        byte[] response = new byte[32];
        for (int i = 0; i < response.Length; i++)
        {
            response[i] = (byte)i;
        }
        var inner = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, response)));

        string dir = Path.Combine(Path.GetTempPath(), "wire-replay-fmt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            await new WireCapturingCallChannel(inner, dir, "fmt-test").InvokeAsync(SampleIid, 3, ReadOnlyMemory<byte>.Empty, CancellationToken.None);
            string file = Directory.GetFiles(dir, "*.hex")[0];

            WireCaptureFile capture = WireCaptureFile.Load(file);
            string window = capture.FormatResponseContext(position: 16);

            await Assert.That(window.Contains("Wire context", StringComparison.Ordinal)).IsTrue();
            await Assert.That(window.Contains("position 16", StringComparison.Ordinal)).IsTrue();
            // Byte at offset 16 is 0x10 — confirm the marker lands there.
            await Assert.That(window.Contains(">>10", StringComparison.Ordinal)).IsTrue();
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }
}

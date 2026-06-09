//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class HexFormatBridgeTests {
    [Test]
    public async Task Constructor_NullOrEmptyArguments_Throw() {
        await Assert.That(() => new HexFormatBridge(null!, "tag")).Throws<ArgumentNullException>();
        await Assert.That(() => new HexFormatBridge(string.Empty, "tag")).Throws<ArgumentException>();
        await Assert.That(() => new HexFormatBridge("captures", null!)).Throws<ArgumentNullException>();
        await Assert.That(() => new HexFormatBridge("captures", string.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task Write_NullRequestAndResponse_ReturnsNullAndCreatesNoFiles() {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try {
            var bridge = new HexFormatBridge(directory, "context");

            string? path = bridge.Write(null, ReadOnlyMemory<byte>.Empty, null, ReadOnlyMemory<byte>.Empty);

            await Assert.That(path).IsNull();
            await Assert.That(Directory.GetFiles(directory, "*.hex").Length).IsEqualTo(0);
        }
        finally {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task Write_RequestAndResponse_WritesExpectedFilenameAndContent() {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try {
            Guid iid = Guid.Parse("11111111-2222-3333-4444-555555555555");
            Guid ipid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
            var timestamp = new DateTimeOffset(2026, 1, 2, 3, 4, 5, 678, TimeSpan.Zero);
            var request = new DecodedOpcPdu {
                Timestamp = timestamp,
                PduType = "request",
                CallId = 77,
                InterfaceId = iid,
                ObjectIpid = ipid,
                Opnum = 12,
            };
            var response = request with {
                PduType = "response",
                Hresult = unchecked((int)0x80004005),
            };
            byte[] requestBytes =
            [
                0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00, 0x01,
                0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                0x10, 0x11,
            ];
            byte[] responseBytes = [0xFE, 0xDC, 0xBA];
            var bridge = new HexFormatBridge(directory, "live-session");

            string? path = bridge.Write(request, requestBytes, response, responseBytes);

            await Assert.That(path).IsNotNull();
            string expectedName = $"20260102T030405.678_000001_live-session_iid-{iid:N}_op-12.hex";
            await Assert.That(Path.GetFileName(path!)).IsEqualTo(expectedName);
            string content = await File.ReadAllTextAsync(path!, TestContext.Current!.CancellationToken);
            await Assert.That(content).Contains("# Opc.Classic wire capture (from network packet capture)");
            await Assert.That(content).Contains("# context: live-session");
            await Assert.That(content).Contains($"# iid: {iid:D}");
            await Assert.That(content).Contains("# opnum: 12");
            await Assert.That(content).Contains("# hresult: 0x80004005");
            await Assert.That(content).Contains("# call_id: 77");
            await Assert.That(content).Contains($"# object_ipid: {ipid:D}");
            await Assert.That(content).Contains("## request (18 bytes)");
            await Assert.That(content).Contains("0000: aa bb cc dd ee ff 00 01 02 03 04 05 06 07 08 09 ");
            await Assert.That(content).Contains("0010: 10 11 ");
            await Assert.That(content).Contains("## response (3 bytes)");
            await Assert.That(content).Contains("0000: fe dc ba ");
        }
        finally {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task Write_ContextTagWithUnsafeCharacters_ReplacesThemWithDashesInFilenameAndBanner() {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try {
            Guid iid = Guid.Parse("22222222-3333-4444-5555-666666666666");
            var request = new DecodedOpcPdu {
                Timestamp = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
                PduType = "request",
                InterfaceId = iid,
                Opnum = 1,
            };
            var bridge = new HexFormatBridge(directory, "bad:tag/with spaces");

            string? path = bridge.Write(request, new byte[] { 0x01 }, null, ReadOnlyMemory<byte>.Empty);

            await Assert.That(path).IsNotNull();
            await Assert.That(Path.GetFileName(path!)).IsEqualTo(
                $"20260102T030405.000_000001_bad-tag-with-spaces_iid-{iid:N}_op-1.hex");
            string content = await File.ReadAllTextAsync(path!, TestContext.Current!.CancellationToken);
            await Assert.That(content).Contains("# context: bad-tag-with-spaces");
        }
        finally {
            TestDirectories.DeleteIfExists(directory);
        }
    }
}

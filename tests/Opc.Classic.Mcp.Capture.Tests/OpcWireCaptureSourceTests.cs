//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class OpcWireCaptureSourceTests
{
    [Test]
    public async Task StartAsync_NullOrMissingReplayDirectory_Throws()
    {
        var source = new OpcWireCaptureSource();
        CancellationToken cancellationToken = TestContext.Current!.CancellationToken;

        await Assert.That(async () => await source.StartAsync(null!, cancellationToken))
            .Throws<ArgumentNullException>();
        await Assert.That(async () => await source.StartAsync(new CaptureStartRequest(), cancellationToken))
            .Throws<CaptureException>();
        await Assert.That(async () => await source.StartAsync(
                new CaptureStartRequest(ReplaySourceDirectory: Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))),
                cancellationToken))
            .Throws<CaptureException>();
    }

    [Test]
    public async Task ReadAllAsync_BeforeStart_YieldsNoPackets()
    {
        var source = new OpcWireCaptureSource();

        var packets = await source.ReadAllAsync(null, TestContext.Current!.CancellationToken).ToListAsync();

        await Assert.That(packets.Count).IsEqualTo(0);
        await Assert.That(source.PacketCount).IsEqualTo(0);
        await Assert.That(source.ByteCount).IsEqualTo(0);
        await Assert.That(source.LinkType).IsEqualTo(0);
        await Assert.That(source.GetRawPcapFilePath()).IsNull();
    }

    [Test]
    public async Task StartAndReadAllAsync_HexFiles_ReplaysRequestAndResponsePacketsWithAnnotations()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            Guid iid = Guid.Parse("11111111-2222-3333-4444-555555555555");
            string firstPath = Path.Combine(directory, "0001.hex");
            string secondPath = Path.Combine(directory, "0002.hex");
            await File.WriteAllTextAsync(
                firstPath,
                BuildHexFile(
                    iid,
                    opnum: 7,
                    "2026-06-07T10:11:12.0000000Z",
                    "0x80004005",
                    "0000: aa bb \n",
                    "0000: 01 02 03 \n"),
                TestContext.Current!.CancellationToken);
            await File.WriteAllTextAsync(
                secondPath,
                BuildHexFile(
                    iid,
                    opnum: 8,
                    "2026-06-07T10:11:13.0000000Z",
                    "0x00000000",
                    "0000: cc \n",
                    string.Empty),
                TestContext.Current.CancellationToken);
            var source = new OpcWireCaptureSource();

            await source.StartAsync(new CaptureStartRequest(ReplaySourceDirectory: directory), TestContext.Current.CancellationToken);
            await source.StopAsync(TestContext.Current.CancellationToken);
            var packets = await source.ReadAllAsync(null, TestContext.Current.CancellationToken).ToListAsync();

            await Assert.That(source.PacketCount).IsEqualTo(3);
            await Assert.That(source.ByteCount).IsEqualTo(6);
            await Assert.That(packets.Count).IsEqualTo(3);
            await Assert.That(packets[0].Timestamp).IsEqualTo(new DateTimeOffset(2026, 6, 7, 10, 11, 12, TimeSpan.Zero));
            await Assert.That(packets[0].OriginalLength).IsEqualTo(2);
            await Assert.That(packets[0].LinkType).IsEqualTo(0);
            await Assert.That(packets[0].Data.ToArray().SequenceEqual(new byte[] { 0xAA, 0xBB })).IsTrue();
            await Assert.That(packets[0].Annotations["direction"]).IsEqualTo("request");
            await Assert.That(packets[0].Annotations["iid"]).IsEqualTo(iid.ToString("D"));
            await Assert.That(packets[0].Annotations["opnum"]).IsEqualTo("7");
            await Assert.That(packets[0].Annotations["source_file"]).IsEqualTo("0001.hex");
            await Assert.That(packets[1].Data.ToArray().SequenceEqual(new byte[] { 0x01, 0x02, 0x03 })).IsTrue();
            await Assert.That(packets[1].Annotations["direction"]).IsEqualTo("response");
            await Assert.That(packets[1].Annotations["hresult"]).IsEqualTo("0x80004005");
            await Assert.That(packets[2].Data.ToArray().SequenceEqual(new byte[] { 0xCC })).IsTrue();
            await Assert.That(packets[2].Annotations["opnum"]).IsEqualTo("8");
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task ReadAllAsync_MaxPackets_LimitsEnumerationInSortedFileOrder()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            Guid iid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
            await File.WriteAllTextAsync(
                Path.Combine(directory, "b.hex"),
                BuildHexFile(iid, 2, "2026-06-07T10:11:14Z", null, "0000: 02 \n", string.Empty),
                TestContext.Current!.CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(directory, "a.hex"),
                BuildHexFile(iid, 1, "2026-06-07T10:11:13Z", null, "0000: 01 \n", "0000: ff \n"),
                TestContext.Current.CancellationToken);
            var source = new OpcWireCaptureSource();
            await source.StartAsync(new CaptureStartRequest(ReplaySourceDirectory: directory), TestContext.Current.CancellationToken);

            var packets = await source.ReadAllAsync(2, TestContext.Current.CancellationToken).ToListAsync();

            await Assert.That(packets.Count).IsEqualTo(2);
            await Assert.That(packets[0].Annotations["source_file"]).IsEqualTo("a.hex");
            await Assert.That(packets[0].Annotations["direction"]).IsEqualTo("request");
            await Assert.That(packets[1].Annotations["source_file"]).IsEqualTo("a.hex");
            await Assert.That(packets[1].Annotations["direction"]).IsEqualTo("response");
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    private static string BuildHexFile(
        Guid iid,
        int opnum,
        string timestampUtc,
        string? hresult,
        string requestRows,
        string responseRows)
        => $"""
            # Opc.Classic wire capture
            # context: test
            # iid: {iid:D}
            # opnum: {opnum}
            {(hresult is null ? string.Empty : "# hresult: " + hresult)}
            # timestamp_utc: {timestampUtc}

            ## request ({requestRows.Length} bytes)
            {requestRows}
            ## response ({responseRows.Length} bytes)
            {responseRows}
            """;
}

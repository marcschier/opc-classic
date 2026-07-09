// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class PcapCaptureSourceTests
{
    [Test]
    public async Task Constants_ExposeExpectedSourceNameAndDefaultFilter()
    {
        string sourceName = GetPcapSourceName();
        string defaultFilter = GetDefaultOpcBpfFilter();

        await Assert.That(sourceName).IsEqualTo("pcap");
        await Assert.That(defaultFilter).IsEqualTo("tcp and (port 135 or (portrange 49152-65535))");
    }

    [Test]
    public async Task BuildServerPortBpfFilter_NullOrEmpty_ReturnsDefaultFilter()
    {
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter(null)).IsEqualTo(PcapCaptureSource.DefaultOpcBpfFilter);
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter(Array.Empty<int>())).IsEqualTo(PcapCaptureSource.DefaultOpcBpfFilter);
    }

    [Test]
    public async Task BuildServerPortBpfFilter_AllInvalidPorts_ReturnsDefaultFilter()
    {
        // Negative, zero, and out-of-range ports are all silently skipped per the
        // documented BPF semantics; if every entry is invalid, behave as if the
        // caller passed an empty list (fall back to the default port-range filter).
        int[] invalid = [-1, 0, 65536, 100000];
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter(invalid)).IsEqualTo(PcapCaptureSource.DefaultOpcBpfFilter);
    }

    [Test]
    public async Task BuildServerPortBpfFilter_SinglePort_NarrowsToPort135PlusGivenPort()
    {
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter([51301]))
            .IsEqualTo("tcp and (port 135 or port 51301)");
    }

    [Test]
    public async Task BuildServerPortBpfFilter_MultiplePorts_AreSortedAndDeduplicated()
    {
        // Duplicate 51301 + reverse order to assert dedupe + sort.
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter([51301, 49500, 51301, 8080]))
            .IsEqualTo("tcp and (port 135 or port 8080 or port 49500 or port 51301)");
    }

    [Test]
    public async Task BuildServerPortBpfFilter_IncludesPort135ExactlyOnce()
    {
        // Caller already included 135 explicitly; should not appear twice in the output.
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter([135, 51301]))
            .IsEqualTo("tcp and (port 135 or port 51301)");
        // 135 always present even if not in the input list.
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter([51301])).Contains("port 135");
    }

    [Test]
    public async Task BuildServerPortBpfFilter_MixedValidAndInvalidPorts_DropsTheInvalidOnes()
    {
        await Assert.That(PcapCaptureSource.BuildServerPortBpfFilter([0, 51301, -5, 65535, 70000]))
            .IsEqualTo("tcp and (port 135 or port 51301 or port 65535)");
    }

    [Test]
    public async Task Constructor_NullOrEmptySessionFolder_Throws()
    {
        await Assert.That(() => new PcapCaptureSource(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => new PcapCaptureSource(string.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task NewInstance_BeforeStart_HasZeroCountsAndNoRawPcapPath()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new PcapCaptureSource(directory);

            await Assert.That(source.PacketCount).IsEqualTo(0);
            await Assert.That(source.ByteCount).IsEqualTo(0);
            await Assert.That(source.LinkType).IsEqualTo(0);
            await Assert.That(source.GetRawPcapFilePath()).IsNull();
            await source.StopAsync(TestContext.Current!.CancellationToken);
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task StartAsync_NullRequestOrMissingInterface_ThrowsBeforeOpeningHardware()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new PcapCaptureSource(directory);
            CancellationToken cancellationToken = TestContext.Current!.CancellationToken;

            await Assert.That(async () => await source.StartAsync(null!, cancellationToken))
                .Throws<ArgumentNullException>();
            await Assert.That(async () => await source.StartAsync(new CaptureStartRequest(), cancellationToken))
                .Throws<CaptureException>();
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task StartStopAndReadAsync_CanceledOrMissingFile_DoNotBindLiveDevice()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new PcapCaptureSource(directory);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await Assert.That(async () => await source.StartAsync(new CaptureStartRequest(InterfaceName: "never-open"), cts.Token))
                .Throws<OperationCanceledException>();
            var packets = await source.ReadAllAsync(null, TestContext.Current!.CancellationToken).ToListAsync();

            await Assert.That(packets.Count).IsEqualTo(0);
            await Assert.That(File.Exists(Path.Combine(directory, "capture.pcap"))).IsFalse();
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task ReadAllAsync_ExistingPcapFile_ReplaysPacketsAndReportsRawPcapPath()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            byte[] frame =
            [
                0x00, 0x11, 0x22, 0x33, 0x44, 0x55,
                0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB,
                0x08, 0x00,
            ];
            string pcapPath = Path.Combine(directory, "capture.pcap");
            using (var writer = new CaptureFileWriterDevice(pcapPath, FileMode.Create))
            {
                writer.Open(new DeviceConfiguration { LinkLayerType = LinkLayers.Ethernet });
                writer.Write(new RawCapture(
                    LinkLayers.Ethernet,
                    new PosixTimeval(new DateTime(2026, 6, 7, 10, 11, 12, DateTimeKind.Utc)),
                    frame,
                    frame.Length));
                writer.Close();
            }

            var source = new PcapCaptureSource(directory);

            var packets = await source.ReadAllAsync(null, TestContext.Current!.CancellationToken).ToListAsync();

            await Assert.That(source.GetRawPcapFilePath()).IsEqualTo(pcapPath);
            await Assert.That(packets.Count).IsEqualTo(1);
            await Assert.That(packets[0].Timestamp).IsEqualTo(new DateTimeOffset(2026, 6, 7, 10, 11, 12, TimeSpan.Zero));
            await Assert.That(packets[0].OriginalLength).IsEqualTo(frame.Length);
            await Assert.That(packets[0].LinkType).IsEqualTo((int)LinkLayers.Ethernet);
            await Assert.That(packets[0].Data.ToArray().SequenceEqual(frame)).IsTrue();
            await Assert.That(packets[0].Annotations.Count).IsEqualTo(0);
            await source.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    private static string GetPcapSourceName() => PcapCaptureSource.SourceName;
    private static string GetDefaultOpcBpfFilter() => PcapCaptureSource.DefaultOpcBpfFilter;
}

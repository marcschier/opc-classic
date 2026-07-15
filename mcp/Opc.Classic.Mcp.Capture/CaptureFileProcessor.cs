// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Bounded reader/decoder for external pcap and pcapng files.
/// </summary>
public static class CaptureFileProcessor
{
    public const long DefaultMaxFileBytes = 50L * 1024 * 1024;
    public const long HardMaxFileBytes = 200L * 1024 * 1024;
    public const int DefaultMaxPackets = 100_000;
    public const int HardMaxPackets = 1_000_000;
    public const int DefaultMaxPdus = 5_000;
    public const int HardMaxPdus = 50_000;

    /// <summary>
    /// Decodes a bounded external capture file.
    /// </summary>
    public static async Task<CaptureFileDecodeResult> DecodeAsync(
        string path,
        long maxFileBytes = DefaultMaxFileBytes,
        int maxPackets = DefaultMaxPackets,
        int maxPdus = DefaultMaxPdus,
        NtlmPassiveUnwrapper? unwrapper = null,
        CancellationToken cancellationToken = default)
    {
        Analysis analysis = await AnalyzeAsync(
            path,
            maxFileBytes,
            maxPackets,
            maxPdus,
            unwrapper,
            cancellationToken).ConfigureAwait(false);
        return new CaptureFileDecodeResult
        {
            Status = analysis.Status,
            Pdus = analysis.Frames.Where(frame => frame.Pdu is not null).Select(frame => frame.Pdu!).ToArray(),
        };
    }

    /// <summary>
    /// Replays and validates a bounded external capture file.
    /// </summary>
    public static async Task<CaptureFileReplayResult> ReplayAsync(
        string path,
        long maxFileBytes = DefaultMaxFileBytes,
        int maxPackets = DefaultMaxPackets,
        int maxPdus = DefaultMaxPdus,
        NtlmPassiveUnwrapper? unwrapper = null,
        CancellationToken cancellationToken = default)
    {
        Analysis analysis = await AnalyzeAsync(
            path,
            maxFileBytes,
            maxPackets,
            maxPdus,
            unwrapper,
            cancellationToken).ConfigureAwait(false);
        return new CaptureFileReplayResult
        {
            Status = analysis.Status,
            Report = new OrpcReplayTool().ReplayDetailed(analysis.Frames, cancellationToken),
        };
    }

    private static async Task<Analysis> AnalyzeAsync(
        string path,
        long maxFileBytes,
        int maxPackets,
        int maxPdus,
        NtlmPassiveUnwrapper? unwrapper,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = Path.GetFullPath(path);
        long effectiveFileLimit = ValidateLimit(maxFileBytes, DefaultMaxFileBytes, HardMaxFileBytes, nameof(maxFileBytes));
        int effectivePacketLimit = (int)ValidateLimit(maxPackets, DefaultMaxPackets, HardMaxPackets, nameof(maxPackets));
        int effectivePduLimit = (int)ValidateLimit(maxPdus, DefaultMaxPdus, HardMaxPdus, nameof(maxPdus));
        byte[] bytes;
        try
        {
            var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            try
            {
                long openedLength = stream.Length;
                bytes = await ReadBoundedAsync(
                    stream,
                    openedLength,
                    effectiveFileLimit,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }
        catch (FileNotFoundException)
        {
            throw new CaptureException($"Capture file '{fullPath}' was not found.");
        }
        catch (DirectoryNotFoundException)
        {
            throw new CaptureException($"Capture file '{fullPath}' was not found.");
        }
        catch (IOException ex)
        {
            throw new CaptureException($"Unable to read capture file '{fullPath}' safely: {ex.Message}", ex);
        }

        var reader = new CaptureContainerReader(bytes);
        var decoder = new OpcDcomDecoder(unwrapper);
        var frames = new List<DecodedDcomFrame>(Math.Min(effectivePduLimit, 4096));
        var firstPduByConnection = new HashSet<string>(StringComparer.Ordinal);
        int packetsRead = 0;
        int ethernetPackets = 0;
        int ipv4TcpPackets = 0;
        int ipv6TcpPackets = 0;
        int fragmentedIpPackets = 0;
        int truncatedPackets = 0;
        int unsupportedLinkPackets = 0;
        int incompleteDceRpcStreams = 0;
        int packetFailureCount = 0;
        var packetFailures = new List<CapturePacketFailure>(capacity: 32);
        bool midSessionLikely = false;
        bool packetLimitReached = false;
        bool pduLimitReached = false;

        foreach (CapturedPacket packet in reader.ReadPackets())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (packetsRead >= effectivePacketLimit)
            {
                packetLimitReached = true;
                break;
            }
            packetsRead++;
            if (packet.OriginalLength > packet.Data.Length)
            {
                truncatedPackets++;
                AddPacketFailure(
                    packetFailures,
                    ref packetFailureCount,
                    packetsRead - 1,
                    "snaplen_truncated",
                    $"Captured {packet.Data.Length} of {packet.OriginalLength} on-wire bytes; packet skipped.");
                continue;
            }
            if (packet.LinkType != 1)
            {
                unsupportedLinkPackets++;
                continue;
            }
            ethernetPackets++;

            PacketClassification classification = ClassifyEthernet(packet.Data.Span);
            if (classification.Fragmented)
            {
                fragmentedIpPackets++;
                continue;
            }
            if (!classification.IsTcp)
            {
                continue;
            }
            if (classification.IpVersion == 4)
            {
                ipv4TcpPackets++;
            }
            else if (classification.IpVersion == 6)
            {
                ipv6TcpPackets++;
            }

            IEnumerable<DecodedDcomFrame> decodedFrames;
            try
            {
                decodedFrames = decoder.DecodeDetailed(packet).ToArray();
            }
            catch (Exception ex) when (ex is InvalidDataException
                or FormatException
                or ArgumentException
                or IndexOutOfRangeException)
            {
                AddPacketFailure(
                    packetFailures,
                    ref packetFailureCount,
                    packetsRead - 1,
                    "packet_decode_failed",
                    ex.Message);
                continue;
            }
            foreach (DecodedDcomFrame frame in decodedFrames)
            {
                if (frames.Count >= effectivePduLimit)
                {
                    pduLimitReached = true;
                    break;
                }
                frames.Add(frame);
                if (frame.Failure is DcomDecodeFailure decodeFailure)
                {
                    AddPacketFailure(
                        packetFailures,
                        ref packetFailureCount,
                        packetsRead - 1,
                        decodeFailure.Code,
                        decodeFailure.Message);
                }
                if (frame.Pdu is DecodedOpcPdu pdu)
                {
                    string connection = CanonicalConnection(pdu.SourceEndpoint, pdu.DestinationEndpoint);
                    if (firstPduByConnection.Add(connection)
                        && pdu.PduType is not ("bind" or "alter_context"))
                    {
                        midSessionLikely = true;
                    }
                }
            }
            if (pduLimitReached)
            {
                break;
            }
        }

        if (!pduLimitReached)
        {
            foreach (DecodedDcomFrame completed in decoder.CompleteDetailed())
            {
                if (completed.Failure?.Code is "truncated_header" or "truncated_fragment")
                {
                    incompleteDceRpcStreams++;
                }
                if (frames.Count < effectivePduLimit)
                {
                    frames.Add(completed);
                    if (completed.Failure is DcomDecodeFailure failure)
                    {
                        AddPacketFailure(
                            packetFailures,
                            ref packetFailureCount,
                            packetIndex: -1,
                            failure.Code,
                            failure.Message);
                    }
                    if (completed.Pdu is DecodedOpcPdu pdu)
                    {
                        string connection = CanonicalConnection(pdu.SourceEndpoint, pdu.DestinationEndpoint);
                        if (firstPduByConnection.Add(connection)
                            && pdu.PduType is not ("bind" or "alter_context"))
                        {
                            midSessionLikely = true;
                        }
                    }
                }
                else
                {
                    pduLimitReached = true;
                }
            }
        }

        var warnings = new List<string>();
        if (fragmentedIpPackets > 0)
        {
            warnings.Add("IP fragments are reported but not reassembled; fragmented packets were skipped.");
        }
        if (midSessionLikely)
        {
            warnings.Add("At least one DCE/RPC connection was first observed after its Bind; decoding and NTLM counters may be incomplete.");
        }
        if (incompleteDceRpcStreams > 0)
        {
            warnings.Add("One or more TCP streams ended with an incomplete DCE/RPC frame.");
        }
        if (truncatedPackets > 0)
        {
            warnings.Add("Snap-length-truncated packets were reported and skipped; decoding continued with later packets.");
        }
        if (unsupportedLinkPackets > 0)
        {
            warnings.Add("Non-Ethernet packets were skipped.");
        }

        return new Analysis(
            frames,
            new CaptureFileStatus
            {
                Path = fullPath,
                Format = reader.Format,
                FileSizeBytes = bytes.LongLength,
                PacketsRead = packetsRead,
                EthernetPackets = ethernetPackets,
                Ipv4TcpPackets = ipv4TcpPackets,
                Ipv6TcpPackets = ipv6TcpPackets,
                FragmentedIpPackets = fragmentedIpPackets,
                TruncatedPackets = truncatedPackets,
                UnsupportedLinkPackets = unsupportedLinkPackets,
                IncompleteDceRpcStreams = incompleteDceRpcStreams,
                PacketFailureCount = packetFailureCount,
                PacketFailures = packetFailures,
                MidSessionLikely = midSessionLikely,
                PacketLimitReached = packetLimitReached,
                PduLimitReached = pduLimitReached,
                Warnings = warnings,
            });
    }

    internal static async Task<byte[]> ReadBoundedAsync(
        Stream stream,
        long openedLength,
        long maxFileBytes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
        {
            throw new CaptureException("Capture stream is not readable.");
        }
        if (openedLength < 0)
        {
            throw new CaptureException("Capture stream reported a negative length.");
        }
        if (openedLength > maxFileBytes)
        {
            throw new CaptureException(
                $"Capture file is {openedLength} bytes, exceeding maxFileBytes={maxFileBytes}.");
        }

        int capacity = checked((int)Math.Min(openedLength, maxFileBytes));
        using var output = new MemoryStream(capacity);
        byte[] buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            int remaining = checked((int)Math.Min(buffer.Length, maxFileBytes + 1 - total));
            if (remaining <= 0)
            {
                throw new CaptureException($"Capture file exceeded maxFileBytes={maxFileBytes} while being read.");
            }
            int read = await stream.ReadAsync(buffer.AsMemory(0, remaining), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }
            output.Write(buffer, 0, read);
            total += read;
            if (total > maxFileBytes)
            {
                throw new CaptureException($"Capture file exceeded maxFileBytes={maxFileBytes} while being read.");
            }
        }

        long finalLength = stream.CanSeek ? stream.Length : total;
        if (finalLength != openedLength || total != openedLength)
        {
            throw new CaptureException(
                $"Capture file changed while being read (openedLength={openedLength}, finalLength={finalLength}, bytesRead={total}).");
        }
        return output.ToArray();
    }

    private static void AddPacketFailure(
        List<CapturePacketFailure> failures,
        ref int totalCount,
        int packetIndex,
        string code,
        string message)
    {
        totalCount++;
        if (failures.Count >= 32)
        {
            return;
        }
        const int maxMessageLength = 512;
        string boundedMessage = message.Length <= maxMessageLength
            ? message
            : message[..maxMessageLength];
        failures.Add(new CapturePacketFailure(packetIndex, code, boundedMessage));
    }

    private static long ValidateLimit(long value, long defaultValue, long hardLimit, string name)
    {
        long effective = value <= 0 ? defaultValue : value;
        if (effective > hardLimit)
        {
            throw new CaptureException($"{name} cannot exceed the hard limit of {hardLimit}.");
        }
        return effective;
    }

    private static string CanonicalConnection(string? first, string? second)
    {
        string a = first ?? string.Empty;
        string b = second ?? string.Empty;
        return string.CompareOrdinal(a, b) <= 0 ? a + "|" + b : b + "|" + a;
    }

    private static PacketClassification ClassifyEthernet(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 14)
        {
            return default;
        }
        int offset = 14;
        ushort etherType = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(12, 2));
        while (etherType is 0x8100 or 0x88A8)
        {
            if (frame.Length < offset + 4)
            {
                return default;
            }
            etherType = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(offset + 2, 2));
            offset += 4;
        }

        if (etherType == 0x0800)
        {
            if (frame.Length < offset + 20)
            {
                return new PacketClassification(4, false, false);
            }
            int headerLength = (frame[offset] & 0x0F) * 4;
            if (headerLength < 20 || frame.Length < offset + headerLength)
            {
                return new PacketClassification(4, false, false);
            }
            ushort fragment = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(offset + 6, 2));
            bool fragmented = (fragment & 0x3FFF) != 0;
            return new PacketClassification(4, frame[offset + 9] == 6, fragmented);
        }

        if (etherType != 0x86DD || frame.Length < offset + 40)
        {
            return default;
        }

        byte nextHeader = frame[offset + 6];
        int cursor = offset + 40;
        for (int extensions = 0; extensions < 8; extensions++)
        {
            if (nextHeader == 6)
            {
                return new PacketClassification(6, true, false);
            }
            if (nextHeader == 44)
            {
                return new PacketClassification(6, false, true);
            }
            if (nextHeader is 0 or 43 or 60)
            {
                if (frame.Length < cursor + 2)
                {
                    return new PacketClassification(6, false, false);
                }
                byte following = frame[cursor];
                int length = (frame[cursor + 1] + 1) * 8;
                if (length < 8 || frame.Length < cursor + length)
                {
                    return new PacketClassification(6, false, false);
                }
                nextHeader = following;
                cursor += length;
                continue;
            }
            if (nextHeader == 51)
            {
                if (frame.Length < cursor + 2)
                {
                    return new PacketClassification(6, false, false);
                }
                byte following = frame[cursor];
                int length = (frame[cursor + 1] + 2) * 4;
                if (length < 8 || frame.Length < cursor + length)
                {
                    return new PacketClassification(6, false, false);
                }
                nextHeader = following;
                cursor += length;
                continue;
            }
            return new PacketClassification(6, false, false);
        }
        return new PacketClassification(6, false, false);
    }

    private sealed record Analysis(IReadOnlyList<DecodedDcomFrame> Frames, CaptureFileStatus Status);
    private readonly record struct PacketClassification(int IpVersion, bool IsTcp, bool Fragmented);

    private sealed class CaptureContainerReader
    {
        private readonly byte[] _bytes;

        public CaptureContainerReader(byte[] bytes)
        {
            _bytes = bytes;
            if (bytes.Length < 4)
            {
                throw new CaptureException("Capture file is too short to contain a pcap or pcapng header.");
            }
            Format = bytes.AsSpan(0, 4).SequenceEqual((ReadOnlySpan<byte>)[0x0A, 0x0D, 0x0D, 0x0A])
                ? "pcapng"
                : "pcap";
        }

        public string Format { get; }

        public IEnumerable<CapturedPacket> ReadPackets() =>
            Format == "pcapng" ? ReadPcapNg() : ReadPcap();

        private IEnumerable<CapturedPacket> ReadPcap()
        {
            if (_bytes.Length < 24)
            {
                throw new CaptureException("Pcap global header is truncated.");
            }
            ReadOnlySpan<byte> magic = _bytes.AsSpan(0, 4);
            bool littleEndian;
            bool nanoseconds;
            if (magic.SequenceEqual((ReadOnlySpan<byte>)[0xD4, 0xC3, 0xB2, 0xA1]))
            {
                littleEndian = true;
                nanoseconds = false;
            }
            else if (magic.SequenceEqual((ReadOnlySpan<byte>)[0xA1, 0xB2, 0xC3, 0xD4]))
            {
                littleEndian = false;
                nanoseconds = false;
            }
            else if (magic.SequenceEqual((ReadOnlySpan<byte>)[0x4D, 0x3C, 0xB2, 0xA1]))
            {
                littleEndian = true;
                nanoseconds = true;
            }
            else if (magic.SequenceEqual((ReadOnlySpan<byte>)[0xA1, 0xB2, 0x3C, 0x4D]))
            {
                littleEndian = false;
                nanoseconds = true;
            }
            else
            {
                throw new CaptureException("Unsupported capture format or pcap magic number.");
            }

            int linkType = checked((int)ReadUInt32(_bytes.AsSpan(20, 4), littleEndian));
            int offset = 24;
            while (offset < _bytes.Length)
            {
                if (_bytes.Length - offset < 16)
                {
                    throw new CaptureException("Pcap packet header is truncated.");
                }
                ReadOnlySpan<byte> header = _bytes.AsSpan(offset, 16);
                uint seconds = ReadUInt32(header[..4], littleEndian);
                uint fraction = ReadUInt32(header.Slice(4, 4), littleEndian);
                uint capturedLength = ReadUInt32(header.Slice(8, 4), littleEndian);
                uint originalLength = ReadUInt32(header.Slice(12, 4), littleEndian);
                offset += 16;
                if (capturedLength > int.MaxValue || capturedLength > _bytes.Length - offset)
                {
                    throw new CaptureException("Pcap packet data length exceeds the remaining file.");
                }
                byte[] data = _bytes.AsSpan(offset, (int)capturedLength).ToArray();
                offset += (int)capturedLength;
                long ticks = nanoseconds ? fraction / 100 : fraction * 10L;
                DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(ticks);
                yield return new CapturedPacket(
                    timestamp,
                    checked((int)Math.Min(originalLength, int.MaxValue)),
                    data,
                    linkType,
                    EmptyAnnotations);
            }
        }

        private IEnumerable<CapturedPacket> ReadPcapNg()
        {
            int offset = 0;
            bool littleEndian = true;
            var interfaces = new List<InterfaceInfo>();
            while (offset < _bytes.Length)
            {
                if (_bytes.Length - offset < 12)
                {
                    throw new CaptureException("Pcapng block header is truncated.");
                }
                uint rawType = BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(offset, 4));
                if (rawType == 0x0A0D0D0A)
                {
                    ReadOnlySpan<byte> bom = _bytes.AsSpan(offset + 8, 4);
                    littleEndian = bom.SequenceEqual((ReadOnlySpan<byte>)[0x4D, 0x3C, 0x2B, 0x1A]);
                    if (!littleEndian && !bom.SequenceEqual((ReadOnlySpan<byte>)[0x1A, 0x2B, 0x3C, 0x4D]))
                    {
                        throw new CaptureException("Pcapng section has an invalid byte-order magic.");
                    }
                    interfaces.Clear();
                }

                uint blockType = ReadUInt32(_bytes.AsSpan(offset, 4), littleEndian);
                uint totalLength = ReadUInt32(_bytes.AsSpan(offset + 4, 4), littleEndian);
                if (totalLength < 12 || (totalLength & 3) != 0 || totalLength > _bytes.Length - offset)
                {
                    throw new CaptureException("Pcapng block length is invalid or exceeds the remaining file.");
                }
                int blockLength = checked((int)totalLength);
                uint trailingLength = ReadUInt32(_bytes.AsSpan(offset + blockLength - 4, 4), littleEndian);
                if (trailingLength != totalLength)
                {
                    throw new CaptureException("Pcapng block length trailer does not match its header.");
                }

                ReadOnlySpan<byte> body = _bytes.AsSpan(offset + 8, blockLength - 12);
                if (blockType == 1)
                {
                    if (body.Length < 8)
                    {
                        throw new CaptureException("Pcapng interface description block is truncated.");
                    }
                    int linkType = ReadUInt16(body[..2], littleEndian);
                    byte timestampResolution = ReadTimestampResolution(body[8..], littleEndian);
                    interfaces.Add(new InterfaceInfo(linkType, timestampResolution));
                }
                else if (blockType == 6)
                {
                    if (body.Length < 20)
                    {
                        throw new CaptureException("Pcapng enhanced packet block is truncated.");
                    }
                    uint interfaceId = ReadUInt32(body[..4], littleEndian);
                    if (interfaceId >= interfaces.Count)
                    {
                        throw new CaptureException("Pcapng packet references an unknown interface.");
                    }
                    ulong timestampRaw = ((ulong)ReadUInt32(body.Slice(4, 4), littleEndian) << 32)
                        | ReadUInt32(body.Slice(8, 4), littleEndian);
                    uint capturedLength = ReadUInt32(body.Slice(12, 4), littleEndian);
                    uint originalLength = ReadUInt32(body.Slice(16, 4), littleEndian);
                    if (capturedLength > int.MaxValue || capturedLength > body.Length - 20)
                    {
                        throw new CaptureException("Pcapng packet data length exceeds its block.");
                    }
                    InterfaceInfo info = interfaces[(int)interfaceId];
                    yield return new CapturedPacket(
                        ToTimestamp(timestampRaw, info.TimestampResolution),
                        checked((int)Math.Min(originalLength, int.MaxValue)),
                        body.Slice(20, (int)capturedLength).ToArray(),
                        info.LinkType,
                        EmptyAnnotations);
                }
                else if (blockType == 3)
                {
                    if (interfaces.Count == 0 || body.Length < 4)
                    {
                        throw new CaptureException("Pcapng simple packet block is missing an interface or packet length.");
                    }
                    uint originalLength = ReadUInt32(body[..4], littleEndian);
                    int capturedLength = Math.Min(
                        checked((int)Math.Min(originalLength, int.MaxValue)),
                        body.Length - 4);
                    yield return new CapturedPacket(
                        DateTimeOffset.UnixEpoch,
                        checked((int)Math.Min(originalLength, int.MaxValue)),
                        body.Slice(4, capturedLength).ToArray(),
                        interfaces[0].LinkType,
                        EmptyAnnotations);
                }
                offset += blockLength;
            }
        }

        private static byte ReadTimestampResolution(ReadOnlySpan<byte> options, bool littleEndian)
        {
            int offset = 0;
            while (offset + 4 <= options.Length)
            {
                ushort code = ReadUInt16(options.Slice(offset, 2), littleEndian);
                ushort length = ReadUInt16(options.Slice(offset + 2, 2), littleEndian);
                offset += 4;
                if (code == 0)
                {
                    break;
                }
                if (length > options.Length - offset)
                {
                    break;
                }
                if (code == 9 && length >= 1)
                {
                    return options[offset];
                }
                offset += (length + 3) & ~3;
            }
            return 6;
        }

        private static DateTimeOffset ToTimestamp(ulong raw, byte resolution)
        {
            double divisor = (resolution & 0x80) == 0
                ? Math.Pow(10, resolution)
                : Math.Pow(2, resolution & 0x7F);
            double seconds = raw / divisor;
            long wholeSeconds = (long)seconds;
            long ticks = (long)((seconds - wholeSeconds) * TimeSpan.TicksPerSecond);
            return DateTimeOffset.FromUnixTimeSeconds(wholeSeconds).AddTicks(ticks);
        }

        private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, bool littleEndian) =>
            littleEndian
                ? BinaryPrimitives.ReadUInt16LittleEndian(bytes)
                : BinaryPrimitives.ReadUInt16BigEndian(bytes);

        private static uint ReadUInt32(ReadOnlySpan<byte> bytes, bool littleEndian) =>
            littleEndian
                ? BinaryPrimitives.ReadUInt32LittleEndian(bytes)
                : BinaryPrimitives.ReadUInt32BigEndian(bytes);

        private sealed record InterfaceInfo(int LinkType, byte TimestampResolution);
    }

    private static readonly IReadOnlyDictionary<string, string?> EmptyAnnotations =
        new Dictionary<string, string?>(0);
}

/// <summary>
/// Decode status for an external capture file.
/// </summary>
public sealed record class CaptureFileStatus
{
    public required string Path { get; init; }
    public required string Format { get; init; }
    public long FileSizeBytes { get; init; }
    public int PacketsRead { get; init; }
    public int EthernetPackets { get; init; }
    public int Ipv4TcpPackets { get; init; }
    public int Ipv6TcpPackets { get; init; }
    public int FragmentedIpPackets { get; init; }
    public int TruncatedPackets { get; init; }
    public int UnsupportedLinkPackets { get; init; }
    public int IncompleteDceRpcStreams { get; init; }
    public int PacketFailureCount { get; init; }
    public IReadOnlyList<CapturePacketFailure> PacketFailures { get; init; } = [];
    public bool MidSessionLikely { get; init; }
    public bool PacketLimitReached { get; init; }
    public bool PduLimitReached { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record CapturePacketFailure(int PacketIndex, string Code, string Message);

public sealed record class CaptureFileDecodeResult
{
    public required CaptureFileStatus Status { get; init; }
    public required IReadOnlyList<DecodedOpcPdu> Pdus { get; init; }
}

public sealed record class CaptureFileReplayResult
{
    public required CaptureFileStatus Status { get; init; }
    public required ReplayReport Report { get; init; }
}

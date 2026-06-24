// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class EndpointMapperDispatcherTests
{
    private static readonly IPEndPoint ListenerEndpoint = new(IPAddress.Parse("192.0.2.10"), 49157);

    [Test]
    [Arguments("99fcfec4-5260-101b-bbcb-00aa0021347a")]
    [Arguments("000001A0-0000-0000-C000-000000000046")]
    public async Task EptMap_returns_tcp_tower_for_mapped_dcom_interface(string interfaceIdText)
    {
        Guid interfaceId = Guid.Parse(interfaceIdText);
        var dispatcher = new EndpointMapperDispatcher(() => ListenerEndpoint);

        DispatchResult result = await dispatcher.DispatchAsync(3, EncodeMapRequest(interfaceId), TestContext.Current!.CancellationToken);

        await Assert.That(result.IsFailure).IsFalse();
        EndpointMapperMapResponse response = DecodeMapResponse(result.Payload.Span);
        await Assert.That(response.Status).IsEqualTo(EndpointMapperDispatcher.EptSuccess);
        await Assert.That(response.Towers.Count).IsEqualTo(1);
        await Assert.That(EndpointMapperTower.TryDecodeTcpTower(response.Towers[0], out EndpointMapperTowerBinding binding)).IsTrue();
        await Assert.That(binding.InterfaceId).IsEqualTo(interfaceId);
        await Assert.That(binding.Address).IsEqualTo(ListenerEndpoint.Address);
        await Assert.That(binding.Port).IsEqualTo(ListenerEndpoint.Port);
    }

    [Test]
    public async Task EptMap_returns_not_registered_for_unknown_interface()
    {
        var dispatcher = new EndpointMapperDispatcher(() => ListenerEndpoint);

        DispatchResult result = await dispatcher.DispatchAsync(3, EncodeMapRequest(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")), TestContext.Current!.CancellationToken);

        await Assert.That(result.IsFailure).IsFalse();
        EndpointMapperMapResponse response = DecodeMapResponse(result.Payload.Span);
        await Assert.That(response.Status).IsEqualTo(EndpointMapperDispatcher.EptNotRegistered);
        await Assert.That(response.Towers.Count).IsEqualTo(0);
    }

    private static byte[] EncodeMapRequest(Guid interfaceId)
    {
        byte[] mapTower = EndpointMapperTower.EncodeTcpTower(interfaceId, 0, 0, new IPEndPoint(IPAddress.Any, 0));
        var buffer = new byte[4 + 4 + 20 + 4 + 8 + mapTower.Length + PaddingTo4(mapTower.Length)];
        var writer = new NdrWriter(buffer);
        writer.WriteNullReferent();
        _ = writer.WriteReferentId();
        writer.WriteRawBytes(new byte[20]);
        writer.WriteUInt32(1);
        EndpointMapperTower.WriteTowerPointee(ref writer, mapTower);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static EndpointMapperMapResponse DecodeMapResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        _ = reader.ReadRawBytes(20);
        uint numTowers = reader.ReadUInt32();
        var towers = new List<byte[]>(checked((int)numTowers));
        if (reader.TryReadReferentId(out _))
        {
            uint maxCount = reader.ReadUInt32();
            uint offset = reader.ReadUInt32();
            uint actualCount = reader.ReadUInt32();
            if (offset != 0 || actualCount != numTowers || actualCount > maxCount)
            {
                throw new InvalidOperationException("Malformed ept_map tower-array response.");
            }

            for (int i = 0; i < actualCount; i++)
            {
                if (!reader.TryReadReferentId(out _))
                {
                    throw new InvalidOperationException("Endpoint mapper returned a null tower pointer.");
                }
            }

            for (int i = 0; i < actualCount; i++)
            {
                if (!EndpointMapperTower.TryReadTower(payload, reader.Position, out byte[] tower, out int bytesRead))
                {
                    throw new InvalidOperationException("Endpoint mapper returned a malformed tower.");
                }

                towers.Add(tower);
                _ = reader.ReadRawBytes(bytesRead);
                reader.AlignTo(4);
            }
        }

        uint status = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(reader.Position, 4));
        return new EndpointMapperMapResponse(towers, status);
    }

    private static int PaddingTo4(int length)
    {
        int misaligned = length & 3;
        return misaligned == 0 ? 0 : 4 - misaligned;
    }

    private sealed record EndpointMapperMapResponse(IReadOnlyList<byte[]> Towers, uint Status);
}

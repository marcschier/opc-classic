//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Mcp.Capture;

namespace Opc.Classic.Mcp.Capture.Tests;

internal sealed class FakeCaptureSource : ICaptureSource {
    public List<CapturedPacket> Packets { get; } = [];
    public CaptureException? StartException { get; set; }
    public CaptureException? StopException { get; set; }
    public CaptureStartRequest? LastStartRequest { get; private set; }
    public int StartCallCount { get; private set; }
    public int StopCallCount { get; private set; }
    public int DisposeCallCount { get; private set; }
    public int LinkType { get; set; }
    public string? RawPcapFilePath { get; set; }

    public long PacketCount => Packets.Count;

    public long ByteCount {
        get {
            long total = 0;
            foreach (CapturedPacket packet in Packets) {
                total += packet.OriginalLength;
            }

            return total;
        }
    }

    public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        StartCallCount++;
        LastStartRequest = request;
        if (StartException is not null) {
            throw StartException;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        StopCallCount++;
        if (StopException is not null) {
            throw StopException;
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
        long? maxPackets,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        long limit = maxPackets ?? long.MaxValue;
        long emitted = 0;
        foreach (CapturedPacket packet in Packets) {
            cancellationToken.ThrowIfCancellationRequested();
            if (emitted >= limit) {
                yield break;
            }

            yield return packet;
            emitted++;
            await Task.Yield();
        }
    }

    public string? GetRawPcapFilePath() => RawPcapFilePath;

    public ValueTask DisposeAsync() {
        DisposeCallCount++;
        return ValueTask.CompletedTask;
    }
}

internal static class TestDirectories {
    public static string CreateUniqueTempDirectory() {
        string path = Path.Combine(
            Path.GetTempPath(),
            "OpcClassicMcpCaptureTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public static void DeleteIfExists(string path) {
        if (Directory.Exists(path)) {
            Directory.Delete(path, recursive: true);
        }
    }
}

internal static class AsyncEnumerableTestExtensions {
    public static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default) {
        var values = new List<T>();
        await foreach (T value in source.WithCancellation(cancellationToken)) {
            values.Add(value);
        }

        return values;
    }
}

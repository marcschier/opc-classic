//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Hda;
using TUnit.Core;

namespace Opc.Classic.Hda.Tests;

public sealed class HdaBrowseElementTests
{
    [Test]
    public async Task Default_HasEmptyNameAndItemId()
    {
        var e = new HdaBrowseElement();
        await Assert.That(e.Name).IsEqualTo(string.Empty);
        await Assert.That(e.ItemId).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Initializer_AssignsAllFields()
    {
        var e = new HdaBrowseElement
        {
            Name = "Tank1",
            ItemId = "Plant1.Tank1",
            BrowseType = HdaBrowseType.Leaf,
        };
        await Assert.That(e.Name).IsEqualTo("Tank1");
        await Assert.That(e.ItemId).IsEqualTo("Plant1.Tank1");
        await Assert.That(e.BrowseType).IsEqualTo(HdaBrowseType.Leaf);
    }
}

public sealed class HdaReadResultTests
{
    [Test]
    public async Task Default_HasOkResultAndEmptyValues()
    {
        var r = new HdaReadResult();
        await Assert.That(r.ResultId).IsEqualTo(OpcResultId.Ok);
        await Assert.That(r.Values.Count).IsEqualTo(0);
        await Assert.That(r.ContinuationHandle).IsNull();
    }

    [Test]
    public async Task Paged_HasContinuationHandle()
    {
        var r = new HdaReadResult { ContinuationHandle = 42 };
        await Assert.That(r.ContinuationHandle).IsEqualTo(42);
    }
}

internal sealed class FakeHdaServer : IHdaServer
{
    public event EventHandler<EventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            State = OpcServerState.Running,
            VendorInfo = "FakeHdaServer",
            MaxReturnValues = 10000,
        });

    public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
        string itemIdPrefix, HdaBrowseType browseType,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await Task.Yield();
        yield return new HdaBrowseElement { Name = "Tank1", ItemId = "Tank1", BrowseType = HdaBrowseType.Leaf };
        yield return new HdaBrowseElement { Name = "Tank2", ItemId = "Tank2", BrowseType = HdaBrowseType.Leaf };
    }

    public Task<IReadOnlyList<HdaAggregate>> GetSupportedAggregatesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaAggregate>>(new[]
        {
            HdaAggregate.Interpolative, HdaAggregate.Average, HdaAggregate.Minimum, HdaAggregate.Maximum,
        });

    public Task<IReadOnlyList<HdaReadResult>> ReadRawAsync(
        IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime,
        int maxValuesPerItem, bool includeBounds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(id => new HdaReadResult
        {
            ItemId = id,
            Values = new[]
            {
                new HdaItemValue { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10), Value = 1.0, Quality = OpcQuality.Good },
                new HdaItemValue { Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5), Value = 2.0, Quality = OpcQuality.Good },
            },
        }).ToList());

    public Task<IReadOnlyList<HdaReadResult>> ReadProcessedAsync(
        IReadOnlyList<AggregateRequest> requests, HdaTime startTime, HdaTime endTime,
        TimeSpan resampleInterval, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaReadResult>>(requests.Select(r => new HdaReadResult
        {
            ItemId = r.ItemId,
            Values = new[]
            {
                new HdaItemValue { Timestamp = DateTimeOffset.UtcNow, Value = 1.5, Quality = OpcQuality.Good },
            },
        }).ToList());

    public Task<IReadOnlyList<HdaReadResult>> ReadAtTimeAsync(
        IReadOnlyList<string> itemIds, IReadOnlyList<DateTimeOffset> timestamps,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(id => new HdaReadResult
        {
            ItemId = id,
            Values = timestamps.Select(ts => new HdaItemValue { Timestamp = ts, Value = 0.0, Quality = OpcQuality.Good }).ToList(),
        }).ToList());

    public Task<IReadOnlyList<HdaAnnotationResult>> ReadAnnotationsAsync(
        IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaAnnotationResult>>(itemIds.Select(id => new HdaAnnotationResult
        {
            ItemId = id,
            Annotations = new[]
            {
                new HdaAnnotation { Timestamp = DateTimeOffset.UtcNow, AnnotationText = "Calibrated", User = "alice" },
            },
        }).ToList());

    public Task<IReadOnlyList<HdaReadResult>> ReadNextAsync(
        IReadOnlyList<string> itemIds, IReadOnlyList<int> continuationHandles,
        int maxValuesPerItem, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(id => new HdaReadResult { ItemId = id }).ToList());

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}

public sealed class IHdaServerContractTests
{
    [Test]
    public async Task GetStatusAsync_ReturnsHdaStatus()
    {
        await using var server = new FakeHdaServer();
        var status = await server.GetStatusAsync();
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Hda);
        await Assert.That(status.MaxReturnValues).IsEqualTo(10000);
    }

    [Test]
    public async Task BrowseAsync_StreamsElements()
    {
        await using var server = new FakeHdaServer();
        var count = 0;
        await foreach (var _ in server.BrowseAsync(string.Empty, HdaBrowseType.Leaf))
        {
            count++;
        }
        await Assert.That(count).IsEqualTo(2);
    }

    [Test]
    public async Task GetSupportedAggregatesAsync_ReturnsList()
    {
        await using var server = new FakeHdaServer();
        var aggregates = await server.GetSupportedAggregatesAsync();
        await Assert.That(aggregates.Count).IsEqualTo(4);
        await Assert.That(aggregates.Contains(HdaAggregate.Interpolative)).IsTrue();
    }

    [Test]
    public async Task ReadRawAsync_ReturnsPerItemValues()
    {
        await using var server = new FakeHdaServer();
        var results = await server.ReadRawAsync(
            new[] { "Tag1", "Tag2" },
            HdaTime.Relative("NOW-1H"),
            HdaTime.Now,
            maxValuesPerItem: 100,
            includeBounds: false);
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results[0].Values.Count).IsEqualTo(2);
        await Assert.That(results[0].ItemId).IsEqualTo("Tag1");
    }

    [Test]
    public async Task ReadProcessedAsync_PerAggregateResults()
    {
        await using var server = new FakeHdaServer();
        var requests = new[]
        {
            new AggregateRequest("Tag1", HdaAggregate.Average),
            new AggregateRequest("Tag2", HdaAggregate.Maximum),
        };
        var results = await server.ReadProcessedAsync(
            requests,
            HdaTime.Relative("NOW-1H"),
            HdaTime.Now,
            TimeSpan.FromMinutes(5));
        await Assert.That(results.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ReadAtTimeAsync_ReturnsValueAtEachTimestamp()
    {
        await using var server = new FakeHdaServer();
        var timestamps = new[]
        {
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddMinutes(-30),
            DateTimeOffset.UtcNow,
        };
        var results = await server.ReadAtTimeAsync(new[] { "Tag1" }, timestamps);
        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].Values.Count).IsEqualTo(3);
    }

    [Test]
    public async Task ReadAnnotationsAsync_ReturnsAnnotations()
    {
        await using var server = new FakeHdaServer();
        var results = await server.ReadAnnotationsAsync(
            new[] { "Tag1" },
            HdaTime.Relative("NOW-1D"),
            HdaTime.Now);
        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].Annotations.Count).IsEqualTo(1);
        await Assert.That(results[0].Annotations[0].User).IsEqualTo("alice");
    }
}

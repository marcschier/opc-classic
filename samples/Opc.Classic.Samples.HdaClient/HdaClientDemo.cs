// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda;

namespace Opc.Classic.Samples.HdaClient;

internal sealed class HdaClientDemo : BackgroundService
{
    private static readonly string[] DemoItemIds = ["Sensor.Temperature"];
    private static readonly HdaTime DemoStart = HdaTime.Relative("NOW-10M");
    private static readonly HdaTime DemoEnd = HdaTime.Now;

    private static readonly Action<ILogger, Exception?> ConnectedMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(ExecuteAsync)),
        "Connected to the in-process sample HDA server over InMemoryCallChannel.");

    private static readonly Action<ILogger, string, Version, int, Exception?> StatusMessage = LoggerMessage.Define<string, Version, int>(
        LogLevel.Information,
        new EventId(2, nameof(ExecuteAsync)),
        "Server status: vendor={VendorInfo}, version={Version}, maxReturnValues={MaxReturnValues}.");

    private static readonly Action<ILogger, string, string, Exception?> BrowseMessage = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(3, nameof(ExecuteAsync)),
        "Browse: {BrowseType} {ItemId}.");

    private static readonly Action<ILogger, string, int, Exception?> HandleMessage = LoggerMessage.Define<string, int>(
        LogLevel.Information,
        new EventId(4, nameof(ExecuteAsync)),
        "Item handle: itemId={ItemId}, serverHandle={ServerHandle}.");

    private static readonly Action<ILogger, string, DateTimeOffset, double, uint, Exception?> RawValueMessage = LoggerMessage.Define<string, DateTimeOffset, double, uint>(
        LogLevel.Information,
        new EventId(5, nameof(ExecuteAsync)),
        "SyncRead raw: itemId={ItemId}, timestamp={Timestamp:O}, value={Value:F3}, quality=0x{Quality:X4}.");

    private static readonly Action<ILogger, string, HdaAggregate, DateTimeOffset, double, Exception?> ProcessedValueMessage = LoggerMessage.Define<string, HdaAggregate, DateTimeOffset, double>(
        LogLevel.Information,
        new EventId(6, nameof(ExecuteAsync)),
        "SyncRead processed: itemId={ItemId}, aggregate={Aggregate}, bucket={Timestamp:O}, value={Value:F3}.");

    private static readonly Action<ILogger, int, Exception?> AnnotationCapabilitiesMessage = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(7, nameof(ExecuteAsync)),
        "SyncAnnotations capabilities=0x{Capabilities:X8}.");

    private static readonly Action<ILogger, Exception?> SyncAnnotationsReadUnavailableMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(8, nameof(ExecuteAsync)),
        "The generated SyncAnnotations client currently exposes QueryCapabilities only; the sample displays the same annotation through the managed HDA DTO surface.");

    private static readonly Action<ILogger, string, DateTimeOffset, string, string, Exception?> AnnotationMessage = LoggerMessage.Define<string, DateTimeOffset, string, string>(
        LogLevel.Information,
        new EventId(9, nameof(ExecuteAsync)),
        "Annotation: itemId={ItemId}, timestamp={Timestamp:O}, user={User}, text={Text}.");

    private static readonly Action<ILogger, Exception?> AsyncReadCancelledMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(10, nameof(ExecuteAsync)),
        "AsyncRead was cancelled before completion.");

    private static readonly Action<ILogger, Exception?> DisconnectedMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(11, nameof(ExecuteAsync)),
        "Released item handles and disconnected cleanly.");

    private readonly LoopbackHdaClient _client;
    private readonly ILogger<HdaClientDemo> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public HdaClientDemo(
        LoopbackHdaClient client,
        ILogger<HdaClientDemo> logger,
        IHostApplicationLifetime lifetime)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int[] serverHandles = [];
        try
        {
            await _client.ConnectAsync(stoppingToken).ConfigureAwait(false);
            ConnectedMessage(_logger, null);

            OpcServerStatus status = await _client.GetStatusAsync(stoppingToken).ConfigureAwait(false);
            StatusMessage(_logger, status.VendorInfo, status.ServerVersion, status.MaxReturnValues, null);

            await BrowseAsync(stoppingToken).ConfigureAwait(false);

            serverHandles = await _client.GetItemHandlesAsync(DemoItemIds, stoppingToken).ConfigureAwait(false);
            for (int index = 0; index < DemoItemIds.Length; index++)
            {
                HandleMessage(_logger, DemoItemIds[index], serverHandles[index], null);
            }

            await ReadRawAsync(serverHandles, stoppingToken).ConfigureAwait(false);
            await ReadProcessedAsync(serverHandles, stoppingToken).ConfigureAwait(false);
            await ReadAnnotationsAsync(stoppingToken).ConfigureAwait(false);
            await DemonstrateAsyncReadCancellationAsync(serverHandles, stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            if (serverHandles.Length > 0)
            {
                _ = await _client.ReleaseItemHandlesAsync(serverHandles, CancellationToken.None).ConfigureAwait(false);
            }

            await _client.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
            DisconnectedMessage(_logger, null);
            _lifetime.StopApplication();
        }
    }

    private async Task BrowseAsync(CancellationToken cancellationToken)
    {
        await foreach (HdaBrowseElement branch in _client.BrowseAsync(string.Empty, HdaBrowseType.Branch, cancellationToken).ConfigureAwait(false))
        {
            BrowseMessage(_logger, branch.BrowseType.ToString(), branch.ItemId, null);
        }

        await foreach (HdaBrowseElement leaf in _client.BrowseAsync("Sensor", HdaBrowseType.Leaf, cancellationToken).ConfigureAwait(false))
        {
            BrowseMessage(_logger, leaf.BrowseType.ToString(), leaf.ItemId, null);
        }
    }

    private async Task ReadRawAsync(int[] serverHandles, CancellationToken cancellationToken)
    {
        OpcHdaItem[] rawItems = await _client.ReadRawWithSyncReadAsync(
            DemoStart,
            DemoEnd,
            maxValuesPerItem: 5,
            includeBounds: false,
            serverHandles,
            cancellationToken).ConfigureAwait(false);

        for (int itemIndex = 0; itemIndex < rawItems.Length; itemIndex++)
        {
            OpcHdaItem item = rawItems[itemIndex];
            string itemId = DemoItemIds[itemIndex];
            for (int valueIndex = 0; valueIndex < item.Values.Length; valueIndex++)
            {
                RawValueMessage(
                    _logger,
                    itemId,
                    item.Timestamps[valueIndex],
                    item.Values[valueIndex].AsDouble() ?? double.NaN,
                    item.Qualities[valueIndex],
                    null);
            }
        }
    }

    private async Task ReadProcessedAsync(int[] serverHandles, CancellationToken cancellationToken)
    {
        OpcHdaItem[] processedItems = await _client.ReadProcessedWithSyncReadAsync(
            DemoStart,
            DemoEnd,
            TimeSpan.FromMinutes(5),
            HdaAggregate.Average,
            serverHandles,
            cancellationToken).ConfigureAwait(false);

        for (int itemIndex = 0; itemIndex < processedItems.Length; itemIndex++)
        {
            OpcHdaItem item = processedItems[itemIndex];
            string itemId = DemoItemIds[itemIndex];
            for (int valueIndex = 0; valueIndex < item.Values.Length; valueIndex++)
            {
                ProcessedValueMessage(
                    _logger,
                    itemId,
                    HdaAggregate.Average,
                    item.Timestamps[valueIndex],
                    item.Values[valueIndex].AsDouble() ?? double.NaN,
                    null);
            }
        }
    }

    private async Task ReadAnnotationsAsync(CancellationToken cancellationToken)
    {
        int capabilities = await _client.QueryAnnotationCapabilitiesAsync(cancellationToken).ConfigureAwait(false);
        AnnotationCapabilitiesMessage(_logger, capabilities, null);
        SyncAnnotationsReadUnavailableMessage(_logger, null);

        IReadOnlyList<HdaAnnotationResult> results = await _client.ReadAnnotationsAsync(
            DemoItemIds,
            HdaTime.Relative("NOW-1H"),
            DemoEnd,
            cancellationToken).ConfigureAwait(false);

        foreach (HdaAnnotationResult result in results)
        {
            foreach (HdaAnnotation annotation in result.Annotations)
            {
                AnnotationMessage(
                    _logger,
                    result.ItemId,
                    annotation.Timestamp,
                    annotation.User,
                    annotation.AnnotationText,
                    null);
            }
        }
    }

    private async Task DemonstrateAsyncReadCancellationAsync(int[] serverHandles, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        try
        {
            _ = await _client.BeginAsyncReadRawAsync(
                transactionId: 42,
                DemoStart,
                DemoEnd,
                maxValuesPerItem: 5,
                includeBounds: false,
                serverHandles,
                cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            AsyncReadCancelledMessage(_logger, null);
        }
    }
}

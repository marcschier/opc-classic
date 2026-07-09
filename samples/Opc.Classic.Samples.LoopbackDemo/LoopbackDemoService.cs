// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed class LoopbackDemoService : BackgroundService
{
    private static readonly Action<ILogger, Exception?> StartingDemo = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(StartingDemo)),
        "Starting OPC DA loopback demo");

    private static readonly Action<ILogger, Exception?> CompletedDemo = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, nameof(CompletedDemo)),
        "Completed OPC DA loopback demo");

    private readonly LoopbackDaClient _client;
    private readonly LoopbackDaRuntime _runtime;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<LoopbackDemoService> _logger;

    public LoopbackDemoService(
        LoopbackDaClient client,
        LoopbackDaRuntime runtime,
        IHostApplicationLifetime lifetime,
        ILogger<LoopbackDemoService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            StartingDemo(_logger, null);
            await RunDemoAsync(stoppingToken).ConfigureAwait(false);
            CompletedDemo(_logger, null);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunDemoAsync(CancellationToken stoppingToken)
    {
        WriteSection("OPC Classic DA in-process loopback");
        Console.WriteLine("Client proxies -> InMemoryCallChannel -> SampleDaServer dispatcher, all in one process.");
        Console.WriteLine();

        OpcServerStatus status = await _client.ConnectAsync(stoppingToken).ConfigureAwait(false);
        Console.WriteLine("Connected");
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  State:   {status.State}"));
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  Vendor:  {status.VendorInfo}"));
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  Version: {status.ServerVersion}"));
        Console.WriteLine();

        string[] itemIds = await _client.BrowseAsync(stoppingToken).ConfigureAwait(false);
        WriteSection("Browse");
        foreach (string itemId in itemIds)
        {
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  • {itemId}"));
        }

        Console.WriteLine();
        WriteSection("Add group + items");
        int groupHandle = await _client.AddGroupAsync(
            "Loopback Demo Group",
            active: true,
            requestedUpdateRate: 1000,
            clientHandle: 500,
            localeId: CultureInfo.InvariantCulture.LCID,
            stoppingToken).ConfigureAwait(false);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  Group server handle: {groupHandle}"));

        LoopbackItemRequest[] itemRequests =
        [
            new("Bucket Brigade.Int4", 1001),
            new("Bucket Brigade.String", 1002),
            new("Random.Real4", 1003),
            new("Saw-toothed Waves.Real8", 1004),
        ];
        IReadOnlyList<LoopbackAddItemResult> addedItems = await _client.AddItemsAsync(
            groupHandle,
            itemRequests,
            stoppingToken).ConfigureAwait(false);
        foreach (LoopbackAddItemResult item in addedItems)
        {
            Console.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"  {item.ItemId,-26} -> server={item.ServerHandle}, client={item.ClientHandle}, type={item.CanonicalDataType}, result={FormatError(item.Error)}"));
        }

        Console.WriteLine();
        WriteSection("Initial read");
        IReadOnlyList<LoopbackReadResult> initialReads = await _client.ReadAsync(addedItems, stoppingToken).ConfigureAwait(false);
        WriteReads(initialReads);

        Console.WriteLine();
        WriteSection("Subscribe + write + OnDataChange");
        LoopbackSubscription? subscription = null;
        var receivedNotifications = new List<LoopbackNotification>();
        Task? consumer = null;
        try
        {
            subscription = await _client.SubscribeAsync(stoppingToken).ConfigureAwait(false);
            consumer = ConsumeNotificationsAsync(subscription, receivedNotifications, stoppingToken);

            LoopbackAddItemResult[] writableItems = addedItems
                .Where(static item => item.ItemId is "Bucket Brigade.Int4" or "Bucket Brigade.String")
                .ToArray();
            OpcVariant[] writeValues = [OpcVariant.FromInt32(42), OpcVariant.FromString("loopback-write")];
            IReadOnlyList<LoopbackWriteResult> writes = await _client.WriteAsync(
                writableItems,
                writeValues,
                stoppingToken).ConfigureAwait(false);
            foreach (LoopbackWriteResult write in writes)
            {
                Console.WriteLine(string.Create(
                    CultureInfo.InvariantCulture,
                    $"  Write {write.ItemId,-22} = {FormatVariant(write.Value),-24} result={FormatError(write.Error)}"));
            }

            int cancelId = await subscription.RefreshAsync(transactionId: 9001, stoppingToken).ConfigureAwait(false);
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  Refresh requested; cancel id {cancelId}"));
            Console.WriteLine("  Streaming notifications for ~5 seconds...");
            await _runtime.RunPublisherAsync(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            if (subscription is not null)
            {
                await subscription.DisposeAsync().ConfigureAwait(false);
            }
        }

        if (consumer is not null)
        {
            await consumer.WaitAsync(TimeSpan.FromSeconds(2), CancellationToken.None).ConfigureAwait(false);
        }

        Console.WriteLine();
        WriteSection("Read after write");
        IReadOnlyList<LoopbackReadResult> finalReads = await _client.ReadAsync(addedItems, stoppingToken).ConfigureAwait(false);
        WriteReads(finalReads);

        await _client.RemoveGroupAsync(groupHandle, stoppingToken).ConfigureAwait(false);
        Console.WriteLine();
        WriteSection("Clean shutdown");
        Console.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  Notifications received: {receivedNotifications.Count}"));
        Console.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  Client->server channel calls: {_client.CallLog.Count}"));
        Console.WriteLine("  Group removed and callback unadvised.");
    }

    private static async Task ConsumeNotificationsAsync(
        LoopbackSubscription subscription,
        List<LoopbackNotification> receivedNotifications,
        CancellationToken stoppingToken)
    {
        await foreach (LoopbackNotification notification in subscription.Notifications(stoppingToken).ConfigureAwait(false))
        {
            receivedNotifications.Add(notification);
            Console.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"  OnDataChange tx={notification.TransactionId}, group={notification.GroupServerHandle}, items={notification.Items.Count}"));
            foreach (LoopbackNotificationItem item in notification.Items)
            {
                Console.WriteLine(string.Create(
                    CultureInfo.InvariantCulture,
                    $"    client={item.ClientHandle}: {FormatVariant(item.Value),-24} quality={item.Quality.Quality}, result={FormatError(item.Error)}"));
            }
        }
    }

    private static void WriteReads(IReadOnlyList<LoopbackReadResult> reads)
    {
        foreach (LoopbackReadResult read in reads)
        {
            Console.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"  Read  {read.ItemId,-26} = {FormatVariant(read.Value),-24} quality={read.Quality.Quality}, result={FormatError(read.Error)}"));
        }
    }

    private static void WriteSection(string title)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }

    private static string FormatError(int error) => error == OpcResultId.Ok.Code
        ? "S_OK"
        : string.Create(CultureInfo.InvariantCulture, $"0x{error:X8}");

    private static string FormatVariant(OpcVariant value)
    {
        string formatted = value.Boxed switch
        {
            null => "<null>",
            float single => single.ToString("0.000", CultureInfo.InvariantCulture),
            double number => number.ToString("0.000", CultureInfo.InvariantCulture),
            string text => string.Create(CultureInfo.InvariantCulture, $"\"{text}\""),
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            object other => other.ToString() ?? string.Empty,
        };

        return string.Create(CultureInfo.InvariantCulture, $"{formatted} [{value.Type}]");
    }
}

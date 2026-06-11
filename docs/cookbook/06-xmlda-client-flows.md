# XML-DA client flows

## What this covers

A .NET 10 XML-DA client uses a caller-owned `HttpClient` to call `GetStatus`, `Read`, `Write`, and the XML-DA polled subscription flow.

## Packages and namespaces

```bash
dotnet add package Opc.Classic.Xml
```

Core types:

- `HttpXmlDaClient`, `XmlDaRequestHeader`, `XmlDaReadRequest`, `XmlDaWriteRequest`, `XmlDaSubscribeRequest`, and `XmlDaSubscriptionPolledRefreshRequest` are in `Opc.Classic.Xml`.
- `XmlDaValue` creates typed write values and exposes returned raw text plus typed accessors.
- `OpcQuality` is in `Opc.Classic` and carries the DA quality bits returned by XML-DA items.

`HttpXmlDaClient` does not own or dispose the supplied `HttpClient`; configure authentication, proxy, TLS, timeout, and retry policy on that `HttpClient`.

## GetStatus

```csharp
using Opc.Classic.Xml;

public static async Task PrintStatusAsync(
    HttpClient httpClient,
    Uri endpoint,
    CancellationToken cancellationToken)
{
    var client = new HttpXmlDaClient(httpClient, endpoint);

    XmlDaServerStatus status = await client.GetStatusAsync(
        new XmlDaRequestHeader("en-US", "status-1"),
        cancellationToken);

    Console.WriteLine($"State: {status.ServerState}");
    Console.WriteLine($"Product: {status.ProductVersion}");
    Console.WriteLine($"Vendor: {status.VendorInfo}");
    Console.WriteLine($"Started: {status.StartTime:O}");
    Console.WriteLine($"Status: {status.StatusInfo ?? "(none)"}");
    Console.WriteLine($"Interfaces: {string.Join(", ", status.SupportedInterfaceVersions)}");
}
```

## Read

```csharp
using Opc.Classic.Xml;

public static async Task ReadItemsAsync(
    HttpClient httpClient,
    Uri endpoint,
    CancellationToken cancellationToken)
{
    var client = new HttpXmlDaClient(httpClient, endpoint);

    XmlDaReadResponse response = await client.ReadAsync(
        new XmlDaReadRequest(
            new XmlDaRequestHeader("en-US", "read-1"),
            new[]
            {
                new XmlDaReadItem("Channel1.Device1.Temperature", "temp", MaxAge: 0),
                new XmlDaReadItem("Channel1.Device1.Pressure", "pressure", MaxAge: 1000),
                new XmlDaReadItem("Channel1.Device1.Running", "running", MaxAge: 1000),
            }),
        cancellationToken);

    Console.WriteLine($"ServerState: {response.ServerState}");
    foreach (XmlDaItemValueResult item in response.Items)
    {
        XmlDaErrorCode code = item.ResultCode;
        string resultId = string.IsNullOrEmpty(item.ResultId)
            ? XmlDaErrorCodes.ToResultId(code)
            : item.ResultId!;

        if (!code.IsSuccess())
        {
            Console.WriteLine($"{item.ItemName}: fault {resultId}");
            continue;
        }

        Console.WriteLine(
            $"{item.ItemName}: value={item.Value?.RawText ?? "(null)"} " +
            $"type={item.Value?.Type.ToString() ?? "(none)"} " +
            $"quality={item.Quality} " +
            $"timestamp={item.Timestamp?.ToString("O") ?? "(none)"} " +
            $"result={resultId}");
    }
}
```

`XmlDaItemValueResult.ResultCode` parses the raw `ResultID`; `XmlDaErrorCode.IsSuccess()` treats `S_OK`, `S_CLAMP`, `S_DATAQUEUEOVERFLOW`, and `S_UNSUPPORTEDRATE` as successful XML-DA results.

## Write

```csharp
using Opc.Classic.Xml;

public static async Task WriteItemsAsync(
    HttpClient httpClient,
    Uri endpoint,
    CancellationToken cancellationToken)
{
    var client = new HttpXmlDaClient(httpClient, endpoint);

    XmlDaWriteResponse response = await client.WriteAsync(
        new XmlDaWriteRequest(
            new XmlDaRequestHeader("en-US", "write-1"),
            new[]
            {
                new XmlDaWriteItem("Channel1.Device1.Setpoint", "setpoint", XmlDaValue.OfDouble(42.5)),
                new XmlDaWriteItem("Channel1.Device1.Enable", "enable", XmlDaValue.OfBoolean(true)),
            }),
        cancellationToken);

    Console.WriteLine($"ServerState: {response.ServerState}");
    foreach (XmlDaWriteItemResult item in response.Items)
    {
        XmlDaErrorCode code = item.ResultCode;
        string resultId = string.IsNullOrEmpty(item.ResultId)
            ? XmlDaErrorCodes.ToResultId(code)
            : item.ResultId!;

        Console.WriteLine($"{item.ItemName}: {resultId}");
        if (!code.IsSuccess())
        {
            Console.WriteLine($"  {item.ErrorText ?? "Write failed."}");
        }
    }
}
```

## Subscribe, poll, and cancel

XML-DA subscriptions are client-polled: call `Subscribe`, repeat `SubscriptionPolledRefresh`, then call `SubscriptionCancel` with the returned server subscription handle.

```csharp
using Opc.Classic.Xml;

public static async Task PollSubscriptionAsync(
    HttpClient httpClient,
    Uri endpoint,
    CancellationToken cancellationToken)
{
    var client = new HttpXmlDaClient(httpClient, endpoint);

    XmlDaSubscribeResponse subscription = await client.SubscribeAsync(
        new XmlDaSubscribeRequest(
            new XmlDaRequestHeader("en-US", "subscribe-1"),
            new[]
            {
                new XmlDaSubscribeItem("Channel1.Device1.Temperature", "temp", RequestedSamplingRate: 1000),
                new XmlDaSubscribeItem("Channel1.Device1.Pressure", "pressure", RequestedSamplingRate: 1000),
            },
            RequestedSamplingRate: 1000,
            SubscriptionPingRate: 10000,
            ReturnValuesOnReply: true,
            EnableBuffering: true),
        cancellationToken);

    Console.WriteLine(
        $"Subscription: {subscription.ServerSubHandle}, revisedRate={subscription.RevisedSamplingRate}");
    foreach (XmlDaItemValueResult item in subscription.Items)
    {
        PrintSubscriptionItem(item);
    }

    try
    {
        XmlDaSubscriptionPolledRefreshResponse refresh = await client.SubscriptionPolledRefreshAsync(
            new XmlDaSubscriptionPolledRefreshRequest(
                new XmlDaRequestHeader("en-US", "poll-1"),
                new[] { subscription.ServerSubHandle },
                WaitTime: 5000,
                ReturnAllItems: false),
            cancellationToken);

        if (refresh.DataBufferOverflow)
        {
            Console.WriteLine("Server reported DataBufferOverflow on the refresh response.");
        }

        foreach (string invalidHandle in refresh.InvalidServerSubHandles)
        {
            Console.WriteLine($"Invalid subscription handle: {invalidHandle}");
        }

        foreach (XmlDaSubscriptionItemList list in refresh.ItemLists)
        {
            Console.WriteLine($"Changes for {list.SubscriptionHandle}");
            foreach (XmlDaItemValueResult item in list.Items)
            {
                PrintSubscriptionItem(item);
            }
        }
    }
    finally
    {
        await client.SubscriptionCancelAsync(
            new XmlDaSubscriptionCancelRequest(subscription.ServerSubHandle, "cancel-1"),
            CancellationToken.None);
    }

    static void PrintSubscriptionItem(XmlDaItemValueResult item)
    {
        XmlDaErrorCode code = item.ResultCode;
        string resultId = string.IsNullOrEmpty(item.ResultId)
            ? XmlDaErrorCodes.ToResultId(code)
            : item.ResultId!;

        if (!code.IsSuccess())
        {
            Console.WriteLine($"{item.ItemName}: fault {resultId}");
            return;
        }

        switch (code)
        {
            case XmlDaErrorCode.Clamp:
                Console.WriteLine($"{item.ItemName}: success {resultId}; value was clamped.");
                break;
            case XmlDaErrorCode.DataQueueOverflow:
                Console.WriteLine($"{item.ItemName}: success {resultId}; queued changes overflowed.");
                break;
            case XmlDaErrorCode.UnsupportedRate:
                Console.WriteLine($"{item.ItemName}: success {resultId}; server revised the sampling rate.");
                break;
            default:
                Console.WriteLine(
                    $"{item.ItemName}: value={item.Value?.RawText ?? "(null)"} " +
                    $"quality={item.Quality} result={resultId}");
                break;
        }
    }
}
```

## Validation aids

- [XML-DA status](../CONFORMANCE.md#opc-xml-da-101) summarizes supported operations, value handling, error codes, and quality bits.
- [OPC XML-DA 1.01 specification coverage](../CONFORMANCE.md#opc-xml-da-101) tracks operation and conformance coverage.
- Opc.Classic.Xml tests exercises the HTTP client and serializers with in-process SOAP payloads.

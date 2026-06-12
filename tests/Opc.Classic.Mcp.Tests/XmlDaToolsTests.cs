//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.Text.Json;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Xml;

namespace Opc.Classic.Mcp.Tests;

public sealed class XmlDaToolsTests
{
    [Test]
    public async Task XmlDa_connect_status_and_browse_round_trip_via_mcp_client()
    {
        var xmlDa = new SyntheticXmlDaClient();
        string name = "xmlda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryXmlDaConnectionRegistry.Register(name, xmlDa);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto connected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["endpointUrl"] = "inmemory://" + name,
            }).ConfigureAwait(false);
        OpcXmlDaServerStatusDto status = await server.CallToolAsync<OpcXmlDaServerStatusDto>(
            "opcclassic.xmlda.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcXmlDaBrowseResponseDto browse = await server.CallToolAsync<OpcXmlDaBrowseResponseDto>(
            "opcclassic.xmlda.browse",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.VendorInfo).IsEqualTo("Synthetic MCP XML-DA Server");
        await Assert.That(browse.Elements.Any(static element => element.ItemName == "Plant.Temperature")).IsTrue();
    }

    [Test]
    public async Task XmlDa_get_properties_read_and_write_round_trip_via_mcp_client()
    {
        var xmlDa = new SyntheticXmlDaClient();
        string name = "xmlda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryXmlDaConnectionRegistry.Register(name, xmlDa);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["endpointUrl"] = "inmemory://" + name,
            }).ConfigureAwait(false);

        OpcXmlDaGetPropertiesResponseDto properties = await server.CallToolAsync<OpcXmlDaGetPropertiesResponseDto>(
            "opcclassic.xmlda.get_properties",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["itemNames"] = new[] { "Plant.Temperature" },
                ["returnPropertyValues"] = true,
            }).ConfigureAwait(false);
        OpcXmlDaItemValueDto[] initialRead = await server.CallToolAsync<OpcXmlDaItemValueDto[]>(
            "opcclassic.xmlda.read",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["items"] = new[] { new OpcXmlDaReadItemDto("Plant.Temperature", "c1") },
            }).ConfigureAwait(false);
        OpcXmlDaWriteResultDto[] write = await server.CallToolAsync<OpcXmlDaWriteResultDto[]>(
            "opcclassic.xmlda.write",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["items"] = new[] { new OpcXmlDaWriteItemDto("Plant.Temperature", 42.5, "c1") },
            }).ConfigureAwait(false);
        OpcXmlDaItemValueDto[] afterWrite = await server.CallToolAsync<OpcXmlDaItemValueDto[]>(
            "opcclassic.xmlda.read",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["items"] = new[] { new OpcXmlDaReadItemDto("Plant.Temperature", "c1") },
            }).ConfigureAwait(false);

        await Assert.That(properties.PropertyLists[0].Properties.Count).IsGreaterThan(0);
        await Assert.That(GetDouble(initialRead[0].Value)).IsEqualTo(21.5);
        await Assert.That(write.All(static item => item.ResultCode == XmlDaErrorCode.Ok.ToString())).IsTrue();
        await Assert.That(GetDouble(afterWrite[0].Value)).IsEqualTo(42.5);
    }

    [Test]
    public async Task XmlDa_subscribe_poll_and_cancel_round_trip_via_mcp_client()
    {
        var xmlDa = new SyntheticXmlDaClient();
        string name = "xmlda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryXmlDaConnectionRegistry.Register(name, xmlDa);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["endpointUrl"] = "inmemory://" + name,
            }).ConfigureAwait(false);

        OpcXmlDaSubscriptionDto subscription = await server.CallToolAsync<OpcXmlDaSubscriptionDto>(
            "opcclassic.xmlda.subscribe",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["items"] = new[] { new OpcXmlDaSubscribeItemDto("Plant.Temperature", "s1") },
                ["returnValuesOnReply"] = true,
            }).ConfigureAwait(false);
        OpcXmlDaSubscriptionPollDto poll = await server.CallToolAsync<OpcXmlDaSubscriptionPollDto>(
            "opcclassic.xmlda.poll_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverSubHandles"] = new[] { subscription.ServerSubHandle },
                ["returnAllItems"] = true,
            }).ConfigureAwait(false);
        OpcResultDto cancel = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.cancel_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverSubHandle"] = subscription.ServerSubHandle,
            }).ConfigureAwait(false);

        await Assert.That(subscription.ServerSubHandle).IsNotNull();
        await Assert.That(poll.ItemLists[0].Items.Count).IsEqualTo(1);
        await Assert.That(cancel.Succeeded).IsTrue();
        await Assert.That(xmlDa.SubscriptionCount).IsEqualTo(0);
    }

    [Test]
    public async Task XmlDa_disconnect_round_trip_via_mcp_client()
    {
        var xmlDa = new SyntheticXmlDaClient();
        string name = "xmlda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryXmlDaConnectionRegistry.Register(name, xmlDa);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["endpointUrl"] = "inmemory://" + name,
            }).ConfigureAwait(false);

        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    private static double GetDouble(object? value) => ((JsonElement)value!).GetDouble();
}

internal sealed class SyntheticXmlDaClient : IXmlDaClient
{
    private readonly Dictionary<string, XmlDaValue> _values = new(StringComparer.Ordinal)
    {
        ["Plant.Temperature"] = XmlDaValue.OfDouble(21.5),
        ["Plant.Running"] = XmlDaValue.OfBoolean(true),
    };
    private readonly Dictionary<string, string[]> _subscriptions = new(StringComparer.Ordinal);
    private int _nextSubscription;

    public int SubscriptionCount => _subscriptions.Count;

    public Task<XmlDaServerStatus> GetStatusAsync(XmlDaRequestHeader header, CancellationToken cancellationToken = default)
    {
        _ = header;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new XmlDaServerStatus(
            DateTimeOffset.UnixEpoch,
            "1.0",
            "Synthetic MCP XML-DA Server",
            new[] { "en-US" },
            new[] { "XML_DA_Version_1_0" },
            XmlDaServerState.Running,
            StatusInfo: null));
    }

    public Task<XmlDaReadResponse> ReadAsync(XmlDaReadRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new XmlDaReadResponse(
            XmlDaServerState.Running,
            request.Items.Select(item => ToValueResult(item.ItemName, item.ClientItemHandle)).ToArray()));
    }

    public Task<XmlDaWriteResponse> WriteAsync(XmlDaWriteRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (XmlDaWriteItem item in request.Items)
        {
            _values[item.ItemName] = item.Value;
        }

        return Task.FromResult(new XmlDaWriteResponse(
            XmlDaServerState.Running,
            request.Items.Select(static item => new XmlDaWriteItemResult(item.ItemName, item.ClientItemHandle, null, null)).ToArray()));
    }

    public Task<XmlDaBrowseResponse> BrowseAsync(XmlDaBrowseRequest request, CancellationToken cancellationToken = default)
    {
        _ = request;
        cancellationToken.ThrowIfCancellationRequested();
        XmlDaBrowseElement[] elements = _values.Keys
            .Order(StringComparer.Ordinal)
            .Select(static itemName => new XmlDaBrowseElement(itemName.Split('.')[^1], string.Empty, itemName, IsItem: true, HasChildren: false))
            .ToArray();
        return Task.FromResult(new XmlDaBrowseResponse(XmlDaServerState.Running, elements, string.Empty, MoreElements: false));
    }

    public Task<XmlDaSubscribeResponse> SubscribeAsync(XmlDaSubscribeRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string handle = "sub-" + Interlocked.Increment(ref _nextSubscription).ToString(CultureInfo.InvariantCulture);
        string[] itemNames = request.Items.Select(static item => item.ItemName).ToArray();
        _subscriptions[handle] = itemNames;
        XmlDaItemValueResult[] values = request.ReturnValuesOnReply
            ? itemNames.Select(itemName => ToValueResult(itemName, request.Items.First(item => item.ItemName == itemName).ClientItemHandle)).ToArray()
            : [];
        return Task.FromResult(new XmlDaSubscribeResponse(XmlDaServerState.Running, handle, request.RequestedSamplingRate, values));
    }

    public Task<XmlDaSubscriptionPolledRefreshResponse> SubscriptionPolledRefreshAsync(XmlDaSubscriptionPolledRefreshRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var invalid = new List<string>();
        var lists = new List<XmlDaSubscriptionItemList>();
        foreach (string handle in request.ServerSubHandles)
        {
            if (!_subscriptions.TryGetValue(handle, out string[]? itemNames))
            {
                invalid.Add(handle);
                continue;
            }

            lists.Add(new XmlDaSubscriptionItemList(handle, itemNames.Select(itemName => ToValueResult(itemName, null)).ToArray()));
        }

        return Task.FromResult(new XmlDaSubscriptionPolledRefreshResponse(XmlDaServerState.Running, DataBufferOverflow: false, invalid, lists));
    }

    public Task<XmlDaSubscriptionCancelResponse> SubscriptionCancelAsync(XmlDaSubscriptionCancelRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _subscriptions.Remove(request.ServerSubHandle);
        return Task.FromResult(new XmlDaSubscriptionCancelResponse(request.ClientRequestHandle));
    }

    public Task<XmlDaGetPropertiesResponse> GetPropertiesAsync(XmlDaGetPropertiesRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        XmlDaItemPropertyList[] lists = request.ItemNames.Select(itemName => new XmlDaItemPropertyList(
            itemName,
            request.ItemPath,
            new[] { new XmlDaPropertyValue("DataType", "Canonical data type", XmlDaValue.OfString(_values[itemName].Type.ToString()), null) },
            ResultId: null)).ToArray();
        return Task.FromResult(new XmlDaGetPropertiesResponse(XmlDaServerState.Running, lists));
    }

    private XmlDaItemValueResult ToValueResult(string itemName, string? clientHandle)
    {
        if (!_values.TryGetValue(itemName, out XmlDaValue? value))
        {
            return new XmlDaItemValueResult(itemName, clientHandle, null, OpcQuality.Bad, DateTimeOffset.UtcNow, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.UnknownItemId));
        }

        return new XmlDaItemValueResult(itemName, clientHandle, value, OpcQuality.Good, DateTimeOffset.UtcNow, null);
    }
}

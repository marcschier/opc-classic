// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Collections.Concurrent;
using System.Globalization;
using Opc.Classic.Xml;

namespace Opc.Classic.Samples.SimulationServer.Xml;

/// <summary>
/// In-memory XML-DA client backed by the shared <see cref="SimulatedPlantModel" /> address space.
/// </summary>
public sealed class SimXmlDaClient : IXmlDaClient
{
    private const string DataTypeProperty = "DataType";
    private const string AccessRightsProperty = "AccessRights";
    private const string EngineeringUnitsProperty = "EngineeringUnits";
    private const string ItemIdProperty = "ItemID";

    private static readonly IReadOnlyList<string> SupportedLocaleIds = ["en-US"];
    private static readonly IReadOnlyList<string> SupportedInterfaceVersions = ["XML_DA_Version_1_0"];

    private readonly SimulatedPlantModel _model;
    private readonly ConcurrentDictionary<string, XmlDaSubscribeItem[]> _subscriptions = new(StringComparer.OrdinalIgnoreCase);
    private int _nextSubscription;

    /// <summary>
    /// Creates an XML-DA simulation client over <paramref name="model" />.
    /// </summary>
    /// <param name="model">Shared deterministic plant model.</param>
    public SimXmlDaClient(SimulatedPlantModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
    }

    /// <inheritdoc />
    public Task<XmlDaServerStatus> GetStatusAsync(XmlDaRequestHeader header, CancellationToken cancellationToken = default)
    {
        _ = header;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new XmlDaServerStatus(
            _model.StartTimeUtc,
            _model.ServerVersion.ToString(),
            _model.VendorInfo,
            SupportedLocaleIds,
            SupportedInterfaceVersions,
            XmlDaServerState.Running,
            StatusInfo: null));
    }

    /// <inheritdoc />
    public Task<XmlDaReadResponse> ReadAsync(XmlDaReadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        XmlDaItemValueResult[] items = request.Items
            .Select(item => ToValueResult(item.ItemName, item.ClientItemHandle, now))
            .ToArray();
        return Task.FromResult(new XmlDaReadResponse(XmlDaServerState.Running, items));
    }

    /// <inheritdoc />
    public Task<XmlDaWriteResponse> WriteAsync(XmlDaWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        XmlDaWriteItemResult[] items = request.Items.Select(ToWriteResult).ToArray();
        return Task.FromResult(new XmlDaWriteResponse(XmlDaServerState.Running, items));
    }

    /// <inheritdoc />
    public Task<XmlDaBrowseResponse> BrowseAsync(XmlDaBrowseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        string branchPath = request.ItemName ?? string.Empty;
        List<XmlDaBrowseElement> elements = [];
        if (request.BrowseFilter is XmlDaBrowseFilter.All or XmlDaBrowseFilter.Branch)
        {
            foreach (string branchName in _model.BrowseBranches(branchPath))
            {
                string itemName = CombinePath(branchPath, branchName);
                if (MatchesFilter(branchName, request.ElementNameFilter))
                {
                    elements.Add(new XmlDaBrowseElement(branchName, request.ItemPath, itemName, IsItem: false, HasChildren: HasChildren(itemName)));
                }
            }
        }

        if (request.BrowseFilter is XmlDaBrowseFilter.All or XmlDaBrowseFilter.Item)
        {
            foreach (SimulatedTag tag in _model.BrowseLeaves(branchPath))
            {
                if (MatchesFilter(tag.Name, request.ElementNameFilter))
                {
                    elements.Add(new XmlDaBrowseElement(tag.Name, request.ItemPath, tag.ItemId, IsItem: true, HasChildren: false));
                }
            }
        }

        elements.Sort(static (left, right) => string.Compare(left.ItemName, right.ItemName, StringComparison.OrdinalIgnoreCase));
        XmlDaBrowseElement[] page = Page(elements, request.ContinuationPoint, request.MaxElementsReturned, out string continuationPoint, out bool moreElements);
        return Task.FromResult(new XmlDaBrowseResponse(XmlDaServerState.Running, page, continuationPoint, moreElements));
    }

    /// <inheritdoc />
    public Task<XmlDaSubscribeResponse> SubscribeAsync(XmlDaSubscribeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        string handle = "sub-" + Interlocked.Increment(ref _nextSubscription).ToString(CultureInfo.InvariantCulture);
        XmlDaSubscribeItem[] items = request.Items.ToArray();
        _subscriptions[handle] = items;
        XmlDaItemValueResult[] values = request.ReturnValuesOnReply
            ? items.Select(item => ToValueResult(item.ItemName, item.ClientItemHandle, DateTimeOffset.UtcNow)).ToArray()
            : [];

        return Task.FromResult(new XmlDaSubscribeResponse(XmlDaServerState.Running, handle, request.RequestedSamplingRate, values));
    }

    /// <inheritdoc />
    public Task<XmlDaSubscriptionPolledRefreshResponse> SubscriptionPolledRefreshAsync(XmlDaSubscriptionPolledRefreshRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var invalid = new List<string>();
        var lists = new List<XmlDaSubscriptionItemList>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (string handle in request.ServerSubHandles)
        {
            if (!_subscriptions.TryGetValue(handle, out XmlDaSubscribeItem[]? items))
            {
                invalid.Add(handle);
                continue;
            }

            XmlDaItemValueResult[] values = items
                .Select(item => ToValueResult(item.ItemName, item.ClientItemHandle, now))
                .ToArray();
            lists.Add(new XmlDaSubscriptionItemList(handle, values));
        }

        return Task.FromResult(new XmlDaSubscriptionPolledRefreshResponse(XmlDaServerState.Running, DataBufferOverflow: false, invalid, lists));
    }

    /// <inheritdoc />
    public Task<XmlDaSubscriptionCancelResponse> SubscriptionCancelAsync(XmlDaSubscriptionCancelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        _subscriptions.TryRemove(request.ServerSubHandle, out _);
        return Task.FromResult(new XmlDaSubscriptionCancelResponse(request.ClientRequestHandle));
    }

    /// <inheritdoc />
    public Task<XmlDaGetPropertiesResponse> GetPropertiesAsync(XmlDaGetPropertiesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        XmlDaItemPropertyList[] lists = request.ItemNames
            .Select(itemName => ToPropertyList(itemName, request))
            .ToArray();
        return Task.FromResult(new XmlDaGetPropertiesResponse(XmlDaServerState.Running, lists));
    }

    private XmlDaWriteItemResult ToWriteResult(XmlDaWriteItem item)
    {
        if (!_model.TryGetTag(item.ItemName, out SimulatedTag tag))
        {
            return new XmlDaWriteItemResult(item.ItemName, item.ClientItemHandle, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.UnknownItemId), "Unknown item id.");
        }

        if (!tag.Writable)
        {
            return new XmlDaWriteItemResult(item.ItemName, item.ClientItemHandle, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.ReadOnly), "Item is read-only.");
        }

        object value = item.Value.Boxed ?? item.Value.RawText;
        return _model.TryWrite(item.ItemName, value)
            ? new XmlDaWriteItemResult(item.ItemName, item.ClientItemHandle, null, null)
            : new XmlDaWriteItemResult(item.ItemName, item.ClientItemHandle, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.Fail), "Write failed.");
    }

    private XmlDaItemValueResult ToValueResult(string itemName, string? clientHandle, DateTimeOffset timestamp)
    {
        if (!_model.TryGetTag(itemName, out SimulatedTag tag))
        {
            return new XmlDaItemValueResult(itemName, clientHandle, null, OpcQuality.Bad, timestamp, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.UnknownItemId));
        }

        return new XmlDaItemValueResult(itemName, clientHandle, ToXmlDaValue(tag, timestamp), OpcQuality.Good, timestamp, null);
    }

    private XmlDaItemPropertyList ToPropertyList(string itemName, XmlDaGetPropertiesRequest request)
    {
        if (!_model.TryGetTag(itemName, out SimulatedTag tag))
        {
            return new XmlDaItemPropertyList(itemName, request.ItemPath, [], XmlDaErrorCodes.ToResultId(XmlDaErrorCode.UnknownItemId));
        }

        IReadOnlyList<string> propertyNames = request.ReturnAllProperties || request.PropertyNames.Count == 0
            ? [DataTypeProperty, AccessRightsProperty, EngineeringUnitsProperty, ItemIdProperty]
            : request.PropertyNames;
        XmlDaPropertyValue[] properties = propertyNames
            .Select(propertyName => ToPropertyValue(tag, propertyName, request.ReturnPropertyValues))
            .ToArray();
        return new XmlDaItemPropertyList(itemName, request.ItemPath, properties, ResultId: null);
    }

    private static XmlDaPropertyValue ToPropertyValue(SimulatedTag tag, string propertyName, bool returnValue)
    {
        if (propertyName.Equals(DataTypeProperty, StringComparison.OrdinalIgnoreCase))
        {
            return new XmlDaPropertyValue(DataTypeProperty, "Canonical XML-DA data type", returnValue ? XmlDaValue.OfString(tag.DataType.ToString()) : null, null);
        }

        if (propertyName.Equals(AccessRightsProperty, StringComparison.OrdinalIgnoreCase))
        {
            return new XmlDaPropertyValue(AccessRightsProperty, "Item access rights", returnValue ? XmlDaValue.OfString(tag.Writable ? "read/write" : "read") : null, null);
        }

        if (propertyName.Equals(EngineeringUnitsProperty, StringComparison.OrdinalIgnoreCase))
        {
            return new XmlDaPropertyValue(EngineeringUnitsProperty, "Engineering units", returnValue ? XmlDaValue.OfString(tag.Units ?? string.Empty) : null, null);
        }

        if (propertyName.Equals(ItemIdProperty, StringComparison.OrdinalIgnoreCase))
        {
            return new XmlDaPropertyValue(ItemIdProperty, "Fully-qualified item id", returnValue ? XmlDaValue.OfString(tag.ItemId) : null, null);
        }

        return new XmlDaPropertyValue(propertyName, "Unknown property", null, XmlDaErrorCodes.ToResultId(XmlDaErrorCode.InvalidPid));
    }

    private XmlDaValue ToXmlDaValue(SimulatedTag tag, DateTimeOffset timestamp)
    {
        object value = _model.ValueAt(tag, timestamp);
        return tag.DataType switch
        {
            SimulatedDataType.Boolean => XmlDaValue.OfBoolean((bool)value),
            SimulatedDataType.Int16 => XmlDaValue.OfInt16((short)value),
            SimulatedDataType.Int32 => XmlDaValue.OfInt32((int)value),
            SimulatedDataType.Single => XmlDaValue.OfSingle((float)value),
            SimulatedDataType.Double => XmlDaValue.OfDouble((double)value),
            SimulatedDataType.String => XmlDaValue.OfString((string)value),
            _ => XmlDaValue.OfString(value.ToString() ?? string.Empty),
        };
    }

    private bool HasChildren(string branchPath) =>
        _model.BrowseBranches(branchPath).Count > 0 || _model.BrowseLeaves(branchPath).Count > 0;

    private static string CombinePath(string branchPath, string name) =>
        string.IsNullOrEmpty(branchPath) ? name : branchPath + "." + name;

    private static XmlDaBrowseElement[] Page(
        IReadOnlyList<XmlDaBrowseElement> elements,
        string continuationPoint,
        int maxElementsReturned,
        out string nextContinuationPoint,
        out bool moreElements)
    {
        int start = int.TryParse(continuationPoint, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed) && parsed > 0 ? parsed : 0;
        if (maxElementsReturned <= 0 || start + maxElementsReturned >= elements.Count)
        {
            nextContinuationPoint = string.Empty;
            moreElements = false;
            return elements.Skip(start).ToArray();
        }

        nextContinuationPoint = (start + maxElementsReturned).ToString(CultureInfo.InvariantCulture);
        moreElements = true;
        return elements.Skip(start).Take(maxElementsReturned).ToArray();
    }

    private static bool MatchesFilter(string name, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter == "*")
        {
            return true;
        }

        if (!filter.Contains('*', StringComparison.Ordinal))
        {
            return name.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        string[] parts = filter.Split('*', StringSplitOptions.RemoveEmptyEntries);
        int index = 0;
        foreach (string part in parts)
        {
            int found = name.IndexOf(part, index, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
            {
                return false;
            }

            index = found + part.Length;
        }

        return true;
    }
}

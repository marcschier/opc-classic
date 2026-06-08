//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;

namespace Opc.Classic.Integration.Tests.Support;

internal sealed class StubAeServer : IOpcAeServer
{
    private int _statusCallCount;
    private int _filterCallCount;
    private int _categoryCallCount;

    public int StatusCallCount => Volatile.Read(ref _statusCallCount);

    public int FilterCallCount => Volatile.Read(ref _filterCallCount);

    public int CategoryCallCount => Volatile.Read(ref _categoryCallCount);

    public int LastEventType { get; private set; }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _statusCallCount);
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(5),
            LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(6),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 10, 1),
            VendorInfo = "Loopback AE Stub Server",
            GroupCount = 0,
            BandWidth = 0,
        });
    }

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _filterCallCount);
        return Task.FromResult(0x07);
    }

    public Task QueryEventCategoriesAsync(
        int eventType,
        out int[] eventCategories,
        out string[] eventCategoryDescriptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _categoryCallCount);
        LastEventType = eventType;
        eventCategories = [1001, 1002];
        eventCategoryDescriptions = ["Process", "System"];
        return Task.CompletedTask;
    }

    public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
    {
        _ = eventCategory;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { "Level", "Pressure" });
    }

    public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
    {
        _ = conditionName;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { "Hi", "HiHi" });
    }

    public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
    {
        _ = source;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new[] { "Level" });
    }

    public Task QueryEventAttributesAsync(
        int eventCategory,
        out int[] attributeIds,
        out string[] attributeDescriptions,
        out ushort[] attributeTypes,
        CancellationToken cancellationToken = default)
    {
        _ = eventCategory;
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [501, 502];
        attributeDescriptions = ["Batch", "Limit"];
        attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
        return Task.CompletedTask;
    }

    public Task TranslateToItemIDsAsync(
        string source,
        int eventCategory,
        string conditionName,
        string subconditionName,
        int[] associatedAttributeIds,
        out string[] attributeItemIds,
        out string[] nodeNames,
        out Guid[] classIds,
        CancellationToken cancellationToken = default)
    {
        _ = eventCategory;
        _ = conditionName;
        _ = subconditionName;
        cancellationToken.ThrowIfCancellationRequested();
        attributeItemIds = associatedAttributeIds.Select(id => source + "." + id).ToArray();
        nodeNames = associatedAttributeIds.Select(static _ => "LoopbackNode").ToArray();
        classIds = associatedAttributeIds.Select(static _ => Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")).ToArray();
        return Task.CompletedTask;
    }

    public Task<OpcConditionState> GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken = default)
    {
        _ = source;
        _ = conditionName;
        _ = attributeIds;
        _ = cancellationToken;
        throw new NotSupportedException("Condition state is not exercised by the loopback AE root-call tests.");
    }

    public Task CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out IOPCEventSubscriptionMgt subscription,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default)
    {
        _ = active;
        _ = bufferTime;
        _ = maxSize;
        _ = clientSubscription;
        _ = requestedInterfaceId;
        _ = cancellationToken;
        subscription = default!;
        revisedBufferTime = 0;
        revisedMaxSize = 0;
        throw new NotSupportedException("AE subscription routing is not exposed by OpcAeServerHost's loopback object registry yet.");
    }

    public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        _ = areas;
        _ = cancellationToken;
        throw new NotSupportedException("Condition enable calls are not exercised by the loopback AE root-call tests.");
    }

    public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        _ = sources;
        _ = cancellationToken;
        throw new NotSupportedException("Condition enable calls are not exercised by the loopback AE root-call tests.");
    }

    public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
    {
        _ = areas;
        _ = cancellationToken;
        throw new NotSupportedException("Condition disable calls are not exercised by the loopback AE root-call tests.");
    }

    public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
    {
        _ = sources;
        _ = cancellationToken;
        throw new NotSupportedException("Condition disable calls are not exercised by the loopback AE root-call tests.");
    }

    public Task<int[]> AckConditionAsync(
        int dwCount,
        string acknowledgerId,
        string comment,
        string[] sources,
        string[] conditionNames,
        long[] activeTimes,
        int[] cookies,
        CancellationToken cancellationToken = default)
    {
        _ = dwCount;
        _ = acknowledgerId;
        _ = comment;
        _ = sources;
        _ = conditionNames;
        _ = activeTimes;
        _ = cookies;
        _ = cancellationToken;
        throw new NotSupportedException("Condition acknowledgements are not exercised by the loopback AE root-call tests.");
    }

    public Task CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        out IOPCEventAreaBrowser areaBrowser,
        CancellationToken cancellationToken = default)
    {
        _ = requestedInterfaceId;
        _ = cancellationToken;
        areaBrowser = default!;
        throw new NotSupportedException("AE area browser tearoffs are not exercised by the loopback AE root-call tests.");
    }
}

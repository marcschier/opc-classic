//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>Contract implemented by user code to provide an in-process managed AE server.</summary>
public interface IOpcAeServer : IOPCEventServer
{
    /// <summary>Gets the AE server runtime status snapshot.</summary>
    new Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the AE filter mask supported by the server.</summary>
    new Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default);

    Task<string[]> IOPCEventServer.QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<string[]> IOPCEventServer.QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<string[]> IOPCEventServer.QuerySourceConditionsAsync(string source, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<OpcConditionState> IOPCEventServer.GetConditionStateAsync(
        string source,
        string conditionName,
        int[] attributeIds,
        CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task IOPCEventServer.DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<int[]> IOPCEventServer.AckConditionAsync(
        string acknowledgerId,
        string comment,
        long[] activeTimes,
        int[] cookies,
        string[] sources,
        string[] conditionNames,
        CancellationToken cancellationToken) =>
        throw NotImplemented();

    private static OpcException NotImplemented() => new(OpcResultId.NotImplemented);
}

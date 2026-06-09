// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Da;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed record LoopbackItemRequest(string ItemId, int ClientHandle);

internal sealed record LoopbackAddItemResult(
    string ItemId,
    int ClientHandle,
    int ServerHandle,
    VarType CanonicalDataType,
    int AccessRights,
    int Error);

internal sealed record LoopbackReadResult(
    string ItemId,
    int ClientHandle,
    int ServerHandle,
    OpcVariant Value,
    OpcQuality Quality,
    DateTimeOffset Timestamp,
    int Error);

internal sealed record LoopbackWriteResult(
    string ItemId,
    int ServerHandle,
    OpcVariant Value,
    int Error);

internal sealed record LoopbackNotification(
    int TransactionId,
    int GroupServerHandle,
    int MasterQuality,
    int MasterError,
    IReadOnlyList<LoopbackNotificationItem> Items);

internal sealed record LoopbackNotificationItem(
    int ClientHandle,
    OpcVariant Value,
    OpcQuality Quality,
    DateTimeOffset Timestamp,
    int Error);

internal sealed record LoopbackItemBinding(
    int ServerHandle,
    string ItemId,
    int ClientHandle,
    bool Active);

internal sealed class LoopbackGroup {
    public LoopbackGroup(int serverHandle, string name, bool active, int updateRateMs, int clientHandle) {
        ServerHandle = serverHandle;
        Name = name;
        Active = active;
        UpdateRateMs = updateRateMs;
        ClientHandle = clientHandle;
    }

    public int ServerHandle { get; }

    public string Name { get; }

    public bool Active { get; }

    public int UpdateRateMs { get; }

    public int ClientHandle { get; }

    public Dictionary<int, LoopbackItemBinding> Items { get; } = new();
}

internal sealed record LoopbackPublishGroup(int ServerHandle, LoopbackItemBinding[] Items);

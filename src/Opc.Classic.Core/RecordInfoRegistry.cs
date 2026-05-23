//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Opc.Classic;

/// <summary>Process-wide registry for optional VT_RECORD layouts.</summary>
/// <remarks>
/// OPC DA/HDA payloads rarely use VT_RECORD. Applications that need it register
/// their known layouts up front so the NDR codec can map the wire GUID to an
/// <see cref="IRecordInfo"/> field table without reflection or COM interop.
/// </remarks>
public static class RecordInfoRegistry
{
    private static readonly Lock Gate = new();
    private static readonly Dictionary<Guid, IRecordInfo> Records = new();

    /// <summary>Registers or replaces a record layout.</summary>
    public static void Register(IRecordInfo recordInfo)
    {
        ArgumentNullException.ThrowIfNull(recordInfo);
        if (recordInfo.Id == Guid.Empty)
        {
            throw new ArgumentException("Record info id must not be empty.", nameof(recordInfo));
        }

        lock (Gate)
        {
            Records[recordInfo.Id] = recordInfo;
        }
    }

    /// <summary>Attempts to find a registered layout by GUID.</summary>
    public static bool TryGet(Guid id, [NotNullWhen(true)] out IRecordInfo? recordInfo)
    {
        lock (Gate)
        {
            return Records.TryGetValue(id, out recordInfo);
        }
    }

    /// <summary>Gets a registered layout or throws when the GUID is unknown.</summary>
    public static IRecordInfo Get(Guid id)
    {
        if (TryGet(id, out IRecordInfo? recordInfo))
        {
            return recordInfo;
        }

        throw new KeyNotFoundException($"No VT_RECORD layout is registered for {id}.");
    }

    /// <summary>Removes a registered layout.</summary>
    public static bool Unregister(Guid id)
    {
        lock (Gate)
        {
            return Records.Remove(id);
        }
    }
}

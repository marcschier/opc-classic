// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;
using System;
using System.Threading;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Stores the oxid details in memory.
/// </summary>
internal sealed class ComOxidDetails
{

    /// <summary>
    /// Create details
    /// </summary>
    /// <param name="localInstance"></param>
    /// <param name="oxid"></param>
    /// <param name="oid"></param>
    /// <param name="iid"></param>
    /// <param name="ipid"></param>
    /// <param name="ptr"></param>
    /// <param name="helper"></param>
    /// <param name="protectionLevel"></param>
    internal ComOxidDetails(LocalCoClass localInstance, Oxid oxid,
        ObjectId oid, string iid, string ipid, InterfacePointer ptr,
        ComOxidRuntimeHelper helper, ProtectionLevel protectionLevel)
    {
        Referent = localInstance;
        Ipid = ipid;
        _ptr = ptr;
        Oxid = oxid;
        Oid = oid;
        IID = iid;
        AuthHint = protectionLevel;
        COMRuntimeHelper = helper;
    }

    /// <summary>
    /// Port for unknown
    /// </summary>
    internal int PortForRemUnknown { set; get; } = -1;

    /// <summary>
    /// IId
    /// </summary>
    internal string IID { get; }

    /// <summary>
    /// Ipid
    /// </summary>
    internal string Ipid { get; }

    /// <summary>
    /// Unknown ipid
    /// </summary>
    internal string RemUnknownIpid { get; set; }

    /// <summary>
    /// Oid
    /// </summary>
    internal ObjectId Oid { get; }

    /// <summary>
    /// Oxid
    /// </summary>
    internal Oxid Oxid { get; }

    /// <summary>
    /// Referent
    /// </summary>
    internal LocalCoClass Referent { get; }

    /// <summary>
    /// Runtime helper
    /// </summary>
    internal ComOxidRuntimeHelper COMRuntimeHelper { get; }

    /// <summary>
    /// Protection level
    /// </summary>
    // default INTEGRITY per Microsoft DCOM hardening (KB5004442).
    internal Opc.Classic.Dcom.Rpc.ProtectionLevel AuthHint { get; } =
        Opc.Classic.Dcom.Rpc.ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;

    /// <summary>
    /// Cancellation source for the RemUnknown listener + per-connection
    /// threads owned by <see cref="ComOxidRuntimeHelper.StartRemUnknown"/>.
    /// Set when the RemUnknown listener starts; cancelled when the OXID is
    /// torn down so the listener loop and any in-flight RemUnknown worker
    /// threads exit cooperatively.
    /// </summary>
    /// <param name="cts"></param>
    internal void SetRemUnknownCancellation(CancellationTokenSource cts) =>
        _remUnknownCts = cts;

    /// <summary>
    /// Request cancellation of the RemUnknown listener + worker threads
    /// associated with this OXID. Safe to call repeatedly or when no
    /// cancellation source has been set.
    /// </summary>
    internal void InterruptRemUnknownThreadGroup()
    {
        try
        {
            _remUnknownCts?.Cancel();
        }
        catch (ObjectDisposedException e)
        {
            Log.Logger.Information(e, "ComOxidDetails interruptRemUnknownThreadGroup");
        }
    }

    private CancellationTokenSource _remUnknownCts;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly InterfacePointer _ptr;
#pragma warning restore IDE0052 // Remove unread private members
}

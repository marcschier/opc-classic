// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Stores the oxid details in memory.
/// </summary>
internal sealed class ComOxidDetails
{
    /// <summary>
    /// Create details
    /// </summary>
    /// <param name="localInstance">Local COM object instance represented by the OXID details.</param>
    /// <param name="oxid">DCOM OXID identifying the object exporter process.</param>
    /// <param name="oid">DCOM OID identifying the exported object instance.</param>
    /// <param name="iid">Interface IID identifying the COM interface being queried or marshaled.</param>
    /// <param name="ipid">DCOM IPID identifying the per-interface object reference.</param>
    /// <param name="ptr">Pointer referent being encoded, decoded, or dereferenced.</param>
    /// <param name="helper">Runtime helper that owns the OXID resolver binding.</param>
    /// <param name="protectionLevel">RPC authentication protection level applied to the message.</param>
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
    /// <param name="cts">Cancellation source that owns the timeout or shutdown token for the operation.</param>
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

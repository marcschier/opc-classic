// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using System;

namespace Opc.Classic.Dcom.Core; 
/// <summary>
/// Stores the oxid details in memory.
/// </summary>
internal sealed class ComOxidDetails {

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
        ComOxidRuntimeHelper helper, ProtectionLevel protectionLevel) {
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
    // Phase 3B: default INTEGRITY per Microsoft DCOM hardening (KB5004442).
    internal Opc.Classic.Dcom.Rpc.ProtectionLevel AuthHint { get; } =
        Opc.Classic.Dcom.Rpc.ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;

    /// <summary>
    /// Set thread group
    /// </summary>
    /// <param name="value"></param>
    internal void SetRemUnknownThreadGroup(ThreadGroup value) =>
        _remUnknownThread = value;

    /// <summary>
    /// Interrupt unknown thread group thread
    /// </summary>
    internal void InterruptRemUnknownThreadGroup() {
        if (_remUnknownThread != null) {
            try {
                // _remUnknownThread.interrupt();

                // old: remUnknownThread.destroy();
            }
            catch (Exception e) {
                Log.Logger.Information(e, "ComOxidDetails interruptRemUnknownThreadGroup");
            }
        }
    }

    // TODO N1.2-followup: replace ThreadGroup with an async RemUnknown listener lease.
    private ThreadGroup _remUnknownThread;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly InterfacePointer _ptr;
#pragma warning restore IDE0052 // Remove unread private members
}

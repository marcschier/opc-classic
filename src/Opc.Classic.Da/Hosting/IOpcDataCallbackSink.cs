//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Server-side callback delivery abstraction for IOPCDataCallback sinks.
/// Unifies the cross-platform DCOM transport path and the Windows CCW
/// activation path so <see cref="OpcDaGroup.TriggerDataChangeAsync"/> and
/// <see cref="OpcDaGroup.TriggerCancelCompleteAsync"/> can fan out to
/// both kinds of subscribed sinks through one interface.
/// </summary>
/// <remarks>
/// <para>
/// Today there are two implementations:
/// </para>
/// <list type="bullet">
///   <item><description>Cross-platform DCOM transport: provided by the
///   host's call-channel adapter. Each <c>OpcDaGroup.AdviseAsync</c>
///   call wraps the client's <c>IOpcInterfaceRef</c> in a sink that
///   marshals the payload over the managed DCOM transport.</description></item>
///   <item><description>Windows SCM CCW: <see cref="Windows.OpcDataCallbackProxy"/>
///   implements this interface and invokes the client-supplied COM vtable
///   directly.</description></item>
/// </list>
/// <para>
/// All methods are synchronous. They block the trigger fan-out caller
/// until the underlying transport / vtable invocation returns.
/// Implementations should be thread-safe — multiple TriggerDataChangeAsync
/// calls may concurrently fan out to the same sink instance.
/// </para>
/// </remarks>
public interface IOpcDataCallbackSink : IDisposable
{
    /// <summary>Delivers an OnDataChange callback (opnum 3).</summary>
    void OnDataChange(OpcDaGroup.DataChangePayload payload);

    /// <summary>Delivers an OnReadComplete callback (opnum 4).</summary>
    void OnReadComplete(OpcDaGroup.DataChangePayload payload);

    /// <summary>Delivers an OnWriteComplete callback (opnum 5).</summary>
    void OnWriteComplete(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors);

    /// <summary>Delivers an OnCancelComplete callback (opnum 6).</summary>
    void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload);
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Dcom;

/// <summary>
/// Generator-emitted helper that wires an out-parameter <c>IOpcInterface</c>
/// OBJREF to the existing call channel's IPID routing table and yields the
/// channel back so the proxy generator can construct a client-side proxy.
/// </summary>
/// <remarks>
/// <para>
/// When an OPC method returns an interface via <c>[out, iid_is(riid)] LPUNKNOWN*</c>,
/// MS-DCOM marshals the result as a referent-prefixed MInterfacePointer
/// containing an OBJREF_STANDARD with an OXID + IPID. The proxy needs to:
/// </para>
/// <list type="number">
///   <item><description>Decode the OBJREF (already handled by <see cref="OpcMInterfacePointerCodec"/>).</description></item>
///   <item><description>Register the new IPID against the requested interface IID on the call channel so subsequent invocations on the sub-proxy reach the right server-side object instance.</description></item>
///   <item><description>Construct the appropriate <c>{InterfaceName}ClientProxy(ICallChannel)</c> wrapping the existing channel.</description></item>
/// </list>
/// <para>
/// This helper centralizes step 2 so the proxy generator only needs to call
/// <see cref="RegisterAndYieldChannel"/> and then <c>new XClientProxy(channel)</c>.
/// </para>
/// </remarks>
public static class OpcSubProxyHelper {
    /// <summary>
    /// Registers the IID→IPID routing for <paramref name="objRef"/> on the
    /// supplied <paramref name="channel"/> (if it supports IPID routing) and
    /// returns the channel so the caller can construct a sub-proxy.
    /// Returns <see langword="null"/> if <paramref name="objRef"/> is null
    /// or has an empty IPID (i.e. the server returned a NULL pointer).
    /// </summary>
    /// <param name="channel">The parent call channel used to invoke the sub-proxy. Must not be null.</param>
    /// <param name="targetInterfaceId">The IID the sub-proxy implements (e.g. <c>IOPCEventSubscriptionMgt.InterfaceId</c>).</param>
    /// <param name="objRef">The OBJREF returned from the wire (typically decoded via <see cref="OpcMInterfacePointerCodec.Read"/>).</param>
    /// <returns>The channel for sub-proxy construction, or <see langword="null"/> if the OBJREF is missing or empty.</returns>
    public static Opc.Classic.ICallChannel? RegisterAndYieldChannel(
        Opc.Classic.ICallChannel channel,
        Guid targetInterfaceId,
        IOpcInterfaceRef? objRef) {
        ArgumentNullException.ThrowIfNull(channel);
        if (targetInterfaceId == Guid.Empty) {
            throw new ArgumentException("Target interface IID must not be empty.", nameof(targetInterfaceId));
        }

        if (objRef is null || objRef.Ipid == Guid.Empty) {
            return null;
        }

        if (channel is DcomCallChannel routable) {
            routable.RegisterInterfaceIpid(targetInterfaceId, objRef.Ipid);
        }

        return channel;
    }
}

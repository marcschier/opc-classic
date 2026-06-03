//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Net;
using Opc.Classic.Dcom;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// Builds OBJREF_STANDARD interface pointers for server-pushed sinks
/// hosted on the MCP host (Track AU). The sink OBJREF is what the MCP
/// client hands to <c>IConnectionPoint::Advise</c> so the OPC server
/// can call back into the host's <c>IOPCDataCallback</c> listener.
/// </summary>
/// <remarks>
/// <para>
/// The OBJREF is built from:
/// </para>
/// <list type="bullet">
///   <item><description>The IID of the sink interface (typically <see cref="IOPCDataCallback"/>).</description></item>
///   <item><description>A freshly-allocated OXID + OID + IPID (the registry-supplied IPID is reused so dispatch routes back to the right sink).</description></item>
///   <item><description>A single TCP DUALSTRINGARRAY string binding (<c>"host[port]"</c>, tower id 0x07) pointing at the listener's <see cref="IPEndPoint"/>.</description></item>
///   <item><description>A single WinNT NTLM security binding (auth-svc 0x0A, auth-level 0xFFFF, empty principal) so a hardened DCOM server can negotiate auth on the callback connection.</description></item>
/// </list>
/// <para>
/// This is the same wire shape <c>DualStringArray(int port)</c> emits for
/// the legacy ComServer's OXID resolver (see
/// <c>src/Opc.Classic.Dcom/Core/DualStringArray.cs</c>) but constructed
/// directly into the <see cref="IOpcInterfaceRef"/> shape used by
/// <see cref="IConnectionPointClientProxy.AdviseAsync"/>.
/// </para>
/// </remarks>
public static class OpcSinkObjRefBuilder
{
    private const ushort TcpTowerId = 0x07;
    private const ushort WinNtSecurityAuthService = 0x000A;
    private const ushort SecurityAuthzNone = 0xFFFF;

    /// <summary>
    /// Builds a sink <see cref="IOpcInterfaceRef"/> targeting
    /// <paramref name="listenerEndpoint"/> for IPID <paramref name="ipid"/>.
    /// </summary>
    /// <param name="iid">The sink interface IID (typically <see cref="IOPCDataCallback"/>).</param>
    /// <param name="ipid">The IPID under which the sink dispatcher is registered with the host's <see cref="OpcObjectRegistry"/>.</param>
    /// <param name="listenerEndpoint">The TCP endpoint the OPC server will connect back to.</param>
    /// <param name="oxid">Optional OXID; a fresh one is generated when omitted.</param>
    /// <param name="oid">Optional OID; a fresh one is generated when omitted.</param>
    /// <returns>An OBJREF_STANDARD interface reference ready to hand to <c>IConnectionPoint::Advise</c>.</returns>
    public static IOpcInterfaceRef Build(
        Guid iid,
        Guid ipid,
        IPEndPoint listenerEndpoint,
        ulong? oxid = null,
        ulong? oid = null)
    {
        ArgumentNullException.ThrowIfNull(listenerEndpoint);
        if (iid == Guid.Empty)
        {
            throw new ArgumentException("Sink IID must not be empty.", nameof(iid));
        }
        if (ipid == Guid.Empty)
        {
            throw new ArgumentException("Sink IPID must not be empty.", nameof(ipid));
        }

        ulong actualOxid = oxid ?? GenerateRandomUInt64();
        ulong actualOid = oid ?? GenerateRandomUInt64();

        (ushort[] bindings, ushort securityOffset) = BuildResolverBindings(listenerEndpoint);

        return new OpcInterfaceRef(
            iid: iid,
            flags: 0u,
            publicRefs: 1u,
            oxid: actualOxid,
            oid: actualOid,
            ipid: ipid,
            securityOffset: securityOffset,
            resolverBindings: bindings);
    }

    /// <summary>
    /// Builds the DUALSTRINGARRAY ushort stream for one TCP string binding
    /// and one WinNT security binding, returning it together with the security
    /// offset (in ushorts).
    /// </summary>
    /// <remarks>
    /// Layout matches MS-DCOM §2.2.19.3 DUALSTRINGARRAY:
    /// <code>
    ///     USHORT[]  stringBindings      // (tower:USHORT, hostport:wchar*, NUL:USHORT)+
    ///     USHORT    stringTerminator(0)
    ///     USHORT[]  securityBindings    // (authsvc:USHORT, authlevel:USHORT, principal:wchar*, NUL:USHORT)+
    ///     USHORT    securityTerminator(0)
    /// </code>
    /// The securityOffset returned is the index (in ushorts) of the first
    /// security binding, which equals the count of ushorts in the string
    /// bindings + their terminator.
    /// </remarks>
    internal static (ushort[] Bindings, ushort SecurityOffset) BuildResolverBindings(IPEndPoint listenerEndpoint)
    {
        ArgumentNullException.ThrowIfNull(listenerEndpoint);

        string hostPort = listenerEndpoint.Address + "[" + listenerEndpoint.Port.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";

        var stringBinding = new List<ushort>(hostPort.Length + 4)
        {
            TcpTowerId,
        };
        for (int i = 0; i < hostPort.Length; i++)
        {
            stringBinding.Add((ushort)hostPort[i]);
        }

        stringBinding.Add(0);  // NUL terminator for the host[port] string

        var securityBinding = new List<ushort>(8)
        {
            WinNtSecurityAuthService,
            SecurityAuthzNone,  // RPC_C_AUTHZ_NONE per MS-DCOM §2.2.19.4 (authz service id, NOT auth level)
            0,  // empty principal (single NUL wchar)
        };

        var bindings = new List<ushort>(stringBinding.Count + securityBinding.Count + 2);
        bindings.AddRange(stringBinding);
        bindings.Add(0);  // stringBindings terminator
        int securityOffsetUShorts = bindings.Count;
        bindings.AddRange(securityBinding);
        bindings.Add(0);  // securityBindings terminator

        if (securityOffsetUShorts > ushort.MaxValue)
        {
            throw new ArgumentException("DUALSTRINGARRAY security offset exceeds UInt16.MaxValue.", nameof(listenerEndpoint));
        }

        return (bindings.ToArray(), (ushort)securityOffsetUShorts);
    }

    private static ulong GenerateRandomUInt64()
    {
        Span<byte> buf = stackalloc byte[8];
        System.Security.Cryptography.RandomNumberGenerator.Fill(buf);
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(buf);
    }
}

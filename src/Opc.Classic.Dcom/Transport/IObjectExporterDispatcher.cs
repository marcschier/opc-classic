//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Net;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Manual <see cref="IOpcServerDispatcher"/> for the well-known DCOM
/// <c>IObjectExporter</c> interface (IID <c>99FCFEC4-5260-101B-BBCB-00AA0021347A</c>,
/// MS-DCOM 3.1.2.5.1.1).
/// </summary>
/// <remarks>
/// <para>
/// Every DCOM endpoint that hosts a callback sink (for example
/// <c>DaCallbackEndpoint</c>) must expose <c>IObjectExporter</c> so that
/// remote OPC servers can resolve the listener's data-port bindings and
/// the IPID of its <c>IRemUnknown</c> before delivering inbound
/// <c>IOPCDataCallback</c> invocations. Without it, the remote server's
/// <c>IConnectionPoint::Advise</c> call appears to succeed but the
/// subsequent <c>ResolveOxid2</c> probe against our endpoint times out
/// and the server abandons the callback channel.
/// </para>
/// <para>
/// This dispatcher implements the minimal opnum set required for
/// inbound callback delivery:
/// </para>
/// <list type="table">
///   <listheader><term>Opnum</term><description>Method</description></listheader>
///   <item><term>1</term><description><c>SimplePing</c> — accepts a SETID, returns S_OK</description></item>
///   <item><term>2</term><description><c>ComplexPing</c> — accepts ping deltas, returns the SETID + backoff factor 0</description></item>
///   <item><term>3</term><description><c>ServerAlive</c> — no-op success</description></item>
///   <item><term>4</term><description><c>ResolveOxid2</c> — returns listener bindings + IRemUnknown IPID + COMVERSION 5.4</description></item>
///   <item><term>5</term><description><c>ServerAlive2</c> — returns COMVERSION 5.4 + listener bindings</description></item>
/// </list>
/// <para>
/// The dispatcher reads the listener's bound endpoint via the
/// <see cref="Func{IPEndPoint}"/> supplied at construction so the
/// reported bindings always reflect the actual port (resolves dynamic
/// port-0 binds). The <see cref="IRemUnknownIpid"/> property exposes
/// the IPID handed out in <c>ResolveOxid2</c> responses; today it's a
/// fixed throw-away GUID because the inbound-callback path does not
/// require <c>RemQueryInterface</c> / <c>RemAddRef</c> /
/// <c>RemRelease</c> (the sink dispatcher is registered under its
/// caller-supplied IPID in <see cref="OpcObjectRegistry"/> and the
/// remote server uses that IPID directly).
/// </para>
/// </remarks>
public sealed class IObjectExporterDispatcher : IOpcServerDispatcher
{
    /// <summary>OPC well-known <c>IObjectExporter</c> interface identifier.</summary>
    public static readonly Guid InterfaceId = OpcGuids.IID_IObjectExporter;

    private const int E_INVALIDARG = unchecked((int)0x80070057u);
    private const int RPC_S_PROCNUM_OUT_OF_RANGE = unchecked((int)0x800706D1u);
    // DCOM 5.4 — the version reported by the legacy ComOxidRuntimeHelper.
    // Modern Windows DCOM (5.6+) accepts 5.4 in interop, so we stay at
    // 5.4 unless a follow-up requires bumping.
    private const ushort ComVersionMajor = 5;
    private const ushort ComVersionMinor = 4;
    // Authn hint: RPC_C_AUTHN_LEVEL_NONE (1). Loopback callbacks don't
    // require authentication; if the server insists, it'll renegotiate.
    private const uint AuthnHintNone = 1;

    private readonly Func<IPEndPoint?> _endpointProvider;
    private readonly Guid _remUnknownIpid;

    /// <summary>Creates a dispatcher that resolves bindings via <paramref name="endpointProvider"/>.</summary>
    /// <param name="endpointProvider">Returns the listener's current TCP endpoint, or null when not bound.</param>
    /// <param name="remUnknownIpid">IPID to report as the listener's <c>IRemUnknown</c>; a freshly-generated GUID when omitted.</param>
    public IObjectExporterDispatcher(Func<IPEndPoint?> endpointProvider, Guid? remUnknownIpid = null)
    {
        ArgumentNullException.ThrowIfNull(endpointProvider);
        _endpointProvider = endpointProvider;
        _remUnknownIpid = remUnknownIpid ?? Guid.NewGuid();
    }

    /// <summary>The IPID this dispatcher reports as the listener's <c>IRemUnknown</c>.</summary>
    public Guid IRemUnknownIpid => _remUnknownIpid;

    /// <inheritdoc/>
    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return opnum switch
        {
            1 => new ValueTask<DispatchResult>(SimplePing(requestPayload.Span)),
            2 => new ValueTask<DispatchResult>(ComplexPing(requestPayload.Span)),
            3 => new ValueTask<DispatchResult>(ServerAlive()),
            4 => new ValueTask<DispatchResult>(ResolveOxid2(requestPayload.Span)),
            5 => new ValueTask<DispatchResult>(ServerAlive2()),
            _ => new ValueTask<DispatchResult>(DispatchResult.Fault(RPC_S_PROCNUM_OUT_OF_RANGE)),
        };
    }

    private static DispatchResult SimplePing(ReadOnlySpan<byte> request)
    {
        // IDL: HRESULT SimplePing([in] SETID *pSetId);
        // pSetId is 8 bytes. The legacy impl maintains ping-set state; for
        // inbound callback delivery we don't need it — clients (and the
        // OPC server pinging us) just need a success response.
        _ = request;
        return DispatchResult.Success(ReadOnlyMemory<byte>.Empty);
    }

    private static DispatchResult ComplexPing(ReadOnlySpan<byte> request)
    {
        // IDL: HRESULT ComplexPing([in, out] SETID *pSetId, [in] USHORT SequenceNum,
        //                          [in] USHORT cAddToSet, [in] USHORT cDelFromSet,
        //                          [in, unique, size_is(cAddToSet)] OID AddToSet[],
        //                          [in, unique, size_is(cDelFromSet)] OID DelFromSet[],
        //                          [out] USHORT *pPingBackoffFactor);
        // Response: SETID (8 bytes; echo back) + USHORT backoff factor + HRESULT (in ORPC envelope).
        // For inbound callbacks we don't track ping sets; echo the SETID and return 0 backoff.
        var buffer = new byte[16];
        if (request.Length >= 8)
        {
            request[..8].CopyTo(buffer);
        }
        // SETID is at offset 0..8, USHORT backoff at offset 8, bytes 10..15 padding.
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(8, 2), 0);
        return DispatchResult.Success(buffer);
    }

    private static DispatchResult ServerAlive()
    {
        // IDL: HRESULT ServerAlive(void);
        // No out-params. Return an empty body.
        return DispatchResult.Success(ReadOnlyMemory<byte>.Empty);
    }

    private DispatchResult ServerAlive2()
    {
        // IDL: HRESULT ServerAlive2([out] COMVERSION *pComVersion,
        //                            [out] DUALSTRINGARRAY **ppdsaOrBindings,
        //                            [out] DWORD *pReserved);
        IPEndPoint? endpoint = _endpointProvider();
        byte[] dualStringArray = EncodeDualStringArrayForListener(endpoint);

        var size = 4 + 4 + 4 + 4 + dualStringArray.Length + 16;
        var buffer = new byte[size];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt16(ComVersionMajor);
        writer.WriteUInt16(ComVersionMinor);
        WriteDualStringArrayPointerPointer(ref writer, dualStringArray);
        writer.WriteUInt32(0);
        return DispatchResult.Success(buffer.AsSpan(0, writer.Position).ToArray());
    }

    private DispatchResult ResolveOxid2(ReadOnlySpan<byte> request)
    {
        // IDL: HRESULT ResolveOxid2(
        //          [in] OXID *pOxid,
        //          [in] USHORT cRequestedProtseqs,
        //          [in, ref, size_is(cRequestedProtseqs)] USHORT arRequestedProtseqs[],
        //          [out] DUALSTRINGARRAY **ppdsaOxidBindings,
        //          [out] IPID *pipidRemUnknown,
        //          [out] DWORD *pAuthnHint,
        //          [out] COMVERSION *pComVersion);
        // Request body: 8-byte OXID + USHORT count + conformant USHORT[count].
        // We don't validate the OXID — every sink lives under our single
        // listener's OXID set, and a wrong OXID would still resolve to the
        // same bindings.
        _ = request;

        IPEndPoint? endpoint = _endpointProvider();
        if (endpoint is null)
        {
            return DispatchResult.Fault(E_INVALIDARG);
        }

        byte[] dualStringArray = EncodeDualStringArrayForListener(endpoint);

        var size = 4 + 4 + dualStringArray.Length + 16 + 16 + 4 + 4;
        var buffer = new byte[size];
        var writer = new NdrWriter(buffer);
        WriteDualStringArrayPointerPointer(ref writer, dualStringArray);
        writer.WriteGuid(_remUnknownIpid);
        writer.WriteUInt32(AuthnHintNone);
        writer.WriteUInt16(ComVersionMajor);
        writer.WriteUInt16(ComVersionMinor);
        return DispatchResult.Success(buffer.AsSpan(0, writer.Position).ToArray());
    }

    private static byte[] EncodeDualStringArrayForListener(IPEndPoint? endpoint)
    {
        // Empty DUALSTRINGARRAY when the listener hasn't bound yet —
        // wire shape is two USHORT zeros (entryCount=0, securityOffset=0).
        if (endpoint is null)
        {
            return new byte[] { 0x00, 0x00, 0x00, 0x00 };
        }

        (ushort[] entries, ushort securityOffset) = BuildResolverBindings(endpoint);
        var buffer = new byte[4 + entries.Length * 2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(0, 2), (ushort)entries.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2, 2), securityOffset);
        for (int i = 0; i < entries.Length; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(4 + i * 2, 2), entries[i]);
        }
        return buffer;
    }

    private static (ushort[] Bindings, ushort SecurityOffset) BuildResolverBindings(IPEndPoint listenerEndpoint)
    {
        // Mirrors mcp/Opc.Classic.Mcp/Tools/OpcSinkObjRefBuilder.BuildResolverBindings
        // but inlined here so the Dcom assembly has no dependency on the MCP host.
        const ushort TcpTowerId = 0x07;
        const ushort WinNtSecurityAuthService = 0x000A;
        const ushort SecurityAuthzNone = 0xFFFF;

        string hostPort = listenerEndpoint.Address + "[" + listenerEndpoint.Port.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";

        var bindings = new System.Collections.Generic.List<ushort>(hostPort.Length + 12)
        {
            TcpTowerId,
        };
        for (int i = 0; i < hostPort.Length; i++)
        {
            bindings.Add((ushort)hostPort[i]);
        }

        bindings.Add(0);   // string-binding NUL terminator
        bindings.Add(0);   // stringBindings array terminator
        int securityOffsetUShorts = bindings.Count;
        bindings.Add(WinNtSecurityAuthService);
        bindings.Add(SecurityAuthzNone);
        bindings.Add(0);   // empty principal
        bindings.Add(0);   // securityBindings array terminator

        if (securityOffsetUShorts > ushort.MaxValue)
        {
            throw new ArgumentException("DUALSTRINGARRAY security offset exceeds UInt16.MaxValue.", nameof(listenerEndpoint));
        }

        return (bindings.ToArray(), (ushort)securityOffsetUShorts);
    }

    private static void WriteDualStringArrayPointerPointer(ref NdrWriter writer, ReadOnlySpan<byte> dualStringArray)
    {
        _ = writer.WriteReferentId();
        _ = writer.WriteReferentId();
        if (dualStringArray.IsEmpty)
        {
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            return;
        }

        writer.WriteRawBytes(dualStringArray);
        writer.AlignTo(4);
    }
}

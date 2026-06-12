//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers;
using System.Buffers.Binary;
using Opc.Classic.Transport;
using Opc.Classic.Dcom.Channels;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// DCOM implementation of <see cref="ICallChannel" /> over a pipelines-backed async transport.
/// </summary>
public sealed class DcomCallChannel : ICallChannel, IAsyncDisposable
{
    private const int AuthenticationVerifierHeaderLength = 8;

    private readonly IAsyncTransport _transport;
    private readonly IAuthContext _authContext;
    private readonly Guid? _objectIpid;
    private readonly Guid[] _preBindIids;
    private readonly SemaphoreSlim _callLock = new(1, 1);
    private readonly Dictionary<Guid, int> _contextIds = new();
    private readonly Dictionary<Guid, Guid> _interfaceIpids = new();
    private int _associationGroupId;
    private int _maxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE;
    private int _maxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE;
    private int _nextCallId = 1;
    private int _nextContextId;
    private bool _bound;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="DcomCallChannel" /> class.</summary>
    /// <param name="transport">The connected async transport.</param>
    /// <param name="authContext">The authentication context for bind and packet protection.</param>
    public DcomCallChannel(IAsyncTransport transport, IAuthContext authContext)
        : this(transport, authContext, objectIpid: null, preBindIids: null)
    {
    }

    /// <summary>Initializes a new instance with presentation contexts to include in the first bind.</summary>
    /// <param name="transport">The connected async transport.</param>
    /// <param name="authContext">The authentication context for bind and packet protection.</param>
    /// <param name="preBindIids">Interface IIDs to pre-declare in the initial DCE bind.</param>
    public DcomCallChannel(
        IAsyncTransport transport,
        IAuthContext authContext,
        IReadOnlyList<Guid> preBindIids)
        : this(transport, authContext, objectIpid: null, preBindIids)
    {
    }

    /// <summary>Initializes a channel that routes calls to a specific DCOM object IPID.</summary>
    /// <param name="transport">The connected async transport.</param>
    /// <param name="authContext">The authentication context for bind and packet protection.</param>
    /// <param name="objectIpid">The object IPID to place in request PDUs.</param>
    public DcomCallChannel(IAsyncTransport transport, IAuthContext authContext, Guid objectIpid)
        : this(transport, authContext, (Guid?)objectIpid, preBindIids: null)
    {
        if (objectIpid == Guid.Empty)
        {
            throw new ArgumentException("Object IPID must not be empty.", nameof(objectIpid));
        }
    }

    /// <summary>Initializes an object-routed channel with presentation contexts to include in the first bind.</summary>
    /// <param name="transport">The connected async transport.</param>
    /// <param name="authContext">The authentication context for bind and packet protection.</param>
    /// <param name="objectIpid">The object IPID to place in request PDUs.</param>
    /// <param name="preBindIids">Interface IIDs to pre-declare in the initial DCE bind.</param>
    public DcomCallChannel(
        IAsyncTransport transport,
        IAuthContext authContext,
        Guid objectIpid,
        IReadOnlyList<Guid> preBindIids)
        : this(transport, authContext, (Guid?)objectIpid, preBindIids)
    {
        if (objectIpid == Guid.Empty)
        {
            throw new ArgumentException("Object IPID must not be empty.", nameof(objectIpid));
        }
    }

    private DcomCallChannel(
        IAsyncTransport transport,
        IAuthContext authContext,
        Guid? objectIpid,
        IReadOnlyList<Guid>? preBindIids)
    {
        ArgumentNullException.ThrowIfNull(transport);
        ArgumentNullException.ThrowIfNull(authContext);

        _transport = transport;
        _authContext = authContext;
        _objectIpid = objectIpid;
        _preBindIids = NormalizePreBindIids(preBindIids);
    }

    /// <summary>
    /// Registers an interface-specific IPID so that subsequent calls to a
    /// different IID than the channel's default object route to the correct
    /// object instance. This is the QueryInterface-without-RemUnknown path: the
    /// caller obtains additional IPIDs via DCOM activation with multiple
    /// requested IIDs, then calls this to associate each IID with its IPID.
    /// </summary>
    public void RegisterInterfaceIpid(Guid interfaceId, Guid ipid)
    {
        if (interfaceId == Guid.Empty)
        {
            throw new ArgumentException("InterfaceId must not be empty.", nameof(interfaceId));
        }
        if (ipid == Guid.Empty)
        {
            throw new ArgumentException("IPID must not be empty.", nameof(ipid));
        }
        _interfaceIpids[interfaceId] = ipid;
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(opnum);
        cancellationToken.ThrowIfCancellationRequested();

        bool diag = string.Equals(System.Environment.GetEnvironmentVariable("OPC_CLASSIC_DCOM_WIRE_DUMP"), "1", System.StringComparison.Ordinal);
        using IDisposable causalityScope = CausalityContext.BeginCall();
        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            int contextId = await EnsurePresentationContextAsync(interfaceId, cancellationToken).ConfigureAwait(false);
            await BindTraceAsync(diag, $"InvokeAsync: iid={interfaceId:D} opnum={opnum} ctx={contextId} payload={requestPayload.Length}b").ConfigureAwait(false);
            Guid causalityId = CausalityContext.Current.Value.GetValueOrDefault();
            byte[] requestStub = OrpcEnvelope.BuildRequestStub(requestPayload, causalityId);
            Guid? routedIpid = _interfaceIpids.TryGetValue(interfaceId, out Guid mapped) ? mapped : _objectIpid;
            var request = new RequestCoPdu
            {
                AllocationHint = requestStub.Length,
                ContextId = contextId,
                Opnum = opnum,
                Stub = requestStub,
                CallId = NextCallId(),
                Object = routedIpid.HasValue ? new UUID(routedIpid.Value.ToString("D")) : null,
            };
            await BindTraceAsync(diag, $"InvokeAsync: writing REQUEST PDU stub={requestStub.Length}b ipid={(routedIpid.HasValue ? routedIpid.Value.ToString("D") : "<none>")}").ConfigureAwait(false);
            await WritePduAsync(request, cancellationToken).ConfigureAwait(false);
            await BindTraceAsync(diag, "InvokeAsync: REQUEST written; awaiting response PDU...").ConfigureAwait(false);

            ConnectionOrientedPdu reply = await ReadFragmentedPduAsync(cancellationToken).ConfigureAwait(false);
            await BindTraceAsync(diag, $"InvokeAsync: received reply PDU type={reply.Type}").ConfigureAwait(false);
            NdrCallResult result = reply switch
            {
                ResponseCoPdu response => new NdrCallResult(0, OrpcEnvelope.ExtractResponseBody(response.Stub)),
                FaultCoPdu fault => new NdrCallResult(unchecked((int)fault.Status), ReadOnlyMemory<byte>.Empty),
                _ => throw new InvalidOperationException($"Unexpected DCE/RPC PDU type {reply.Type}.")
            };
            // Optional diagnostic hex-dump: set OPC_CLASSIC_DCOM_WIRE_DUMP=1 to log
            // request and response bytes to stderr. Useful for byte-exact wire-format
            // debugging against captured Wireshark traces from Windows OPC clients.
            if (diag)
            {
                await System.Console.Error.WriteLineAsync($"[wire] iid={interfaceId:D} opnum={opnum} hresult=0x{result.Hresult:X8}").ConfigureAwait(false);
                await System.Console.Error.WriteLineAsync($"[wire] request  ({requestPayload.Length}b): {System.Convert.ToHexString(requestPayload.Span)}").ConfigureAwait(false);
                await System.Console.Error.WriteLineAsync($"[wire] response ({result.ResponsePayload.Length}b): {System.Convert.ToHexString(result.ResponsePayload.Span)}").ConfigureAwait(false);
            }
            return result;
        }
        finally
        {
            _callLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _callLock.Dispose();
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private async ValueTask<int> EnsurePresentationContextAsync(Guid interfaceId, CancellationToken cancellationToken)
    {
        if (_contextIds.TryGetValue(interfaceId, out int contextId))
        {
            return contextId;
        }

        if (!_bound)
        {
            PendingPresentationContext[] initialContexts = CreateInitialPresentationContexts(interfaceId);
            PresentationResult[] bindResults = await BindAsync(initialContexts, cancellationToken).ConfigureAwait(false);
            _bound = true;
            if (string.Equals(System.Environment.GetEnvironmentVariable("OPC_CLASSIC_DCOM_WIRE_DUMP"), "1", System.StringComparison.Ordinal))
            {
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                var sb = new System.Text.StringBuilder();
                sb.Append(culture, $"[bind] req-iid={interfaceId:D} contexts={initialContexts.Length}");
                for (int i = 0; i < initialContexts.Length && i < bindResults.Length; i++)
                {
                    sb.Append(culture, $" | [{i}] iid={initialContexts[i].InterfaceId:D} -> {bindResults[i]}");
                }
                await System.Console.Error.WriteLineAsync(sb.ToString()).ConfigureAwait(false);
            }
            if (_contextIds.TryGetValue(interfaceId, out contextId))
            {
                return contextId;
            }

            PresentationResult? result = FindPresentationResult(initialContexts, bindResults, interfaceId);
            if (result is not null)
            {
                throw new InvalidOperationException($"Presentation context rejected for IID {interfaceId:D}: {result}.");
            }

            throw new InvalidOperationException($"Bind acknowledge did not accept presentation context for IID {interfaceId:D}.");
        }

        contextId = _nextContextId++;
        await AlterContextAsync(interfaceId, contextId, cancellationToken).ConfigureAwait(false);
        _contextIds.Add(interfaceId, contextId);
        return contextId;
    }

    private async ValueTask AlterContextAsync(Guid interfaceId, int contextId, CancellationToken cancellationToken)
    {
        var alter = new AlterContextPdu
        {
            AssociationGroupId = _associationGroupId,
            ContextList = [CreatePresentationContext(interfaceId, contextId)],
            MaxReceiveFragment = _maxReceiveFragment,
            MaxTransmitFragment = _maxTransmitFragment,
            CallId = NextCallId(),
        };
        await WritePduAsync(alter, cancellationToken).ConfigureAwait(false);

        DecodedPdu decoded = await ReadSinglePduAsync(cancellationToken).ConfigureAwait(false);
        if (decoded.Pdu is not AlterContextResponsePdu alterAck)
        {
            if (decoded.Pdu is FaultCoPdu fault)
            {
                throw new InvalidOperationException($"AlterContext failed with fault 0x{unchecked((int)fault.Status):X8}.");
            }
            throw new InvalidOperationException($"Expected alter_context_response PDU, received type {decoded.Pdu.Type}.");
        }

        ValidatePresentationResults(alterAck.ResultList, "AlterContext response had no presentation results.", "AlterContext presentation context rejected");
    }

    private async ValueTask<PresentationResult[]> BindAsync(PendingPresentationContext[] contexts, CancellationToken cancellationToken)
    {
        bool diag = string.Equals(System.Environment.GetEnvironmentVariable("OPC_CLASSIC_DCOM_WIRE_DUMP"), "1", System.StringComparison.Ordinal);
        await BindTraceAsync(diag, $"BindAsync entered: ctx_count={contexts.Length} auth_ctx={_authContext.GetType().Name} protection={_authContext.ProtectionLevel}").ConfigureAwait(false);
        byte[] initialToken = _authContext.BuildInitialToken();
        await BindTraceAsync(diag, $"initial_token: {initialToken.Length} bytes").ConfigureAwait(false);
        var bind = new BindPdu
        {
            AssociationGroupId = _associationGroupId,
            ContextList = ToPresentationContexts(contexts),
            MaxReceiveFragment = _maxReceiveFragment,
            MaxTransmitFragment = _maxTransmitFragment,
            CallId = NextCallId(),
        };
        await BindTraceAsync(diag, "writing BIND PDU...").ConfigureAwait(false);
        await WritePduAsync(bind, cancellationToken, initialToken).ConfigureAwait(false);
        await BindTraceAsync(diag, "BIND PDU written; awaiting BIND_ACK...").ConfigureAwait(false);
        DecodedPdu decoded = await ReadSinglePduAsync(cancellationToken).ConfigureAwait(false);
        await BindTraceAsync(diag, $"received PDU type={decoded.Pdu.Type} auth_body_len={decoded.AuthenticationBody.Length}").ConfigureAwait(false);
        if (decoded.Pdu is not BindAcknowledgePdu bindAck)
        {
            if (decoded.Pdu is FaultCoPdu fault)
            {
                throw new InvalidOperationException($"Bind failed with fault 0x{unchecked((int)fault.Status):X8}.");
            }
            throw new InvalidOperationException($"Expected bind_ack PDU, received type {decoded.Pdu.Type}.");
        }
        PresentationResult[] results = GetPresentationResults(bindAck.ResultList, "Bind acknowledge did not include presentation results.");
        _associationGroupId = bindAck.AssociationGroupId;
        _maxTransmitFragment = bindAck.MaxReceiveFragment;
        _maxReceiveFragment = bindAck.MaxTransmitFragment;
        await BindTraceAsync(diag, "processing challenge token...").ConfigureAwait(false);
        byte[] nextToken = _authContext.ProcessChallengeToken(decoded.AuthenticationBody);
        await BindTraceAsync(diag, $"next_token: {nextToken.Length} bytes (will {(nextToken.Length > 0 ? "send AUTH3" : "skip AUTH3")})").ConfigureAwait(false);
        if (nextToken.Length > 0)
        {
            var auth3 = new Auth3Pdu { CallId = NextCallId() };
            await WritePduAsync(auth3, cancellationToken, nextToken).ConfigureAwait(false);
            await BindTraceAsync(diag, "AUTH3 PDU written").ConfigureAwait(false);
        }
        RegisterAcceptedPresentationContexts(contexts, results);
        await BindTraceAsync(diag, $"BindAsync complete: accepted {results.Length} presentation contexts").ConfigureAwait(false);
        return results;
    }

    private static async ValueTask BindTraceAsync(bool enabled, string message)
    {
        if (!enabled)
        {
            return;
        }
        await System.Console.Error.WriteLineAsync($"[bind-trace] {message}").ConfigureAwait(false);
    }

    private async ValueTask WritePduAsync(
        ConnectionOrientedPdu pdu,
        CancellationToken cancellationToken,
        ReadOnlyMemory<byte> authenticationBody = default)
    {
        if (pdu is IFragmentable fragmentable && authenticationBody.IsEmpty)
        {
            foreach (var fragment in fragmentable.GetFragments(_maxTransmitFragment))
            {
                await WriteSinglePduAsync(fragment, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        await WriteSinglePduAsync(pdu, cancellationToken, authenticationBody).ConfigureAwait(false);
    }

    private async ValueTask WriteSinglePduAsync(
        ConnectionOrientedPdu pdu,
        CancellationToken cancellationToken,
        ReadOnlyMemory<byte> authenticationBody = default)
    {
        byte[] bytes = PduCodec.EncodePdu(pdu, _maxTransmitFragment);
        if (!authenticationBody.IsEmpty)
        {
            bytes = AttachAuthenticationVerifier(bytes, authenticationBody);
        }
        else if (pdu.Type == AlterContextPdu.ALTER_CONTEXT_TYPE
            && _authContext.ProtectionLevel >= OpcProtectionLevel.Integrity)
        {
            // Per MS-RPCE §3.3.1.5.3.1: alter_context at PKT_INTEGRITY/PRIVACY
            // carries a verifier header with auth_type/auth_level but a zero-length
            // auth_value (no signature, no token). Use AttachEmptyAuthVerifier.
            bytes = AttachEmptyAuthVerifier(bytes);
        }
        else
        {
            bytes = ApplyPacketProtection(bytes);
        }

        Memory<byte> output = _transport.Output.GetMemory(bytes.Length);
        bytes.AsSpan().CopyTo(output.Span);
        _transport.Output.Advance(bytes.Length);
        await _transport.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private byte[] AttachEmptyAuthVerifier(byte[] pduBytes)
    {
        // Emit an 8-byte auth verifier header with our current auth_type/auth_level
        // and a zero-length auth_value, per MS-RPCE §3.3.1.5.3.1 for alter_context.
        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] result = new byte[fragmentLength];
        pduBytes.CopyTo(result, 0);
        Span<byte> verifier = result.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = _authContext.AuthenticationServiceCode;
        verifier[1] = (byte)ToRpcProtectionLevel(_authContext.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], 0);

        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), 0);
        return result;
    }

    private async ValueTask<ConnectionOrientedPdu> ReadFragmentedPduAsync(CancellationToken cancellationToken)
    {
        DecodedPdu decoded = await ReadSinglePduAsync(cancellationToken).ConfigureAwait(false);
        ConnectionOrientedPdu pdu = decoded.Pdu;
        if (pdu is not IFragmentable fragmentable || pdu.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
        {
            return pdu;
        }

        var fragments = new List<ConnectionOrientedPdu> { pdu };
        while (!pdu.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
        {
            decoded = await ReadSinglePduAsync(cancellationToken).ConfigureAwait(false);
            pdu = decoded.Pdu;
            fragments.Add(pdu);
        }

        return fragmentable.Reassemble(fragments);
    }

    private async ValueTask<DecodedPdu> ReadSinglePduAsync(CancellationToken cancellationToken)
    {
        byte[] frame = await PduCodec.ReadPduFrameAsync(_transport.Input, cancellationToken).ConfigureAwait(false);
        AuthenticationStrippedFrame stripped = StripAuthenticationVerifier(frame);
        byte pduType = stripped.PduBytes[ConnectionOrientedPdu.TYPE_OFFSET];
        if (stripped.AuthenticationBody.Length > 0 && IsPacketProtectedPdu(pduType))
        {
            VerifyPacketProtection(stripped);
        }

        ConnectionOrientedPdu pdu = PduCodec.DecodePdu(stripped.PduBytes);
        return new DecodedPdu(pdu, stripped.AuthenticationBody);
    }

    private void VerifyPacketProtection(AuthenticationStrippedFrame stripped)
    {
        Span<byte> pduBody = stripped.PduBytes.AsSpan(ConnectionOrientedPdu.HEADER_LENGTH);
        if (!_authContext.VerifyAndUnseal(pduBody, stripped.AuthenticationBody))
        {
            throw new InvalidOperationException("DCE/RPC authentication verifier validation failed.");
        }
    }

    private byte[] ApplyPacketProtection(byte[] pduBytes)
    {
        if (_authContext.ProtectionLevel < OpcProtectionLevel.Integrity)
        {
            return pduBytes;
        }
        return ApplyPacketProtectionCore(pduBytes);
    }

    private byte[] ApplyPacketProtectionCore(byte[] pduBytes)
    {
        // Per MS-RPCE §3.3.1.5.2.2 the signature covers the entire PDU EXCEPT
        // the auth_value field. Build the full PDU with a zeroed auth_value
        // placeholder and final frag_length/auth_length headers; hand the
        // signed-region span to the auth context; then copy the returned
        // signature into the placeholder.
        const int authValueLength = 16;
        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength + authValueLength;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] protectedPdu = new byte[fragmentLength];
        pduBytes.CopyTo(protectedPdu, 0);

        Span<byte> verifier = protectedPdu.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = _authContext.AuthenticationServiceCode;
        verifier[1] = (byte)ToRpcProtectionLevel(_authContext.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], 0);

        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)authValueLength);

        Span<byte> signedRegion = protectedPdu.AsSpan(0, verifierStart + AuthenticationVerifierHeaderLength);
        _authContext.SignAndSeal(signedRegion, out byte[] signature);
        if (signature is null || signature.Length == 0)
        {
            return pduBytes;
        }
        if (signature.Length != authValueLength)
        {
            throw new InvalidOperationException(
                $"Auth context returned a {signature.Length}-byte signature; DCE/RPC expects {authValueLength}.");
        }
        signature.CopyTo(protectedPdu.AsSpan(verifierStart + AuthenticationVerifierHeaderLength, authValueLength));
        return protectedPdu;
    }

    private byte[] AttachAuthenticationVerifier(byte[] pduBytes, ReadOnlyMemory<byte> body)
    {
        if (body.IsEmpty)
        {
            return pduBytes;
        }

        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength + body.Length;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] protectedPdu = new byte[fragmentLength];
        pduBytes.CopyTo(protectedPdu, 0);
        body.Span.CopyTo(protectedPdu.AsSpan(verifierStart + AuthenticationVerifierHeaderLength));

        Span<byte> verifier = protectedPdu.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = _authContext.AuthenticationServiceCode;
        verifier[1] = (byte)ToRpcProtectionLevel(_authContext.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], 0);

        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)body.Length);
        return protectedPdu;
    }

    private static AuthenticationStrippedFrame StripAuthenticationVerifier(byte[] frame)
    {
        if (frame.Length < ConnectionOrientedPdu.HEADER_LENGTH)
        {
            throw new InvalidOperationException("DCE/RPC frame is shorter than the common header.");
        }

        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET));
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        if (authLength == 0)
        {
            return new AuthenticationStrippedFrame(frame, []);
        }

        int verifierStart = fragmentLength - authLength - AuthenticationVerifierHeaderLength;
        if (verifierStart < ConnectionOrientedPdu.HEADER_LENGTH || verifierStart + AuthenticationVerifierHeaderLength > frame.Length)
        {
            throw new InvalidOperationException("DCE/RPC authentication verifier is malformed.");
        }

        int padding = frame[verifierStart + 2];
        int strippedLength = verifierStart - padding;
        if (strippedLength < ConnectionOrientedPdu.HEADER_LENGTH || strippedLength > frame.Length)
        {
            throw new InvalidOperationException("DCE/RPC authentication verifier padding is malformed.");
        }

        byte[] authenticationBody = frame.AsSpan(verifierStart + AuthenticationVerifierHeaderLength, authLength).ToArray();
        byte[] pduBytes = frame.AsSpan(0, strippedLength).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)strippedLength);
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), 0);
        return new AuthenticationStrippedFrame(pduBytes, authenticationBody);
    }

    private PendingPresentationContext[] CreateInitialPresentationContexts(Guid interfaceId)
    {
        List<Guid> interfaceIds = BuildInitialInterfaceList(interfaceId);
        var contexts = new PendingPresentationContext[interfaceIds.Count];
        for (int i = 0; i < contexts.Length; i++)
        {
            int contextId = _nextContextId++;
            contexts[i] = new PendingPresentationContext(interfaceIds[i], CreatePresentationContext(interfaceIds[i], contextId));
        }

        return contexts;
    }

    private List<Guid> BuildInitialInterfaceList(Guid interfaceId)
    {
        var interfaceIds = new List<Guid>(_preBindIids.Length + 1) { interfaceId };
        foreach (Guid preBindIid in _preBindIids)
        {
            if (!interfaceIds.Contains(preBindIid))
            {
                interfaceIds.Add(preBindIid);
            }
        }

        return interfaceIds;
    }

    private static Guid[] NormalizePreBindIids(IReadOnlyList<Guid>? interfaceIds)
    {
        if (interfaceIds is null || interfaceIds.Count == 0)
        {
            return [];
        }

        var normalized = new List<Guid>(interfaceIds.Count);
        for (int i = 0; i < interfaceIds.Count; i++)
        {
            Guid interfaceId = interfaceIds[i];
            if (interfaceId != Guid.Empty && !normalized.Contains(interfaceId))
            {
                normalized.Add(interfaceId);
            }
        }

        return normalized.ToArray();
    }

    private static PresentationContext[] ToPresentationContexts(PendingPresentationContext[] contexts)
    {
        var presentationContexts = new PresentationContext[contexts.Length];
        for (int i = 0; i < contexts.Length; i++)
        {
            presentationContexts[i] = contexts[i].Context;
        }

        return presentationContexts;
    }

    private void RegisterAcceptedPresentationContexts(PendingPresentationContext[] contexts, PresentationResult[] results)
    {
        int count = Math.Min(contexts.Length, results.Length);
        for (int i = 0; i < count; i++)
        {
            if (results[i].Result == PresentationResultCode.ACCEPTANCE)
            {
                _contextIds[contexts[i].InterfaceId] = contexts[i].Context.ContextId;
            }
        }
    }

    private static PresentationResult? FindPresentationResult(
        PendingPresentationContext[] contexts,
        PresentationResult[] results,
        Guid interfaceId)
    {
        int count = Math.Min(contexts.Length, results.Length);
        for (int i = 0; i < count; i++)
        {
            if (contexts[i].InterfaceId == interfaceId)
            {
                return results[i];
            }
        }

        return null;
    }

    private static PresentationResult[] GetPresentationResults(PresentationResult[]? results, string emptyMessage)
    {
        if (results is null || results.Length == 0)
        {
            throw new InvalidOperationException(emptyMessage);
        }

        return results;
    }

    private static void ValidatePresentationResults(
        PresentationResult[]? results,
        string emptyMessage,
        string rejectionMessagePrefix)
    {
        foreach (PresentationResult result in GetPresentationResults(results, emptyMessage))
        {
            if (result.Result != PresentationResultCode.ACCEPTANCE)
            {
                throw new InvalidOperationException($"{rejectionMessagePrefix}: {result}.");
            }
        }
    }

    private static PresentationContext CreatePresentationContext(Guid interfaceId, int contextId) =>
        new(contextId, new PresentationSyntax(new UUID(interfaceId.ToString("D")), 0, 0));

    private static bool IsPacketProtectedPdu(byte pduType) =>
        pduType is RequestCoPdu.REQUEST_TYPE or ResponseCoPdu.RESPONSE_TYPE or FaultCoPdu.FAULT_TYPE;

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static ProtectionLevel ToRpcProtectionLevel(OpcProtectionLevel protectionLevel) => protectionLevel switch
    {
        OpcProtectionLevel.None => ProtectionLevel.PROTECTION_LEVEL_NONE,
        OpcProtectionLevel.Connect => ProtectionLevel.PROTECTION_LEVEL_CONNECT,
        OpcProtectionLevel.Call => ProtectionLevel.PROTECTION_LEVEL_CALL,
        OpcProtectionLevel.Packet => ProtectionLevel.PROTECTION_LEVEL_PACKET,
        OpcProtectionLevel.Integrity => ProtectionLevel.PROTECTION_LEVEL_INTEGRITY,
        OpcProtectionLevel.Privacy => ProtectionLevel.PROTECTION_LEVEL_PRIVACY,
        _ => ProtectionLevel.PROTECTION_LEVEL_NONE,
    };

    private int NextCallId() => _nextCallId++;

    private readonly record struct PendingPresentationContext(Guid InterfaceId, PresentationContext Context);
    private readonly record struct DecodedPdu(ConnectionOrientedPdu Pdu, byte[] AuthenticationBody);
    private readonly record struct AuthenticationStrippedFrame(byte[] PduBytes, byte[] AuthenticationBody);
}


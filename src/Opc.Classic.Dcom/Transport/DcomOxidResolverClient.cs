// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Client helper for IObjectExporter::ResolveOxid2 / ResolveOxid.
/// </summary>
public static class DcomOxidResolverClient
{
    private const int EndpointMapperPort = 135;
    private const int ResolveOxidOpnum = 0;
    private const int ResolveOxid2Opnum = 4;
    private const ushort RpcProtocolSequenceTcp = 0x07;

    /// <summary>
    /// Resolves OXID bindings through IObjectExporter, using ResolveOxid2 with ResolveOxid fallback.
    /// </summary>
    public static async Task<byte[]> ResolveOxidBindingsAsync(
        string fallbackHost,
        ulong oxid,
        ReadOnlyMemory<byte> oxidResolverBindings,
        DcomCallChannelFactory channelFactory,
        IAuthContext authContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(authContext);
        cancellationToken.ThrowIfCancellationRequested();

        if (oxid == 0)
        {
            throw new ArgumentException("OXID must not be zero.", nameof(oxid));
        }

        EndPoint resolverEndpoint = DualStringArrayResolver.ResolveFirstTransport(fallbackHost, oxidResolverBindings.Span)
            ?? throw new InvalidOperationException("Activation did not return an IObjectExporter resolver endpoint.");
        return await ResolveOxidBindingsAsync(
            resolverEndpoint,
            oxid,
            channelFactory,
            authContext,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves OXID bindings through IObjectExporter using OBJREF resolver bindings.
    /// </summary>
    public static async Task<byte[]> ResolveOxidBindingsAsync(
        string fallbackHost,
        ulong oxid,
        IReadOnlyList<ushort> resolverBindings,
        DcomCallChannelFactory channelFactory,
        IAuthContext authContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);
        ArgumentNullException.ThrowIfNull(resolverBindings);
        EndPoint resolverEndpoint = DualStringArrayResolver.ResolveFirstTransport(fallbackHost, resolverBindings)
            ?? new DnsEndPoint(fallbackHost, EndpointMapperPort);
        return await ResolveOxidBindingsAsync(
            resolverEndpoint,
            oxid,
            channelFactory,
            authContext,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task<byte[]> ResolveOxidBindingsAsync(
        EndPoint resolverEndpoint,
        ulong oxid,
        DcomCallChannelFactory channelFactory,
        IAuthContext authContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(authContext);
        cancellationToken.ThrowIfCancellationRequested();

        if (oxid == 0)
        {
            throw new ArgumentException("OXID must not be zero.", nameof(oxid));
        }

        try
        {
            ICallChannel resolverChannel = await channelFactory.ConnectAsync(
                resolverEndpoint,
                Guid.Empty,
                authContext,
                new[] { OpcGuids.IID_IObjectExporter },
                cancellationToken).ConfigureAwait(false);
            return await ResolveOxidBindingsAsync(resolverChannel, oxid, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeAuthContextAsync(authContext).ConfigureAwait(false);
        }
    }

    private static async Task<byte[]> ResolveOxidBindingsAsync(
        ICallChannel resolverChannel,
        ulong oxid,
        CancellationToken cancellationToken)
    {
        try
        {
            if (resolverChannel is not DcomCallChannel rawResolverChannel)
            {
                throw new InvalidOperationException("IObjectExporter::ResolveOxid2 requires a DCE/RPC channel.");
            }

            ReadOnlyMemory<byte> resolvePayload = EncodeResolveOxidRequest(oxid);
            NdrCallResult result = await rawResolverChannel.InvokeRawAsync(
                OpcGuids.IID_IObjectExporter,
                ResolveOxid2Opnum,
                resolvePayload,
                cancellationToken).ConfigureAwait(false);
            if (result.IsFailure && IsProcnumOutOfRange(result.Hresult))
            {
                result = await rawResolverChannel.InvokeRawAsync(
                    OpcGuids.IID_IObjectExporter,
                    ResolveOxidOpnum,
                    resolvePayload,
                    cancellationToken).ConfigureAwait(false);
                return ReadResolveOxidBindings(result, expectComVersion: false);
            }

            return ReadResolveOxidBindings(result, expectComVersion: true);
        }
        finally
        {
            await DisposeChannelAsync(resolverChannel).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns true when a ResolveOxid2 fault means the server only supports ResolveOxid.
    /// </summary>
    public static bool IsProcnumOutOfRange(int hresult) =>
        hresult is 0x000006D1
            or unchecked((int)0x800706D1u)
            or unchecked((int)0x1C010002u)
            or unchecked((int)0xC002002Eu);

    public static byte[] ReadResolveOxidBindings(NdrCallResult result, bool expectComVersion)
    {
        if (result.IsFailure)
        {
            string operation = expectComVersion ? "IObjectExporter::ResolveOxid2" : "IObjectExporter::ResolveOxid";
            throw new InvalidOperationException($"{operation} RPC fault 0x{unchecked((uint)result.Hresult):X8}.");
        }

        byte[] bindings = ReadResolveOxidBindings(result.ResponsePayload.Span, expectComVersion, out _, out int hresult);
        if (hresult < 0)
        {
            string operation = expectComVersion ? "IObjectExporter::ResolveOxid2" : "IObjectExporter::ResolveOxid";
            throw new InvalidOperationException($"{operation} returned HRESULT 0x{unchecked((uint)hresult):X8}.");
        }

        return bindings;
    }

    public static byte[] ReadResolveOxidBindings(
        ReadOnlySpan<byte> payload,
        bool expectComVersion,
        out Guid remUnknownIpid,
        out int hresult)
    {
        var reader = new NdrReader(payload);
        if (!reader.TryReadReferentId(out uint dsaReferentId) || dsaReferentId == 0)
        {
            throw new InvalidOperationException("IObjectExporter returned a NULL DUALSTRINGARRAY pointer.");
        }

        uint maxCount = reader.ReadUInt32();
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        if (maxCount < entryCount)
        {
            throw new InvalidOperationException("IObjectExporter returned an invalid DUALSTRINGARRAY conformance count.");
        }

        var bindings = new byte[checked(4 + (entryCount * sizeof(ushort)))];
        BinaryPrimitives.WriteUInt16LittleEndian(bindings, entryCount);
        BinaryPrimitives.WriteUInt16LittleEndian(bindings.AsSpan(2), securityOffset);
        for (int i = 0; i < entryCount; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bindings.AsSpan(4 + i * sizeof(ushort)), reader.ReadUInt16());
        }

        reader.AlignTo(4);
        remUnknownIpid = reader.ReadGuid();
        _ = reader.ReadUInt32();
        if (expectComVersion)
        {
            _ = reader.ReadUInt16();
            _ = reader.ReadUInt16();
        }

        hresult = reader.ReadInt32();
        return bindings;
    }

    private static byte[] EncodeResolveOxidRequest(ulong oxid) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt64(oxid);
        writer.WriteUInt16(1);
        writer.AlignTo(4);
        writer.WriteConformanceHeader(1);
        writer.WriteUInt16(RpcProtocolSequenceTcp);
        writer.AlignTo(4);
    });

    private static byte[] WritePayload(NdrWriteAction action)
    {
        for (int size = 64; size <= 4096; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < 4096)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the ResolveOxid payload.");
    }

    private static async ValueTask DisposeAuthContextAsync(IAuthContext authContext)
    {
        if (ReferenceEquals(authContext, NoOpAuthContext.Instance))
        {
            return;
        }

        if (authContext is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (authContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static async ValueTask DisposeChannelAsync(ICallChannel channel)
    {
        if (channel is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (channel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}

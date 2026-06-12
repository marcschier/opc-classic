//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707, MA0048 // OPC IDL naming preserved; grouped internal proxy types share this file

using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Dcom;

internal sealed class IOPCServerListClientProxy
{
    private static readonly Guid InterfaceId = OpcGuids.IID_IOPCServerList;
    private readonly ICallChannel _channel;

    public IOPCServerListClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        Guid[] implementedCategories,
        Guid[] requiredCategories,
        CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            InterfaceId,
            opnum: 3,
            OpcEnumProxyCodec.EncodeCategoryRequest(implementedCategories, requiredCategories),
            "IOPCServerList::EnumClassesOfCategories",
            cancellationToken);

    public async Task<OpcServerListClassDetails> GetClassDetailsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(classId));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            opnum: 4,
            payload,
            "IOPCServerList::GetClassDetails",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return new OpcServerListClassDetails(
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr(),
            null);
    }
}

internal sealed class IOPCServerList2ClientProxy
{
    private static readonly Guid InterfaceId = OpcGuids.IID_IOPCServerList2;
    private readonly ICallChannel _channel;

    public IOPCServerList2ClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        Guid[] implementedCategories,
        Guid[] requiredCategories,
        CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            InterfaceId,
            opnum: 3,
            OpcEnumProxyCodec.EncodeCategoryRequest(implementedCategories, requiredCategories),
            "IOPCServerList2::EnumClassesOfCategories",
            cancellationToken);

    public async Task<OpcServerListClassDetails> GetClassDetailsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(classId));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            opnum: 4,
            payload,
            "IOPCServerList2::GetClassDetails",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        return new OpcServerListClassDetails(
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr(),
            reader.ReadUnicodeStringPtr());
    }
}

/// <summary>Managed proxy for OPC Common <c>IOPCEnumGUID</c> enumerators returned by OPCEnum.</summary>
public sealed class IOPCEnumGUIDClientProxy
{
    /// <summary>OPC Common <c>IOPCEnumGUID</c> interface identifier.</summary>
    public static readonly Guid InterfaceId = OpcGuids.IID_IOPCEnumGUID;

    private readonly ICallChannel _channel;

    /// <summary>Initializes a new instance of the <see cref="IOPCEnumGUIDClientProxy" /> class.</summary>
    public IOPCEnumGUIDClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    /// <summary>OPC Common <c>IOPCEnumGUID</c> DCE/RPC operation numbers.</summary>
    public static class Opnums
    {
        /// <summary><c>IOPCEnumGUID::Next</c> operation number.</summary>
        public const int Next = 3;

        /// <summary><c>IOPCEnumGUID::Skip</c> operation number.</summary>
        public const int Skip = 4;

        /// <summary><c>IOPCEnumGUID::Reset</c> operation number.</summary>
        public const int Reset = 5;

        /// <summary><c>IOPCEnumGUID::Clone</c> operation number.</summary>
        public const int Clone = 6;
    }

    /// <summary>Fetches up to <paramref name="count" /> GUIDs from the enumerator.</summary>
    public async Task<OpcEnumGuidNextResult> NextAsync(int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteInt32(count));
        NdrCallResult result = await OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            Opnums.Next,
            payload,
            "IOPCEnumGUID::Next",
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(result.ResponsePayload.Span);
        Guid[] classIds = reader.ReadVaryingConformantGuidArray();
        int fetched = reader.ReadInt32();
        return new OpcEnumGuidNextResult(classIds, fetched);
    }

    /// <summary>Skips <paramref name="count" /> GUIDs in the enumerator.</summary>
    public Task SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        ReadOnlyMemory<byte> payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteInt32(count));
        return OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            Opnums.Skip,
            payload,
            "IOPCEnumGUID::Skip",
            cancellationToken);
    }

    /// <summary>Resets the enumerator to its first GUID.</summary>
    public Task ResetAsync(CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeAsync(
            _channel,
            InterfaceId,
            Opnums.Reset,
            ReadOnlyMemory<byte>.Empty,
            "IOPCEnumGUID::Reset",
            cancellationToken);

    /// <summary>Clones the enumerator at its current position.</summary>
    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default) =>
        OpcEnumProxyCodec.InvokeInterfaceRefAsync(
            _channel,
            InterfaceId,
            Opnums.Clone,
            ReadOnlyMemory<byte>.Empty,
            "IOPCEnumGUID::Clone",
            cancellationToken);
}

/// <summary>Server implementation contract for OPC Common <c>IOPCEnumGUID</c>.</summary>
public interface IOPCEnumGUIDServer
{
    /// <summary>Fetches up to <paramref name="count" /> GUIDs from the enumerator.</summary>
    Task<OpcEnumGuidNextResult> NextAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Skips up to <paramref name="count" /> GUIDs and returns the number actually skipped.</summary>
    Task<int> SkipAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Resets the enumerator to its first GUID.</summary>
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>Clones the enumerator at its current position.</summary>
    Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default);
}

/// <summary>Server-side dispatcher for OPC Common <c>IOPCEnumGUID</c>.</summary>
public sealed class IOPCEnumGUIDServerDispatcher : IOpcServerDispatcher
{
    private readonly IOPCEnumGUIDServer _server;

    /// <summary>Initializes a new instance of the <see cref="IOPCEnumGUIDServerDispatcher" /> class.</summary>
    public IOPCEnumGUIDServerDispatcher(IOPCEnumGUIDServer server) =>
        _server = server ?? throw new ArgumentNullException(nameof(server));

    /// <inheritdoc />
    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return opnum switch
            {
                IOPCEnumGUIDClientProxy.Opnums.Next => await DispatchNextAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IOPCEnumGUIDClientProxy.Opnums.Skip => await DispatchSkipAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                IOPCEnumGUIDClientProxy.Opnums.Reset => await DispatchResetAsync(cancellationToken).ConfigureAwait(false),
                IOPCEnumGUIDClientProxy.Opnums.Clone => await DispatchCloneAsync(cancellationToken).ConfigureAwait(false),
                _ => DispatchResult.NotImplemented(opnum),
            };
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }

    private async ValueTask<DispatchResult> DispatchNextAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int count = reader.ReadInt32();
        OpcEnumGuidNextResult next = await _server.NextAsync(count, cancellationToken).ConfigureAwait(false);
        byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteConformantGuidArray(next.ClassIds);
            writer.WriteInt32(next.Fetched);
        });
        int hresult = next.Fetched < count ? OpcResultId.False.Code : OpcResultId.Ok.Code;
        return DispatchResult.Success(payload, hresult);
    }

    private async ValueTask<DispatchResult> DispatchSkipAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int count = reader.ReadInt32();
        int skipped = await _server.SkipAsync(count, cancellationToken).ConfigureAwait(false);
        int hresult = skipped < count ? OpcResultId.False.Code : OpcResultId.Ok.Code;
        return DispatchResult.Success(Array.Empty<byte>(), hresult);
    }

    private async ValueTask<DispatchResult> DispatchResetAsync(CancellationToken cancellationToken)
    {
        await _server.ResetAsync(cancellationToken).ConfigureAwait(false);
        return DispatchResult.Success(Array.Empty<byte>());
    }

    private async ValueTask<DispatchResult> DispatchCloneAsync(CancellationToken cancellationToken)
    {
        IOpcInterfaceRef clone = await _server.CloneAsync(cancellationToken).ConfigureAwait(false);
        byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) =>
            OpcInterfaceRefCodec.Write(ref writer, clone));
        return DispatchResult.Success(payload);
    }
}

internal sealed record OpcServerListClassDetails(string? ProgId, string? UserType, string? VerIndProgId);

/// <summary>Result returned by <c>IOPCEnumGUID::Next</c>.</summary>
/// <param name="ClassIds">The GUIDs returned by the enumerator.</param>
/// <param name="Fetched">The number of valid entries returned in <paramref name="ClassIds" />.</param>
public sealed record OpcEnumGuidNextResult(Guid[] ClassIds, int Fetched);

internal static class OpcEnumProxyCodec
{
    private const int DefaultPayloadSize = 1024;
    private const int MaximumPayloadSize = 65536;

    internal delegate void NdrWriteAction(ref NdrWriter writer);

    public static ReadOnlyMemory<byte> EncodeCategoryRequest(Guid[] implementedCategories, Guid[] requiredCategories) =>
        WritePayload((ref NdrWriter writer) =>
        {
            // IDL: [in] ULONG cImplemented, [in, size_is(cImplemented)] CATID rgcatidImpl[],
            //      [in] ULONG cRequired,    [in, size_is(cRequired)] CATID rgcatidReq[]
            // Per DCE/RPC §14.3.4 the size_is parameter is emitted independently AND
            // also as the conformant-array max_count prefix; both must be present.
            Guid[] impl = implementedCategories ?? Array.Empty<Guid>();
            Guid[] req = requiredCategories ?? Array.Empty<Guid>();
            writer.WriteUInt32((uint)impl.Length);
            writer.WriteConformantGuidArray(impl);
            writer.WriteUInt32((uint)req.Length);
            writer.WriteConformantGuidArray(req);
        });

    public static async Task<IOpcInterfaceRef> InvokeInterfaceRefAsync(
        ICallChannel channel,
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await InvokeAsync(channel, interfaceId, opnum, payload, operationDescription, cancellationToken)
            .ConfigureAwait(false);
        return DecodeInterfaceRefResponse(result.ResponsePayload.Span, operationDescription);
    }

    /// <summary>
    /// Decodes a <c>[out] IUnknown**</c> response payload wrapped in
    /// <c>MInterfacePointer</c> per MS-DCOM 2.2.18.7 (pointer referent +
    /// ulCntData + conformant max_count + OBJREF bytes) and returns the
    /// embedded OBJREF as an <see cref="IOpcInterfaceRef"/>.
    /// </summary>
    /// <remarks>
    /// The IDL <c>[out] IFoo** ppFoo</c> emits an outer 4-byte pointer
    /// referent on the wire (non-zero indicates a non-null result) followed
    /// by a marshaled <c>MInterfacePointer</c> structure containing the
    /// element count, conformant array max_count, and the OBJREF byte stream
    /// (MEOW + STDOBJREF + DUALSTRINGARRAY). A bare OBJREF without the
    /// wrapper is detected by the MEOW signature (0x574F454D) appearing in
    /// the first four bytes — fallback path preserved for direct-OBJREF
    /// activation responses.
    /// </remarks>
    public static IOpcInterfaceRef DecodeInterfaceRefResponse(ReadOnlySpan<byte> responsePayload, string operationDescription)
    {
        if (responsePayload.Length < sizeof(uint))
        {
            throw new InvalidOperationException($"{operationDescription} returned an empty payload.");
        }

        uint firstWord = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(responsePayload);
        if (firstWord == 0x574F454D)
        {
            var bareReader = new NdrReader(responsePayload);
            return OpcInterfaceRefCodec.Read(ref bareReader);
        }

        // Wrapped MInterfacePointer: referent (4) + ulCntData (4) + max_count (4) + OBJREF bytes.
        var reader = new NdrReader(responsePayload);
        _ = reader.ReadUInt32();
        uint ulCntData = reader.ReadUInt32();
        uint conformantMaxCount = reader.ReadUInt32();
        if (conformantMaxCount < ulCntData)
        {
            throw new InvalidOperationException($"{operationDescription} returned a malformed MInterfacePointer (max_count {conformantMaxCount} less than ulCntData {ulCntData}).");
        }

        if (ulCntData == 0)
        {
            throw new InvalidOperationException($"{operationDescription} returned an empty MInterfacePointer.");
        }

        ReadOnlySpan<byte> objRefSpan = reader.ReadRawBytes((int)ulCntData);
        var objRefReader = new NdrReader(objRefSpan);
        return OpcInterfaceRefCodec.Read(ref objRefReader);
    }

    public static async Task<NdrCallResult> InvokeAsync(
        ICallChannel channel,
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await channel.InvokeAsync(interfaceId, opnum, payload, cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), operationDescription);
        return result;
    }

    public static byte[] WritePayload(NdrWriteAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        for (int size = DefaultPayloadSize; size <= MaximumPayloadSize; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < MaximumPayloadSize)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the OPCEnum DCOM payload.");
    }
}

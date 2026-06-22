// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>
/// Managed proxy for the OPC Common <c>IOPCCommon</c> debug/metadata methods.
/// </summary>
public sealed class OpcCommonClientProxy
{
    private const int DefaultPayloadSize = 256;
    private const int MaximumPayloadSize = 8192;

    private readonly ICallChannel _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcCommonClientProxy" /> class.
    /// </summary>
    public OpcCommonClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    /// <summary>
    /// OPC Common <c>IOPCCommon</c> interface identifier.
    /// </summary>
    public static Guid InterfaceId => OpcGuids.IID_IOPCCommon;

    /// <summary>
    /// OPC Common <c>IOPCCommon</c> DCE/RPC operation numbers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1034:Nested types should not be visible",
        Justification = "Opnums is the conventional nested constants table for proxy classes across the Opc.Classic.* code base and matches the shape emitted by OpcInterfaceGenerator.")]
    public static class Opnums
    {
        /// <summary>
        /// <c>IOPCCommon::SetLocaleID</c> operation number.
        /// </summary>
        public const int SetLocaleId = 3;

        /// <summary>
        /// <c>IOPCCommon::GetLocaleID</c> operation number.
        /// </summary>
        public const int GetLocaleId = 4;

        /// <summary>
        /// <c>IOPCCommon::QueryAvailableLocaleIDs</c> operation number.
        /// </summary>
        public const int QueryAvailableLocaleIds = 5;

        /// <summary>
        /// <c>IOPCCommon::GetErrorString</c> operation number.
        /// </summary>
        public const int GetErrorString = 6;

        /// <summary>
        /// <c>IOPCCommon::SetClientName</c> operation number.
        /// </summary>
        public const int SetClientName = 7;
    }

    /// <summary>
    /// Sets the locale used for subsequent localized server strings.
    /// </summary>
    public async Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default)
    {
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(localeId));
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.SetLocaleId,
            payload,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::SetLocaleID");
    }

    /// <summary>
    /// Gets the current locale used for localized server strings.
    /// </summary>
    public async Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.GetLocaleId,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::GetLocaleID");
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadInt32();
    }

    /// <summary>
    /// Lists locale IDs supported by the server.
    /// </summary>
    public async Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default)
    {
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.QueryAvailableLocaleIds,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::QueryAvailableLocaleIDs");
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadConformantInt32Array();
    }

    /// <summary>
    /// Resolves an HRESULT to localized server text.
    /// </summary>
    public async Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default)
    {
        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(errorCode));
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.GetErrorString,
            payload,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::GetErrorString");
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadUnicodeStringPtr() ?? string.Empty;
    }

    /// <summary>
    /// Sets the optional client name that servers may use for diagnostics and debugging metadata.
    /// </summary>
    public async Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);

        byte[] payload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(clientName));
        NdrCallResult result = await _channel.InvokeAsync(
            InterfaceId,
            Opnums.SetClientName,
            payload,
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IOPCCommon::SetClientName");
    }

    internal static byte[] WritePayload(NdrWriteAction action)
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

        throw new InvalidOperationException("Unable to encode the IOPCCommon DCOM payload.");
    }

    internal delegate void NdrWriteAction(ref NdrWriter writer);
}

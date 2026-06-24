// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Dcom;

/// <summary>
/// Server-side dispatcher for OPC Common <c>IOPCServerList2</c>.
/// </summary>
public sealed class IOPCServerList2ServerDispatcher : IOpcServerDispatcher
{
    private readonly OpcEnumServer _server;
    private readonly IOPCServerListServerDispatcher _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="IOPCServerList2ServerDispatcher" /> class.
    /// </summary>
    public IOPCServerList2ServerDispatcher(OpcEnumServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _inner = new IOPCServerListServerDispatcher(server);
    }

    /// <inheritdoc />
    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        if (opnum == 3)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<DispatchResult>(_inner.DispatchEnumClassesOfCategories(requestPayload, OpcGuids.IID_IOPCEnumGUID));
        }

        if (opnum == 4)
        {
            return DispatchGetClassDetailsAsync(requestPayload, cancellationToken);
        }

        return _inner.DispatchAsync(opnum, requestPayload, cancellationToken);
    }

    private ValueTask<DispatchResult> DispatchGetClassDetailsAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var reader = new NdrReader(requestPayload.Span);
            OpcEnumClassDetails details = _server.GetClassDetails(reader.ReadGuid());
            byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteUnicodeStringPtr(details.ProgId);
                writer.WriteUnicodeStringPtr(details.UserType);
                writer.WriteUnicodeStringPtr(details.VersionIndependentProgId);
            });
            return new ValueTask<DispatchResult>(DispatchResult.Success(payload));
        }
        catch (OpcException exception)
        {
            return new ValueTask<DispatchResult>(DispatchResult.Fault(exception.ResultId.Code));
        }
    }
}

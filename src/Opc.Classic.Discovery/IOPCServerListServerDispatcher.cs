// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Dcom;

/// <summary>
/// Server-side dispatcher for OPC Common <c>IOPCServerList</c>.
/// </summary>
public sealed class IOPCServerListServerDispatcher : IOpcServerDispatcher
{
    private readonly OpcEnumServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="IOPCServerListServerDispatcher" /> class.
    /// </summary>
    public IOPCServerListServerDispatcher(OpcEnumServer server) =>
        _server = server ?? throw new ArgumentNullException(nameof(server));

    /// <inheritdoc />
    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return new ValueTask<DispatchResult>(opnum switch
            {
                3 => DispatchEnumClassesOfCategories(requestPayload, OpcGuids.IID_IEnumGUID),
                4 => DispatchGetClassDetails(requestPayload, includeVersionIndependentProgId: false),
                5 => DispatchClsidFromProgId(requestPayload),
                _ => DispatchResult.NotImplemented(opnum),
            });
        }
        catch (OpcException exception)
        {
            return new ValueTask<DispatchResult>(DispatchResult.Fault(exception.ResultId.Code));
        }
    }
    internal DispatchResult DispatchEnumClassesOfCategories(ReadOnlyMemory<byte> requestPayload, Guid enumeratorIid)
    {
        DecodeCategoryRequest(requestPayload, out Guid[] implementedCategories, out Guid[] requiredCategories);
        IOpcInterfaceRef interfaceRef = _server.EnumClassesOfCategories(implementedCategories, requiredCategories, enumeratorIid);
        byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => OpcInterfaceRefCodec.Write(ref writer, interfaceRef));
        return DispatchResult.Success(payload);
    }

    private DispatchResult DispatchGetClassDetails(ReadOnlyMemory<byte> requestPayload, bool includeVersionIndependentProgId)
    {
        var reader = new NdrReader(requestPayload.Span);
        OpcEnumClassDetails details = _server.GetClassDetails(reader.ReadGuid());
        byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUnicodeStringPtr(details.ProgId);
            writer.WriteUnicodeStringPtr(details.UserType);
            if (includeVersionIndependentProgId)
            {
                writer.WriteUnicodeStringPtr(details.VersionIndependentProgId);
            }
        });
        return DispatchResult.Success(payload);
    }

    private DispatchResult DispatchClsidFromProgId(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        Guid clsid = _server.ClsidFromProgId(reader.ReadUnicodeStringPtr() ?? string.Empty);
        byte[] payload = OpcEnumProxyCodec.WritePayload((ref NdrWriter writer) => writer.WriteGuid(clsid));
        return DispatchResult.Success(payload);
    }

    private static void DecodeCategoryRequest(
        ReadOnlyMemory<byte> requestPayload,
        out Guid[] implementedCategories,
        out Guid[] requiredCategories)
    {
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadUInt32();
        implementedCategories = reader.ReadConformantGuidArray();
        _ = reader.ReadUInt32();
        requiredCategories = reader.ReadConformantGuidArray();
    }
}

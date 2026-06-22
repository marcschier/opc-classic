// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Wraps the source-generated <c>IOPCEventServerServerDispatcher</c> so that
/// <c>IOPCEventServer::CreateEventSubscription</c> (opnum 4) is dispatched
/// manually for managed listeners whose backing server implements
/// <see cref="IAeServer"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <c>OpcServerDispatchGenerator</c> emits a hardcoded
/// <c>DispatchResult.NotImplemented(4)</c> for opnum 4 because its
/// <c>CanWriteType</c> predicate rejects the <c>out IOPCEventSubscriptionMgt</c>
/// interface tearoff parameter. This interceptor synthesizes the OBJREF wire
/// response the generated client proxy already knows how to decode (via
/// <c>OpcSubProxyHelper.RegisterAndYieldChannel</c>), allocates a fresh IPID
/// for the new subscription with the per-host <see cref="OpcObjectRegistry"/>,
/// and registers an <c>IOPCEventSubscriptionMgtServerDispatcher</c> bound to
/// the adapter so subsequent client calls on <c>IOPCEventSubscriptionMgt</c>
/// route through the OBJREF's IPID to the per-subscription dispatcher.
/// </para>
/// <para>
/// All other opnums are forwarded verbatim to the underlying source-generated
/// dispatcher (which itself routes through <see cref="IAeServerToOpcAeServerAdapter"/>
/// for query / enable / disable methods).
/// </para>
/// <para>
/// The Windows CCW path is unaffected: <c>OpcAeServerCcw</c> dispatches via
/// <see cref="OpcAeServerDispatcher"/>, which already has its own
/// <see cref="IAeServer"/> fallback for <c>CreateEventSubscription</c> and
/// never invokes this interceptor.
/// </para>
/// </remarks>
internal sealed class AeEventServerDispatcherInterceptor : IOpcServerDispatcher
{
    private const int CreateEventSubscriptionOpnum = 4;

    private static readonly Action<ILogger, Guid, int, int, Exception?> SubscriptionRegistered = LoggerMessage.Define<Guid, int, int>(
        LogLevel.Debug,
        new EventId(1, nameof(SubscriptionRegistered)),
        "AeEventServerDispatcherInterceptor: registered subscription tearoff IPID={Ipid} bufferTime={BufferTime} maxSize={MaxSize}");

    private readonly IOpcServerDispatcher _inner;
    private readonly IAeServer _aeServer;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly ILogger _logger;

    public AeEventServerDispatcherInterceptor(
        IOpcServerDispatcher inner,
        IAeServer aeServer,
        OpcObjectRegistry objectRegistry,
        ILogger logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _aeServer = aeServer ?? throw new ArgumentNullException(nameof(aeServer));
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        opnum == CreateEventSubscriptionOpnum
            ? DispatchCreateEventSubscriptionAsync(requestPayload, cancellationToken)
            : _inner.DispatchAsync(opnum, requestPayload, cancellationToken);

    private async ValueTask<DispatchResult> DispatchCreateEventSubscriptionAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (!TryDecodeCreateEventSubscriptionRequest(requestPayload.Span, out CreateRequest request))
        {
            return DispatchResult.Fault(OpcResultId.InvalidArg.Code);
        }

        try
        {
            SubscriptionRegistration? registration = await CreateAndRegisterSubscriptionAsync(request, cancellationToken).ConfigureAwait(false);
            return registration is { } registered
                ? EncodeCreateEventSubscriptionResponse(request, registered)
                : DispatchResult.Fault(OpcResultId.Fail.Code);
        }
        catch (OpcException ex)
        {
            return DispatchResult.Fault(ex.ResultId.Code);
        }
    }

    private static bool TryDecodeCreateEventSubscriptionRequest(ReadOnlySpan<byte> payload, out CreateRequest request)
    {
        try
        {
            var reader = new NdrReader(payload);
            request = new CreateRequest(
                Active: reader.ReadInt32() != 0,
                BufferTime: reader.ReadInt32(),
                MaxSize: reader.ReadInt32(),
                ClientSubscription: reader.ReadInt32(),
                RequestedInterfaceId: reader.ReadGuid());
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or IndexOutOfRangeException or InvalidOperationException)
        {
            request = default;
            return false;
        }
    }

    private async ValueTask<SubscriptionRegistration?> CreateAndRegisterSubscriptionAsync(
        CreateRequest request,
        CancellationToken cancellationToken)
    {
        IAeSubscription aeSubscription;
        try
        {
            aeSubscription = await _aeServer
                .CreateSubscriptionAsync(request.Active, request.BufferTime, request.MaxSize, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OpcException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
#pragma warning disable CA1031
        catch
        {
            return null;
        }
#pragma warning restore CA1031

        IOPCEventSubscriptionMgt subscriptionMgt = OpcAeServerDispatcher.CreateEventSubscriptionAdapter(
            aeSubscription, request.BufferTime, request.MaxSize, request.ClientSubscription);

        // Register the new subscription tearoff under a fresh IPID. The
        // generated client proxy decodes the OBJREF, registers the
        // (IID, IPID) route on the channel via OpcSubProxyHelper, and
        // builds an IOPCEventSubscriptionMgtClientProxy bound to that IPID.
        // Subsequent client calls on IOPCEventSubscriptionMgt then route
        // through RpcServerConnectionProcessor's object-registry lookup to
        // the dispatcher we register here.
        var subscriptionDispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCEventSubscriptionMgt.InterfaceId] = new IOPCEventSubscriptionMgtServerDispatcher(subscriptionMgt),
        };
        Guid ipid = _objectRegistry.Register(subscriptionDispatchers);
        SubscriptionRegistered(_logger, ipid, request.BufferTime, request.MaxSize, null);
        return new SubscriptionRegistration(ipid);
    }

    private static DispatchResult EncodeCreateEventSubscriptionResponse(
        CreateRequest request, SubscriptionRegistration registration)
    {
        // Synthesize an OBJREF for the new subscription tearoff. Mirrors the
        // DA default-impl pattern (IOpcDaServer.CreateSyntheticInterfaceRef):
        // OXID and OID are placeholders because the tearoff lives on the
        // existing call channel; only the IID + IPID matter for routing.
        var interfaceRef = new OpcInterfaceRef(
            iid: request.RequestedInterfaceId == Guid.Empty ? IOPCEventSubscriptionMgt.InterfaceId : request.RequestedInterfaceId,
            flags: 0u,
            publicRefs: 1u,
            oxid: 1ul,
            oid: 0ul,
            ipid: registration.Ipid,
            securityOffset: 0,
            resolverBindings: []);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(InitialResponseBufferSize);
        try
        {
            var writer = new NdrWriter(new Span<byte>(buffer, 0, InitialResponseBufferSize));
            OpcMInterfacePointerCodec.Write(ref writer, interfaceRef);
            writer.WriteInt32(request.BufferTime);
            writer.WriteInt32(request.MaxSize);
            return DispatchResult.Success(
                new ReadOnlySpan<byte>(buffer, 0, writer.Position).ToArray(),
                OpcResultId.Ok.Code);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private const int InitialResponseBufferSize = 1024;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct CreateRequest(
        bool Active,
        int BufferTime,
        int MaxSize,
        int ClientSubscription,
        Guid RequestedInterfaceId);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct SubscriptionRegistration(Guid Ipid);
}

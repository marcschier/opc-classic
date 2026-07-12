// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Server-side dispatcher for modern <c>IRemoteSCMActivator</c> opnums 3 and 4.
/// </summary>
public sealed class RemoteSCMActivatorDispatcher : IRpcRequestContextDispatcher
{
    private const int RemoteGetClassObjectOpnum = 3;
    private const int RemoteCreateInstanceOpnum = 4;
    private const int EInvalidArg = unchecked((int)0x80070057u);
    private const OpcProtectionLevel RequiredActivationProtectionLevel = OpcProtectionLevel.Integrity;

    private readonly IRemoteSCMActivatorServer _activator;
    private readonly ILogger _logger;

    public RemoteSCMActivatorDispatcher(IRemoteSCMActivatorServer activator, ILogger? logger = null)
    {
        _activator = activator ?? throw new ArgumentNullException(nameof(activator));
        _logger = logger ?? NullLogger.Instance;
    }

    public static Guid InterfaceId { get; } = Guid.Parse(Interfaces.IID_IRemoteSCMActivator);

    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(opnum, requestPayload, isAuthenticated: false, OpcProtectionLevel.None, cancellationToken);

    ValueTask<DispatchResult> IRpcRequestContextDispatcher.DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        RpcRequestContext requestContext,
        CancellationToken cancellationToken) =>
        DispatchAsync(opnum, requestPayload, requestContext.IsAuthenticated, requestContext.ProtectionLevel, cancellationToken);

    private async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        bool isAuthenticated,
        OpcProtectionLevel protectionLevel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (opnum is not RemoteGetClassObjectOpnum and not RemoteCreateInstanceOpnum)
        {
            return DispatchResult.NotImplemented(opnum);
        }

        if (!isAuthenticated || protectionLevel < RequiredActivationProtectionLevel)
        {
            ModernActivationRejected(_logger, protectionLevel, null);
            return DispatchResult.Fault(global::Opc.Classic.OpcResultId.AccessDenied.Code);
        }

        try
        {
            if (opnum == RemoteCreateInstanceOpnum)
            {
                DecodedCreateInstanceRequest decoded = DecodeCreateInstanceRequest(requestPayload.Span);
                RemoteCreateInstanceResponse response = await _activator.RemoteCreateInstanceAsync(decoded.Request, cancellationToken).ConfigureAwait(false);
                return DispatchResult.Success(
                    decoded.IsModern
                        ? EncodeModernCreateInstanceResponse(response, decoded.Request)
                        : EncodeResponse(response.Hresult, response.EncodedActivationProperties, response.ObjRef));
            }
            else
            {
                RemoteGetClassObjectRequest request = DecodeGetClassObjectRequest(requestPayload.Span);
                RemoteGetClassObjectResponse response = await _activator.RemoteGetClassObjectAsync(request, cancellationToken).ConfigureAwait(false);
                return DispatchResult.Success(EncodeResponse(response.Hresult, response.EncodedActivationProperties, response.ObjRef));
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OverflowException)
        {
            ModernActivationMalformed(_logger, ex);
            return DispatchResult.Fault(EInvalidArg);
        }
    }

    private static DecodedCreateInstanceRequest DecodeCreateInstanceRequest(ReadOnlySpan<byte> payload)
    {
        if (ActivationPropertiesCodec.TryDecodeRemoteCreateInstanceRequest(payload, out RemoteCreateInstanceActivationRequest activationRequest))
        {
            int[] protocolSequences = new int[activationRequest.RequestedProtocolSequences.Count];
            for (int i = 0; i < protocolSequences.Length; i++)
            {
                protocolSequences[i] = activationRequest.RequestedProtocolSequences[i];
            }

            Guid requestedIid = activationRequest.RequestedIids.Count == 0 ? Guid.Empty : activationRequest.RequestedIids[0];
            return new DecodedCreateInstanceRequest(new RemoteCreateInstanceRequest(activationRequest.ClassId, requestedIid, protocolSequences)
            {
                RequestedIids = activationRequest.RequestedIids,
                RawActivationProperties = activationRequest.ActivationPropertiesBlob,
            }, IsModern: true);
        }

        DecodedRequest decoded = DecodeRequest(payload);
        return new DecodedCreateInstanceRequest(new RemoteCreateInstanceRequest(decoded.Clsid, decoded.RequestedIid, decoded.ProtocolSequences)
        {
            RawActivationProperties = decoded.ActivationProperties,
        }, IsModern: false);
    }

    private static RemoteGetClassObjectRequest DecodeGetClassObjectRequest(ReadOnlySpan<byte> payload)
    {
        DecodedRequest decoded = DecodeRequest(payload);
        return new RemoteGetClassObjectRequest(decoded.Clsid, decoded.RequestedIid, decoded.ProtocolSequences)
        {
            RawActivationProperties = decoded.ActivationProperties,
        };
    }

    private static DecodedRequest DecodeRequest(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        Guid clsid = reader.ReadGuid();
        Guid requestedIid = reader.ReadGuid();
        uint count = reader.ReadUInt32();
        if (count > 0x8000)
        {
            throw new InvalidOperationException("IRemoteSCMActivator protocol-sequence count is invalid.");
        }

        var protocolSequences = new int[count];
        for (int i = 0; i < protocolSequences.Length; i++)
        {
            protocolSequences[i] = reader.ReadInt32();
        }

        uint propertiesLength = reader.RemainingBytes >= sizeof(uint) ? reader.ReadUInt32() : 0;
        if (propertiesLength > reader.RemainingBytes)
        {
            throw new InvalidOperationException("IRemoteSCMActivator activation-properties length exceeds the payload.");
        }

        byte[] properties = propertiesLength == 0
            ? Array.Empty<byte>()
            : reader.ReadRawBytes(checked((int)propertiesLength)).ToArray();
        return new DecodedRequest(clsid, requestedIid, protocolSequences, properties);
    }

    private static byte[] EncodeResponse(int hresult, byte[] encodedActivationProperties, byte[] objRef)
    {
        if (encodedActivationProperties.Length > 0)
        {
            return encodedActivationProperties;
        }

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(hresult);
            writer.WriteUInt32(unchecked((uint)objRef.Length));
            writer.WriteRawBytes(objRef);
        });
    }

    private static byte[] EncodeModernCreateInstanceResponse(RemoteCreateInstanceResponse response, RemoteCreateInstanceRequest request)
    {
        IReadOnlyList<ActivationInterfaceResult> interfaceResults = response.InterfaceResults.Count == 0
            ? CreateInterfaceResultsFromLegacyResponse(request, response)
            : response.InterfaceResults;
        return ActivationPropertiesCodec.EncodeRemoteCreateInstanceResponse(
            response.OxidValue,
            response.OxidBindings,
            response.IpidRemUnknown == Guid.Empty ? response.Ipid : response.IpidRemUnknown,
            authnHint: 1,
            serverVersion: (5, 7),
            interfaceResults,
            response.Hresult);
    }

    private static ActivationInterfaceResult[] CreateInterfaceResultsFromLegacyResponse(RemoteCreateInstanceRequest request, RemoteCreateInstanceResponse response)
    {
        IReadOnlyList<Guid> requestedIids = request.RequestedIids.Count == 0
            ? new[] { request.RequestedIid == Guid.Empty ? Guid.Parse(Interfaces.IID_IUnknown) : request.RequestedIid }
            : request.RequestedIids;
        var results = new ActivationInterfaceResult[requestedIids.Count];
        for (int i = 0; i < results.Length; i++)
        {
            bool primarySuccess = response.Hresult == 0 && response.ObjRef.Length > 0 && i == 0;
            int hresult = response.Hresult != 0
                ? response.Hresult
                : primarySuccess ? 0 : RemoteSCMActivatorServer.E_NOINTERFACE;
            results[i] = new ActivationInterfaceResult(
                requestedIids[i],
                hresult,
                primarySuccess ? response.ObjRef : Array.Empty<byte>());
        }

        return results;
    }

    private static byte[] WritePayload(NdrWriteAction action)
    {
        for (int size = 256; size <= 1024 * 1024; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer.AsSpan(0, writer.Position).ToArray();
            }
            catch (InvalidOperationException) when (size < 1024 * 1024)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the IRemoteSCMActivator response.");
    }

    private static readonly Action<ILogger, OpcProtectionLevel, Exception?> ModernActivationRejected =
        LoggerMessage.Define<OpcProtectionLevel>(
            LogLevel.Warning,
            new EventId(1, nameof(ModernActivationRejected)),
            "IRemoteSCMActivator rejected because RPC authentication is missing or level {ProtectionLevel} is below packet integrity.");

    private static readonly Action<ILogger, Exception?> ModernActivationMalformed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(ModernActivationMalformed)),
            "IRemoteSCMActivator request body was malformed.");

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed record DecodedRequest(Guid Clsid, Guid RequestedIid, int[] ProtocolSequences, byte[] ActivationProperties);

    private sealed record DecodedCreateInstanceRequest(RemoteCreateInstanceRequest Request, bool IsModern);
}

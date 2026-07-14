// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery;

/// <summary>
/// OPCEnum (OPC.ServerList.1) DCOM discovery client.
/// </summary>
public sealed class OpcEnumClient : IOpcDiscovery
{
    private const int RemoteCreateInstanceOpnum = 4;
    private const int LegacyRemoteActivationOpnum = 0;
    private const int EnumerationBatchSize = 64;
    private const int DefaultPayloadSize = 4096;
    private const int MaximumPayloadSize = 65536;
    private const int ClassContext = 0x14;
    private const int RpcProtocolSequenceTcp = 7;
    private const uint ObjRefSignature = 0x574F454D;

    private static readonly Guid RemoteScmActivatorInterfaceId = new("000001A0-0000-0000-C000-000000000046");
    private static readonly Guid LegacyActivationInterfaceId = new(Interfaces.IID_IActivation);
    private static readonly Guid[] DefaultCategoryIdsArray =
    {
        OpcGuids.CATID_OPCDAServer20,
        OpcGuids.CATID_OPCDAServer30,
        OpcGuids.CATID_OPCHDAServer10,
        OpcGuids.CATID_OPCAEServer10,
    };

    private readonly IOpcEnumCallChannelFactory _channelFactory;
    private readonly Guid[] _categoryIds;
    private readonly OpcProtectionLevel _activationProtectionLevel;
    private readonly ActivationRetryPolicy _activationRetryPolicy;

    private delegate void NdrWriteAction(ref NdrWriter writer);

    /// <summary>
    /// Initializes an OPCEnum client from an OPC URL.
    /// </summary>
    public OpcEnumClient(OpcUrl serverListUrl)
        : this(serverListUrl, new DcomOpcEnumCallChannelFactory(), null)
    {
    }

    /// <summary>
    /// Initializes an OPCEnum client from an OPC URL and injectable DCOM channel factory.
    /// </summary>
    public OpcEnumClient(
        OpcUrl serverListUrl,
        IOpcEnumCallChannelFactory channelFactory,
        IEnumerable<Guid>? categoryIds = null)
        : this(serverListUrl, channelFactory, categoryIds, ActivationRetryPolicy.Default)
    {
    }

    /// <summary>
    /// Initializes an OPCEnum client with an injectable activation retry policy.
    /// </summary>
    public OpcEnumClient(
        OpcUrl serverListUrl,
        IOpcEnumCallChannelFactory channelFactory,
        IEnumerable<Guid>? categoryIds,
        ActivationRetryPolicy activationRetryPolicy)
    {
        ArgumentNullException.ThrowIfNull(serverListUrl);
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(activationRetryPolicy);

        ServerListUrl = serverListUrl;
        Host = NormalizeHost(serverListUrl.Host);
        _channelFactory = channelFactory;
        _categoryIds = NormalizeCategories(categoryIds);
        _activationProtectionLevel = NormalizeActivationProtection(channelFactory.ActivationProtectionLevel);
        _activationRetryPolicy = activationRetryPolicy;
    }

    /// <summary>
    /// Initializes an OPCEnum client for a host and injectable DCOM channel factory.
    /// </summary>
    public OpcEnumClient(
        string host,
        IOpcEnumCallChannelFactory channelFactory,
        IEnumerable<Guid>? categoryIds = null)
        : this(OpcUrl.Parse($"opcda://{NormalizeHost(host)}/OPC.ServerList.1"), channelFactory, categoryIds)
    {
    }

    /// <summary>
    /// Initializes an OPCEnum client for a host with an injectable activation retry policy.
    /// </summary>
    public OpcEnumClient(
        string host,
        IOpcEnumCallChannelFactory channelFactory,
        IEnumerable<Guid>? categoryIds,
        ActivationRetryPolicy activationRetryPolicy)
        : this(
            OpcUrl.Parse($"opcda://{NormalizeHost(host)}/OPC.ServerList.1"),
            channelFactory,
            categoryIds,
            activationRetryPolicy)
    {
    }

    /// <summary>
    /// The default OPCEnum category IDs used by discovery.
    /// </summary>
    public static IReadOnlyList<Guid> DefaultCategoryIds { get; } = Array.AsReadOnly(DefaultCategoryIdsArray);

    /// <summary>
    /// The OpcEnum server-list endpoint URL.
    /// </summary>
    public OpcUrl ServerListUrl { get; }

    /// <summary>
    /// The default host passed to OPCEnum activation.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Enumerates OPCEnum descriptors for the configured host and categories.
    /// </summary>
    public Task<OpcServerDescriptor[]> EnumerateAsync(CancellationToken cancellationToken = default) =>
        EnumerateAsync(null, null, cancellationToken);

    /// <summary>
    /// Enumerates OPCEnum descriptors for a host and category list.
    /// </summary>
    public async Task<OpcServerDescriptor[]> EnumerateAsync(
        string? host = null,
        IEnumerable<Guid>? categories = null,
        CancellationToken cancellationToken = default)
    {
        string targetHost = string.IsNullOrWhiteSpace(host) ? Host : NormalizeHost(host);
        Guid[] requestedCategories = categories is null ? CopyCategories(_categoryIds) : NormalizeCategories(categories);
        if (requestedCategories.Length == 0)
        {
            return Array.Empty<OpcServerDescriptor>();
        }

        ActivatedServerList activated = await ActivateServerListAsync(targetHost, cancellationToken).ConfigureAwait(false);
        ICallChannel? serverListChannel = null;
        try
        {
            Guid serverListIid = activated.SupportsServerList2 ? OpcGuids.IID_IOPCServerList2 : OpcGuids.IID_IOPCServerList;
            serverListChannel = await _channelFactory.CreateObjectChannelAsync(
                targetHost,
                activated.InterfaceRef,
                serverListIid,
                activated.OxidBindings,
                cancellationToken).ConfigureAwait(false);

            if (activated.SupportsServerList2)
            {
                try
                {
                    return await EnumerateWithServerList2Async(targetHost, serverListChannel, requestedCategories, activated.OxidBindings, cancellationToken).ConfigureAwait(false);
                }
                catch (PresentationContextRejectedException ex) when (
                    ex.InterfaceId == OpcGuids.IID_IOPCServerList2
                    && ex.IsAbstractSyntaxNotSupported)
                {
                    // OPCEnum's activator marshaled an OBJREF claiming IOPCServerList2
                    // support, but the underlying RPC server rejects the bind for that
                    // specific IID (common with older OPC Core Components installs that
                    // only ship IOPCServerList). Discard the IOPCServerList2 channel
                    // and re-bind against IOPCServerList (DA 2.0). The IID-specific
                    // filter prevents us from mis-treating an IEnumGUID downstream
                    // bind failure as an IOPCServerList2-not-implemented signal.
                    await DisposeChannelAsync(serverListChannel).ConfigureAwait(false);
                    serverListChannel = await _channelFactory.CreateObjectChannelAsync(
                        targetHost,
                        activated.InterfaceRef,
                        OpcGuids.IID_IOPCServerList,
                        activated.OxidBindings,
                        cancellationToken).ConfigureAwait(false);
                    return await EnumerateWithServerListAsync(targetHost, serverListChannel, requestedCategories, activated.OxidBindings, cancellationToken).ConfigureAwait(false);
                }
            }

            return await EnumerateWithServerListAsync(targetHost, serverListChannel, requestedCategories, activated.OxidBindings, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeChannelAsync(serverListChannel).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string targetHost = string.IsNullOrWhiteSpace(host) ? Host : NormalizeHost(host);
        OpcServerDescriptor[] descriptors = await EnumerateAsync(targetHost, null, cancellationToken).ConfigureAwait(false);
        foreach (OpcServerDescriptor descriptor in descriptors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new OpcServerEntry(
                descriptor.ClassId,
                descriptor.ProgId,
                descriptor.UserType,
                targetHost,
                descriptor.Categories);
        }
    }

    private async Task<OpcServerDescriptor[]> EnumerateWithServerList2Async(
        string host,
        ICallChannel serverListChannel,
        Guid[] requestedCategories,
        ReadOnlyMemory<byte> oxidBindings,
        CancellationToken cancellationToken)
    {
        var serverList = new IOPCServerList2ClientProxy(serverListChannel);
        CategoryMerge merge = await EnumerateClassesAsync(
            host,
            requestedCategories,
            (category, token) => serverList.EnumClassesOfCategoriesAsync(new[] { category }, Array.Empty<Guid>(), token),
            oxidBindings,
            cancellationToken).ConfigureAwait(false);

        var descriptors = new List<OpcServerDescriptor>(merge.ClassIds.Count);
        foreach (Guid classId in merge.ClassIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpcServerListClassDetails details = await serverList.GetClassDetailsAsync(classId, cancellationToken).ConfigureAwait(false);
            descriptors.Add(CreateDescriptor(classId, details.ProgId, details.UserType, details.VerIndProgId, merge.CategoriesByClassId[classId]));
        }

        return descriptors.ToArray();
    }

    private async Task<OpcServerDescriptor[]> EnumerateWithServerListAsync(
        string host,
        ICallChannel serverListChannel,
        Guid[] requestedCategories,
        ReadOnlyMemory<byte> oxidBindings,
        CancellationToken cancellationToken)
    {
        var serverList = new IOPCServerListClientProxy(serverListChannel);
        CategoryMerge merge = await EnumerateClassesAsync(
            host,
            requestedCategories,
            (category, token) => serverList.EnumClassesOfCategoriesAsync(new[] { category }, Array.Empty<Guid>(), token),
            oxidBindings,
            cancellationToken).ConfigureAwait(false);

        var descriptors = new List<OpcServerDescriptor>(merge.ClassIds.Count);
        foreach (Guid classId in merge.ClassIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpcServerListClassDetails details = await serverList.GetClassDetailsAsync(classId, cancellationToken).ConfigureAwait(false);
            descriptors.Add(CreateDescriptor(classId, details.ProgId, details.UserType, null, merge.CategoriesByClassId[classId]));
        }

        return descriptors.ToArray();
    }

    private async Task<CategoryMerge> EnumerateClassesAsync(
        string host,
        IReadOnlyList<Guid> requestedCategories,
        Func<Guid, CancellationToken, Task<IOpcInterfaceRef>> enumFactory,
        ReadOnlyMemory<byte> oxidBindings,
        CancellationToken cancellationToken)
    {
        var classIds = new List<Guid>();
        var categoriesByClassId = new Dictionary<Guid, List<Guid>>();

        foreach (Guid category in requestedCategories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IOpcInterfaceRef enumRef = await enumFactory(category, cancellationToken).ConfigureAwait(false);
            ICallChannel? enumChannel = null;
            try
            {
                // The enumerator returned by EnumClassesOfCategories lives on the
                // same OXID as the parent IOPCServerList(2) object — both run in
                // the OPCEnum process. Reuse the parent's OXID bindings so the
                // enumerator channel binds the actual data port instead of the
                // OXID resolver (port 135).
                enumChannel = await _channelFactory.CreateObjectChannelAsync(
                    host,
                    enumRef,
                    OpcGuids.IID_IOPCEnumGUID,
                    oxidBindings,
                    cancellationToken).ConfigureAwait(false);
                var enumerator = new IOPCEnumGUIDClientProxy(enumChannel);
                await AddEnumeratedClassIdsAsync(enumerator, category, classIds, categoriesByClassId, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await DisposeChannelAsync(enumChannel).ConfigureAwait(false);
            }
        }

        return new CategoryMerge(classIds, categoriesByClassId);
    }

    private static async Task AddEnumeratedClassIdsAsync(
        IOPCEnumGUIDClientProxy enumerator,
        Guid category,
        List<Guid> classIds,
        Dictionary<Guid, List<Guid>> categoriesByClassId,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpcEnumGuidNextResult next = await enumerator.NextAsync(EnumerationBatchSize, cancellationToken).ConfigureAwait(false);
            if (next.Fetched <= 0 || next.ClassIds.Length == 0)
            {
                break;
            }

            int count = Math.Min(next.Fetched, next.ClassIds.Length);
            for (int i = 0; i < count; i++)
            {
                Guid classId = next.ClassIds[i];
                if (!categoriesByClassId.TryGetValue(classId, out List<Guid>? categories))
                {
                    categories = new List<Guid>();
                    categoriesByClassId.Add(classId, categories);
                    classIds.Add(classId);
                }

                if (!categories.Contains(category))
                {
                    categories.Add(category);
                }
            }

            if (next.Fetched < EnumerationBatchSize)
            {
                break;
            }
        }
    }

    private async Task<ActivatedServerList> ActivateServerListAsync(string host, CancellationToken cancellationToken)
    {
        try
        {
            ActivationOutcome serverList2 = await RemoteCreateInstanceAsync(host, OpcGuids.IID_IOPCServerList2, cancellationToken)
                .ConfigureAwait(false);
            return new ActivatedServerList(serverList2.InterfaceRef, serverList2.OxidBindings, SupportsServerList2: true);
        }
        catch (OpcException ex) when (ex.ResultId.Code == global::Opc.Classic.OpcResultId.NoInterface.Code)
        {
            ActivationOutcome serverList = await RemoteCreateInstanceAsync(host, OpcGuids.IID_IOPCServerList, cancellationToken)
                .ConfigureAwait(false);
            return new ActivatedServerList(serverList.InterfaceRef, serverList.OxidBindings, SupportsServerList2: false);
        }
    }

    private async Task<ActivationOutcome> RemoteCreateInstanceAsync(
        string host,
        Guid requestedIid,
        CancellationToken cancellationToken)
    {
        try
        {
            NdrCallResult result = await _activationRetryPolicy.ExecuteAsync(
                token => InvokeRemoteCreateInstanceAsync(host, requestedIid, token),
                IsTransientColdStartResult,
                cancellationToken).ConfigureAwait(false);
            return DecodeRemoteCreateInstanceResponse(result, requestedIid);
        }
        catch (BindException)
        {
            return await LegacyRemoteActivationAsync(host, requestedIid, cancellationToken).ConfigureAwait(false);
        }
        catch (PresentationContextRejectedException ex) when (ex.InterfaceId == RemoteScmActivatorInterfaceId)
        {
            return await LegacyRemoteActivationAsync(host, requestedIid, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<NdrCallResult> InvokeRemoteCreateInstanceAsync(
        string host,
        Guid requestedIid,
        CancellationToken cancellationToken)
    {
        ICallChannel? activationChannel = null;
        try
        {
            activationChannel = await _channelFactory.CreateActivationChannelAsync(host, cancellationToken).ConfigureAwait(false);
            byte[] payload = EncodeRemoteCreateInstanceRequest(host, OpcGuids.CLSID_OpcEnum, requestedIid, _activationProtectionLevel);
            return await activationChannel.InvokeAsync(
                RemoteScmActivatorInterfaceId,
                RemoteCreateInstanceOpnum,
                payload,
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeChannelAsync(activationChannel).ConfigureAwait(false);
        }
    }

    // The authoritative outer HRESULT follows ppActProperties and is valid whether
    // the pointer is NULL or populated.
    private static bool IsTransientColdStartResult(NdrCallResult result)
    {
        return ActivationPropertiesCodec.TryReadRemoteCreateInstanceHresult(
            result.ResponsePayload.Span,
            out int hresult)
            && IsTransientActivationFailure(hresult);
    }

    internal static bool IsTransientActivationFailure(int hresult) => unchecked((uint)hresult) switch
    {
        0x800706BAu => true, // RPC_S_SERVER_UNAVAILABLE
        0x800706BFu => true, // RPC_S_CALL_FAILED_DNE
        0x80080005u => true, // CO_E_SERVER_EXEC_FAILURE
        0x8001010Au => true, // RPC_E_SERVERCALL_RETRYLATER
        _ => false,
    };

    private async Task<ActivationOutcome> LegacyRemoteActivationAsync(
        string host,
        Guid requestedIid,
        CancellationToken cancellationToken)
    {
        ICallChannel? activationChannel = null;
        try
        {
            activationChannel = await _channelFactory.CreateActivationChannelAsync(host, cancellationToken).ConfigureAwait(false);
            var request = new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
                OpcGuids.CLSID_OpcEnum,
                new[] { requestedIid },
                ClientImpLevel: 3,
                Mode: 0,
                RequestedProtocolSequences: new[] { (ushort)RpcProtocolSequenceTcp });
            byte[] payload = Opc.Classic.Dcom.Activation.IActivationCodec.EncodeRemoteActivationRequest(request);
            NdrCallResult result = await activationChannel.InvokeAsync(
                LegacyActivationInterfaceId,
                LegacyRemoteActivationOpnum,
                payload,
                cancellationToken).ConfigureAwait(false);
            return DecodeLegacyRemoteActivationResponse(result);
        }
        finally
        {
            await DisposeChannelAsync(activationChannel).ConfigureAwait(false);
        }
    }

    private static OpcServerDescriptor CreateDescriptor(
        Guid classId,
        string? progId,
        string? userType,
        string? verIndProgId,
        IReadOnlyList<Guid> categories) =>
        new(
            classId,
            string.IsNullOrWhiteSpace(progId) ? classId.ToString("B") : progId,
            string.IsNullOrWhiteSpace(userType) ? string.IsNullOrWhiteSpace(progId) ? classId.ToString("B") : progId : userType,
            string.IsNullOrWhiteSpace(verIndProgId) ? null : verIndProgId,
            new ReadOnlyCollection<Guid>(CopyCategories(categories)));

    private static byte[] EncodeRemoteCreateInstanceRequest(
        string host,
        Guid clsid,
        Guid requestedIid,
        OpcProtectionLevel activationProtectionLevel)
    {
        _ = host;
        _ = activationProtectionLevel;
        return ActivationPropertiesCodec.EncodeRemoteCreateInstanceRequest(
            clsid,
            new[] { requestedIid },
            new[] { (ushort)RpcProtocolSequenceTcp },
            ClassContext,
            clientImpersonationLevel: 2,
            clientComVersion: (5, 7));
    }

    private static OpcProtectionLevel NormalizeActivationProtection(OpcProtectionLevel protectionLevel) =>
        protectionLevel == OpcProtectionLevel.Privacy ? OpcProtectionLevel.Privacy : OpcProtectionLevel.Integrity;

    private static int ToActivationAuthenticationLevel(OpcProtectionLevel protectionLevel) =>
        (int)NormalizeActivationProtection(protectionLevel);

    private static ActivationOutcome DecodeLegacyRemoteActivationResponse(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"IActivation::RemoteActivation RPC fault 0x{unchecked((uint)result.Hresult):X8}.");
        }

        ThrowIfFailed(result.Hresult, "IActivation::RemoteActivation");
        var response = Opc.Classic.Dcom.Activation.IActivationCodec.DecodeRemoteActivationResponse(
            result.ResponsePayload.Span,
            expectedInterfaceCount: 1);
        ThrowIfFailed(response.Hresult, "IActivation::RemoteActivation");
        if (response.InterfaceResults.Count == 0)
        {
            throw new InvalidOperationException("IActivation::RemoteActivation returned no OPCEnum interface results.");
        }

        Opc.Classic.Dcom.Activation.RemoteActivationInterfaceResult interfaceResult = response.InterfaceResults[0];
        ThrowIfFailed(interfaceResult.Hresult, "IActivation::RemoteActivation");
        if (interfaceResult.ObjRef.IsEmpty)
        {
            throw new InvalidOperationException("IActivation::RemoteActivation returned no OPCEnum OBJREF.");
        }

        if (TryDecodeObjRef(interfaceResult.ObjRef.Span, out IOpcInterfaceRef? objRef))
        {
            return new ActivationOutcome(objRef!, response.OxidBindings);
        }

        throw new InvalidOperationException("IActivation::RemoteActivation returned an invalid OPCEnum OBJREF.");
    }

    private static ActivationOutcome DecodeRemoteCreateInstanceResponse(NdrCallResult result, Guid requestedIid)
    {
        ThrowIfFailed(result.Hresult, "IRemoteSCMActivator::RemoteCreateInstance");
        if (result.ResponsePayload.IsEmpty)
        {
            // Empty payload after RPC success usually means the call surfaced an RPC
            // fault PDU whose status code DcomCallChannel placed in result.Hresult.
            // Surface a clearer error so operators don't chase an OBJREF-format issue
            // when the real problem is anonymous activation being refused.
            int rpcFault = result.Hresult;
            string hint = rpcFault switch
            {
                0 => "no RPC fault status; the SCM may have returned an empty activation result.",
                0x00000005 => "rpc_s_access_denied (0x05) - supply NTLMv2/Kerberos credentials with sufficient DCOM Launch/Access permission for OPCEnum (the OPC.ServerList AppID).",
                0x00000721 => "rpc_s_sec_pkg_error (0x721) - the DCOM per-call security check failed on the server; the RPC bind authenticated but the signed request was rejected (packet-integrity/NTLM signing mismatch).",
                _ => $"RPC fault status 0x{rpcFault:X8}.",
            };
            throw new InvalidOperationException("IRemoteSCMActivator::RemoteCreateInstance returned no OPCEnum OBJREF: " + hint);
        }

        ReadOnlySpan<byte> response = result.ResponsePayload.Span;
        ActivationPropertiesFormatException? formatException = null;
        try
        {
            return DecodeActivationPropertiesResponse(response, requestedIid);
        }
        catch (ActivationPropertiesFormatException ex)
        {
            formatException = ex;
        }

        if (TryDecodeObjRef(response, out IOpcInterfaceRef? directObjRef))
        {
            return new ActivationOutcome(directObjRef!, ReadOnlyMemory<byte>.Empty);
        }

        if (TryDecodeActivationProperties(response, out IOpcInterfaceRef? activationObjRef))
        {
            return new ActivationOutcome(activationObjRef!, ReadOnlyMemory<byte>.Empty);
        }

        try
        {
            return DecodeLengthPrefixedObjRef(response);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OverflowException)
        {
            throw formatException;
        }
    }

    private static ActivationOutcome DecodeActivationPropertiesResponse(
        ReadOnlySpan<byte> response,
        Guid requestedIid)
    {
        ActivationPropertiesOutData activationPropertiesOut =
            ActivationPropertiesCodec.DecodeRemoteCreateInstanceResponse(response);
        ThrowIfFailed(activationPropertiesOut.Hresult, "IRemoteSCMActivator::RemoteCreateInstance");
        if (activationPropertiesOut.InterfaceResults.Count == 0)
        {
            throw new InvalidOperationException("IRemoteSCMActivator::RemoteCreateInstance returned no interface results.");
        }

        ActivationInterfaceResult interfaceResult =
            SelectInterfaceResult(activationPropertiesOut.InterfaceResults, requestedIid);
        ThrowIfFailed(interfaceResult.Hresult, "IRemoteSCMActivator::RemoteCreateInstance");
        if (interfaceResult.ObjRef.Length == 0)
        {
            throw new InvalidOperationException("IRemoteSCMActivator::RemoteCreateInstance returned no interface OBJREF.");
        }

        if (TryDecodeObjRef(interfaceResult.ObjRef, out IOpcInterfaceRef? objRef))
        {
            return new ActivationOutcome(objRef!, activationPropertiesOut.OxidBindings);
        }

        throw new InvalidOperationException("IRemoteSCMActivator::RemoteCreateInstance returned an invalid interface OBJREF.");
    }

    private static ActivationInterfaceResult SelectInterfaceResult(IReadOnlyList<ActivationInterfaceResult> interfaceResults, Guid requestedIid)
    {
        for (int i = 0; i < interfaceResults.Count; i++)
        {
            if (interfaceResults[i].Iid == requestedIid)
            {
                return interfaceResults[i];
            }
        }

        return interfaceResults[0];
    }

    private static ActivationOutcome DecodeLengthPrefixedObjRef(ReadOnlySpan<byte> response)
    {
        var reader = new NdrReader(response);
        int innerHresult = reader.ReadInt32();
        ThrowIfFailed(innerHresult, "IRemoteSCMActivator::RemoteCreateInstance");
        if (reader.RemainingBytes < sizeof(uint))
        {
            throw new InvalidOperationException("RemoteCreateInstance response did not include a length-prefixed OBJREF.");
        }

        uint objRefLength = reader.ReadUInt32();
        if (objRefLength > reader.RemainingBytes)
        {
            throw new InvalidOperationException("RemoteCreateInstance OBJREF length exceeds the remaining response payload.");
        }

        byte[] objRefBytes = reader.ReadRawBytes((int)objRefLength).ToArray();
        if (TryDecodeObjRef(objRefBytes, out IOpcInterfaceRef? objRef))
        {
            return new ActivationOutcome(objRef!, ReadOnlyMemory<byte>.Empty);
        }

        throw new InvalidOperationException("RemoteCreateInstance returned an invalid OPCEnum OBJREF.");
    }

    private static bool TryDecodeActivationProperties(ReadOnlySpan<byte> response, out IOpcInterfaceRef? objRef)
    {
        objRef = null;
        if (!ActivationInfoCodec.TryDecode(response, out ActivationProperties properties)
            || properties.ScmReplyInfo?.ObjRef is not { Length: > 0 } objRefBytes)
        {
            return false;
        }

        return TryDecodeObjRef(objRefBytes, out objRef);
    }

    private static bool TryDecodeObjRef(ReadOnlySpan<byte> payload, out IOpcInterfaceRef? objRef)
    {
        objRef = null;
        if (payload.Length < sizeof(uint) || BinaryPrimitives.ReadUInt32LittleEndian(payload) != ObjRefSignature)
        {
            return false;
        }

        try
        {
            var reader = new NdrReader(payload);
            objRef = OpcInterfaceRefCodec.Read(ref reader);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static byte[] WritePayload(NdrWriteAction action)
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

        throw new InvalidOperationException("Unable to encode the RemoteCreateInstance payload.");
    }

    private static Guid[] NormalizeCategories(IEnumerable<Guid>? categories)
    {
        if (categories is null)
        {
            return CopyCategories(DefaultCategoryIdsArray);
        }

        var distinct = new List<Guid>();
        foreach (Guid category in categories)
        {
            if (category != Guid.Empty && !distinct.Contains(category))
            {
                distinct.Add(category);
            }
        }

        return distinct.ToArray();
    }

    private static Guid[] CopyCategories(IReadOnlyList<Guid> categories)
    {
        var copy = new Guid[categories.Count];
        for (int i = 0; i < categories.Count; i++)
        {
            copy[i] = categories[i];
        }

        return copy;
    }

    private static string NormalizeHost(string host) =>
        string.IsNullOrWhiteSpace(host) ? "localhost" : host.Trim();

    private static void ThrowIfFailed(int hresult, string operationDescription) =>
        OpcException.ThrowIfFailed(new OpcResultId(hresult, null), operationDescription);

    private static async ValueTask DisposeChannelAsync(ICallChannel? channel)
    {
        switch (channel)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    private sealed record ActivatedServerList(IOpcInterfaceRef InterfaceRef, ReadOnlyMemory<byte> OxidBindings, bool SupportsServerList2);
    private sealed record ActivationOutcome(IOpcInterfaceRef InterfaceRef, ReadOnlyMemory<byte> OxidBindings);

    private sealed record CategoryMerge(
        IReadOnlyList<Guid> ClassIds,
        IReadOnlyDictionary<Guid, List<Guid>> CategoriesByClassId);
}

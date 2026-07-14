// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Discovery.Tests;

public sealed class OpcEnumClientTests
{
    [Test]
    public async Task EnumerateAsync_returns_descriptors_from_synthetic_opcenum_server()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000031");
        var server = new SyntheticOpcEnumServer()
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.DA.1", "Vendor DA", "Vendor.DA");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        await Assert.That(descriptors[0].ClassId).IsEqualTo(classId);
        await Assert.That(descriptors[0].ProgId).IsEqualTo("Vendor.DA.1");
        await Assert.That(descriptors[0].UserType).IsEqualTo("Vendor DA");
        await Assert.That(descriptors[0].VerIndProgId).IsEqualTo("Vendor.DA");
        await Assert.That(descriptors[0].Categories.Count).IsEqualTo(1);
        await Assert.That(descriptors[0].Categories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
    }

    [Test]
    public async Task EnumerateAsync_merges_multi_category_results()
    {
        var da20Only = Guid.Parse("10138C2C-0000-0000-0000-000000000032");
        var da20AndDa30 = Guid.Parse("10138C2C-0000-0000-0000-000000000033");
        var hdaOnly = Guid.Parse("10138C2C-0000-0000-0000-000000000034");
        var server = new SyntheticOpcEnumServer()
            .AddServer(OpcGuids.CATID_OPCDAServer20, da20Only, "Vendor.DA20.1", "Vendor DA20", "Vendor.DA20")
            .AddServer(OpcGuids.CATID_OPCDAServer20, da20AndDa30, "Vendor.Both.1", "Vendor Both", "Vendor.Both")
            .AddServer(OpcGuids.CATID_OPCDAServer30, da20AndDa30, "Vendor.Both.1", "Vendor Both", "Vendor.Both")
            .AddServer(OpcGuids.CATID_OPCHDAServer10, hdaOnly, "Vendor.HDA.1", "Vendor HDA", "Vendor.HDA");
        var client = new OpcEnumClient(
            "opc-host",
            server,
            new[] { OpcGuids.CATID_OPCDAServer20, OpcGuids.CATID_OPCDAServer30, OpcGuids.CATID_OPCHDAServer10 });

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(3);
        OpcServerDescriptor merged = descriptors.Single(descriptor => descriptor.ClassId == da20AndDa30);
        await Assert.That(merged.Categories.Count).IsEqualTo(2);
        await Assert.That(merged.Categories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
        await Assert.That(merged.Categories[1]).IsEqualTo(OpcGuids.CATID_OPCDAServer30);
    }

    [Test]
    public async Task EnumerateAsync_returns_empty_result_when_no_categories_have_servers()
    {
        var server = new SyntheticOpcEnumServer();
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task EnumerateAsync_maps_failed_hresult_to_OpcException()
    {
        const int accessDenied = unchecked((int)0x80070005u);
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000035");
        var server = new SyntheticOpcEnumServer { GetClassDetailsHresult = accessDenied }
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Fail.1", "Vendor Fail", "Vendor.Fail");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        Exception exception = await CaptureAsync(() => client.EnumerateAsync(CancellationToken.None));

        await Assert.That(exception is OpcException).IsTrue();
        await Assert.That(((OpcException)exception).ResultId.Code).IsEqualTo(accessDenied);
    }

    [Test]
    public async Task DiscoverAsync_projects_descriptors_to_server_entries()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000036");
        var server = new SyntheticOpcEnumServer()
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Entry.1", "Vendor Entry", "Vendor.Entry");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        List<OpcServerEntry> entries = await ToListAsync(client);

        await Assert.That(entries.Count).IsEqualTo(1);
        await Assert.That(entries[0].Clsid).IsEqualTo(classId);
        await Assert.That(entries[0].ProgId).IsEqualTo("Vendor.Entry.1");
        await Assert.That(entries[0].FriendlyName).IsEqualTo("Vendor Entry");
        await Assert.That(entries[0].Host).IsEqualTo("opc-host");
    }

    [Test]
    public async Task RemoteCreateInstance_declares_packet_integrity_activation_authentication()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000037");
        var server = new SyntheticOpcEnumServer()
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Auth.1", "Vendor Auth", "Vendor.Auth");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        _ = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(server.ActivationRequests.Count).IsGreaterThan(0);
        await Assert.That(server.ActivationRequests[0].SecurityInfo).IsNotNull();
        await Assert.That(server.ActivationRequests[0].SecurityInfo!.AuthenticationLevel).IsEqualTo((int)OpcProtectionLevel.Integrity);
        await Assert.That(server.ActivationRequests[0].SecurityInfo!.ImpersonationLevel).IsEqualTo(2);
    }

    [Test]
    public async Task RemoteCreateInstance_preserves_packet_privacy_activation_authentication()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000038");
        var server = new SyntheticOpcEnumServer { ActivationProtectionLevel = OpcProtectionLevel.Privacy }
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Privacy.1", "Vendor Privacy", "Vendor.Privacy");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        _ = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(server.ActivationRequests.Count).IsGreaterThan(0);
        await Assert.That(server.ActivationRequests[0].SecurityInfo!.AuthenticationLevel).IsEqualTo((int)OpcProtectionLevel.Privacy);
    }

    [Test]
    public async Task DcomOpcEnumCallChannelFactory_upgrades_weak_activation_protection_to_integrity()
    {
        OpcUrl url = OpcUrl.Parse("opcda://opc-host/OPC.ServerList.1");
        var connectData = OpcConnectData.WithNtlmV2(url, new NetworkCredential("user", "p"), OpcProtectionLevel.Connect);
        var factory = new DcomOpcEnumCallChannelFactory(connectData);

        await Assert.That(factory.ActivationProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task EnumerateAsync_downgrades_to_IOPCServerList_when_IOPCServerList2_bind_is_rejected()
    {
        // Regression: some OPCEnum installs (older OPC Core Components, certain
        // vendor SDKs) marshal an OBJREF claiming IOPCServerList2 support but
        // the underlying RPC server only speaks IOPCServerList (DA 2.0) over
        // the wire — the bind PDU returns PROVIDER_REJECTION /
        // ABSTRACT_SYNTAX_NOT_SUPPORTED. The client must downgrade silently to
        // IOPCServerList rather than bubble the rejection to the caller.
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000099");
        var server = new SyntheticOpcEnumServer { RejectServerList2Bind = true }
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Downgrade.1", "Vendor Downgrade", "Vendor.Downgrade");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        await Assert.That(descriptors[0].ClassId).IsEqualTo(classId);
        await Assert.That(descriptors[0].ProgId).IsEqualTo("Vendor.Downgrade.1");
    }

    [Test]
    public async Task EnumerateAsync_falls_back_to_legacy_activation_when_remote_scm_bind_is_rejected()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000039");
        var server = new SyntheticOpcEnumServer { RejectModernActivationBind = true }
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Legacy.1", "Vendor Legacy", "Vendor.Legacy");
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        await Assert.That(descriptors[0].ProgId).IsEqualTo("Vendor.Legacy.1");
        await Assert.That(server.Calls.Any(call => call.InterfaceId == new Guid(Opc.Classic.Dcom.Interfaces.IID_IActivation) && call.Opnum == 0)).IsTrue();
    }

    [Test]
    public async Task EnumerateAsync_does_not_fallback_to_legacy_activation_on_auth_failure()
    {
        var server = new SyntheticOpcEnumServer { RejectModernActivationWithAuthFailure = true };
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        Exception exception = await CaptureAsync(() => client.EnumerateAsync(CancellationToken.None));

        await Assert.That(exception is InvalidOperationException).IsTrue();
        await Assert.That(exception.Message).IsEqualTo("Packet integrity verification failed.");
        await Assert.That(server.Calls.Any(call => call.InterfaceId == new Guid(Opc.Classic.Dcom.Interfaces.IID_IActivation))).IsFalse();
    }

    [Test]
    public async Task EnumerateAsync_does_not_fallback_for_modern_activation_application_hresult()
    {
        const int accessDenied = unchecked((int)0x80070005u);
        var server = new SyntheticOpcEnumServer();
        server.ModernActivationHresults.Enqueue(accessDenied);
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        Exception exception = await CaptureAsync(() => client.EnumerateAsync(CancellationToken.None));

        await Assert.That(exception is OpcException).IsTrue();
        await Assert.That(((OpcException)exception).ResultId.Code).IsEqualTo(accessDenied);
        await Assert.That(server.Calls.Count(call => call.InterfaceId == RemoteScmActivatorInterfaceIdForTests)).IsEqualTo(1);
        await Assert.That(server.Calls.Any(call => call.InterfaceId == new Guid(Opc.Classic.Dcom.Interfaces.IID_IActivation))).IsFalse();
    }

    [Test]
    public async Task EnumerateAsync_does_not_fallback_for_malformed_modern_activation_properties()
    {
        var server = new SyntheticOpcEnumServer { ReturnMalformedModernActivation = true };
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        Exception exception = await CaptureAsync(() => client.EnumerateAsync(CancellationToken.None));

        await Assert.That(exception is ActivationPropertiesFormatException).IsTrue();
        await Assert.That(server.Calls.Any(call => call.InterfaceId == new Guid(Opc.Classic.Dcom.Interfaces.IID_IActivation))).IsFalse();
    }

    [Test]
    public async Task EnumerateAsync_does_not_fallback_for_modern_activation_rpc_fault()
    {
        const int accessDenied = unchecked((int)0x80070005u);
        var server = new SyntheticOpcEnumServer { ModernActivationRpcFault = accessDenied };
        var client = new OpcEnumClient("opc-host", server, new[] { OpcGuids.CATID_OPCDAServer20 });

        Exception exception = await CaptureAsync(() => client.EnumerateAsync(CancellationToken.None));

        await Assert.That(exception is OpcException).IsTrue();
        await Assert.That(((OpcException)exception).ResultId.Code).IsEqualTo(accessDenied);
        await Assert.That(server.Calls.Any(call => call.InterfaceId == new Guid(Opc.Classic.Dcom.Interfaces.IID_IActivation))).IsFalse();
    }

    [Test]
    public async Task EnumerateAsync_retries_populated_transient_modern_activation_without_sleeping()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000040");
        var server = new SyntheticOpcEnumServer { PopulateModernActivationFailures = true }
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.Retry.1", "Vendor Retry", "Vendor.Retry");
        server.ModernActivationHresults.Enqueue(unchecked((int)0x800706BAu));
        server.ModernActivationHresults.Enqueue(unchecked((int)0x80080005u));
        server.ModernActivationHresults.Enqueue(0);
        var delays = new List<TimeSpan>();
        var retryPolicy = new ActivationRetryPolicy(
            5,
            TimeSpan.FromMilliseconds(10),
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });
        var client = new OpcEnumClient(
            "opc-host",
            server,
            new[] { OpcGuids.CATID_OPCDAServer20 },
            retryPolicy);

        OpcServerDescriptor[] descriptors = await client.EnumerateAsync(CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        await Assert.That(server.Calls.Count(call => call.InterfaceId == RemoteScmActivatorInterfaceIdForTests)).IsEqualTo(3);
        await Assert.That(delays.Count).IsEqualTo(2);
    }

    [Test, Skip("Requires reachable OPCEnum.exe host")]
    public async Task OpcEnum_real_network_enumerates_reachable_host()
    {
        OpcServerDescriptor[] descriptors = await OpcDiscovery.EnumerateAsync(
            "localhost",
            OpcEnumClient.DefaultCategoryIds,
            CancellationToken.None);

        await Assert.That(descriptors.Length).IsGreaterThanOrEqualTo(0);
    }

    private static async Task<List<OpcServerEntry>> ToListAsync(IOpcDiscovery discovery)
    {
        var entries = new List<OpcServerEntry>();
        await foreach (OpcServerEntry entry in discovery.DiscoverAsync())
        {
            entries.Add(entry);
        }

        return entries;
    }

    private static async Task<Exception> CaptureAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ex;
        }

        throw new InvalidOperationException("Expected an exception.");
    }

    private static readonly Guid RemoteScmActivatorInterfaceIdForTests =
        new("000001A0-0000-0000-C000-000000000046");
}

internal sealed class SyntheticOpcEnumServer : IOpcEnumCallChannelFactory
{
    private static readonly Guid RemoteScmActivatorInterfaceId = new("000001A0-0000-0000-C000-000000000046");
    private static readonly Guid LegacyActivationInterfaceId = new(Opc.Classic.Dcom.Interfaces.IID_IActivation);
    private readonly Dictionary<Guid, List<Guid>> _categoryClasses = new();
    private readonly Dictionary<Guid, SyntheticOpcServerDetails> _details = new();
    private readonly Queue<IReadOnlyList<Guid>> _pendingEnums = new();
    private readonly InMemoryCallChannel _channel;
    private IReadOnlyList<Guid>? _currentEnum;
    private int _currentEnumIndex;

    public SyntheticOpcEnumServer() =>
        _channel = new InMemoryCallChannel(HandleCallAsync);

    public int GetClassDetailsHresult { get; init; }
    public OpcProtectionLevel ActivationProtectionLevel { get; init; } = OpcProtectionLevel.Integrity;

    /// <summary>
    /// When true, the synthetic channel throws the same
    /// <c>InvalidOperationException("Presentation context rejected: PROVIDER_REJECTION; ABSTRACT_SYNTAX_NOT_SUPPORTED.")</c>
    /// that <c>DcomCallChannel.EnsurePresentationContextAsync</c> produces when an
    /// OPCEnum host returns an OBJREF claiming <c>IOPCServerList2</c> support but
    /// the underlying RPC server only speaks <c>IOPCServerList</c> (DA 2.0).
    /// Exercises the downgrade-on-bind-rejection fallback in
    /// <c>OpcEnumClient.EnumerateAsync</c>.
    /// </summary>
    public bool RejectServerList2Bind { get; init; }
    public bool RejectModernActivationBind { get; init; }
    public bool RejectModernActivationWithAuthFailure { get; init; }
    public bool ReturnMalformedModernActivation { get; init; }
    public int ModernActivationRpcFault { get; init; }
    public bool PopulateModernActivationFailures { get; init; }
    public Queue<int> ModernActivationHresults { get; } = new();
    public List<ActivationProperties> ActivationRequests { get; } = new();
    public IReadOnlyList<InMemoryCall> Calls => _channel.CallLog;

    public SyntheticOpcEnumServer AddServer(
        Guid categoryId,
        Guid classId,
        string progId,
        string userType,
        string? verIndProgId)
    {
        if (!_categoryClasses.TryGetValue(categoryId, out List<Guid>? classIds))
        {
            classIds = new List<Guid>();
            _categoryClasses.Add(categoryId, classIds);
        }

        if (!classIds.Contains(classId))
        {
            classIds.Add(classId);
        }

        _details[classId] = new SyntheticOpcServerDetails(progId, userType, verIndProgId);
        return this;
    }

    public ValueTask<ICallChannel> CreateActivationChannelAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        _ = host;
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<ICallChannel>(_channel);
    }

    public ValueTask<ICallChannel> CreateObjectChannelAsync(
        string host,
        IOpcInterfaceRef interfaceRef,
        Guid interfaceId,
        CancellationToken cancellationToken = default)
    {
        _ = host;
        _ = interfaceRef;
        _ = interfaceId;
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<ICallChannel>(_channel);
    }

    private Task<NdrCallResult> HandleCallAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (interfaceId == RemoteScmActivatorInterfaceId && opnum == 4)
        {
            if (RejectModernActivationBind)
            {
                throw new PresentationContextRejectedException(
                    RemoteScmActivatorInterfaceId,
                    new PresentationResult(
                        PresentationResultCode.PROVIDER_REJECTION,
                        PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED),
                    "Modern SCM presentation context rejected.");
            }

            if (RejectModernActivationWithAuthFailure)
            {
                throw new InvalidOperationException("Packet integrity verification failed.");
            }

            if (ReturnMalformedModernActivation)
            {
                return Task.FromResult(new NdrCallResult(0, new byte[] { 1, 2, 3 }));
            }

            if (ModernActivationRpcFault != 0)
            {
                return Task.FromResult(
                    new NdrCallResult(
                        ModernActivationRpcFault,
                        ReadOnlyMemory<byte>.Empty,
                        IsFault: true));
            }

            if (ModernActivationHresults.TryDequeue(out int activationHresult)
                && activationHresult != 0)
            {
                if (PopulateModernActivationFailures)
                {
                    RemoteCreateInstanceActivationRequest failedRequest =
                        ActivationPropertiesCodec.DecodeRemoteCreateInstanceRequest(requestPayload.Span);
                    return Task.FromResult(new NdrCallResult(
                        0,
                        EncodeModernActivationResponse(failedRequest, activationHresult)));
                }

                var buffer = new byte[8];
                var writer = new NdrWriter(buffer);
                writer.WriteUInt32(0);
                writer.WriteInt32(activationHresult);
                return Task.FromResult(new NdrCallResult(0, buffer));
            }

            RemoteCreateInstanceActivationRequest request = ActivationPropertiesCodec.DecodeRemoteCreateInstanceRequest(requestPayload.Span);
            ActivationRequests.Add(CreateActivationProperties(request));
            byte[] response = EncodeModernActivationResponse(request, hresult: 0);
            return Task.FromResult(new NdrCallResult(0, response));
        }

        if (interfaceId == LegacyActivationInterfaceId && opnum == 0)
        {
            Opc.Classic.Dcom.Activation.RemoteActivationRequest request = IActivationCodec.DecodeRemoteActivationRequest(requestPayload.Span);
            ActivationRequests.Add(new ActivationProperties(
                SpecialPropertiesData.Empty,
                null,
                null,
                null,
                new SecurityInfo((int)ActivationProtectionLevel, (int)request.ClientImpLevel, 0)));
            byte[] objRef = EncodeObjRef(request.RequestedIids.Count == 0 ? OpcGuids.IID_IOPCServerList2 : request.RequestedIids[0]);
            var response = new Opc.Classic.Dcom.Activation.RemoteActivationResponse(
                0,
                Guid.NewGuid(),
                Guid.NewGuid(),
                5,
                (5, 4),
                new[] { new RemoteActivationInterfaceResult(0, objRef) })
            {
                OxidBindings = CreateDualStringArray(),
            };
            return Task.FromResult(new NdrCallResult(0, IActivationCodec.EncodeRemoteActivationResponse(response)));
        }

        if (interfaceId == OpcGuids.IID_IOPCServerList2 && RejectServerList2Bind)
        {
            // Simulate the bind-time PROVIDER_REJECTION that DcomCallChannel surfaces
            // when an OPCEnum host's RPC server doesn't actually speak IOPCServerList2
            // despite the activator returning an OBJREF claiming the IID. The IID
            // is embedded in the message so OpcEnumClient's IID-specific catch can
            // tell this rejection apart from downstream IEnumGUID bind failures.
            throw new PresentationContextRejectedException(
                OpcGuids.IID_IOPCServerList2,
                new PresentationResult(
                    PresentationResultCode.PROVIDER_REJECTION,
                    PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED),
                "IOPCServerList2 presentation context rejected.");
        }

        if ((interfaceId == OpcGuids.IID_IOPCServerList2 || interfaceId == OpcGuids.IID_IOPCServerList) && opnum == 3)
        {
            Guid categoryId = DecodeFirstImplementedCategory(requestPayload);
            _pendingEnums.Enqueue(_categoryClasses.TryGetValue(categoryId, out List<Guid>? classIds)
                ? classIds.ToArray()
                : Array.Empty<Guid>());
            return Task.FromResult(new NdrCallResult(0, EncodeObjRef(OpcGuids.IID_IOPCEnumGUID)));
        }

        if (interfaceId == OpcGuids.IID_IOPCEnumGUID && opnum == 3)
        {
            return Task.FromResult(HandleNext(requestPayload));
        }

        if ((interfaceId == OpcGuids.IID_IOPCServerList2 || interfaceId == OpcGuids.IID_IOPCServerList) && opnum == 4)
        {
            if (GetClassDetailsHresult != 0)
            {
                return Task.FromResult(new NdrCallResult(GetClassDetailsHresult, ReadOnlyMemory<byte>.Empty));
            }

            Guid classId = DecodeClassId(requestPayload);
            SyntheticOpcServerDetails details = _details[classId];
            return Task.FromResult(new NdrCallResult(0, EncodeClassDetails(details)));
        }

        return Task.FromResult(new NdrCallResult(unchecked((int)0x80004001u), ReadOnlyMemory<byte>.Empty));
    }

    private static byte[] EncodeModernActivationResponse(
        RemoteCreateInstanceActivationRequest request,
        int hresult)
    {
        Guid requestedIid = request.RequestedIids.Count == 0
            ? OpcGuids.IID_IOPCServerList2
            : request.RequestedIids[0];
        byte[] objRef = EncodeObjRef(requestedIid);
        return ActivationPropertiesCodec.EncodeRemoteCreateInstanceResponse(
            oxid: 1,
            oxidBindings: CreateDualStringArray(),
            ipidRemUnknown: Guid.NewGuid(),
            authnHint: 5,
            serverVersion: (5, 4),
            new[] { new ActivationInterfaceResult(requestedIid, 0, objRef) },
            hresult);
    }

    private NdrCallResult HandleNext(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        int requested = reader.ReadInt32();
        _currentEnum ??= _pendingEnums.Count == 0 ? Array.Empty<Guid>() : _pendingEnums.Dequeue();

        int remaining = Math.Max(0, _currentEnum.Count - _currentEnumIndex);
        int fetched = Math.Min(requested, remaining);
        var batch = new Guid[fetched];
        for (int i = 0; i < batch.Length; i++)
        {
            batch[i] = _currentEnum[_currentEnumIndex++];
        }

        if (_currentEnumIndex >= _currentEnum.Count)
        {
            _currentEnum = null;
            _currentEnumIndex = 0;
        }

        int hresult = fetched < requested ? 1 : 0;
        return new NdrCallResult(hresult, EncodeNext(batch, fetched));
    }

    private static Guid DecodeFirstImplementedCategory(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        // IDL: [in] ULONG cImplemented, [in, size_is(cImplemented)] CATID rgcatidImpl[],
        //      [in] ULONG cRequired,    [in, size_is(cRequired)] CATID rgcatidReq[]
        _ = reader.ReadUInt32();
        Guid[] implementedCategories = reader.ReadConformantGuidArray();
        _ = reader.ReadUInt32();
        _ = reader.ReadConformantGuidArray();
        return implementedCategories.Length == 0 ? Guid.Empty : implementedCategories[0];
    }

    private static ActivationProperties DecodeActivationProperties(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadGuid();
        _ = reader.ReadGuid();
        uint protocolSequenceCount = reader.ReadUInt32();
        for (uint i = 0; i < protocolSequenceCount; i++)
        {
            _ = reader.ReadInt32();
        }

        uint activationPropertiesLength = reader.ReadUInt32();
        return ActivationInfoCodec.Decode(reader.ReadRawBytes((int)activationPropertiesLength));
    }

    private ActivationProperties CreateActivationProperties(RemoteCreateInstanceActivationRequest request) =>
        new(
            SpecialPropertiesData.Empty,
            null,
            null,
            null,
            new SecurityInfo((int)ActivationProtectionLevel, 2, 0));

    private static Guid DecodeClassId(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        return reader.ReadGuid();
    }

    private static byte[] EncodeObjRef(Guid iid) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt32(0x574F454Du);
        writer.WriteUInt32(0x00000001u);
        writer.WriteGuid(iid);
        writer.WriteUInt32(0);
        writer.WriteUInt32(5);
        writer.WriteUInt64(1);
        writer.WriteUInt64(2);
        writer.WriteGuid(Guid.NewGuid());
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    });

    private static byte[] CreateDualStringArray() => [0x02, 0x00, 0x01, 0x00, 0x07, 0x00, 0x00, 0x00];

    private static byte[] EncodeClassDetails(SyntheticOpcServerDetails details) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUnicodeStringPtr(details.ProgId);
        writer.WriteUnicodeStringPtr(details.UserType);
        writer.WriteUnicodeStringPtr(details.VerIndProgId);
    });

    private static byte[] EncodeNext(Guid[] classIds, int fetched) => WritePayload((ref NdrWriter writer) =>
    {
        // IEnumGUID::Next response shape: [out, size_is(celt), length_is(*pceltFetched)] GUID* rgelt
        // marshaled as a varying-conformant array (max_count + offset + actual_count + elements),
        // followed by [out] ULONG* pceltFetched.
        writer.WriteUInt32((uint)classIds.Length);
        writer.WriteUInt32(0);
        writer.WriteUInt32((uint)classIds.Length);
        for (int i = 0; i < classIds.Length; i++)
        {
            writer.WriteGuid(classIds[i]);
        }

        writer.WriteInt32(fetched);
    });

    private static byte[] WritePayload(NdrWriteAction action)
    {
        var buffer = new byte[4096];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed record SyntheticOpcServerDetails(string ProgId, string UserType, string? VerIndProgId);
}

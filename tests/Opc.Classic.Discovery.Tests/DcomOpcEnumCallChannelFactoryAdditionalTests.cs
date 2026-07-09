// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Discovery.Tests;

public sealed class DcomOpcEnumCallChannelFactoryAdditionalTests
{
    [Test]
    public async Task Constructors_Normalize_activation_protection_to_integrity_or_privacy()
    {
        var defaultFactory = new DcomOpcEnumCallChannelFactory();
        var connectFactory = new DcomOpcEnumCallChannelFactory(CreateConnectData(OpcProtectionLevel.Connect));
        var privacyFactory = new DcomOpcEnumCallChannelFactory(CreateConnectData(OpcProtectionLevel.Privacy));
        var injectedFactory = new DcomOpcEnumCallChannelFactory(
            new DcomCallChannelFactory(new ThrowingTransportFactory()),
            static () => NoOpAuthContext.Instance,
            OpcProtectionLevel.None);

        await Assert.That(defaultFactory.ActivationProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
        await Assert.That(connectFactory.ActivationProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
        await Assert.That(privacyFactory.ActivationProtectionLevel).IsEqualTo(OpcProtectionLevel.Privacy);
        await Assert.That(injectedFactory.ActivationProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task Create_channel_methods_Validate_inputs_before_transport_connection()
    {
        var factory = new DcomOpcEnumCallChannelFactory(
            new DcomCallChannelFactory(new ThrowingTransportFactory()),
            static () => NoOpAuthContext.Instance);
        IOpcInterfaceRef interfaceRef = CreateInterfaceRef(["object-host[49153]"]);

        await Assert.That(async () => await factory.CreateActivationChannelAsync("  ", CancellationToken.None)).Throws<ArgumentException>();
        await Assert.That(async () => await factory.CreateObjectChannelAsync("opc-host", null!, OpcGuids.IID_IOPCServerList, CancellationToken.None)).Throws<ArgumentNullException>();
        await Assert.That(async () => await factory.CreateObjectChannelAsync(" ", interfaceRef, OpcGuids.IID_IOPCServerList, CancellationToken.None)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ResolveObjectEndpoint_Uses_tcp_resolver_binding_when_present()
    {
        IOpcInterfaceRef interfaceRef = CreateInterfaceRef(["ignored-ncacn", "object-host[49153]"]);

        var endpoint = (DnsEndPoint)InvokeStatic<EndPoint>("ResolveObjectEndpoint", "fallback-host", interfaceRef);

        await Assert.That(endpoint.Host).IsEqualTo("object-host");
        await Assert.That(endpoint.Port).IsEqualTo(49153);
    }

    [Test]
    public async Task ResolveObjectEndpoint_Falls_back_to_endpoint_mapper_without_tcp_binding()
    {
        IOpcInterfaceRef interfaceRef = new OpcInterfaceRef(
            OpcGuids.IID_IOPCServerList2,
            flags: 0,
            publicRefs: 1,
            oxid: 2,
            oid: 3,
            ipid: Guid.Parse("10138C2C-0000-0000-0000-00000000D201"),
            securityOffset: 0,
            resolverBindings: [0]);

        var endpoint = (DnsEndPoint)InvokeStatic<EndPoint>("ResolveObjectEndpoint", "fallback-host", interfaceRef);

        await Assert.That(endpoint.Host).IsEqualTo("fallback-host");
        await Assert.That(endpoint.Port).IsEqualTo(135);
    }

    [Test]
    public async Task ResolveDataPortEndpoint_Uses_oxid_data_port_and_fallback_host()
    {
        IOpcInterfaceRef interfaceRef = CreateInterfaceRef(["object-host[49153]"]);

        var endpoint = (DnsEndPoint)InvokeStatic<DnsEndPoint>(
            "ResolveDataPortEndpoint",
            "fallback-host",
            interfaceRef,
            new ReadOnlyMemory<byte>(CreateOxidBindings("[57539]")));

        await Assert.That(endpoint.Host).IsEqualTo("fallback-host");
        await Assert.That(endpoint.Port).IsEqualTo(57539);
    }

    [Test]
    public async Task ResolveDataPortEndpoint_Falls_back_to_resolver_binding_when_oxid_binding_has_no_port()
    {
        IOpcInterfaceRef interfaceRef = CreateInterfaceRef(["ignored-ncacn", "object-host[49153]"]);

        var endpoint = (DnsEndPoint)InvokeStatic<DnsEndPoint>(
            "ResolveDataPortEndpoint",
            "fallback-host",
            interfaceRef,
            new ReadOnlyMemory<byte>(CreateOxidBindings("data-host")));

        await Assert.That(endpoint.Host).IsEqualTo("object-host");
        await Assert.That(endpoint.Port).IsEqualTo(49153);
    }

    [Test]
    public async Task TryParseHostPort_Parses_host_port_and_rejects_missing_brackets()
    {
        object?[] validArgs = ["data-host[49154]", "fallback-host", null, 0];
        object?[] fallbackArgs = ["[49155]", "fallback-host", null, 0];
        object?[] invalidArgs = ["data-host", "fallback-host", null, 0];

        bool valid = InvokeStatic<bool>("TryParseHostPort", validArgs);
        bool fallback = InvokeStatic<bool>("TryParseHostPort", fallbackArgs);
        bool invalid = InvokeStatic<bool>("TryParseHostPort", invalidArgs);

        await Assert.That(valid).IsTrue();
        await Assert.That(validArgs[2]).IsEqualTo("data-host");
        await Assert.That(validArgs[3]).IsEqualTo(49154);
        await Assert.That(fallback).IsTrue();
        await Assert.That(fallbackArgs[2]).IsEqualTo("fallback-host");
        await Assert.That(fallbackArgs[3]).IsEqualTo(49155);
        await Assert.That(invalid).IsFalse();
        await Assert.That(invalidArgs[2]).IsEqualTo(string.Empty);
        await Assert.That(invalidArgs[3]).IsEqualTo(0);
    }

    private static OpcConnectData CreateConnectData(OpcProtectionLevel protectionLevel) =>
        OpcConnectData.WithNtlmV2(
            OpcUrl.Parse("opcda://opc-host/OPC.ServerList.1"),
            new NetworkCredential("operator", "password"),
            protectionLevel);

    private static IOpcInterfaceRef CreateInterfaceRef(IReadOnlyList<string> networkAddresses)
    {
        var resolverBindings = new List<ushort>();
        resolverBindings.Add(0x09);
        AddNullTerminatedString(resolverBindings, networkAddresses[0]);
        if (networkAddresses.Count > 1)
        {
            resolverBindings.Add(0x07);
            AddNullTerminatedString(resolverBindings, networkAddresses[1]);
        }

        return new OpcInterfaceRef(
            OpcGuids.IID_IOPCServerList2,
            flags: 0,
            publicRefs: 1,
            oxid: 2,
            oid: 3,
            ipid: Guid.Parse("10138C2C-0000-0000-0000-00000000D202"),
            securityOffset: 0,
            resolverBindings);
    }

    private static void AddNullTerminatedString(List<ushort> entries, string value)
    {
        foreach (char c in value)
        {
            entries.Add(c);
        }

        entries.Add(0);
    }

    private static byte[] CreateOxidBindings(string address)
    {
        var entries = new List<ushort> { 0, 0, 0x07 };
        AddNullTerminatedString(entries, address);
        entries[1] = checked((ushort)entries.Count);

        byte[] bytes = new byte[entries.Count * sizeof(ushort)];
        for (int i = 0; i < entries.Count; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(i * sizeof(ushort)), entries[i]);
        }

        return bytes;
    }

    private static T InvokeStatic<T>(string methodName, params object?[] args)
    {
        MethodInfo method = typeof(DcomOpcEnumCallChannelFactory).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic)!;
        try
        {
            return (T)method.Invoke(null, args)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private sealed class ThrowingTransportFactory : IAsyncTransportFactory
    {
        public ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            _ = endpoint;
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Transport connection should not be attempted by validation-only tests.");
        }
    }
}

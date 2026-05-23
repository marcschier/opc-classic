//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Opc.Classic.Testing;
using Opc.Classic.Transport;
using TUnit.Core;

namespace Opc.Classic.Tests.Transport;

public sealed class AsyncTransportTests
{
    [Test]
    public async Task InMemoryAsyncTransport_round_trips_bytes_via_pipes()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] payload = [0x01, 0x02, 0x03, 0x04];

        await transport.WriteInboundAsync(payload);

        ReadResult result = await transport.Input.ReadAsync();
        byte[] actual = result.Buffer.ToArray();
        transport.Input.AdvanceTo(result.Buffer.End);

        await Assert.That(actual).IsEquivalentTo(payload);
    }

    [Test]
    public async Task InMemoryAsyncTransport_FlushAsync_advances_outbound_reader()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] payload = [0x10, 0x20, 0x30];

        Memory<byte> writable = transport.Output.GetMemory(payload.Length);
        payload.CopyTo(writable);
        transport.Output.Advance(payload.Length);
        await transport.FlushAsync();

        ReadResult result = await transport.ReadOutbound.ReadAsync();
        byte[] actual = result.Buffer.ToArray();
        transport.ReadOutbound.AdvanceTo(result.Buffer.End);

        await Assert.That(actual).IsEquivalentTo(payload);
    }

    [Test]
    public async Task InMemoryAsyncTransport_DisposeAsync_completes_writers()
    {
        var transport = new InMemoryAsyncTransport();

        await transport.DisposeAsync();

        ReadResult inbound = await transport.Input.ReadAsync();
        ReadResult outbound = await transport.ReadOutbound.ReadAsync();
        transport.Input.AdvanceTo(inbound.Buffer.End);
        transport.ReadOutbound.AdvanceTo(outbound.Buffer.End);

        await Assert.That(inbound.IsCompleted).IsTrue();
        await Assert.That(outbound.IsCompleted).IsTrue();
    }

    [Test]
    public async Task IAsyncTransport_RemoteEndpoint_is_set()
    {
        await using IAsyncTransport transport = new InMemoryAsyncTransport();

        var endpoint = transport.RemoteEndpoint as IPEndPoint;

        await Assert.That(endpoint is not null).IsTrue();
        await Assert.That(endpoint!.Address).IsEqualTo(IPAddress.Loopback);
        await Assert.That(endpoint.Port).IsEqualTo(0);
    }

    [Test]
    public async Task IAsyncTransport_contract_is_AOT_clean()
    {
        Type[] contractTypes =
        [
            typeof(IAsyncTransport),
            typeof(IAsyncTransportFactory),
            typeof(IAsyncEndpoint),
        ];
        Type[] disallowedAttributes =
        [
            typeof(RequiresAssemblyFilesAttribute),
            typeof(RequiresDynamicCodeAttribute),
            typeof(RequiresUnreferencedCodeAttribute),
        ];
        string[] referencedAssemblyNames = typeof(IAsyncTransport).Assembly
            .GetReferencedAssemblies()
            .Select(static assemblyName => assemblyName.Name ?? string.Empty)
            .ToArray();

        foreach (Type contractType in contractTypes)
        {
            foreach (Type disallowedAttribute in disallowedAttributes)
            {
                int attributeCount = contractType.GetCustomAttributes(disallowedAttribute, inherit: false).Length;
                await Assert.That(attributeCount).IsEqualTo(0);
            }
        }

        string reflectionEmitAssemblyName = "System.Reflection.Emit";
        string lightweightEmitAssemblyName = "System.Reflection.Emit.Lightweight";
        await Assert.That(referencedAssemblyNames.Contains(reflectionEmitAssemblyName, StringComparer.Ordinal)).IsFalse();
        await Assert.That(referencedAssemblyNames.Contains(lightweightEmitAssemblyName, StringComparer.Ordinal)).IsFalse();
    }
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Net;
using System.Text;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class DualStringArrayResolverTests
{
    [Test]
    public async Task ResolveFirstTransport_returns_null_for_empty_buffer()
    {
        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("localhost", ReadOnlySpan<byte>.Empty);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ResolveFirstTransport_decodes_tcp_binding_with_port()
    {
        byte[] bindings = BuildDualStringArray((0x0007, "myhost[8194]"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", bindings);

        await Assert.That(result).IsTypeOf<DnsEndPoint>();
        var dns = (DnsEndPoint)result!;
        await Assert.That(dns.Host).IsEqualTo("myhost");
        await Assert.That(dns.Port).IsEqualTo(8194);
    }

    [Test]
    public async Task ResolveFirstTransport_decodes_named_pipe_dce_form()
    {
        byte[] bindings = BuildDualStringArray((0x000F, "computer[\\PIPE\\OPCxxx]"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", bindings);

        await Assert.That(result).IsTypeOf<NcacnNpEndPoint>();
        var pipe = (NcacnNpEndPoint)result!;
        await Assert.That(pipe.Host).IsEqualTo("computer");
        await Assert.That(pipe.PipeName).IsEqualTo("OPCxxx");
    }

    [Test]
    public async Task ResolveFirstTransport_decodes_named_pipe_unc_form()
    {
        byte[] bindings = BuildDualStringArray((0x000F, "\\\\.\\PIPE\\OPCxxx"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("computer", bindings);

        await Assert.That(result).IsTypeOf<NcacnNpEndPoint>();
        var pipe = (NcacnNpEndPoint)result!;
        await Assert.That(pipe.Host).IsEqualTo("computer");
        await Assert.That(pipe.PipeName).IsEqualTo("OPCxxx");
    }

    [Test]
    public async Task ResolveFirstTransport_returns_first_recognised_binding()
    {
        byte[] bindings = BuildDualStringArray(
            (0x0099, "ignored"),
            (0x0007, "host[8194]"),
            (0x000F, "host[\\PIPE\\foo]"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", bindings);

        await Assert.That(result).IsTypeOf<DnsEndPoint>();
    }

    [Test]
    public async Task ResolveFirstNamedPipe_skips_tcp_binding()
    {
        byte[] bindings = BuildDualStringArray(
            (0x0007, "host[8194]"),
            (0x000F, "host[\\PIPE\\foo]"));

        NcacnNpEndPoint? pipe = DualStringArrayResolver.ResolveFirstNamedPipe("fallback", bindings);

        await Assert.That(pipe).IsNotNull();
        await Assert.That(pipe!.PipeName).IsEqualTo("foo");
    }

    [Test]
    public async Task ResolveFirstTcp_skips_pipe_binding()
    {
        byte[] bindings = BuildDualStringArray(
            (0x000F, "host[\\PIPE\\foo]"),
            (0x0007, "host[8194]"));

        DnsEndPoint? dns = DualStringArrayResolver.ResolveFirstTcp("fallback", bindings);

        await Assert.That(dns).IsNotNull();
        await Assert.That(dns!.Port).IsEqualTo(8194);
    }

    [Test]
    public async Task ResolveFirstTransport_handles_resolver_bindings_ushort_array()
    {
        ushort[] entries = BuildResolverBindings(
            (0x0007, "host[8194]"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", entries);

        await Assert.That(result).IsTypeOf<DnsEndPoint>();
        var dns = (DnsEndPoint)result!;
        await Assert.That(dns.Port).IsEqualTo(8194);
    }

    [Test]
    public async Task ResolveFirstTransport_uses_fallback_when_pipe_host_is_dot()
    {
        byte[] bindings = BuildDualStringArray((0x000F, "\\\\.\\PIPE\\foo"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", bindings);

        await Assert.That(result).IsTypeOf<NcacnNpEndPoint>();
        var pipe = (NcacnNpEndPoint)result!;
        await Assert.That(pipe.Host).IsEqualTo("fallback");
    }

    [Test]
    public async Task ResolveFirstTransport_skips_tcp_binding_without_port_suffix()
    {
        byte[] bindings = BuildDualStringArray(
            (0x0007, "host-only"),
            (0x0007, "host[8194]"));

        EndPoint? result = DualStringArrayResolver.ResolveFirstTransport("fallback", bindings);

        await Assert.That(result).IsTypeOf<DnsEndPoint>();
        var dns = (DnsEndPoint)result!;
        await Assert.That(dns.Port).IsEqualTo(8194);
    }

    private static byte[] BuildDualStringArray(params (ushort TowerId, string Address)[] entries)
    {
        // DUALSTRINGARRAY wire layout (MS-DCOM 2.2.19.1):
        //   ushort wNumEntries;       <-- total UCS-2 chars in StringArray + SecurityArray + terminators
        //   ushort wSecurityOffset;   <-- offset within StringArray where SECURITYBINDING entries begin
        //   ushort StringArray[*];    <-- sequence of STRINGBINDING entries
        //   ushort SecurityArray[*];  <-- sequence of SECURITYBINDING entries
        //
        // Each STRINGBINDING is { TowerId(ushort), Address(ushort* NUL), Terminator(0) }.
        // The whole StringArray ends with an extra terminating 0.
        var stringWords = new List<ushort>();
        foreach ((ushort tower, string address) in entries)
        {
            stringWords.Add(tower);
            foreach (char ch in address)
            {
                stringWords.Add((char)ch);
            }
            stringWords.Add(0);
        }
        stringWords.Add(0);

        // Security binding terminator: just a NUL ushort.
        var securityWords = new List<ushort> { 0 };

        int numEntries = stringWords.Count + securityWords.Count;
        int secOffset = stringWords.Count;

        var buffer = new byte[4 + (numEntries * 2)];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(0), (ushort)numEntries);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2), (ushort)(secOffset + 2));
        int offset = 4;
        foreach (ushort word in stringWords)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), word);
            offset += 2;
        }
        foreach (ushort word in securityWords)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), word);
            offset += 2;
        }
        return buffer;
    }

    private static ushort[] BuildResolverBindings(params (ushort TowerId, string Address)[] entries)
    {
        var words = new List<ushort>();
        foreach ((ushort tower, string address) in entries)
        {
            words.Add(tower);
            foreach (char ch in address)
            {
                words.Add(ch);
            }
            words.Add(0);
        }
        words.Add(0);
        return words.ToArray();
    }
}

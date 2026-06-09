//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Opc.Classic.Security;
using TUnit.Core;

namespace Opc.Classic.Tests.Security;

public sealed class ChannelBindingsTests {
    private const string TlsServerEndpointPrefix = "tls-server-end-point:";
    private static readonly byte[] EmptyChannelBindingsHash = Convert.FromHexString("441018525208457705BF09A8EE3C1093");

    [Test]
    public async Task EmptyChannelBindingsHash_MatchesSerializedEmptyStruct() {
        var bindings = new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: ReadOnlyMemory<byte>.Empty,
            ApplicationData: ReadOnlyMemory<byte>.Empty);

        byte[] actual = ChannelBindingsHash.Compute(bindings);

        await Assert.That(actual.SequenceEqual(EmptyChannelBindingsHash)).IsTrue();
    }

    [Test]
    public async Task ForTlsServerCert_HashesPrefixedSha256CertificateDigest() {
        byte[] certDer = new byte[32];
        byte[] certHash = SHA256.HashData(certDer);
        byte[] expectedApplicationData = BuildTlsServerEndpointApplicationData(certHash);
        byte[] expected = ComputeExpectedHash(expectedApplicationData);

        byte[] actual = ChannelBindingsHash.ForTlsServerCert(certDer);

        await Assert.That(actual.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task ForTlsServerEndpoint_ApplicationDataStartsWithAsciiPrefix() {
        byte[] certDer = new byte[32];
        ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(certDer);
        byte[] prefixBytes = Encoding.ASCII.GetBytes(TlsServerEndpointPrefix);
        byte[] applicationData = bindings.ApplicationData.ToArray();

        await Assert.That(prefixBytes.Length).IsEqualTo(21);
        await Assert.That(applicationData.AsSpan(0, prefixBytes.Length).SequenceEqual(prefixBytes)).IsTrue();
    }

    [Test]
    public async Task Compute_IsDeterministicAndReturnsMd5Length() {
        var bindings = new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: new byte[] { 1, 2, 3 },
            AcceptorAddrType: 0,
            AcceptorAddress: new byte[] { 4, 5 },
            ApplicationData: Encoding.ASCII.GetBytes("app-data"));

        byte[] first = ChannelBindingsHash.Compute(bindings);
        byte[] second = ChannelBindingsHash.Compute(bindings);

        await Assert.That(first.Length).IsEqualTo(16);
        await Assert.That(first.SequenceEqual(second)).IsTrue();
    }

    private static byte[] BuildTlsServerEndpointApplicationData(byte[] certHash) {
        byte[] prefixBytes = Encoding.ASCII.GetBytes(TlsServerEndpointPrefix);
        var appData = new byte[prefixBytes.Length + certHash.Length];
        Buffer.BlockCopy(prefixBytes, 0, appData, 0, prefixBytes.Length);
        Buffer.BlockCopy(certHash, 0, appData, prefixBytes.Length, certHash.Length);
        return appData;
    }

    private static byte[] ComputeExpectedHash(ReadOnlySpan<byte> applicationData) {
        var buffer = new byte[4 + 4 + 4 + 4 + 4 + applicationData.Length];
        int offset = 0;

        void WriteU32(uint value) {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), value);
            offset += 4;
        }

        WriteU32(0);
        WriteU32(0);
        WriteU32(0);
        WriteU32(0);
        WriteU32((uint)applicationData.Length);
        applicationData.CopyTo(buffer.AsSpan(offset));

#pragma warning disable CA5351 // Expected value mirrors the MS-NLMP channel-bindings checksum.
        return MD5.HashData(buffer);
#pragma warning restore CA5351
    }
}

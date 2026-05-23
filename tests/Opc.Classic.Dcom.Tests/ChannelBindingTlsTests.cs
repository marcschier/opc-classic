//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Security;
using SharpInterop.Rpc.Auth.ntlm;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class ChannelBindingTlsTests
{
    private const ushort MsvAvChannelBindings = 0x000A;
    private const string TlsServerEndpointPrefix = "tls-server-end-point:";
    private const string Sha256CertFile = "tls-server-endpoint-sha256.cer";
    private const string Sha384CertFile = "tls-server-endpoint-sha384.cer";

    [Test]
    public async Task FixedSha256Certificate_ProducesExpectedTlsServerEndpointApplicationData()
    {
        byte[] certDer = ReadTestCertificate(Sha256CertFile);
        byte[] expectedDigest = Convert.FromHexString("8A0769B7D62A1699BADEA8E3EC0B97CF7DC459FB61CA3E098F3641E3C4974D85");
        byte[] expected = BuildExpectedApplicationData(expectedDigest);

        ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(certDer);
        byte[] actual = bindings.ApplicationData.ToArray();

        await Assert.That(actual.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task Tls13Sha384Certificate_UsesSha384ForTlsServerEndpointApplicationData()
    {
        byte[] certDer = ReadTestCertificate(Sha384CertFile);
        byte[] expectedDigest = Convert.FromHexString(
            "B8B2EF7D3076E418A7BCDE700F90E70291D2F7D9255BDE6A50239422F938A6D4F1FB05218082B70C4CAA0133277513E0");
        byte[] expected = BuildExpectedApplicationData(expectedDigest);

        ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(certDer, SslProtocols.Tls13);
        byte[] actual = bindings.ApplicationData.ToArray();

        await Assert.That(actual.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task NtlmAuthenticate_IncludesMsvAvChannelBindings_WhenTlsBindingHashIsConfigured()
    {
        byte[] channelBindingsHash = ChannelBindingsHash.ForTlsServerCert(ReadTestCertificate(Sha256CertFile));
        Type3Message type3 = CreateNtlmType3(channelBindingsHash);

        bool found = TryGetNtlmV2AvPair(type3.GetNTResponse(), MsvAvChannelBindings, out byte[] actual);

        await Assert.That(found).IsTrue();
        await Assert.That(actual.SequenceEqual(channelBindingsHash)).IsTrue();
    }

    [Test]
    public async Task NtlmAuthenticate_NoTlsOmitsOrZerosMsvAvChannelBindings()
    {
        Type3Message type3 = CreateNtlmType3(channelBindingsHash: null);

        bool found = TryGetNtlmV2AvPair(type3.GetNTResponse(), MsvAvChannelBindings, out byte[] actual);

        if (found)
        {
            await Assert.That(actual.All(static b => b == 0)).IsTrue();
        }
        else
        {
            await Assert.That(found).IsFalse();
        }
    }

    [Test]
    public async Task NtlmServerRoundTrip_VerifiesMatchingTlsChannelBindings()
    {
        byte[] channelBindingsHash = ChannelBindingsHash.ForTlsServerCert(ReadTestCertificate(Sha256CertFile));
        var client = CreateNtlmAuthentication(channelBindingsHash);
        var server = CreateNtlmAuthentication(channelBindingsHash);

        Type3Message type3 = client.CreateType3(server.CreateType2(client.CreateType1()));

        InvokeCreateSecurityWhenServer(server, type3);

        await Assert.That(server.Security).IsNotNull();
    }

    [Test]
    public async Task SslStreamLoopback_ExtractsServerCertificateAndComputesTlsServerEndpointCbt()
    {
        using X509Certificate2 certificate = CreateLoopbackCertificate();
        using var listener = new TcpListener(IPAddress.Loopback, port: 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        Task<Exception?> serverTask = RunSslServerAsync(listener, certificate);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        using var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);

        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = "localhost",
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            RemoteCertificateValidationCallback = static (_, _, _, _) => true,
        });

        ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(sslStream);
        byte[] remoteCertificateDer = sslStream.RemoteCertificate!.Export(X509ContentType.Cert);
        byte[] expectedDigest = sslStream.SslProtocol == SslProtocols.Tls13
            ? SHA384.HashData(remoteCertificateDer)
            : SHA256.HashData(remoteCertificateDer);
        byte[] expected = BuildExpectedApplicationData(expectedDigest);
        Exception? serverException = await serverTask;

        await Assert.That(bindings.ApplicationData.ToArray().SequenceEqual(expected)).IsTrue();
        await Assert.That(serverException).IsNull();
    }

    private static byte[] ReadTestCertificate(string fileName) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", fileName));

    private static byte[] BuildExpectedApplicationData(byte[] digest)
    {
        byte[] prefix = System.Text.Encoding.ASCII.GetBytes(TlsServerEndpointPrefix);
        var expected = new byte[prefix.Length + digest.Length];
        Buffer.BlockCopy(prefix, 0, expected, 0, prefix.Length);
        Buffer.BlockCopy(digest, 0, expected, prefix.Length, digest.Length);
        return expected;
    }

    private static Type3Message CreateNtlmType3(byte[]? channelBindingsHash)
    {
        var client = CreateNtlmAuthentication(channelBindingsHash);
        var server = CreateNtlmAuthentication(channelBindingsHash: null);
        Type2Message type2 = server.CreateType2(client.CreateType1());
        return client.CreateType3(type2);
    }

    private static NtlmAuthentication CreateNtlmAuthentication(byte[]? channelBindingsHash)
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "true");
        properties.SetProperty("rpc.ntlm.seal", "true");
        properties.SetProperty("rpc.ntlm.keyExchange", "true");
        properties.SetProperty("rpc.ntlm.keyLength", "128");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.allowV1", "false");
        properties.SetProperty("rpc.ntlm.sso", "false");
        properties.SetProperty("rpc.ntlm.domain", "DOMAIN");
        properties.SetProperty(SharpInterop.Rpc.Security.USERNAME, "User");
        properties.SetProperty(SharpInterop.Rpc.Security.PASSWORD, "Password");
        if (channelBindingsHash is not null)
        {
            properties.SetProperty("rpc.ntlm.channelBindingsHash", channelBindingsHash);
        }

        return new NtlmAuthentication(properties);
    }

    private static bool TryGetNtlmV2AvPair(byte[] ntResponse, ushort avId, out byte[] value)
    {
        const int ntProofLength = 16;
        const int avPairsOffsetInBlob = 28;
        int offset = ntProofLength + avPairsOffsetInBlob;
        while (offset + 4 <= ntResponse.Length)
        {
            ushort currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(ntResponse.AsSpan(offset, sizeof(ushort)));
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(ntResponse.AsSpan(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > ntResponse.Length - offset)
            {
                break;
            }

            if (currentAvId == 0)
            {
                break;
            }

            if (currentAvId == avId)
            {
                value = ntResponse.AsSpan(offset, length).ToArray();
                return true;
            }

            offset += length;
        }

        value = [];
        return false;
    }

    private static void InvokeCreateSecurityWhenServer(NtlmAuthentication authentication, Type3Message type3)
    {
        MethodInfo method = typeof(NtlmAuthentication).GetMethod(
            "CreateSecurityWhenServer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            method.Invoke(authentication, new object[] { type3 });
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static X509Certificate2 CreateLoopbackCertificate()
    {
        using RSA rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=localhost",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        subjectAlternativeNames.AddDnsName("localhost");
        subjectAlternativeNames.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(subjectAlternativeNames.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, critical: true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));
        using X509Certificate2 certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddMinutes(10));
        return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pkcs12), password: null);
    }

    private static async Task<Exception?> RunSslServerAsync(TcpListener listener, X509Certificate2 certificate)
    {
        try
        {
            using TcpClient serverClient = await listener.AcceptTcpClientAsync();
            using var serverSsl = new SslStream(serverClient.GetStream(), leaveInnerStreamOpen: false);
            await serverSsl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
            {
                ServerCertificate = certificate,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                ClientCertificateRequired = false,
            });
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
        finally
        {
            listener.Stop();
        }
    }
}

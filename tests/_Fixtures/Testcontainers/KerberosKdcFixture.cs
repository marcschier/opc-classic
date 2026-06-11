//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Opc.Classic.Tests.Fixtures;

/// <summary>
/// Testcontainers-based MIT Kerberos KDC for cross-process integration tests.
/// Run as <c>await using var kdc = await KerberosKdcFixture.StartAsync();</c>
/// and use <c>kdc.Realm</c>, <c>kdc.Kdc</c> for client configuration.
/// </summary>
public sealed class KerberosKdcFixture : IAsyncDisposable
{
    private const string Image = "gcavalcante8808/krb5-server:latest";
    private const int KdcPort = 88;
    private const string MasterPassword = "testcontainers";

    private readonly IContainer _container;

    private KerberosKdcFixture(IContainer container, string realm)
    {
        _container = container;
        Realm = realm;
    }

    public string Realm { get; }

    public string Host => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(KdcPort);

    public string Kdc => $"{Host}:{Port}";

    public static async Task<KerberosKdcFixture> StartAsync(
        string realm = "EXAMPLE.COM",
        CancellationToken cancellationToken = default)
    {
        var container = new ContainerBuilder()
            .WithImage(Image)
            .WithPortBinding(KdcPort, true)
            .WithEnvironment("KRB5_REALM", realm)
            .WithEnvironment("KRB5_KDC", "localhost")
            .WithEnvironment("KRB5_PASS", MasterPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(KdcPort))
            .Build();

        await container.StartAsync(cancellationToken).ConfigureAwait(false);
        return new KerberosKdcFixture(container, realm);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync().ConfigureAwait(false);
    }
}

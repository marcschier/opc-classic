//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics;
using System.Text;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Kerberos.NET.Crypto;
using TUnit.Core.Interfaces;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KdcFixture : IAsyncInitializer, IAsyncDisposable
{
    public const string RunEnvironmentVariable = "OPC_CLASSIC_RUN_KDC_TESTS";
    public const string RealmName = "OPCCLASSIC.LOCAL";
    public const string TestUserName = "testuser";
    public const string TestUserPassword = "correct horse battery staple";
    public const string ServerSpn = "host/opcserver.opcclassic.local";
    public const string ClientSpn = "host/opcclient.opcclassic.local";
    public const string ShortLivedServerSpn = "host/short.opcclassic.local";
    private const int KdcContainerPort = 88;
    private const string MasterPassword = "testcontainers-master";

    private IContainer? _container;
    private string? _imageName;
    private string? _keytabDirectory;
    private KeyTable? _serverKeyTable;
    private KeyTable? _shortLivedServerKeyTable;
    private KeyTable? _testUserKeyTable;

    public string Realm => RealmName;
    public string KdcHost => _container?.Hostname ?? "127.0.0.1";
    public int KdcPort => _container?.GetMappedPublicPort(KdcContainerPort) ?? 0;
    public string KdcEndpoint => $"{KdcHost}:{KdcPort}";
    public string Krb5Conf { get; private set; } = string.Empty;
    public string Krb5ConfPath { get; private set; } = string.Empty;
    public string TestUserKeytabPath { get; private set; } = string.Empty;
    public KeyTable ServerKeyTable => _serverKeyTable ?? throw new InvalidOperationException("The KDC fixture is not initialized.");
    public KeyTable ShortLivedServerKeyTable => _shortLivedServerKeyTable ?? throw new InvalidOperationException("The KDC fixture is not initialized.");
    public KeyTable TestUserKeyTable => _testUserKeyTable ?? throw new InvalidOperationException("The KDC fixture is not initialized.");
    public string? SkipReason { get; private set; }
    public bool IsAvailable => SkipReason is null;

    public async Task InitializeAsync()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable(RunEnvironmentVariable), "1", StringComparison.Ordinal))
        {
            SkipReason = $"Requires Docker — set {RunEnvironmentVariable}=1 to enable.";
            return;
        }

        if (!IsDockerDaemonAvailable())
        {
            SkipReason = $"Requires Docker — set {RunEnvironmentVariable}=1 to enable and start Docker.";
            return;
        }

        _keytabDirectory = Path.Combine(AppContext.BaseDirectory, "kerberos-kdc");
        Directory.CreateDirectory(_keytabDirectory);

        _imageName = $"opc-classic-krb5-kdc:{Guid.NewGuid():N}";
        await BuildImageAsync(_imageName, FindKdcDockerfileDirectory(), CancellationToken.None).ConfigureAwait(false);

        _container = new ContainerBuilder()
            .WithImage(_imageName)
            .WithPortBinding(KdcContainerPort, true)
            .WithEnvironment("KRB5_REALM", RealmName)
            .WithEnvironment("KRB5_MASTER_PASSWORD", MasterPassword)
            .WithEnvironment("KRB5_TESTUSER_PASSWORD", TestUserPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(KdcContainerPort))
            .Build();

        await _container.StartAsync().ConfigureAwait(false);

        Krb5Conf = CreateKrb5Conf(KdcEndpoint);
        Krb5ConfPath = Path.Combine(_keytabDirectory, "krb5.conf");
        await File.WriteAllTextAsync(Krb5ConfPath, Krb5Conf).ConfigureAwait(false);

        _serverKeyTable = await ReadKeytabAsync("/keytabs/server.keytab", "server.keytab").ConfigureAwait(false);
        _shortLivedServerKeyTable = await ReadKeytabAsync("/keytabs/short.keytab", "short.keytab").ConfigureAwait(false);
        _testUserKeyTable = await ReadKeytabAsync("/keytabs/testuser.keytab", "testuser.keytab").ConfigureAwait(false);
    }

    public KerberosAuthInfo CreatePasswordAuthInfo(string spn = ServerSpn) =>
        new(RealmName, spn, TestUserName, null, TestUserPassword, null);

    public KerberosAuthInfo CreateUserKeytabAuthInfo(string spn = ServerSpn) =>
        new(RealmName, spn, TestUserName, null, null, TestUserKeytabPath);

    public IDisposable UseKrb5Config() => new EnvironmentVariableScope("KRB5_CONFIG", Krb5ConfPath);

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }

        if (_imageName is not null)
        {
            await RunProcessAsync("docker", ["image", "rm", "-f", _imageName], TimeSpan.FromMinutes(1), CancellationToken.None, throwOnError: false)
                .ConfigureAwait(false);
        }

        if (_keytabDirectory is not null && Directory.Exists(_keytabDirectory))
        {
            Directory.Delete(_keytabDirectory, recursive: true);
        }
    }

    private static string CreateKrb5Conf(string kdcEndpoint) =>
        "[libdefaults]\n" +
        "    default_realm = OPCCLASSIC.LOCAL\n" +
        "    dns_lookup_kdc = false\n" +
        "    dns_lookup_realm = false\n" +
        "    dns_canonicalize_hostname = false\n" +
        "    udp_preference_limit = 1\n" +
        "    default_tgs_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96\n" +
        "    default_tkt_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96\n" +
        "    permitted_enctypes = aes256-cts-hmac-sha1-96 aes128-cts-hmac-sha1-96\n" +
        "[realms]\n" +
        "    OPCCLASSIC.LOCAL = {\n" +
        $"        kdc = {kdcEndpoint}\n" +
        "    }\n" +
        "[domain_realm]\n" +
        "    .opcclassic.local = OPCCLASSIC.LOCAL\n" +
        "    opcclassic.local = OPCCLASSIC.LOCAL\n";

    private async Task<KeyTable> ReadKeytabAsync(string containerPath, string fileName)
    {
        if (_container is null || _keytabDirectory is null)
        {
            throw new InvalidOperationException("The KDC fixture is not initialized.");
        }

        byte[] keytabBytes = await _container.ReadFileAsync(containerPath).ConfigureAwait(false);
        string hostPath = Path.Combine(_keytabDirectory, fileName);
        await File.WriteAllBytesAsync(hostPath, keytabBytes).ConfigureAwait(false);

        if (fileName.Equals("testuser.keytab", StringComparison.Ordinal))
        {
            TestUserKeytabPath = hostPath;
        }

        return new KeyTable(keytabBytes);
    }

    private static string FindKdcDockerfileDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException("Could not locate repository root containing Opc.Classic.slnx.");
        }

        return Path.Combine(directory.FullName, "tests", "Opc.Classic.Dcom.Kerberos.Tests", "krb5-kdc");
    }

    private static Task BuildImageAsync(string imageName, string dockerfileDirectory, CancellationToken cancellationToken) =>
        RunProcessAsync("docker", ["build", "-t", imageName, dockerfileDirectory], TimeSpan.FromMinutes(5), cancellationToken);

    private static bool IsDockerDaemonAvailable()
    {
        try
        {
            RunProcessAsync("docker", ["info", "--format", "{{.ServerVersion}}"], TimeSpan.FromSeconds(5), CancellationToken.None).GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task RunProcessAsync(
        string fileName,
        string[] arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken,
        bool throwOnError = true)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var output = new StringBuilder();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        foreach (string argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.OutputDataReceived += (_, args) => AppendLine(output, args.Data);
        process.ErrorDataReceived += (_, args) => AppendLine(output, args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            TryKill(process);
            if (throwOnError)
            {
                throw new TimeoutException($"{fileName} {string.Join(' ', arguments)} timed out after {timeout}.");
            }

            return;
        }

        if (throwOnError && process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} {string.Join(' ', arguments)} failed with exit code {process.ExitCode}: {output}");
        }
    }

    private static void AppendLine(StringBuilder builder, string? value)
    {
        if (value is not null)
        {
            builder.AppendLine(value);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _previousValue;

        public EnvironmentVariableScope(string name, string value)
        {
            _name = name;
            _previousValue = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_name, _previousValue);
        }
    }
}

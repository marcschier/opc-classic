// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Tests;

public sealed class DocumentationConsistencyTests
{
    [Test]
    public async Task TestServer_fresh_machine_setup_requires_registration_and_scm_acl_grant()
    {
        string repositoryRoot = FindRepositoryRoot();
        string document = File.ReadAllText(Path.Combine(repositoryRoot, "interop", "docs", "testserver.md"));
        string normalizedDocument = System.Text.RegularExpressions.Regex.Replace(document, @"\s+", " ");

        await Assert.That(normalizedDocument.Contains(
            @".\interop\tools\register-testserver.ps1",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(normalizedDocument.Contains(
            @".\interop\tools\grant-testserver-acl.ps1",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(normalizedDocument.Contains(
            "`AllowEveryoneAccess` controls the TestServer's application-level OPC access checks",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(normalizedDocument.Contains(
            "so launch/access permissions are permissive by default",
            StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task Packet_privacy_listener_sample_imports_ntlm_auth_namespace()
    {
        string repositoryRoot = FindRepositoryRoot();
        string document = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs",
            "cookbook",
            "07-enabling-packet-privacy.md"));

        await Assert.That(document.Contains(
            "using Opc.Classic.Dcom.Rpc.Auth.ntlm;",
            StringComparison.Ordinal)).IsTrue();
        await Assert.That(document.Contains(
            "using Opc.Classic.Dcom.Rpc.Auth;",
            StringComparison.Ordinal)).IsFalse();
        await Assert.That(typeof(Opc.Classic.Dcom.Rpc.Auth.ntlm.AuthenticationSource).Namespace)
            .IsEqualTo("Opc.Classic.Dcom.Rpc.Auth.ntlm");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}

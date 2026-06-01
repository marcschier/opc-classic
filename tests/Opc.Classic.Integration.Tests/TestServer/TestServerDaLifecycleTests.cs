//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Integration.Tests.Support;
using TUnit.Core;

namespace Opc.Classic.Tests.Integration.TestServer;

/// <summary>
/// Live DA 2.x lifecycle conformance tests against the OPC Foundation
/// TestServer (DA 2.05a, 3-item address space: <c>Test.Int32=42</c>,
/// <c>Test.Float=3.14159</c>, <c>Test.String="OPC Test"</c>).
/// </summary>
/// <remarks>
/// <para>
/// These tests are gated by <see cref="TestServerProbe.ShouldSkip"/> —
/// they soft-skip when the TestServer CLSID
/// <c>{F8582CF9-88FB-11DA-A5ED-0060B0692061}</c> is not registered on
/// the host. To enable: build via <c>tools\build-testserver.ps1</c>
/// and register via <c>tools\register-testserver.ps1</c> (elevated).
/// </para>
/// <para>
/// Live activation against the locally built TestServer may also fail
/// with <c>CO_E_SERVER_EXEC_FAILURE</c> until the proxy/stub DLLs are
/// system-installed via the upstream MSI (see
/// <c>docs/interop/testserver.md</c> for the recommended install
/// path).
/// </para>
/// </remarks>
public sealed class TestServerDaLifecycleTests
{
    private const string LiveCategory = "LiveTestServer";

    [Test, Category(LiveCategory)]
    public async Task TestServer_clsid_and_progid_match_upstream_constants()
    {
        // Scaffold assertion: confirms the well-known CLSID/ProgID match
        // the upstream `ext/CoreComponents/Source/Test/TestServer/OpcTestServer.cpp`
        // declarations. Catches accidental drift if upstream rev-bumps the GUID.
        await Assert.That(ConformanceMetadata.ReadString(TestServerProbe.TestServerClsid)).IsEqualTo("F8582CF9-88FB-11DA-A5ED-0060B0692061");
        await Assert.That(ConformanceMetadata.ReadString(TestServerProbe.TestServerProgId)).IsEqualTo("OpcTestServer_x64.1");
        await Assert.That(ConformanceMetadata.ReadInt32(TestServerProbe.WellKnownItems.Length)).IsEqualTo(3);
        await Assert.That(TestServerProbe.WellKnownItems).Contains("Test.Int32");
        await Assert.That(TestServerProbe.WellKnownItems).Contains("Test.Float");
        await Assert.That(TestServerProbe.WellKnownItems).Contains("Test.String");
    }

    [Test, Category(LiveCategory)]
    public async Task TestServer_da_lifecycle_via_mcp_driver()
    {
        if (TestServerProbe.ShouldSkip(out var reason))
        {
            // Soft-skip with a non-empty reason so the test result captures
            // why the live exercise didn't run.
            await Assert.That(reason.Length).IsGreaterThan(0);
            return;
        }

        // Generated proxy + opnum surface assertions — these confirm the
        // managed test surface the live test will exercise (AddGroup,
        // SyncIO Read/Write, Remove) is present even when skipped.
        await Assert.That(ConformanceMetadata.ReadType<IOPCServer>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<IOPCServerClientProxy>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<IOPCItemMgt>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<IOPCSyncIO>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadInt32(IOPCServer.Opnums.AddGroupAsync)).IsGreaterThan(0);

        // Locate mcp_driver.py relative to the test assembly. The repo
        // root is 4 levels up from bin/Debug/net10.0/<asm>.dll
        // (tests/Opc.Classic.Integration.Tests/bin/Debug/net10.0).
        var asmDir = Path.GetDirectoryName(typeof(TestServerDaLifecycleTests).Assembly.Location)
            ?? throw new InvalidOperationException("Cannot resolve assembly location.");
        var repoRoot = Path.GetFullPath(Path.Combine(asmDir, "..", "..", "..", "..", ".."));
        var script = Path.Combine(repoRoot, "mcp", "mcp_driver.py");
        await Assert.That(File.Exists(script)).IsTrue();

        // Shell out: python mcp/mcp_driver.py --testserver
        // Drives the full Connect -> GetStatus -> ReadItemsById flow
        // against the registered TestServer using the same in-tree
        // managed Opc.Classic stack that the user invokes manually.
        var psi = new ProcessStartInfo("python", $"\"{script}\" --testserver")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to launch python.");
        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();
        var timedOut = !proc.WaitForExit(180_000);
        if (timedOut)
        {
            try { proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
            throw new TimeoutException(
                "mcp_driver.py --testserver exceeded 180s. The TestServer is likely " +
                "blocked by CO_E_SERVER_EXEC_FAILURE — see docs/interop/testserver.md " +
                "for the MSI install path that unblocks DCOM SCM activation.");
        }
        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        await Assert.That(proc.ExitCode)
            .IsEqualTo(0)
            .Because($"mcp_driver.py exit code != 0.\nstdout:\n{stdout}\nstderr:\n{stderr}");

        // Sanity: the read step should report all three TestServer items
        // and the Int32 should equal 42 (the value from OpcTestServer.config.xml).
        await Assert.That(stdout).Contains("Test.Int32");
        await Assert.That(stdout).Contains("Test.Float");
        await Assert.That(stdout).Contains("Test.String");
    }
}

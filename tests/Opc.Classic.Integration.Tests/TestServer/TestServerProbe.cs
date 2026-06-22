// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Tests.Integration.TestServer;

internal static class TestServerProbe
{
    /// <summary>OPC Foundation TestServer x64 CLSID (per
    /// <c>interop/samples/OpcTestServer/OpcTestServer.cpp</c>).</summary>
    public const string TestServerClsid = "F8582CF9-88FB-11DA-A5ED-0060B0692061";

    /// <summary>Matching ProgID emitted by <c>OpcTestServer_x64.exe /regserver</c>
    /// or by <c>interop\tools\register-testserver.ps1</c>.</summary>
    public const string TestServerProgId = "OpcTestServer_x64.1";

    /// <summary>
    /// The three items exposed by <c>OpcTestServer.config.xml</c>.
    /// </summary>
    public static readonly string[] WellKnownItems = ["Test.Int32", "Test.Float", "Test.String"];

    /// <summary>Returns true when the live TestServer integration tests
    /// should be skipped because TestServer isn't registered on the host
    /// or because the explicit opt-in env var <c>OPC_CLASSIC_LIVE_TESTSERVER</c>
    /// is not set to <c>1</c>.</summary>
    public static bool ShouldSkip(out string reason)
    {
        if (!System.OperatingSystem.IsWindows())
        {
            reason = "TestServer conformance tests require Windows";
            return true;
        }

        if (!string.Equals(System.Environment.GetEnvironmentVariable("OPC_CLASSIC_LIVE_TESTSERVER"), "1", StringComparison.Ordinal))
        {
            reason = "Set OPC_CLASSIC_LIVE_TESTSERVER=1 to opt into live TestServer activation tests (default off because " +
                     "DCOM SCM activation requires elevated TestServer and proxy/stub registration — see interop/docs/testserver.md).";
            return true;
        }

        if (!IsRegistered(TestServerClsid))
        {
            reason = $"TestServer CLSID {{{TestServerClsid}}} is not registered. " +
                     "Build with tools\\build-testserver.ps1 and register with " +
                     "tools\\register-testserver.ps1 (elevated).";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool IsRegistered(string clsid)
    {
        if (!System.OperatingSystem.IsWindows())
        {
            return false;
        }

        using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"CLSID\{{{clsid}}}\LocalServer32");
        return key is not null;
    }
}

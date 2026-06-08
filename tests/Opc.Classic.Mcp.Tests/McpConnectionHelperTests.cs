//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Reflection;
using System.Runtime.ExceptionServices;
using Opc.Classic.Mcp.Tools;
using TUnit.Core;

namespace Opc.Classic.Mcp.Tests;

public sealed class McpConnectionHelperTests
{
    [Test]
    public async Task OpcMcpDcomConnectionHelper_NormalizeRequest_Parses_opc_scheme_url()
    {
        object request = InvokeStatic<object>(
            "OpcMcpDcomConnectionHelper",
            "NormalizeRequest",
            "  fallback-host  ",
            "  Old.ProgId  ",
            "  ",
            "DOMAIN\\operator",
            "p@ss",
            true,
            "opcae://opc-remote/Vendor.Server.1",
            "pkt_privacy",
            "opcae");

        await Assert.That(GetProperty<string>(request, "Host")).IsEqualTo("opc-remote");
        await Assert.That(GetProperty<string?>(request, "ProgId")).IsEqualTo("Vendor.Server.1");
        await Assert.That(GetProperty<string?>(request, "Clsid")).IsNull();
        await Assert.That(GetProperty<string?>(request, "Username")).IsEqualTo("DOMAIN\\operator");
        await Assert.That(GetProperty<string?>(request, "Password")).IsEqualTo("p@ss");
        await Assert.That(GetProperty<bool>(request, "UseKerberos")).IsTrue();
        await Assert.That(GetProperty<string?>(request, "ConnectionString")).IsEqualTo("opcae://opc-remote/Vendor.Server.1");
        await Assert.That(GetProperty<string?>(request, "AuthLevel")).IsEqualTo("pkt_privacy");
    }

    [Test]
    public async Task OpcMcpDcomConnectionHelper_NormalizeRequest_Parses_guid_path_as_clsid()
    {
        const string classId = "10138C2C-0000-0000-0000-00000000A001";

        object request = InvokeStatic<object>(
            "OpcMcpDcomConnectionHelper",
            "NormalizeRequest",
            "",
            null,
            null,
            null,
            null,
            false,
            "dcom://opc-guid-host/" + classId,
            null,
            "opchda");

        await Assert.That(GetProperty<string>(request, "Host")).IsEqualTo("opc-guid-host");
        await Assert.That(GetProperty<string?>(request, "ProgId")).IsNull();
        await Assert.That(GetProperty<string?>(request, "Clsid")).IsEqualTo(classId);
        await Assert.That(GetProperty<string?>(request, "ConnectionString")).IsEqualTo("dcom://opc-guid-host/" + classId);
    }

    [Test]
    [Arguments("inmemory://loopback", "localhost")]
    [Arguments("inmemory://host/path", "hostpath")]
    [Arguments("inmemory:local-channel", "local-channel")]
    [Arguments("INMEMORY:/trimmed/", "trimmed")]
    public async Task OpcMcpDcomConnectionHelper_TryGetInMemoryKey_Parses_supported_forms(string connectionString, string expected)
    {
        string? actual = InvokeStatic<string?>("OpcMcpDcomConnectionHelper", "TryGetInMemoryKey", connectionString);

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task OpcMcpDcomConnectionHelper_TryGetInMemoryKey_Returns_null_for_blank_or_other_schemes()
    {
        await Assert.That(InvokeStatic<string?>("OpcMcpDcomConnectionHelper", "TryGetInMemoryKey", (object?)null)).IsNull();
        await Assert.That(InvokeStatic<string?>("OpcMcpDcomConnectionHelper", "TryGetInMemoryKey", "  ")).IsNull();
        await Assert.That(InvokeStatic<string?>("OpcMcpDcomConnectionHelper", "TryGetInMemoryKey", "opcda://host/Server")).IsNull();
    }

    [Test]
    public async Task OpcClassicDcomConnectionFactory_NormalizeRequest_Parses_connection_url_and_trims_fields()
    {
        const string classId = "10138C2C-0000-0000-0000-00000000B001";
        object original = CreateClassicConnectionRequest(
            "  fallback  ",
            "  Legacy.Prog.1  ",
            "  ",
            " user ",
            "password",
            false,
            "dcom://opc-batch/" + classId,
            "connect");

        object normalized = InvokeStatic<object>("OpcClassicDcomConnectionFactory", "NormalizeRequest", original);

        await Assert.That(GetProperty<string>(normalized, "Host")).IsEqualTo("opc-batch");
        await Assert.That(GetProperty<string?>(normalized, "ProgId")).IsEqualTo("Legacy.Prog.1");
        await Assert.That(GetProperty<string?>(normalized, "Clsid")).IsEqualTo(classId);
        await Assert.That(GetProperty<string?>(normalized, "Username")).IsEqualTo(" user ");
        await Assert.That(GetProperty<string?>(normalized, "Password")).IsEqualTo("password");
        await Assert.That(GetProperty<bool>(normalized, "UseKerberos")).IsFalse();
        await Assert.That(GetProperty<string?>(normalized, "ConnectionString")).IsEqualTo("dcom://opc-batch/" + classId);
        await Assert.That(GetProperty<string?>(normalized, "AuthLevel")).IsEqualTo("connect");
    }

    [Test]
    public async Task OpcClassicDcomConnectionFactory_TryGetInMemoryKey_Parses_uri_and_prefixed_forms()
    {
        await Assert.That(InvokeStatic<string?>("OpcClassicDcomConnectionFactory", "TryGetInMemoryKey", "inmemory://batch-loop")).IsEqualTo("batch-loop");
        await Assert.That(InvokeStatic<string?>("OpcClassicDcomConnectionFactory", "TryGetInMemoryKey", "inmemory://host/path")).IsEqualTo("hostpath");
        await Assert.That(InvokeStatic<string?>("OpcClassicDcomConnectionFactory", "TryGetInMemoryKey", "inmemory:batch-channel")).IsEqualTo("batch-channel");
        await Assert.That(InvokeStatic<string?>("OpcClassicDcomConnectionFactory", "TryGetInMemoryKey", "opcda://host/Server")).IsNull();
    }

    private static object CreateClassicConnectionRequest(
        string host,
        string? progId,
        string? clsid,
        string? username,
        string? password,
        bool useKerberos,
        string? connectionString,
        string? authLevel)
    {
        Type type = GetToolType("OpcClassicConnectionRequest");
        return Activator.CreateInstance(
            type,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [host, progId, clsid, username, password, useKerberos, connectionString, authLevel],
            culture: null)!;
    }

    private static T? GetProperty<T>(object instance, string propertyName) =>
        (T?)instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(instance);

    private static T InvokeStatic<T>(string typeName, string methodName, params object?[] args)
    {
        Type type = GetToolType(typeName);
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
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

    private static Type GetToolType(string typeName) =>
        typeof(CaptureTools).Assembly.GetType("Opc.Classic.Mcp.Tools." + typeName, throwOnError: true)!;
}

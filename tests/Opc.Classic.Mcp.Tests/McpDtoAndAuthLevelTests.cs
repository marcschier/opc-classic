// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Mcp.Tests;

public sealed class McpDtoAndAuthLevelTests
{
    [Test]
    public async Task OpcHdaWriteValueDto_Construction_and_equality_preserve_values()
    {
        DateTimeOffset timestamp = DateTimeOffset.Parse("2026-06-07T19:15:30.1234567+00:00", CultureInfo.InvariantCulture);
        var dto = new OpcHdaWriteValueDto(17, timestamp, 42.25d, 192);
        var same = new OpcHdaWriteValueDto(17, timestamp, 42.25d, 192);
        var differentQuality = dto with { Quality = 0 };

        await Assert.That(dto.ServerHandle).IsEqualTo(17);
        await Assert.That(dto.Timestamp).IsEqualTo(timestamp);
        await Assert.That(dto.Value).IsEqualTo(42.25d);
        await Assert.That(dto.Quality).IsEqualTo(192);
        await Assert.That(dto).IsEqualTo(same);
        await Assert.That(dto == differentQuality).IsFalse();
    }

    [Test]
    public async Task OpcHdaWriteAnnotationDto_Construction_and_equality_preserve_values()
    {
        DateTimeOffset timestamp = DateTimeOffset.Parse("2026-06-07T20:00:00+00:00", CultureInfo.InvariantCulture);
        DateTimeOffset annotationTime = DateTimeOffset.Parse("2026-06-07T20:01:02+00:00", CultureInfo.InvariantCulture);
        var dto = new OpcHdaWriteAnnotationDto(22, timestamp, "verified by shift lead", "DOMAIN\\operator", annotationTime);
        var same = new OpcHdaWriteAnnotationDto(22, timestamp, "verified by shift lead", "DOMAIN\\operator", annotationTime);
        var differentUser = dto with { User = "DOMAIN\\auditor" };

        await Assert.That(dto.ServerHandle).IsEqualTo(22);
        await Assert.That(dto.Timestamp).IsEqualTo(timestamp);
        await Assert.That(dto.AnnotationText).IsEqualTo("verified by shift lead");
        await Assert.That(dto.User).IsEqualTo("DOMAIN\\operator");
        await Assert.That(dto.AnnotationTime).IsEqualTo(annotationTime);
        await Assert.That(dto).IsEqualTo(same);
        await Assert.That(dto == differentUser).IsFalse();
    }

    [Test]
    public async Task CaptureInterfaceDto_Construction_and_equality_preserve_values()
    {
        string[] addresses = ["127.0.0.1", "::1"];
        var dto = new CaptureInterfaceDto(
            "\\Device\\NPF_Loopback",
            "Npcap Loopback Adapter",
            "Adapter for loopback traffic capture",
            addresses,
            "Ethernet",
            IsLoopback: true);
        var same = new CaptureInterfaceDto(
            "\\Device\\NPF_Loopback",
            "Npcap Loopback Adapter",
            "Adapter for loopback traffic capture",
            addresses,
            "Ethernet",
            IsLoopback: true);
        var differentLoopback = dto with { IsLoopback = false };

        await Assert.That(dto.Name).IsEqualTo("\\Device\\NPF_Loopback");
        await Assert.That(dto.FriendlyName).IsEqualTo("Npcap Loopback Adapter");
        await Assert.That(dto.Description).IsEqualTo("Adapter for loopback traffic capture");
        await Assert.That(dto.Addresses.Count).IsEqualTo(2);
        await Assert.That(dto.Addresses[0]).IsEqualTo("127.0.0.1");
        await Assert.That(dto.LinkType).IsEqualTo("Ethernet");
        await Assert.That(dto.IsLoopback).IsTrue();
        await Assert.That(dto).IsEqualTo(same);
        await Assert.That(dto == differentLoopback).IsFalse();
    }

    [Test]
    [Arguments("default", OpcProtectionLevel.Integrity)]
    [Arguments(" none ", OpcProtectionLevel.None)]
    [Arguments("connect", OpcProtectionLevel.Connect)]
    [Arguments("CALL", OpcProtectionLevel.Call)]
    [Arguments("packet", OpcProtectionLevel.Packet)]
    [Arguments("pkt", OpcProtectionLevel.Packet)]
    [Arguments("pkt_integrity", OpcProtectionLevel.Integrity)]
    [Arguments("packet-integrity", OpcProtectionLevel.Integrity)]
    [Arguments("pkt_privacy", OpcProtectionLevel.Privacy)]
    [Arguments("packet-privacy", OpcProtectionLevel.Privacy)]
    [Arguments("0", OpcProtectionLevel.Integrity)]
    [Arguments("1", OpcProtectionLevel.None)]
    [Arguments("2", OpcProtectionLevel.Connect)]
    [Arguments("3", OpcProtectionLevel.Call)]
    [Arguments("4", OpcProtectionLevel.Packet)]
    [Arguments("5", OpcProtectionLevel.Integrity)]
    [Arguments("6", OpcProtectionLevel.Privacy)]
    public async Task OpcMcpAuthLevel_ParseOrDefault_Maps_supported_values(string value, OpcProtectionLevel expected)
    {
        OpcProtectionLevel actual = InvokeAuthLevelParseOrDefault(value);

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task OpcMcpAuthLevel_ParseOrDefault_Defaults_blank_values_to_integrity()
    {
        await Assert.That(InvokeAuthLevelParseOrDefault(null)).IsEqualTo(OpcProtectionLevel.Integrity);
        await Assert.That(InvokeAuthLevelParseOrDefault(string.Empty)).IsEqualTo(OpcProtectionLevel.Integrity);
        await Assert.That(InvokeAuthLevelParseOrDefault("   ")).IsEqualTo(OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task OpcMcpAuthLevel_IsSpecified_Returns_false_only_for_null_or_whitespace()
    {
        await Assert.That(InvokeAuthLevelIsSpecified(null)).IsFalse();
        await Assert.That(InvokeAuthLevelIsSpecified(string.Empty)).IsFalse();
        await Assert.That(InvokeAuthLevelIsSpecified(" \t ")).IsFalse();
        await Assert.That(InvokeAuthLevelIsSpecified("connect")).IsTrue();
    }

    [Test]
    public async Task OpcMcpAuthLevel_ParseOrDefault_Unsupported_value_throws_argument_exception()
    {
        await Assert.That(() => InvokeAuthLevelParseOrDefault("signed-only")).Throws<ArgumentException>();
    }

    private static OpcProtectionLevel InvokeAuthLevelParseOrDefault(string? authLevel) =>
        InvokeStatic<OpcProtectionLevel>("OpcMcpAuthLevel", "ParseOrDefault", authLevel);

    private static bool InvokeAuthLevelIsSpecified(string? authLevel) =>
        InvokeStatic<bool>("OpcMcpAuthLevel", "IsSpecified", authLevel);

    private static T InvokeStatic<T>(string typeName, string methodName, params object?[] args)
    {
        Type type = typeof(CaptureTools).Assembly.GetType("Opc.Classic.Mcp.Tools." + typeName, throwOnError: true)!;
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
}

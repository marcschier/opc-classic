//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Reflection;
using OpcClassic.Dcom.Internal;
using SharpInterop.Rpc;
using SharpInterop.Rpc.Auth.ntlm;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace OpcClassic.Dcom.Tests;

public sealed class NtlmDefaultsTests
{
    [Test]
    public async Task Default_ntlmv2_property_is_true()
    {
        var properties = CreateActivationProperties();
        ConfigureActivationProtection(properties, sessionSecurityEnabled: false);

        await Assert.That(properties.GetProperty("rpc.ntlm.ntlmv2")).IsEqualTo("true");
    }

    [Test]
    public async Task Default_ntlm2_session_property_is_true()
    {
        var properties = CreateActivationProperties();
        ConfigureActivationProtection(properties, sessionSecurityEnabled: false);

        await Assert.That(properties.GetProperty("rpc.ntlm.ntlm2")).IsEqualTo("true");
    }

    [Test]
    public async Task Ntlmv1_without_optin_throws()
    {
        var properties = CreateActivationProperties();
        properties.SetProperty("rpc.ntlm.ntlmv2", "false");

        await Assert.That(() => { _ = new NtlmAuthentication(properties); })
            .Throws<NotSupportedException>();
    }

    [Test]
    public async Task Ntlmv1_with_optin_succeeds()
    {
        var properties = CreateActivationProperties();
        properties.SetProperty("rpc.ntlm.ntlmv2", "false");
        properties.SetProperty("rpc.ntlm.allowV1", "true");

        var authentication = new NtlmAuthentication(properties);

        await Assert.That(authentication).IsNotNull();
    }

    private static ProtectionLevel ConfigureActivationProtection(PropertyBag properties, bool sessionSecurityEnabled)
    {
        var runtimeType = typeof(ProtectionLevel).Assembly.GetType(
            "SharpInterop.Core.ComOxidRuntime", throwOnError: true)!;
        var method = runtimeType.GetMethod(
            "ConfigureActivationProtection", BindingFlags.Static | BindingFlags.NonPublic)!;

        return (ProtectionLevel)method.Invoke(
            null, new object?[] { properties, sessionSecurityEnabled, "user", "password" })!;
    }

    private static PropertyBag CreateActivationProperties()
    {
        var properties = new PropertyBag();
        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", "false");
        properties.SetProperty("rpc.ntlm.seal", "false");
        properties.SetProperty("rpc.ntlm.keyExchange", "false");
        return properties;
    }
}

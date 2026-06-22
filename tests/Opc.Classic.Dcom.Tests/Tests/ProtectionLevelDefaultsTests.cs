// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Reflection;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;

namespace Opc.Classic.Dcom.Tests;

public sealed class ProtectionLevelDefaultsTests
{
    [Test]
    public async Task Default_protectionLevel_is_INTEGRITY()
    {
        var properties = CreateActivationProperties();
        var protectionLevel = ConfigureActivationProtection(properties, sessionSecurityEnabled: false);

        await Assert.That(protectionLevel).IsEqualTo(ProtectionLevel.PROTECTION_LEVEL_INTEGRITY);
    }

    [Test]
    public async Task Sign_property_is_set_by_default()
    {
        var properties = CreateActivationProperties();
        ConfigureActivationProtection(properties, sessionSecurityEnabled: false);

        await Assert.That(properties.GetProperty("rpc.ntlm.sign")).IsEqualTo("true");

        var type1 = new NtlmAuthentication(properties).CreateType1();
        await Assert.That(type1.GetFlag(NtlmFlags.NtlmsspNegotiateSign)).IsTrue();
    }

    [Test]
    public async Task Session_security_enabled_escalates_to_PRIVACY()
    {
        var properties = CreateActivationProperties();
        var protectionLevel = ConfigureActivationProtection(properties, sessionSecurityEnabled: true);

        await Assert.That(protectionLevel).IsEqualTo(ProtectionLevel.PROTECTION_LEVEL_PRIVACY);
        await Assert.That(properties.GetProperty("rpc.ntlm.sign")).IsEqualTo("true");
        await Assert.That(properties.GetProperty("rpc.ntlm.seal")).IsEqualTo("true");
    }

    private static ProtectionLevel ConfigureActivationProtection(PropertyBag properties, bool sessionSecurityEnabled)
    {
        var runtimeType = typeof(ProtectionLevel).Assembly.GetType(
            "Opc.Classic.Dcom.Core.ComOxidRuntime", throwOnError: true)!;
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

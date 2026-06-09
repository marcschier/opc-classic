//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Reflection;
using Opc.Classic.Security.Dcom;
using TUnit.Core;

namespace Opc.Classic.Security.Tests;

public sealed class DcomInterfaceIdTests {
    [Test]
    public async Task IOPCSecurityNT_InterfaceId_MatchesOpcSecurityHeader() {
        var actual = IOPCSecurityNT.InterfaceId;
        var expected = new Guid("7AA83A01-6C77-11D3-84F9-00008630A38B");

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task IOPCSecurityPrivate_InterfaceId_MatchesOpcSecurityHeader() {
        var actual = IOPCSecurityPrivate.InterfaceId;
        var expected = new Guid("7AA83A02-6C77-11D3-84F9-00008630A38B");

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task SecurityInterfaces_ArePartialOpcInterfaces() {
        var ntIsOpcInterface = IsGeneratedOpcInterface(typeof(IOPCSecurityNT));
        var privateIsOpcInterface = IsGeneratedOpcInterface(typeof(IOPCSecurityPrivate));

        await Assert.That(ntIsOpcInterface).IsTrue();
        await Assert.That(privateIsOpcInterface).IsTrue();
    }

    private static bool IsGeneratedOpcInterface(Type interfaceType) {
        return interfaceType.GetCustomAttributesData().Any(IsOpcInterfaceAttribute)
            && interfaceType.GetProperty("InterfaceId", BindingFlags.Public | BindingFlags.Static) is not null;
    }

    private static bool IsOpcInterfaceAttribute(CustomAttributeData attribute) {
        return string.Equals(
            attribute.AttributeType.FullName,
            "Opc.Classic.Generators.OpcInterfaceAttribute",
            StringComparison.Ordinal);
    }
}

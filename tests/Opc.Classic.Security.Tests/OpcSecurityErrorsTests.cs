//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Reflection;
using TUnit.Core;

namespace Opc.Classic.Security.Tests;

public sealed class OpcSecurityErrorsTests {
    [Test]
    public async Task Values_MatchOpcSecuritySpec() {
        await AssertConstantAsync(nameof(OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE), unchecked((int)0xC0040301u));
        await AssertConstantAsync(nameof(OpcSecurityErrors.OPC_E_LOW_IMPERS_LEVEL), unchecked((int)0xC0040302u));
        await AssertConstantAsync(nameof(OpcSecurityErrors.OPC_S_LOW_AUTHN_LEVEL), 0x00040303);
    }

    private static async Task AssertConstantAsync(string fieldName, int expected) {
        var field = typeof(OpcSecurityErrors).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        await Assert.That(field).IsNotNull();
        await Assert.That(field!.IsLiteral).IsTrue();
        await Assert.That(field.GetRawConstantValue()).IsEqualTo(expected);
    }
}

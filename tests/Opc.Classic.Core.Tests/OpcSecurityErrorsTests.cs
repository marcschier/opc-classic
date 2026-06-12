//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Reflection;

namespace Opc.Classic.Tests;

public sealed class OpcSecurityErrorsTests
{
    [Test]
    public async Task Values_MatchOpcSecuritySpec()
    {
        await Assert.That(GetConstantValue(nameof(OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE))).IsEqualTo(unchecked((int)0xC0040301u));
        await Assert.That(GetConstantValue(nameof(OpcSecurityErrors.OPC_E_LOW_IMPERS_LEVEL))).IsEqualTo(unchecked((int)0xC0040302u));
        await Assert.That(GetConstantValue(nameof(OpcSecurityErrors.OPC_S_LOW_AUTHN_LEVEL))).IsEqualTo(0x00040303);
    }

    private static int GetConstantValue(string fieldName)
    {
        var field = typeof(OpcSecurityErrors).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        if (field is null || !field.IsLiteral)
        {
            throw new InvalidOperationException($"{fieldName} must be a public constant.");
        }

        return (int)field.GetRawConstantValue()!;
    }
}

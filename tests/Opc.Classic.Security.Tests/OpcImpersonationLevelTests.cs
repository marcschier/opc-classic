// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Security.Tests;

public sealed class OpcImpersonationLevelTests
{
    [Test]
    public async Task Values_MatchOpcSecuritySpec()
    {
        await AssertValueAsync(OpcImpersonationLevel.Default, 0);
        await AssertValueAsync(OpcImpersonationLevel.Anonymous, 1);
        await AssertValueAsync(OpcImpersonationLevel.Identify, 2);
        await AssertValueAsync(OpcImpersonationLevel.Impersonate, 3);
        await AssertValueAsync(OpcImpersonationLevel.Delegate, 4);
    }

    private static async Task AssertValueAsync(OpcImpersonationLevel level, int expected)
    {
        var actual = (int)level;
        await Assert.That(actual).IsEqualTo(expected);
    }
}

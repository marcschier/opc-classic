//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using TUnit.Core;

namespace OpcClassic.Tests;

public sealed class OpcClientOptionsTests
{
    [Test]
    public async Task OpcClientOptions_default_timeout_is_30_seconds()
    {
        var options = new OpcClientOptions();

        await Assert.That(options.OperationTimeout).IsEqualTo(TimeSpan.FromSeconds(30));
        await Assert.That(options.EnableCircuitBreaker).IsFalse();
    }

    [Test]
    public async Task OpcClientOptions_with_expression_creates_new_options()
    {
        var options = new OpcClientOptions { OperationTimeout = TimeSpan.FromSeconds(30) };
        var updated = options with { OperationTimeout = TimeSpan.FromSeconds(60) };

        await Assert.That(updated.OperationTimeout).IsEqualTo(TimeSpan.FromSeconds(60));
        await Assert.That(options.OperationTimeout).IsEqualTo(TimeSpan.FromSeconds(30));
    }
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Subscriptions;

public sealed class OpcDaSubscriptionTests
{
    [Test]
    public async Task Constructor_CreatesSubscriptionImplementingInterface()
    {
        IOpcDaSubscription subscription = new OpcDaSubscription();

        await Assert.That(subscription.GetType()).IsEqualTo(typeof(OpcDaSubscription));
    }

    [Test]
    public async Task DataChanges_PlaceholderThrowsNotImplementedException()
    {
        var subscription = new OpcDaSubscription();

        await Assert.That(() => subscription.DataChanges(TestContext.Current!.CancellationToken))
            .Throws<NotImplementedException>();
    }

    [Test]
    public async Task DisposeAsync_PlaceholderThrowsNotImplementedException()
    {
        var subscription = new OpcDaSubscription();

        await Assert.That(async () => await subscription.DisposeAsync())
            .Throws<NotImplementedException>();
    }
}

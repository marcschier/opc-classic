//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Reflection;
using Opc.Classic.Dcom.Common;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

[NotInParallel]
public sealed class DcomTimingsTests
{
    [Test]
    public async Task Default_PingPeriod_is_spec_mandated_80_seconds()
    {
        ResetTimings();
        try
        {
            await Assert.That(DcomTimings.PingPeriod).IsEqualTo(TimeSpan.FromSeconds(80));
        }
        finally
        {
            ResetTimings();
        }
    }

    [Test]
    public async Task Default_ObjectExpiryPeriod_is_two_ping_periods()
    {
        ResetTimings();
        try
        {
            await Assert.That(DcomTimings.ObjectExpiryPeriod).IsEqualTo(TimeSpan.FromSeconds(160));
        }
        finally
        {
            ResetTimings();
        }
    }

    [Test]
    public async Task Setting_PingPeriod_to_zero_throws()
    {
        ResetTimings();
        try
        {
            await Assert.That(() =>
            {
                DcomTimings.PingPeriod = TimeSpan.Zero;
            }).Throws<ArgumentOutOfRangeException>();
        }
        finally
        {
            ResetTimings();
        }
    }

    [Test]
    public async Task Setting_ObjectExpiryPeriod_below_two_ping_periods_throws()
    {
        ResetTimings();
        try
        {
            DcomTimings.PingPeriod = TimeSpan.FromSeconds(10);

            await Assert.That(() =>
            {
                DcomTimings.ObjectExpiryPeriod = TimeSpan.FromSeconds(19);
            }).Throws<ArgumentException>();
        }
        finally
        {
            ResetTimings();
        }
    }

    [Test]
    public async Task ObjectId_expires_after_configured_ObjectExpiryPeriod_without_ping()
    {
        ResetTimings();
        try
        {
            var timeProvider = new ManualTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
            SetTimeProvider(timeProvider);
            DcomTimings.PingPeriod = TimeSpan.FromSeconds(1);
            DcomTimings.ObjectExpiryPeriod = TimeSpan.FromSeconds(2);
            var oid = CreateObjectId();

            await Assert.That(HasExpired(oid)).IsFalse();
            timeProvider.Advance(DcomTimings.ObjectExpiryPeriod);
            await Assert.That(HasExpired(oid)).IsFalse();
            timeProvider.Advance(TimeSpan.FromTicks(1));

            await Assert.That(HasExpired(oid)).IsTrue();
        }
        finally
        {
            ResetTimings();
        }
    }

    private static object CreateObjectId()
    {
        var objectIdType = typeof(DcomTimings).Assembly.GetType("Opc.Classic.Dcom.Core.ObjectId", throwOnError: true)!;
        var constructor = objectIdType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(byte[]), typeof(bool)],
            modifiers: null)!;

        return constructor.Invoke([new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, false]);
    }

    private static bool HasExpired(object objectId)
    {
        var method = objectId.GetType().GetMethod("HasExpired", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (bool)method.Invoke(objectId, null)!;
    }

    private static void SetTimeProvider(TimeProvider timeProvider)
    {
        var property = typeof(DcomTimings).GetProperty("TimeProvider", BindingFlags.Static | BindingFlags.NonPublic)!;
        property.SetValue(null, timeProvider);
    }

    private static void ResetTimings()
    {
        var method = typeof(DcomTimings).GetMethod("ResetForTesting", BindingFlags.Static | BindingFlags.NonPublic)!;
        method.Invoke(null, null);
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ManualTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan delta) => _utcNow = _utcNow.Add(delta);
    }
}

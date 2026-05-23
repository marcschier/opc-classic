//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Opc.Classic.Dcom.Internal;
using TUnit.Core;

namespace Opc.Classic.Dcom.Logging.Tests;

public sealed class FakeLoggerSampleTests
{
    [Test, NotInParallel]
    public async Task FakeLogger_captures_emitted_log_lines()
    {
        var provider = new FakeLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));

        LogHost.ConfigureFactory(loggerFactory);
        try
        {
            Log.Logger.Information("hello {Name}", "world");

            var collector = provider.Collector;
            await Assert.That(collector.Count).IsGreaterThanOrEqualTo(1);
            var lastEntry = collector.LatestRecord;
            await Assert.That(lastEntry.Message).Contains("hello");
        }
        finally
        {
            LogHost.ConfigureFactory(NullLoggerFactory.Instance);
        }
    }
}

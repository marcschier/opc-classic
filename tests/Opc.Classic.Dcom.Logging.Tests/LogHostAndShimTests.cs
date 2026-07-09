// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Logging.Tests;

public sealed class LogHostAndShimTests
{
    /// <summary>
    /// Recording logger that captures every Log invocation for inspection.
    /// </summary>
    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Calls { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Calls.Add((logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        public CapturingLogger CapturedLogger { get; } = new();
        public ILogger CreateLogger(string categoryName) => CapturedLogger;
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }

    // -------- LogHost --------

    [Test, NotInParallel]
    public async Task LogHost_DefaultsTo_NullLoggerFactory()
    {
        LogHost.ConfigureFactory(null);  // ensure clean state
        await Assert.That(LogHost.Factory).IsEqualTo((ILoggerFactory)NullLoggerFactory.Instance);
    }

    [Test, NotInParallel]
    public async Task LogHost_ConfigureFactory_Installs()
    {
        try
        {
            var factory = new CapturingLoggerFactory();
            LogHost.ConfigureFactory(factory);
            await Assert.That(LogHost.Factory).IsEqualTo((ILoggerFactory)factory);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task LogHost_CreateLogger_DelegatesToFactory()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            var logger = LogHost.CreateLogger("test");
            await Assert.That(logger).IsEqualTo((ILogger)factory.CapturedLogger);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    // -------- Shim Log -> ILogger routing --------

    [Test, NotInParallel]
    public async Task Shim_Information_RoutesToILogger_AtInformationLevel()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            Log.Logger.Information("hello");
            await Assert.That(factory.CapturedLogger.Calls.Count).IsEqualTo(1);
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Information);
            await Assert.That(factory.CapturedLogger.Calls[0].Message).Contains("hello");
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_Warning_RoutesWithException()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            var ex = new InvalidOperationException("boom");
            Log.Logger.Warning(ex, "trouble: {Subsystem}", "auth");
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Warning);
            await Assert.That(factory.CapturedLogger.Calls[0].Exception).IsEqualTo(ex);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_Error_RoutesWithException()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            var ex = new TimeoutException();
            Log.Logger.Error(ex, "rpc failed");
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Error);
            await Assert.That(factory.CapturedLogger.Calls[0].Exception).IsEqualTo(ex);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_Fatal_MapsToCritical()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            Log.Logger.Fatal("system down");
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Critical);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_Verbose_MapsToTrace()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            Log.Logger.Verbose("ndr frame: {Bytes}", 64);
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Trace);
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_Debug_RoutesWithArgs()
    {
        var factory = new CapturingLoggerFactory();
        try
        {
            LogHost.ConfigureFactory(factory);
            Log.Logger.Debug("opnum={Opnum} bytes={Bytes}", 3, 64);
            await Assert.That(factory.CapturedLogger.Calls[0].Level).IsEqualTo(LogLevel.Debug);
            await Assert.That(factory.CapturedLogger.Calls[0].Message).Contains("opnum=3");
            await Assert.That(factory.CapturedLogger.Calls[0].Message).Contains("bytes=64");
        }
        finally
        {
            LogHost.ConfigureFactory(null);
        }
    }

    [Test, NotInParallel]
    public async Task Shim_WithoutConfiguredFactory_DoesNotThrow()
    {
        LogHost.ConfigureFactory(null);
        // Should be a no-op via NullLoggerFactory:
        Log.Logger.Information("nobody is listening");
        Log.Logger.Warning(new Exception(), "still nobody");
        await Assert.That(NoThrowSentinel()).IsTrue();
    }

    private static bool NoThrowSentinel() => true;
}

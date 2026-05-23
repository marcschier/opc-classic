//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Serilog-API-shaped shim that routes through Microsoft.Extensions.Logging.
// Source-compatible drop-in for `Log.Logger.{Information|Warning|Error|Debug|Verbose|Fatal}`
// calls — see LogHost.cs file header for the migration plan.
//

using System;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Dcom.Internal;

/// <summary>
/// Shim providing the Serilog static-API surface (<c>Log.Logger.X(...)</c>)
/// but routing through Microsoft.Extensions.Logging.ILogger.
/// </summary>
public static class Log
{
    /// <summary>The shim logger — call <c>Log.Logger.Information(...)</c> etc.</summary>
    public static IShimLogger Logger { get; } = new ShimLogger();
}

/// <summary>Serilog-compatible logger surface routed through ILogger.</summary>
public interface IShimLogger
{
    /// <summary>Returns true if the corresponding log level is enabled.</summary>
    bool IsEnabled(LogLevel level);

    /// <summary>Information-level log.</summary>
    void Information(string message);

    /// <summary>Information-level log with structured arguments.</summary>
    void Information(string template, params object?[] args);

    /// <summary>Information-level log carrying an exception.</summary>
    void Information(Exception exception, string message);

    /// <summary>Information-level log carrying an exception + structured arguments.</summary>
    void Information(Exception exception, string template, params object?[] args);

    /// <summary>Debug-level log.</summary>
    void Debug(string message);

    /// <summary>Debug-level log with structured arguments.</summary>
    void Debug(string template, params object?[] args);

    /// <summary>Debug-level log carrying an exception.</summary>
    void Debug(Exception exception, string message);

    /// <summary>Debug-level log carrying an exception + structured arguments.</summary>
    void Debug(Exception exception, string template, params object?[] args);

    /// <summary>Verbose-level log (mapped to Trace).</summary>
    void Verbose(string message);

    /// <summary>Verbose-level log (mapped to Trace) with arguments.</summary>
    void Verbose(string template, params object?[] args);

    /// <summary>Verbose-level log carrying an exception.</summary>
    void Verbose(Exception exception, string message);

    /// <summary>Verbose-level log carrying an exception + structured arguments.</summary>
    void Verbose(Exception exception, string template, params object?[] args);

    /// <summary>Warning-level log.</summary>
    void Warning(string message);

    /// <summary>Warning-level log with structured arguments.</summary>
    void Warning(string template, params object?[] args);

    /// <summary>Warning-level log carrying an exception.</summary>
    void Warning(Exception exception, string message);

    /// <summary>Warning-level log carrying an exception + structured arguments.</summary>
    void Warning(Exception exception, string template, params object?[] args);

    /// <summary>Error-level log.</summary>
    void Error(string message);

    /// <summary>Error-level log with structured arguments.</summary>
    void Error(string template, params object?[] args);

    /// <summary>Error-level log carrying an exception.</summary>
    void Error(Exception exception, string message);

    /// <summary>Error-level log carrying an exception + structured arguments.</summary>
    void Error(Exception exception, string template, params object?[] args);

    /// <summary>Fatal-level log (mapped to Critical).</summary>
    void Fatal(string message);

    /// <summary>Fatal-level log with structured arguments.</summary>
    void Fatal(string template, params object?[] args);

    /// <summary>Fatal-level log carrying an exception.</summary>
    void Fatal(Exception exception, string message);

    /// <summary>Fatal-level log carrying an exception + structured arguments.</summary>
    void Fatal(Exception exception, string template, params object?[] args);
}

internal sealed class ShimLogger : IShimLogger
{
    private static ILogger Get() => LogHost.CreateLogger("Opc.Classic.Dcom");

    public bool IsEnabled(LogLevel level) => Get().IsEnabled(level);

    public void Information(string message) => Get().LogInformation("{Message}", message);
    public void Information(string template, params object?[] args)
        => Get().LogInformation(template, args);
    public void Information(Exception exception, string message)
        => Get().LogInformation(exception, "{Message}", message);
    public void Information(Exception exception, string template, params object?[] args)
        => Get().LogInformation(exception, template, args);

    public void Debug(string message) => Get().LogDebug("{Message}", message);
    public void Debug(string template, params object?[] args)
        => Get().LogDebug(template, args);
    public void Debug(Exception exception, string message)
        => Get().LogDebug(exception, "{Message}", message);
    public void Debug(Exception exception, string template, params object?[] args)
        => Get().LogDebug(exception, template, args);

    public void Verbose(string message) => Get().LogTrace("{Message}", message);
    public void Verbose(string template, params object?[] args)
        => Get().LogTrace(template, args);
    public void Verbose(Exception exception, string message)
        => Get().LogTrace(exception, "{Message}", message);
    public void Verbose(Exception exception, string template, params object?[] args)
        => Get().LogTrace(exception, template, args);

    public void Warning(string message) => Get().LogWarning("{Message}", message);
    public void Warning(string template, params object?[] args)
        => Get().LogWarning(template, args);
    public void Warning(Exception exception, string message)
        => Get().LogWarning(exception, "{Message}", message);
    public void Warning(Exception exception, string template, params object?[] args)
        => Get().LogWarning(exception, template, args);

    public void Error(string message) => Get().LogError("{Message}", message);
    public void Error(string template, params object?[] args)
        => Get().LogError(template, args);
    public void Error(Exception exception, string message)
        => Get().LogError(exception, "{Message}", message);
    public void Error(Exception exception, string template, params object?[] args)
        => Get().LogError(exception, template, args);

    public void Fatal(string message) => Get().LogCritical("{Message}", message);
    public void Fatal(string template, params object?[] args)
        => Get().LogCritical(template, args);
    public void Fatal(Exception exception, string message)
        => Get().LogCritical(exception, "{Message}", message);
    public void Fatal(Exception exception, string template, params object?[] args)
        => Get().LogCritical(exception, template, args);
}

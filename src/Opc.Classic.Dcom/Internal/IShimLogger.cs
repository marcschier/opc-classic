// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Opc.Classic.Dcom.Internal;

/// <summary>
/// Serilog-compatible logger surface routed through ILogger.
/// </summary>
public interface IShimLogger
{
    /// <summary>
    /// Returns true if the corresponding log level is enabled.
    /// </summary>
    bool IsEnabled(LogLevel level);

    /// <summary>
    /// Information-level log.
    /// </summary>
    void Information(string message);

    /// <summary>
    /// Information-level log with structured arguments.
    /// </summary>
    void Information(string template, params object?[] args);

    /// <summary>
    /// Information-level log carrying an exception.
    /// </summary>
    void Information(Exception exception, string message);

    /// <summary>
    /// Information-level log carrying an exception + structured arguments.
    /// </summary>
    void Information(Exception exception, string template, params object?[] args);

    /// <summary>
    /// Debug-level log.
    /// </summary>
    void Debug(string message);

    /// <summary>
    /// Debug-level log with structured arguments.
    /// </summary>
    void Debug(string template, params object?[] args);

    /// <summary>
    /// Debug-level log carrying an exception.
    /// </summary>
    void Debug(Exception exception, string message);

    /// <summary>
    /// Debug-level log carrying an exception + structured arguments.
    /// </summary>
    void Debug(Exception exception, string template, params object?[] args);

    /// <summary>
    /// Verbose-level log (mapped to Trace).
    /// </summary>
    void Verbose(string message);

    /// <summary>
    /// Verbose-level log (mapped to Trace) with arguments.
    /// </summary>
    void Verbose(string template, params object?[] args);

    /// <summary>
    /// Verbose-level log carrying an exception.
    /// </summary>
    void Verbose(Exception exception, string message);

    /// <summary>
    /// Verbose-level log carrying an exception + structured arguments.
    /// </summary>
    void Verbose(Exception exception, string template, params object?[] args);

    /// <summary>
    /// Warning-level log.
    /// </summary>
    void Warning(string message);

    /// <summary>
    /// Warning-level log with structured arguments.
    /// </summary>
    void Warning(string template, params object?[] args);

    /// <summary>
    /// Warning-level log carrying an exception.
    /// </summary>
    void Warning(Exception exception, string message);

    /// <summary>
    /// Warning-level log carrying an exception + structured arguments.
    /// </summary>
    void Warning(Exception exception, string template, params object?[] args);

    /// <summary>
    /// Error-level log.
    /// </summary>
    void Error(string message);

    /// <summary>
    /// Error-level log with structured arguments.
    /// </summary>
    void Error(string template, params object?[] args);

    /// <summary>
    /// Error-level log carrying an exception.
    /// </summary>
    void Error(Exception exception, string message);

    /// <summary>
    /// Error-level log carrying an exception + structured arguments.
    /// </summary>
    void Error(Exception exception, string template, params object?[] args);

    /// <summary>
    /// Fatal-level log (mapped to Critical).
    /// </summary>
    void Fatal(string message);

    /// <summary>
    /// Fatal-level log with structured arguments.
    /// </summary>
    void Fatal(string template, params object?[] args);

    /// <summary>
    /// Fatal-level log carrying an exception.
    /// </summary>
    void Fatal(Exception exception, string message);

    /// <summary>
    /// Fatal-level log carrying an exception + structured arguments.
    /// </summary>
    void Fatal(Exception exception, string template, params object?[] args);
}

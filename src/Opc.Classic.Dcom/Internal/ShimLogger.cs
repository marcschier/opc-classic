//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.Logging;

namespace Opc.Classic.Dcom.Internal;

#pragma warning disable CA1848, CA1873, CA2254 // Compatibility shim preserves Serilog-style dynamic templates.
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
#pragma warning restore CA1848, CA1873, CA2254

// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Opt-in, file-based tracing for the native COM activation path — the
/// <see cref="ComClassObjectRegistrar"/> class factory and the per-spec CCWs.
/// </summary>
/// <remarks>
/// <para>
/// A DCOM server launched by the Windows SCM (RPCSS) has no attached console
/// and does not inherit the launching process's environment, so ordinary
/// logging cannot capture what RPCSS asks of the class factory / object during
/// activation. When an activation fails only on some hosts — for example the
/// cross-impl matrix reporting <c>E_NOINTERFACE</c> on a CI runner while the
/// identical build activates cleanly on a developer workstation — this trace
/// records the exact <c>IClassFactory::CreateInstance</c> /
/// <c>IUnknown::QueryInterface</c> sequence (the requested IIDs and the
/// HRESULTs returned) to a file next to the server executable so the two hosts
/// can be diffed.
/// </para>
/// <para>
/// Tracing is <b>off unless explicitly enabled</b> by dropping an empty marker
/// file named <c>ccw-trace.enabled</c> in the server executable's directory
/// (<see cref="AppContext.BaseDirectory"/> — reliable for an RPCSS-launched
/// process, which cannot see caller environment variables). When the marker is
/// present, trace lines are appended to <c>ccw-trace.log</c> in the same
/// directory. Without the marker every <see cref="Trace"/> call is a single
/// reference read that returns immediately, so this is safe to leave compiled
/// into shipping builds. All I/O is wrapped so a diagnostic failure can never
/// escape into the unmanaged COM caller and crash the activation it observes.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class ComActivationDiagnostics
{
    private const string MarkerFileName = "ccw-trace.enabled";
    private const string LogFileName = "ccw-trace.log";

    private static readonly Lock s_gate = new();
    private static readonly string? s_logPath = ResolveLogPath();

    /// <summary>
    /// Gets a value indicating whether activation tracing is enabled for this
    /// process (the marker file was present at startup).
    /// </summary>
    internal static bool IsEnabled => s_logPath is not null;

    /// <summary>
    /// Appends a timestamped diagnostic line when tracing is enabled; otherwise
    /// returns immediately. Never throws.
    /// </summary>
    internal static void Trace(string message)
    {
        string? path = s_logPath;
        if (path is null)
        {
            return;
        }

        try
        {
            string line = string.Create(
                CultureInfo.InvariantCulture,
                $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ} [pid {Environment.ProcessId} tid {Environment.CurrentManagedThreadId}] {message}{Environment.NewLine}");
            lock (s_gate)
            {
                File.AppendAllText(path, line);
            }
        }
#pragma warning disable CA1031 // A diagnostic must never disrupt the activation it is observing.
        catch (Exception)
#pragma warning restore CA1031
        {
            // Intentionally swallowed: a failed trace write must not surface to
            // the unmanaged COM caller.
            return;
        }
    }

    private static string? ResolveLogPath()
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(baseDir))
            {
                return null;
            }

            return File.Exists(Path.Combine(baseDir, MarkerFileName))
                ? Path.Combine(baseDir, LogFileName)
                : null;
        }
#pragma warning disable CA1031 // Diagnostics must degrade silently to disabled.
        catch (Exception)
#pragma warning restore CA1031
        {
            return null;
        }
    }
}

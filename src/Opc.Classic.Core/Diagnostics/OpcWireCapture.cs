//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// opt-in diagnostic that writes every NDR request + response payload
// to disk so an engineer can compare wire bytes against a Wireshark capture or a
// canonical MIDL layout without re-running the live server.
//

namespace Opc.Classic.Diagnostics;

/// <summary>
/// Static gate for the wire-capture diagnostic. Activated by the
/// <c>OPCCLASSIC_WIRE_CAPTURE_DIR</c> environment variable; consumers
/// call <see cref="Wrap(ICallChannel,string)"/> at channel construction
/// to attach the capturing decorator when the gate is open.
/// </summary>
/// <remarks>
/// The capture is intentionally opt-in via env var so production / CI runs do
/// not pay the disk-write tax. When the var is unset, <see cref="Wrap"/>
/// returns the channel unchanged so call sites add a single line of code with
/// zero hot-path cost.
/// </remarks>
public static class OpcWireCapture
{
    private const string EnvVarName = "OPCCLASSIC_WIRE_CAPTURE_DIR";

    /// <summary>
    /// Returns the configured capture directory, or <see langword="null"/> when capture is disabled.
    /// </summary>
    public static string? CaptureDirectory => Environment.GetEnvironmentVariable(EnvVarName);

    /// <summary>
    /// True when the <c>OPCCLASSIC_WIRE_CAPTURE_DIR</c> environment variable is set.
    /// </summary>
    public static bool IsEnabled => !string.IsNullOrWhiteSpace(CaptureDirectory);

    /// <summary>
    /// Wraps <paramref name="channel"/> in a <see cref="WireCapturingCallChannel"/>
    /// when <see cref="IsEnabled"/> is true, otherwise returns <paramref name="channel"/>
    /// unchanged.
    /// </summary>
    /// <param name="channel">Underlying channel to (optionally) wrap.</param>
    /// <param name="contextTag">
    /// A short tag used to disambiguate captures from different sessions / endpoints
    /// (typically the DCOM CLSID, ProgID, or host:port). Used as a filename prefix
    /// so a multi-session run produces self-describing artifacts on disk.
    /// </param>
    public static ICallChannel Wrap(ICallChannel channel, string contextTag)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(contextTag);

        string? directory = CaptureDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return channel;
        }

        return new WireCapturingCallChannel(channel, directory, contextTag);
    }
}

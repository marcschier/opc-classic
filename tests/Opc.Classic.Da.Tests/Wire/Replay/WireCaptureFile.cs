//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Opc.Classic.Da.Tests.Wire.Replay;

/// <summary>
/// Loader for the per-call hex dumps emitted by
/// <c>Opc.Classic.Diagnostics.WireCapturingCallChannel</c>. Format is
/// documented in <c>interop/docs/wire-captures/README.md</c>: a small
/// banner of <c># key: value</c> metadata, then <c>## request (N bytes)</c>
/// and <c>## response (N bytes)</c> sections each containing offset-prefixed
/// hex rows.
/// </summary>
/// <remarks>
/// The replay tooling is deliberately small — its job is to turn a
/// developer-facing diagnostic artifact back into <c>byte[]</c> so an
/// engineer can paste the bytes into a unit test or run a decoder against
/// them and read the resulting hex window from the failure message.
/// </remarks>
public sealed class WireCaptureFile {
    private static readonly Regex BannerLine = new(
        @"^#\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HexRow = new(
        @"^[0-9A-Fa-f]{4}:\s+((?:[0-9A-Fa-f]{2}\s+)+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>The case-insensitive banner metadata block (e.g. iid, opnum, hresult, timestamp_utc).</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>The decoded request payload bytes (proxy → server).</summary>
    public byte[] RequestPayload { get; }

    /// <summary>The decoded response payload bytes (server → proxy).</summary>
    public byte[] ResponsePayload { get; }

    private WireCaptureFile(
        IReadOnlyDictionary<string, string> metadata,
        byte[] requestPayload,
        byte[] responsePayload) {
        Metadata = metadata;
        RequestPayload = requestPayload;
        ResponsePayload = responsePayload;
    }

    /// <summary>Loads a capture file from disk.</summary>
    public static WireCaptureFile Load(string path) {
        ArgumentNullException.ThrowIfNull(path);
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"Wire capture file not found: {path}", path);
        }
        return Parse(File.ReadAllText(path));
    }

    /// <summary>Parses a capture file contents (typically the result of <see cref="File.ReadAllText(string)"/>).</summary>
    public static WireCaptureFile Parse(string text) {
        ArgumentNullException.ThrowIfNull(text);

        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var requestBytes = new List<byte>(256);
        var responseBytes = new List<byte>(256);
        List<byte>? currentSection = null;

        foreach (string rawLine in text.Split('\n')) {
            string line = rawLine.TrimEnd('\r');
            if (line.StartsWith("## request", StringComparison.Ordinal)) {
                currentSection = requestBytes;
                continue;
            }
            if (line.StartsWith("## response", StringComparison.Ordinal)) {
                currentSection = responseBytes;
                continue;
            }
            if (line.StartsWith("#", StringComparison.Ordinal)) {
                Match banner = BannerLine.Match(line);
                if (banner.Success) {
                    metadata[banner.Groups[1].Value] = banner.Groups[2].Value;
                }
                continue;
            }
            if (currentSection is null || line.Length == 0) {
                continue;
            }
            Match row = HexRow.Match(line);
            if (!row.Success) {
                continue;
            }
            foreach (string token in row.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
                if (token.Length != 2) {
                    continue;
                }
                currentSection.Add(byte.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
        }

        return new WireCaptureFile(metadata, requestBytes.ToArray(), responseBytes.ToArray());
    }

    /// <summary>
    /// Renders a side-by-side hex window for an arbitrary failure offset. Useful
    /// for printing what the proxy WAS reading when a decoder threw.
    /// </summary>
    public string FormatResponseContext(int position, int contextBytes = 16) =>
        global::Opc.Classic.Ndr.NdrReader.FormatHexContext(ResponsePayload, position, contextBytes);

    /// <summary>Returns the parsed IID metadata key, or <see cref="Guid.Empty"/> if not present.</summary>
    public Guid Iid =>
        Metadata.TryGetValue("iid", out string? value) && Guid.TryParse(value, out Guid parsed)
            ? parsed
            : Guid.Empty;

    /// <summary>Returns the parsed opnum, or -1 if not present.</summary>
    public int Opnum =>
        Metadata.TryGetValue("opnum", out string? value)
        && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed
            : -1;
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Xml;

/// <summary>
/// XML-DA server status payload (the <c>GetStatusResponse</c> body).
/// Mirrors the spec <c>ServerStatus</c> complex type with all-optional
/// fields exposed as nullable / default-empty .NET equivalents.
/// </summary>
/// <param name="StartTime">UTC time the server (re)started.</param>
/// <param name="ProductVersion">Vendor-supplied version string.</param>
/// <param name="VendorInfo">Free-form vendor identification.</param>
/// <param name="SupportedLocaleIds">Locale identifiers the server can localize errors / messages into (e.g. <c>en-US</c>).</param>
/// <param name="SupportedInterfaceVersions">List of XML-DA interface versions advertised by the server (e.g. <c>XML_DA_Version_1_0</c>).</param>
/// <param name="ServerState">Current high-level server state.</param>
/// <param name="StatusInfo">Vendor-extensible status detail string.</param>
public sealed record XmlDaServerStatus(
    DateTimeOffset StartTime,
    string ProductVersion,
    string VendorInfo,
    System.Collections.Generic.IReadOnlyList<string> SupportedLocaleIds,
    System.Collections.Generic.IReadOnlyList<string> SupportedInterfaceVersions,
    XmlDaServerState ServerState,
    string? StatusInfo);

// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Opc.Classic;

/// <summary>
/// Parsed representation of an OPC Classic URL.
/// </summary>
/// <remarks>
/// Supported schemes (per OPC Foundation conventions):
/// <list type="bullet">
///   <item><description><c>opcda://</c> — OPC Data Access (DA 2.x / 3.0)</description></item>
///   <item><description><c>opcae://</c> — OPC Alarms &amp; Events</description></item>
///   <item><description><c>opchda://</c> — OPC Historical Data Access</description></item>
///   <item><description><c>opcdx://</c> — OPC Data eXchange</description></item>
///   <item><description><c>opc.xml-da://</c> — OPC XML-DA over HTTP/SOAP</description></item>
/// </list>
/// <para>
/// URL grammar (informal): <c>scheme://host[:port]/progid-or-clsid[?query]</c>.
/// The path segment is the ProgID (e.g. <c>Matrikon.OPC.Simulation.1</c>) or
/// a string-formatted CLSID (e.g. <c>{F8582CF2-88FB-11D0-B850-00C0F0104305}</c>).
/// </para>
/// </remarks>
public sealed class OpcUrl : IEquatable<OpcUrl>
{
    /// <summary>
    /// The full original URL string.
    /// </summary>
    public string Original { get; }

    /// <summary>
    /// The scheme (e.g. <c>opcda</c>, <c>opcae</c>, <c>opchda</c>).
    /// </summary>
    public OpcUrlScheme Scheme { get; }

    /// <summary>
    /// The host name or IP address (empty string for <c>localhost</c>).
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// The TCP port. Zero if not specified.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// The ProgID or CLSID identifying the server on the host.
    /// </summary>
    public string ServerId { get; }

    /// <summary>
    /// True if <see cref="ServerId"/> is a string-formatted CLSID.
    /// </summary>
    public bool IsClsid { get; }

    private OpcUrl(string original, OpcUrlScheme scheme, string host, int port, string serverId, bool isClsid)
    {
        Original = original;
        Scheme = scheme;
        Host = host;
        Port = port;
        ServerId = serverId;
        IsClsid = isClsid;
    }

    /// <summary>
    /// Parse a URL. Throws <see cref="FormatException"/> on syntax errors.
    /// </summary>
    /// <remarks>
    /// OPC Classic URL schemes (<c>opcda</c>, <c>opcae</c>, <c>opchda</c>, <c>opcdx</c>, <c>opc.xml-da</c>) are not
    /// registered <see cref="Uri"/> schemes; the URL is parsed directly rather than going through
    /// <see cref="Uri"/> to preserve OPC ProgID/CLSID path semantics and avoid platform-specific
    /// <see cref="Uri"/> normalization differences.
    /// </remarks>
    [SuppressMessage(
        "Design", "CA1054:URI-like parameters should not be strings",
        Justification = "OPC URL schemes are not registered with System.Uri; parsing the raw string preserves OPC-specific semantics across platforms.")]
    public static OpcUrl Parse(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return TryParse(url, out var parsed)
            ? parsed!
            : throw new FormatException($"'{url}' is not a valid OPC URL.");
    }

    /// <summary>
    /// Attempt to parse a URL. Returns <see langword="false"/> with
    /// <paramref name="result"/> <see langword="null"/> on failure.
    /// </summary>
    /// <remarks>See <see cref="Parse(string)" /> for why the URL is parsed as a raw string.</remarks>
    [SuppressMessage(
        "Design", "CA1054:URI-like parameters should not be strings",
        Justification = "OPC URL schemes are not registered with System.Uri; parsing the raw string preserves OPC-specific semantics across platforms.")]
    public static bool TryParse(string? url, out OpcUrl? result)
    {
        result = null;
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        var schemeEnd = url.IndexOf("://", StringComparison.Ordinal);
        if (schemeEnd < 0 || !TryParseScheme(url[..schemeEnd], out var scheme))
        {
            return false;
        }

        var rest = url[(schemeEnd + 3)..];
        if (!TrySplitAuthorityAndPath(rest, out var authority, out var path))
        {
            return false;
        }

        if (!TrySplitHostAndPort(authority, out var host, out var port))
        {
            return false;
        }

        result = new OpcUrl(url, scheme, host, port, path, LooksLikeClsid(path));
        return true;
    }

    private static bool TrySplitAuthorityAndPath(string rest, out string authority, out string path)
    {
        authority = string.Empty;
        path = string.Empty;
        if (rest.Length == 0)
        {
            return false;
        }
        var pathStart = rest.IndexOf('/');
        if (pathStart < 0)
        {
            return false; // path is required
        }
        authority = rest[..pathStart];
        path = rest[(pathStart + 1)..];
        var queryStart = path.IndexOf('?', StringComparison.Ordinal);
        if (queryStart >= 0)
        {
            path = path[..queryStart];
        }
        return path.Length != 0;
    }

    private static bool TrySplitHostAndPort(string authority, out string host, out int port)
    {
        host = authority;
        port = 0;
        var portStart = authority.LastIndexOf(':');
        if (portStart <= 0 || authority.IndexOf(':', StringComparison.Ordinal) != portStart)
        {
            return true;
        }
        host = authority[..portStart];
        var portText = authority[(portStart + 1)..];
        if (!int.TryParse(portText, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out port)
            || port is <= 0 or > 65535)
        {
            return false;
        }
        return true;
    }

    private static bool TryParseScheme(string text, out OpcUrlScheme scheme)
    {
        switch (text.ToLowerInvariant())
        {
            case "opcda":
                scheme = OpcUrlScheme.Da; return true;
            case "opcae":
                scheme = OpcUrlScheme.Ae; return true;
            case "opchda":
                scheme = OpcUrlScheme.Hda; return true;
            case "opcdx":
                scheme = OpcUrlScheme.Dx; return true;
            case "opc.xml-da":
                scheme = OpcUrlScheme.XmlDa; return true;
            default:
                scheme = default; return false;
        }
    }

    private static bool LooksLikeClsid(string text)
    {
        // {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX} — 38 chars with braces; 36 without.
        if (text.Length == 38 && text[0] == '{' && text[^1] == '}')
        {
            return Guid.TryParseExact(text, "B", out _);
        }
        return text.Length == 36 && Guid.TryParseExact(text, "D", out _);
    }

    /// <inheritdoc />
    public bool Equals(OpcUrl? other) =>
        other is not null &&
        Scheme == other.Scheme &&
        string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase) &&
        Port == other.Port &&
        string.Equals(ServerId, other.ServerId, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as OpcUrl);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            Scheme,
            Host.ToUpperInvariant(),
            Port,
            ServerId.ToUpperInvariant());

    /// <inheritdoc />
    public override string ToString() => Original;
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Net;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Parses listener address strings used in OPC server options
/// (<c>"127.0.0.1:0"</c>, <c>"[::1]:51303"</c>, <c>"0.0.0.0:51303"</c>).
/// </summary>
public static class ListenAddressParser
{
    /// <summary>
    /// Parses a <c>host:port</c> string into an <see cref="IPEndPoint"/>.
    /// The host portion may be a literal IPv4 address, an IPv6 address
    /// in brackets, or empty / asterisk (meaning <see cref="IPAddress.Any"/>).
    /// A port of <c>0</c> means "dynamic" — the TCP listener will pick one.
    /// </summary>
    /// <param name="address">The listener address, e.g. <c>"127.0.0.1:0"</c>.</param>
    /// <returns>The parsed endpoint.</returns>
    /// <exception cref="ArgumentException">If <paramref name="address"/> is null/empty.</exception>
    /// <exception cref="FormatException">If the host or port portion cannot be parsed.</exception>
    public static IPEndPoint Parse(string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        int separator = address.LastIndexOf(':');
        if (separator <= 0 || separator == address.Length - 1)
        {
            throw new FormatException($"Listener address '{address}' must be host:port.");
        }

        string host = address[..separator];
        string portText = address[(separator + 1)..];

        if (!int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int port)
            || port < 0 || port > 65535)
        {
            throw new FormatException($"Listener address '{address}' has an invalid port.");
        }

        IPAddress hostAddress;
        if (host is "*" or "0.0.0.0")
        {
            hostAddress = IPAddress.Any;
        }
        else if (host is "::" or "[::]")
        {
            hostAddress = IPAddress.IPv6Any;
        }
        else if (host.StartsWith('[') && host.EndsWith(']'))
        {
            hostAddress = IPAddress.Parse(host[1..^1]);
        }
        else if (!IPAddress.TryParse(host, out IPAddress? parsed))
        {
            throw new FormatException($"Listener address '{address}' has an unparseable host '{host}'. Use a literal IP address.");
        }
        else
        {
            hostAddress = parsed;
        }

        return new IPEndPoint(hostAddress, port);
    }
}

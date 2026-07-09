// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Text;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Decodes the DUALSTRINGARRAY resolver-bindings block returned by
/// <c>IActivation::RemoteActivation</c> / <c>IObjectExporter::ResolveOxid</c>
/// into the concrete <see cref="EndPoint"/> values an
/// <see cref="IAsyncTransportFactory"/> can connect through.
/// </summary>
/// <remarks>
/// <para>
/// The DUALSTRINGARRAY wire format (MS-DCOM §2.2.19.1) is a length-prefixed
/// little-endian UCS-2 buffer holding two arrays of NUL-terminated
/// string-bindings:
/// <code>
///   STRINGBINDING := TowerId(uint16) UnicodeAddress(uint16*)+ NUL
/// </code>
/// followed by a separator and a SECURITYBINDING array.
/// </para>
/// <para>
/// Tower-ID 0x0007 = <c>ncacn_ip_tcp</c>; the address is encoded as
/// <c>HOSTNAME[PORT]</c>. Tower-ID 0x000F = <c>ncacn_np</c>; the address
/// is encoded as either <c>HOSTNAME[\PIPE\NAME]</c> (DCE form) or
/// <c>\\HOSTNAME\PIPE\NAME</c> (UNC form). Tower-ID 0x0000 terminates a
/// string-binding sub-array.
/// </para>
/// <para>
/// The resolver returns either an <see cref="DnsEndPoint"/> (TCP) or an
/// <see cref="NcacnNpEndPoint"/> (named pipe); callers dispatch through
/// <see cref="TransportFactoryDispatcher"/> to pick the right transport.
/// </para>
/// </remarks>
public static class DualStringArrayResolver
{
    private const ushort TowerIdTcp = 0x0007;
    private const ushort TowerIdNamedPipe = 0x000F;
    private const ushort TowerIdTerminator = 0x0000;

    /// <summary>
    /// Decodes a raw DUALSTRINGARRAY byte buffer and returns the first
    /// transport endpoint that matches a recognised tower. TCP and named
    /// pipe entries are returned in encounter order. Returns
    /// <see langword="null"/> when no recognised binding is present.
    /// </summary>
    /// <param name="fallbackHost">Host name used when the decoded
    /// binding's host component is empty.</param>
    /// <param name="bindings">Raw DUALSTRINGARRAY bytes (typically
    /// <c>RemoteActivationResponse.OxidBindings</c>).</param>
    public static EndPoint? ResolveFirstTransport(string fallbackHost, ReadOnlySpan<byte> bindings)
    {
        foreach (StringBindingEntry entry in DecodeStringBindings(bindings))
        {
            EndPoint? candidate = TryBuildEndpoint(fallbackHost, entry);
            if (candidate is not null)
            {
                return candidate;
            }
        }
        return null;
    }

    /// <summary>
    /// Decodes a raw DUALSTRINGARRAY buffer and returns the first
    /// <see cref="NcacnNpEndPoint"/> (named pipe) entry, or
    /// <see langword="null"/> when no pipe binding is present.
    /// </summary>
    public static NcacnNpEndPoint? ResolveFirstNamedPipe(string fallbackHost, ReadOnlySpan<byte> bindings)
    {
        foreach (StringBindingEntry entry in DecodeStringBindings(bindings))
        {
            if (entry.TowerId == TowerIdNamedPipe)
            {
                EndPoint? candidate = TryBuildEndpoint(fallbackHost, entry);
                if (candidate is NcacnNpEndPoint pipeEndpoint)
                {
                    return pipeEndpoint;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Decodes a raw DUALSTRINGARRAY buffer and returns the first
    /// <see cref="DnsEndPoint"/> (TCP) entry whose address contains an
    /// explicit <c>[port]</c> suffix, or <see langword="null"/> when no
    /// such binding is present. Matches the legacy
    /// <c>ResolveObjectEndpointFromOxidBindings</c> contract.
    /// </summary>
    public static DnsEndPoint? ResolveFirstTcp(string fallbackHost, ReadOnlySpan<byte> bindings)
    {
        foreach (StringBindingEntry entry in DecodeStringBindings(bindings))
        {
            if (entry.TowerId == TowerIdTcp)
            {
                EndPoint? candidate = TryBuildEndpoint(fallbackHost, entry);
                if (candidate is DnsEndPoint tcpEndpoint)
                {
                    return tcpEndpoint;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Decodes the pre-parsed UCS-2 resolver-bindings array returned by
    /// <see cref="IOpcInterfaceRef.ResolverBindings"/> and returns the
    /// first transport endpoint. Used by the OBJREF-driven activation
    /// path (which already split the wire bytes into <see langword="ushort"/>
    /// codepoints) as opposed to the raw DUALSTRINGARRAY path.
    /// </summary>
    public static EndPoint? ResolveFirstTransport(string fallbackHost, IReadOnlyList<ushort> entries)
    {
        foreach (StringBindingEntry entry in DecodeStringBindings(entries))
        {
            EndPoint? candidate = TryBuildEndpoint(fallbackHost, entry);
            if (candidate is not null)
            {
                return candidate;
            }
        }
        return null;
    }

    private static EndPoint? TryBuildEndpoint(string fallbackHost, in StringBindingEntry entry)
    {
        if (entry.TowerId == TowerIdTcp)
        {
            if (TryParseTcpAddress(entry.Address, fallbackHost, out string host, out int port))
            {
                return new DnsEndPoint(host, port);
            }
            return null;
        }

        if (entry.TowerId == TowerIdNamedPipe)
        {
            if (TryParseNamedPipeAddress(entry.Address, fallbackHost, out string host, out string pipeName))
            {
                return new NcacnNpEndPoint(host, pipeName);
            }
            return null;
        }

        return null;
    }

    private static bool TryParseTcpAddress(string address, string fallbackHost, out string host, out int port)
    {
        host = fallbackHost;
        port = 0;
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        int bracket = address.LastIndexOf('[');
        if (bracket < 0 || !address.EndsWith(']'))
        {
            return false;
        }

        string portText = address.Substring(bracket + 1, address.Length - bracket - 2);
        if (!int.TryParse(portText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out port))
        {
            return false;
        }

        string parsedHost = address[..bracket];
        host = string.IsNullOrWhiteSpace(parsedHost) ? fallbackHost : parsedHost;
        return true;
    }

    private static bool TryParseNamedPipeAddress(string address, string fallbackHost, out string host, out string pipeName)
    {
        host = fallbackHost;
        pipeName = string.Empty;
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        string trimmed = address.Trim();

        // UNC form: \\HOSTNAME\PIPE\NAME (or \\.\PIPE\NAME for local).
        if (trimmed.StartsWith("\\\\", StringComparison.Ordinal))
        {
            string rest = trimmed[2..];
            int firstSep = rest.IndexOf('\\');
            if (firstSep <= 0)
            {
                return false;
            }
            string parsedHost = rest[..firstSep];
            string remainder = rest[(firstSep + 1)..];
            host = string.IsNullOrWhiteSpace(parsedHost) || string.Equals(parsedHost, ".", StringComparison.Ordinal) ? fallbackHost : parsedHost;
            pipeName = NormalizePipeNameFragment(remainder);
            return !string.IsNullOrWhiteSpace(pipeName);
        }

        // DCE form: HOSTNAME[\PIPE\NAME] or HOSTNAME[PIPE\NAME] or
        // HOSTNAME[NAME]. The bracket body is the pipe path.
        int bracket = trimmed.LastIndexOf('[');
        if (bracket >= 0 && trimmed.EndsWith(']'))
        {
            string parsedHost = trimmed[..bracket];
            string bracketBody = trimmed.Substring(bracket + 1, trimmed.Length - bracket - 2);
            host = string.IsNullOrWhiteSpace(parsedHost) ? fallbackHost : parsedHost;
            pipeName = NormalizePipeNameFragment(bracketBody);
            return !string.IsNullOrWhiteSpace(pipeName);
        }

        // No brackets: treat the entire string as a pipe name on the
        // fallback host. The DCOM resolver occasionally emits this form
        // for legacy LRPC servers.
        pipeName = NormalizePipeNameFragment(trimmed);
        return !string.IsNullOrWhiteSpace(pipeName);
    }

    private static string NormalizePipeNameFragment(string fragment)
    {
        string value = fragment.Trim().Replace('/', '\\');
        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']')
        {
            value = value[1..^1];
        }
        int index = 0;
        while (index < value.Length && value[index] == '\\')
        {
            index++;
        }
        if (index > 0)
        {
            value = value[index..];
        }
        if (value.StartsWith("PIPE\\", StringComparison.OrdinalIgnoreCase))
        {
            value = value[5..];
        }
        return value;
    }

    private static IEnumerable<StringBindingEntry> DecodeStringBindings(ReadOnlyMemory<byte> bindings)
    {
        ReadOnlySpan<byte> span = bindings.Span;
        return DecodeStringBindings(span).ToArray();
    }

    private static IEnumerable<StringBindingEntry> DecodeStringBindings(ReadOnlySpan<byte> bindings)
    {
        var list = new List<StringBindingEntry>();
        if (bindings.Length < 4)
        {
            return list;
        }

        ushort secOffset = BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(2));
        int idx = 4;
        int entriesConsumed = 2;
        while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
        {
            ushort tower = BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
            idx += 2;
            entriesConsumed++;
            if (tower == TowerIdTerminator)
            {
                break;
            }

            var addressBuilder = new StringBuilder();
            while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
            {
                ushort ch = BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
                idx += 2;
                entriesConsumed++;
                if (ch == 0)
                {
                    break;
                }
                addressBuilder.Append((char)ch);
            }

            list.Add(new StringBindingEntry(tower, addressBuilder.ToString()));
        }
        return list;
    }

    private static IEnumerable<StringBindingEntry> DecodeStringBindings(IReadOnlyList<ushort> entries)
    {
        var list = new List<StringBindingEntry>();
        for (int index = 0; index < entries.Count;)
        {
            ushort towerId = entries[index++];
            if (towerId == TowerIdTerminator)
            {
                break;
            }

            var addressBuilder = new StringBuilder();
            while (index < entries.Count)
            {
                ushort value = entries[index++];
                if (value == 0)
                {
                    break;
                }
                addressBuilder.Append((char)value);
            }

            list.Add(new StringBindingEntry(towerId, addressBuilder.ToString()));
        }
        return list;
    }

    private readonly record struct StringBindingEntry(ushort TowerId, string Address);
}

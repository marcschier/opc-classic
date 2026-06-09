//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net;

namespace Opc.Classic.Dcom.Transport;

/// <summary>Endpoint for DCE/RPC over SMB named pipes (<c>ncacn_np</c>).</summary>
public sealed class NcacnNpEndPoint : EndPoint {
    private const int DefaultSmbPort = 445;
    private const string ProtocolSequence = "ncacn_np:";
    private const string DefaultHost = "localhost";

    /// <summary>Initializes a named-pipe RPC endpoint.</summary>
    public NcacnNpEndPoint(string host, string pipeEndpoint, int port = DefaultSmbPort) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (port is <= 0 or > 65535) {
            throw new ArgumentOutOfRangeException(nameof(port), port, "SMB port must be 1..65535.");
        }

        Host = TrimServerSlashes(host.Trim());
        PipeName = NormalizePipeName(pipeEndpoint);
        Port = port;
    }

    /// <summary>Gets the SMB server host name or address.</summary>
    public string Host { get; }

    /// <summary>Gets the normalized named-pipe path relative to IPC$.</summary>
    public string PipeName { get; }

    /// <summary>Gets the SMB TCP port.</summary>
    public int Port { get; }

    /// <summary>Parses <c>ncacn_np:host[\PIPE\name]</c> or a bare pipe endpoint.</summary>
    public static NcacnNpEndPoint Parse(string address, string defaultHost = DefaultHost, int port = DefaultSmbPort) {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultHost);

        string value = address.Trim();
        if (!value.StartsWith(ProtocolSequence, StringComparison.OrdinalIgnoreCase)) {
            return new NcacnNpEndPoint(defaultHost, value, port);
        }

        string body = value[ProtocolSequence.Length..];
        int bracketStart = body.IndexOf('[', StringComparison.Ordinal);
        int bracketEnd = body.LastIndexOf(']');
        if (bracketStart < 0 || bracketEnd <= bracketStart) {
            throw new FormatException("ncacn_np addresses must use ncacn_np:host[\\PIPE\\name].");
        }

        string host = body[..bracketStart];
        if (string.IsNullOrWhiteSpace(host)) {
            host = defaultHost;
        }

        string pipe = body.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
        return new NcacnNpEndPoint(host, pipe, port);
    }

    /// <summary>Normalizes <c>\PIPE\name</c> endpoints to the pipe path used by SMB2 CREATE.</summary>
    public static string NormalizePipeName(string pipeEndpoint) {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeEndpoint);

        string value = pipeEndpoint.Trim().Replace('/', '\\');
        if (value.StartsWith(ProtocolSequence, StringComparison.OrdinalIgnoreCase)) {
            return Parse(value).PipeName;
        }

        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']') {
            value = value[1..^1];
        }

        value = TrimLeadingSlashes(value);
        if (value.Equals("PIPE", StringComparison.OrdinalIgnoreCase)) {
            throw new FormatException("Named-pipe endpoint is missing the pipe name.");
        }

        if (value.StartsWith("PIPE\\", StringComparison.OrdinalIgnoreCase)) {
            value = TrimLeadingSlashes(value[4..]);
        }

        if (string.IsNullOrWhiteSpace(value)) {
            throw new FormatException("Named-pipe endpoint is missing the pipe name.");
        }

        return value;
    }

    /// <inheritdoc />
    public override string ToString() => $"ncacn_np:{Host}[\\PIPE\\{PipeName}]";

    private static string TrimServerSlashes(string host) => TrimLeadingSlashes(host);

    private static string TrimLeadingSlashes(string value) {
        int index = 0;
        while (index < value.Length && value[index] == '\\') {
            index++;
        }

        return index == 0 ? value : value[index..];
    }
}

// SPDX-License-Identifier: MIT

using System.Net;

namespace SharpCifs.Netbios;

public sealed class NbtAddress {
    private readonly string _hostName;

    private NbtAddress(string hostName) => _hostName = hostName;

    public static NbtAddress GetLocalHost() => new(Dns.GetHostName());

    public string GetHostName() => _hostName;
}
// SPDX-License-Identifier: MIT

namespace SharpCifs;

public sealed class UniAddress {
    private UniAddress(string hostName) => HostName = hostName;

    public string HostName { get; }

    public static UniAddress GetByName(string hostName) => new(hostName);

    public override string ToString() => HostName;
}
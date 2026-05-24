// SPDX-License-Identifier: MIT

namespace SharpCifs.Smb;

public sealed class NtlmPasswordAuthentication {
    public NtlmPasswordAuthentication(string domain, string username, string password) {
        Domain = domain;
        Username = username;
        Password = password;
    }

    public string Domain { get; }

    public string Username { get; }

    public string Password { get; }
}
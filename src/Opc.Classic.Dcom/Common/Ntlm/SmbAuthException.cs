// SPDX-License-Identifier: MIT

namespace SharpCifs.Smb;

public sealed class SmbAuthException : SmbException {
    public SmbAuthException() {
    }

    public SmbAuthException(string message)
        : base(message) {
    }
}

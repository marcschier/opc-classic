// SPDX-License-Identifier: MIT

using System.IO;

namespace SharpCifs.Smb;

public class SmbException : IOException {
    public SmbException() {
    }

    public SmbException(string message)
        : base(message) {
    }

    public SmbException(string message, System.Exception innerException)
        : base(message, innerException) {
    }

    public virtual int GetNtStatus() => HResult;
}
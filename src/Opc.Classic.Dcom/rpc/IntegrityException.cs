// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc; 

/// <inheritdoc/>
public class IntegrityException : RpcException {

    /// <inheritdoc/>
    public IntegrityException() {
    }

    /// <inheritdoc/>
    public IntegrityException(string message) : base(message) {
    }
}

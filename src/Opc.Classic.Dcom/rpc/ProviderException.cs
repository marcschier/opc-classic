// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc; 

/// <summary>
/// Provider exception
/// </summary>
public class ProviderException : RpcException {

    /// <inheritdoc/>
    public ProviderException() {
    }

    /// <inheritdoc/>
    public ProviderException(string message) :
        base(message) {
    }
}

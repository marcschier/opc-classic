// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Registry;

/// <summary>
/// Policy handle for each key.
/// </summary>
public class PolicyHandle {

    /// <summary>
    /// Handle to the Key
    /// </summary>
    public byte[] Handle { get; }

    /// <summary>
    /// True, if the key was newly created.
    /// </summary>
    public bool NewlyCreated { get; }

    /// <summary>
    /// Create handle
    /// </summary>
    /// <param name="newlyCreated"> </param>
    public PolicyHandle(bool newlyCreated) {
        NewlyCreated = newlyCreated;
        Handle = new byte[20];
    }
}

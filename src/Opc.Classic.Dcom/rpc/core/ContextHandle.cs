// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Context handle
/// </summary>
public class ContextHandle : NdrOp
{

    /// <summary>
    /// Attributes
    /// </summary>
    public int Attributes { get; set; }

    /// <summary>
    /// id
    /// </summary>
    public UUID Uuid { get; set; }

    /// <summary>
    /// Create handle
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="uuid"></param>
    public ContextHandle(int attributes, UUID uuid)
    {
        Attributes = attributes;
        Uuid = uuid;
    }
}

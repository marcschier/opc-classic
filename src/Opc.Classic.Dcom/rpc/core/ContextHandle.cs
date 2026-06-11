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
    /// <param name="attributes">Descriptor attributes that qualify the COM or Automation metadata.</param>
    /// <param name="uuid">UUID value encoded in the RPC or COM descriptor.</param>
    public ContextHandle(int attributes, UUID uuid)
    {
        Attributes = attributes;
        Uuid = uuid;
    }
}

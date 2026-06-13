// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>SAFEARRAYBOUNDS</i> structure of COM Automation.
/// </summary>
[Serializable]
public sealed class SafeArrayBounds
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly int cElements;
    public readonly int lLbound;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create safe array bounds structure
    /// </summary>
    /// <param name="values">Values being stored, encoded, or assigned.</param>
    internal SafeArrayBounds(Struct values)
    {
        if (values == null)
        {
            cElements = -1;
            lLbound = -1;
            return;
        }
        cElements = (int)values.GetMember(0);
        lLbound = (int)values.GetMember(0);
    }
}

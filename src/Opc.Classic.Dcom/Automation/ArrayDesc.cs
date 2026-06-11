// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>ARRAYDESC</i> structure of COM Automation.
/// Definition from MSDN: <i> Contained within the TYPEDESC, which describes the
/// type of the array's elements, and information about the array's dimensions.
/// </i>
/// </summary>
[Serializable]
public sealed class ArrayDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly TypeDesc typeDesc;
    public readonly short cDims;
    public readonly SafeArrayBounds[] safeArrayBounds;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values"></param>
    internal ArrayDesc(Struct values)
    {
        if (values == null)
        {
            typeDesc = null;
            cDims = -1;
            safeArrayBounds = null;
            return;
        }

        typeDesc = new TypeDesc((Struct)values.GetMember(0));
        cDims = (short)values.GetMember(1);
        var arry = (ComArray)values.GetMember(2);
        var arry2 = (object[])arry.ArrayInstance;

        if (arry2 != null)
        {
            safeArrayBounds = new SafeArrayBounds[arry2.Length];
            for (var i = 0; i < arry2.Length; i++)
            {
                safeArrayBounds[i] = new SafeArrayBounds((Struct)arry2[i]);
            }
        }
        else
        {
            safeArrayBounds = null;
        }
    }

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values"></param>
    internal ArrayDesc(ComPointer values) : this(values.IsNull ? null :
        (Struct)values.Referent)
    {
    }
}

// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>TYPEDESC</i> structure of COM Automation and
/// describes the type of a variable, the return type of a function,
/// or the type of a function parameter.
/// </summary>
[Serializable]
public sealed class TypeDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly ComPointer typeDesc;
    public readonly ComPointer arrayDesc;
    public readonly int hrefType;
    public readonly short vt;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary> pointer </summary>
    public static readonly short VT_PTR = 0x1a;
    /// <summary> safe array </summary>
    public static readonly short VT_SAFEARRAY = 0x1b;
    /// <summary> c-style array </summary>
    public static readonly short VT_CARRAY = 0x1c;
    /// <summary> user </summary>
    public static readonly short VT_USERDEFINED = 0x1d;

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values"></param>
    internal TypeDesc(ComPointer values) :
        this(values.IsNull ? null : (Struct)values.Referent)
    {
    }

    /// <summary>
    /// Create type description
    /// </summary>
    /// <param name="values"></param>
    internal TypeDesc(Struct values)
    {
        if (values == null)
        {
            typeDesc = null;
            arrayDesc = null;
            hrefType = -1;
            vt = -1;
            return;
        }

        vt = (short)values.GetMember(1);
        var union = (Union)values.GetMember(0);
        if (vt.Equals(VT_PTR) || vt.Equals(VT_SAFEARRAY))
        {
            var pointer = (ComPointer)union.Members[VT_PTR];
            if (pointer == null)
            {
                pointer = (ComPointer)union.Members[VT_SAFEARRAY];
            }
            typeDesc = new ComPointer(new TypeDesc(pointer), false);
            arrayDesc = null;
            hrefType = -1;
        }
        else if (vt.Equals(VT_CARRAY))
        {
            hrefType = -1;
            typeDesc = null;
            var pointer = (ComPointer)union.Members[VT_CARRAY];
            arrayDesc = new ComPointer(new ArrayDesc(pointer));
        }
        else if (vt.Equals(VT_USERDEFINED))
        {
            typeDesc = null;
            arrayDesc = null;
            hrefType = (int)union.Members[VT_USERDEFINED];
        }
        else
        {
            typeDesc = null;
            arrayDesc = null;
            hrefType = -1;
        }
    }
}

// SPDX-License-Identifier: MIT


using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>VARDESC</i> structure of COM Automation
/// </summary>
[Serializable]
public sealed class VarDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly int memberId;
    public readonly ComPointer lpstrSchema;
    public readonly Union u;
    public readonly ElemDesc elemdescVar;
    public readonly short wVarFlags;
    public readonly int varkind;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary> Per instance discriminant </summary>
    public const int VAR_PERINSTANCE = 0;
    /// <summary> Static discriminant </summary>
    public const int VAR_STATIC = 1;
    /// <summary> Const discriminant </summary>
    public const int VAR_CONST = 2;
    /// <summary> Dispatch discriminant </summary>
    public const int VAR_DISPATCH = 3;

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values">Values being stored, encoded, or assigned.</param>
    internal VarDesc(ComPointer values) :
        this(values.IsNull ? null : (Struct)values.Referent)
    {
    }

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="filledStruct">Structure instance populated with decoded COM field values.</param>
    internal VarDesc(Struct filledStruct)
    {
        if (filledStruct == null)
        {
            memberId = -1;
            lpstrSchema = null;
            u = null;
            elemdescVar = null;
            wVarFlags = -1;
            varkind = -1;
            return;
        }
        memberId = (int)filledStruct.GetMember(0);
        lpstrSchema = (ComPointer)filledStruct.GetMember(1);
        u = (Union)filledStruct.GetMember(2);
        elemdescVar = new ElemDesc((Struct)filledStruct.GetMember(3));
        wVarFlags = (short)filledStruct.GetMember(4);
        varkind = (int)filledStruct.GetMember(5);
    }
}

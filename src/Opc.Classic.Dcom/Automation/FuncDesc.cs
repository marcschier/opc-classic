// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>FUNCDESC</i> structure of COM Automation.
/// <i> Describes a function.</i>
/// See http://msdn.microsoft.com/en-us/library/ms221425(VS.85).aspx .
/// </summary>
/// <remarks>
///  MEMBERID memid;           // Function member ID.
///  /* [size_is] */ SCODE __RPC_FAR *lprgscode;
///  /* [size_is] */ ELEMDESC __RPC_FAR *lprgelemdescParam;
///  FuncKind funckind;        // Specifies whether the function is virtual, static, or dispatch-only.
///  InvokeKind invkind;       // Invocation kind. Indicates if this is a property function, and if so, what kind.
///  CallConv callconv;        // Specifies the function's calling
///                            // convention.
///  short cParams;            // Count of total number of parameters.
///  short cParamsOpt;         // Count of optional parameters (detailed
///                            // description follows).
///  short oVft;               // For FUNC_VIRTUAL, specifies the offset in the VTBL.
///  short cScodes;            // Count of permitted return values.
///  ELEMDESC elemdescFunc;    // Contains the return type of the function.
///  WORD wFuncFlags;          // Definition of flags follows.
/// </remarks>
[Serializable]
public sealed class FuncDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly int memberId;
    public readonly ComPointer lprgscode;
    public readonly ComPointer lprgelemdescParam;
    public readonly int funcKind;
    public readonly int invokeKind;
    public readonly int callConv;
    public readonly short cParams;
    public readonly short cParamsOpt;
    public readonly short oVft;
    public readonly short cScodes;
    public readonly ElemDesc elemdescFunc;
    public readonly short wFuncFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values">Values being stored, encoded, or assigned.</param>
    internal FuncDesc(ComPointer values) :
        this(values.IsNull ? null : (Struct)values.Referent)
    {
    }

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="filledStruct">Structure instance populated with decoded COM field values.</param>
    internal FuncDesc(Struct filledStruct)
    {
        if (filledStruct == null)
        {
            _values = null;
            memberId = -1;
            lprgscode = null;
            lprgelemdescParam = null;
            funcKind = -1;
            invokeKind = -1;
            callConv = -1;
            cParams = -1;
            cParamsOpt = -1;
            oVft = -1;
            cScodes = -1;
            elemdescFunc = null;
            wFuncFlags = -1;
            return;
        }
        _values = filledStruct;
        memberId = (int)_values.GetMember(0);
        lprgscode = (ComPointer)_values.GetMember(1);
        var ptr = (ComPointer)_values.GetMember(2);
        ComArray arrayOfElemDesc = null;
        if (!ptr.IsNull)
        {
            var arry = (ComArray)ptr.Referent;
            var obj = (object[])arry.ArrayInstance;
            arrayOfElemDesc = new ComArray(obj);
        }
        lprgelemdescParam = new ComPointer(arrayOfElemDesc);
        funcKind = (int)_values.GetMember(3);
        invokeKind = (int)_values.GetMember(4);
        callConv = (int)_values.GetMember(5);
        cParams = (short)_values.GetMember(6);
        cParamsOpt = (short)_values.GetMember(7);
        oVft = (short)_values.GetMember(8);
        cScodes = (short)_values.GetMember(9);
        elemdescFunc = new ElemDesc((Struct)_values.GetMember(10));
        wFuncFlags = (short)_values.GetMember(11);
    }

    private readonly Struct _values;
}

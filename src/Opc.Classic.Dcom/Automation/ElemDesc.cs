// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation; 
/// <summary>
/// Implements the <i>ELEMDESC</i> structure of COM Automation.
/// Definition from MSDN: <i> Includes the type description and process-transfer
/// information for a variable a function, or a function parameter.</i>
/// </summary>
[Serializable]
public sealed class ElemDesc {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly TypeDesc TypeDesc;
    public readonly ParamDesc paramDesc;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Element descriptor
    /// </summary>
    /// <param name="values"></param>
    public ElemDesc(Struct values) {
        if (values == null) {
            TypeDesc = null;
            paramDesc = null;
            return;
        }
        TypeDesc = new TypeDesc((Struct)values.GetMember(0));
        paramDesc = new ParamDesc((Struct)values.GetMember(1));
    }

    /// <summary>
    /// Element descriptor
    /// </summary>
    /// <param name="ptrValues"></param>
    internal ElemDesc(ComPointer ptrValues) :
        this(ptrValues.IsNull ? null : (Struct)ptrValues.Referent) {
    }
}

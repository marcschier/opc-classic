// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>PARAMDESC</i> structure of COM Automation. Contains
/// information needed for transferring a structure element, parameter,
/// or function return value between processes.
/// </summary>
[Serializable]
public sealed class ParamDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly ComPointer lpVarValue;
    public readonly ParamFlag wPARAMFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create param description
    /// </summary>
    /// <param name="values">Values being stored, encoded, or assigned.</param>
    internal ParamDesc(Struct values)
    {
        if (values == null)
        {
            lpVarValue = null;
            wPARAMFlags = (ParamFlag)(-1);
            return;
        }

        lpVarValue = (ComPointer)values.GetMember(0);
        wPARAMFlags = (ParamFlag)values.GetMember(1);
    }
}

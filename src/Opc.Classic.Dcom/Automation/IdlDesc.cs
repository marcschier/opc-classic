// SPDX-License-Identifier: MIT


using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// IDL description
/// </summary>
[Serializable]
public sealed class IdlDesc
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly ComPointer dwReserved;
    public readonly IdlFlag wIDLFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create description
    /// </summary>
    /// <param name="values"></param>
    internal IdlDesc(Struct values)
    {
        if (values == null)
        {
            dwReserved = null;
            wIDLFlags = (IdlFlag)(-1);
            return;
        }
        dwReserved = (ComPointer)values.GetMember(0);
        wIDLFlags = (IdlFlag)values.GetMember(1);
    }
}

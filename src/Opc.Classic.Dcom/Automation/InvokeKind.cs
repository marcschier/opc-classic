// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>INVOKEKIND</i> type of COM Automation.
/// </summary>
public enum InvokeKind
{

    /// <summary>
    /// The member is called using a normal function invocation syntax
    /// </summary>
    INVOKE_FUNC = 1,

    /// <summary>
    /// The function is invoked using a normal property-access syntax.
    /// </summary>
    INVOKE_PROPERTYGET = 2,

    /// <summary>
    /// The function is invoked using a property value assignment syntax.
    /// Syntactically, a typical programming language might represent
    /// changing a property in the same way as assignment.
    /// For example:object.property : = value.
    /// </summary>
    INVOKE_PROPERTYPUT = 4,

    /// <summary>
    /// The function is invoked using a property reference
    /// assignment syntax.
    /// </summary>
    INVOKE_PROPERTYPUTREF = 8,
}

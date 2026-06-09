// SPDX-License-Identifier: MIT


using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Creates automation related objects.
/// Internal Factory, to be used only by the framework.
/// </summary>
public static class AutomationFactory {

    /// <summary>
    /// Narrow object
    /// </summary>
    /// <param name="comObject"></param>
    /// <exception cref="InteropException"></exception>
    /// <returns></returns>
    public static IComObject NarrowObject(IComObject comObject) {
        var retval = comObject;
        switch (comObject.InterfaceIdentifier.ToUpperInvariant()) {
            case Interfaces.IID_IDispatch:
                return new DispatchImpl(retval);
            case Interfaces.IID_ITypeInfo:
                return new TypeInfoImpl(retval);
            case Interfaces.IID_ITypeLib:
                return new TypeLibImpl(retval);
            case Interfaces.IID_IEnumVARIANT:
                return new EnumVARIANTImpl(retval);
            default:
                return comObject;
        }
    }
}

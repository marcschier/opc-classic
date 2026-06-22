// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Dispatch constants
/// </summary>
public static class DispatchFlags
{
    /// <summary>
    /// Flag for selecting a <code>method</code>.
    /// </summary>
    public const int DISPATCH_METHOD = unchecked((int)0xFFFFFFF1);

    /// <summary>
    /// Flag for selecting a Property <code>propget</code>.
    /// </summary>
    public const int DISPATCH_PROPERTYGET = unchecked((int)0xFFFFFFF2);

    /// <summary>
    /// Flag for selecting a Property <code>propput</code>.
    /// </summary>
    public const int DISPATCH_PROPERTYPUT = unchecked((int)0xFFFFFFF4);

    /// <summary>
    /// COM <code>DISPID</code> for property "put" or "putRef".
    /// </summary>
    public const int DISPATCH_DISPID_PUTPUTREF = unchecked((int)0xFFFFFFFD);

    /// <summary>
    /// Flag for selecting a Property <code>propputref</code>.
    /// </summary>
    public const int DISPATCH_PROPERTYPUTREF = unchecked((int)0xFFFFFFF8);
}

//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Automation; 

/// <summary>
/// Dispatch constants
/// </summary>
public static class DispatchFlags {

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

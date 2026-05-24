//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Core;
using System;

namespace SharpInterop.Automation; 
/// <summary>
/// Implements the <i>SAFEARRAYBOUNDS</i> structure of COM Automation.
/// </summary>
[Serializable]
public sealed class SafeArrayBounds {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly int cElements;
    public readonly int lLbound;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create safe array bounds structure
    /// </summary>
    /// <param name="values"></param>
    internal SafeArrayBounds(Struct values) {
        if (values == null) {
            cElements = -1;
            lLbound = -1;
            return;
        }
        cElements = (int)values.GetMember(0);
        lLbound = (int)values.GetMember(0);
    }
}

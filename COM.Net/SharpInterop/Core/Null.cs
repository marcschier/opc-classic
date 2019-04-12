//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using System;

    /// <summary>
    /// Null type
    /// </summary>
    [Serializable]
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
    public sealed class Null {
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.

        /// <summary>
        /// Null value
        /// </summary>
        public static Null Value { get; } = new Null();
    }
}
//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using System;

    /// <summary>
    /// Empty type
    /// </summary>
    [Serializable]
    public sealed class Empty {

        /// <summary>
        /// Empty value
        /// </summary>
        public static Empty Value { get; } = new Empty();
    }
}
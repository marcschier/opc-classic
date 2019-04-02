// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using System;


    /// <summary>
    /// Class representing the unsigned c++ short.
    /// </summary>
    public sealed class JIUnsignedShort : IJIUnsigned {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="shortValue"></param>
        internal JIUnsignedShort(int? shortValue) {
            if (shortValue == null || shortValue < 0) {
                throw new ArgumentException(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UNSIGNED_NEGATIVE));
            }
            _value = shortValue;
        }

        /// <inheritdoc/>
        public int Type => JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;

        /// <inheritdoc/>
        public object Value => _value;

        private readonly int? _value;
    }
}
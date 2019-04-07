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

    /// <summary>
    /// Class representing the unsigned c++ byte.
    /// </summary>
    public sealed class JIUnsignedByte : IJIUnsigned {

        internal JIUnsignedByte(short? byteValue) {
            if (byteValue == null || (short)byteValue < 0) {
                throw new System.ArgumentException(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UNSIGNED_NEGATIVE));
            }
            _value = byteValue;
        }

        /// <inheritdoc/>
        public int Type => JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;

        /// <inheritdoc/>
        public object Value => _value;

        private readonly short? _value;
    }
}
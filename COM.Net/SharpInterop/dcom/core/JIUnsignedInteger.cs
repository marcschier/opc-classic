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
    /// Class representing the unsigned c++ integer.
    /// </summary>
    public sealed class JIUnsignedInteger : IJIUnsigned {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="intValue"></param>
		internal JIUnsignedInteger(long? intValue) {
            if (intValue == null || (long)intValue < 0) {
                throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UNSIGNED_NEGATIVE));
            }
            value = intValue;
        }

        /// <inheritdoc/>
        public int Type => JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;

        /// <inheritdoc/>
        public object Value => value;

        private readonly long? value;
    }
}
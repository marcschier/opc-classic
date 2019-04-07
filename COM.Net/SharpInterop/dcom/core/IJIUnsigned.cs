//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {

    /// <summary>
    /// Representation of a C++ <i>unsigned</i> number. An unsigned number can
    /// be obtained by using <seealso cref="JIUnsignedFactory.GetUnsigned(object, int)"/>.
    /// </summary>
    public interface IJIUnsigned {

        /// <summary>
        /// Returns the unsigned type (<code>byte</code>,<code>short</code>,<code>int</code>).
        /// </summary>
        /// <returns> <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE"/> or
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT"/> or
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT"/> </returns>
        int Type { get; }

        /// <summary>
        /// Returns the number represented by this object.
        /// </summary>
        /// <returns> value  </returns>
        object Value { get; }
    }
}
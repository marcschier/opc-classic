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
    /// Representation of C++ "Unsigned Types".
    /// </summary>
    public static class JIUnsignedFactory {

        /// <summary>
        /// Returns an implementation for each of the <code>unsigned</code> type.
        /// Only 3 types are supported at present
        /// <code>Byte</code>, <code>Short</code>, <code>Integer</code>.
        /// Since Java has no support for unsigned types, use a
        /// <code>Short</code> for a <code>Byte</code>, <code>Integer</code>
        /// for a <code>Short</code> and <code>Long</code> for an
        /// <code>Integer</code>. This is to accomodate the entire
        /// spectrum for the <code>unsigned</code> type and prevent the rollover problem.
        /// </summary>
        /// <param name="value"> <code>Short</code>, <code>Integer</code>, <code>Long</code> only </param>
        /// <param name="flag">  JIFlags unsigned flags
        /// </param>
        /// <exception cref="System.ArgumentException"> if the <code>value</code>
        /// is not an instance of the supported types or an incorrect
        /// <code>flag</code> has been provided. </exception>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT"> </seealso>
        public static IJIUnsigned GetUnsigned(object value, int flag) {
            IJIUnsigned retVal = null;
            if (!(value is short?) && !(value is long?) && !(value is int?)) {
                throw new System.ArgumentException(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UNSIGNED_INCORRECT_TYPE));
            }
            switch (flag) {
                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
                    retVal = new JIUnsignedByte((short?)value);
                    break;
                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
                    retVal = new JIUnsignedShort((int?)value);
                    break;
                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
                    retVal = new JIUnsignedInteger((long?)value);
                    break;
                default:
                    throw new System.ArgumentException(
                        JISystem.getLocalizedMessage(JIErrorCodes.JI_UNSIGNED_INCORRECT_TYPE));
            }
            return retVal;
        }
    }
}
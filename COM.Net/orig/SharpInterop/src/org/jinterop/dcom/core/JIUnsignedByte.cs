/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {

    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JISystem = org.jinterop.dcom.common.JISystem;

    /// <summary>
    /// Class representing the unsigned c++ byte.
    /// 
    /// @since 1.15(b)
    /// 
    /// </summary>
    public sealed class JIUnsignedByte : IJIUnsigned {

        private readonly short? ByteValue;

        public JIUnsignedByte(short? byteValue) {
            if (byteValue == null || (short)byteValue < 0) {
                throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNSIGNED_NEGATIVE));
            }
            this.ByteValue = byteValue;
        }

        public int Type {
            get {
                return JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
            }
        }

        public Number Value {
            get {
                return ByteValue;
            }
        }

    }

}
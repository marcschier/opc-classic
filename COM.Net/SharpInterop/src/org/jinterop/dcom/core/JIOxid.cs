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
    using System.Linq;

    /// <summary>
    /// Oxid
    /// </summary>
    [Serializable]
    internal sealed class JIOxid {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="oxid"></param>
        internal JIOxid(byte[] oxid) {
            _oxid = oxid;
        }

        /// <summary>
        /// Oxid
        /// </summary>
        internal byte[] OXID => _oxid;

        /// <inheritdoc/>
        public override int GetHashCode() {
            var result = 1;
            //from SUN
            for (var i = 0; i < OXID.Length; i++) {
                result = 31 * result + OXID[i];
            }
            return result;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JIOxid other)) {
                return false;
            }
            return _oxid.SequenceEqual(other.OXID);
        }

        internal byte[] _oxid;
    }
}
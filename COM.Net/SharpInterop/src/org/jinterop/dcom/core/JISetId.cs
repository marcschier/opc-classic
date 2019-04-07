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

    [Serializable]
    internal sealed class JISetId {

        /// <summary>
        /// Identifier
        /// </summary>
        internal byte[] SetID { get; }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="setid"></param>
        internal JISetId(byte[] setid) {
            SetID = setid;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            var result = 1;
            //from SUN
            for (var i = 0; i < SetID.Length; i++) {
                result = 31 * result + SetID[i];
            }
            return result;
            //return Arrays.hashCode(setid);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JISetId other)) {
                return false;
            }
            return SetID.SequenceEqual(other.SetID);
        }
    }
}
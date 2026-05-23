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
    using System.Linq;

    [Serializable]
    internal sealed class SetId {

        /// <summary>
        /// Identifier
        /// </summary>
        internal byte[] Value { get; }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="setid"></param>
#pragma warning disable RECS0154 // Parameter is never used
        internal SetId(byte[] setid) => Value = setid;
#pragma warning restore RECS0154 // Parameter is never used

        /// <inheritdoc/>
        public override int GetHashCode() {
            var result = 1;
            // from SUN
            for (var i = 0; i < Value.Length; i++) {
                result = (31 * result) + Value[i];
            }
            return result;
            // return Arrays.hashCode(setid);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is SetId other)) {
                return false;
            }
            return Value.SequenceEqual(other.Value);
        }
    }
}
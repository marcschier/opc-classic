//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Core {
    using Opc.Classic.Dcom.Internal.LegacyNdr;

    /// <summary>
    /// Context handle
    /// </summary>
    public class ContextHandle : NdrOp {

        /// <summary>
        /// Attributes
        /// </summary>
        public int Attributes { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public UUID Uuid { get; set; }

        /// <summary>
        /// Create handle
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="uuid"></param>
        public ContextHandle(int attributes, UUID uuid) {
            Attributes = attributes;
            Uuid = uuid;
        }
    }
}
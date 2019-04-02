// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.common {
    using rpc.core;
    using System.Collections;

    /// <summary>
    /// Framework Internal.
    /// </summary>
    public interface IJICOMRuntimeWorker {

        /// <summary>
        /// Op
        /// </summary>
        int Opnum { set; }

        /// <summary>
        /// Current iid
        /// </summary>
        string CurrentIID { set; }

        /// <summary>
        /// Current object
        /// </summary>
        UUID CurrentObjectID { set; get; }

        /// <summary>
        /// Query interface ids
        /// </summary>
        IList QIedIIDs { get; }

        /// <summary>
        /// Resolver
        /// </summary>
        bool Resolver { get; }

        /// <summary>
        /// Worker
        /// </summary>
        /// <returns></returns>
        bool workerOver();
    }
}
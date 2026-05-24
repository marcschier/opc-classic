//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Rpc.Core;
using System.Collections.Generic;

namespace SharpInterop.Common; 
/// <summary>
/// Framework Internal.
/// </summary>
public interface IComRuntimeWorker {

    /// <summary>
    /// Set op number
    /// </summary>
    int Opnum { get; set; }

    /// <summary>
    /// Current iid
    /// </summary>
    string CurrentIID { get; set; }

    /// <summary>
    /// Current object
    /// </summary>
    UUID CurrentObjectID { get; set; }

    /// <summary>
    /// Query interface ids
    /// </summary>
    List<string> QIedIIDs { get; }

    /// <summary>
    /// Resolver
    /// </summary>
    bool Resolver { get; }

    /// <summary>
    /// Worker
    /// </summary>
    /// <returns></returns>
    bool WorkerOver();
}

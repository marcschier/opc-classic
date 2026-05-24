//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc; 

/// <summary>
/// Rpc constants
/// </summary>
public enum RpcImpersonationLevel {

    /// <summary>
    /// identfiy
    /// </summary>
    RPC_C_IMP_LEVEL_IDENTIFY = 2,

    /// <summary>
    /// Impersonate
    /// </summary>
    RPC_C_IMP_LEVEL_IMPERSONATE = 3,
}

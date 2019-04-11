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

namespace org.jinterop.dcom.common {

    /// <summary>
    /// Interface for setting user credentials.
    /// 
    /// @since 1.0
    /// </summary>
    public interface IJIAuthInfo {

        /// <summary>
        /// Returns username.
        /// 
        /// @return
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Returns password.
        /// 
        /// @return
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Returns user's domain.
        /// 
        /// @return
        /// </summary>
        string Domain { get; }

    }

}
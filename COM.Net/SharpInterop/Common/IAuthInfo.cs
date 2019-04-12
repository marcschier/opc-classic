//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Common {

    /// <summary>
    /// Interface for setting user credentials.
    /// </summary>
    public interface IAuthInfo {

        /// <summary>
        /// Returns username.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Returns password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Returns user's domain.
        /// </summary>
        string Domain { get; }
    }
}
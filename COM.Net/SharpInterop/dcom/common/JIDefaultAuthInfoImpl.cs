//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.common {

    /// <summary>
    /// Default implementation of <code>IJIAuthInfo</code>.
    /// </summary>
    public sealed class JIDefaultAuthInfoImpl : IJIAuthInfo {

        /// <summary>
        ///Creates the AuthInfo Object.
        /// </summary>
        /// <param name="domain"> </param>
        /// <param name="username"> </param>
        /// <param name="password"> </param>
        public JIDefaultAuthInfoImpl(string domain, string username, 
            string password) {
            UserName = username;
            Password = password;
            Domain = domain;
        }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; } = null;

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; } = null;

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; } = null;
    }
}
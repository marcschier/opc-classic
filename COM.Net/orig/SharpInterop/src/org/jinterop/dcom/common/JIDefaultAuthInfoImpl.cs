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
    /// Default implementation of <code>IJIAuthInfo</code>.
    /// 
    /// @since 1.0
    /// </summary>
    public sealed class JIDefaultAuthInfoImpl : IJIAuthInfo {

        private string Username = null;
        private string Password_Renamed = null;
        private string Domain_Renamed = null;

        /// <summary>
        ///Creates the AuthInfo Object.
        /// </summary>
        /// <param name="domain"> </param>
        /// <param name="username"> </param>
        /// <param name="password"> </param>
        public JIDefaultAuthInfoImpl(string domain, string username, string password) {
            this.Username = username;
            this.Password_Renamed = password;
            this.Domain_Renamed = domain;
        }
        public string UserName {
            get {
                return Username;
            }
        }

        public string Password {
            get {
                return Password_Renamed;
            }
        }

        public string Domain {
            get {
                return Domain_Renamed;
            }
        }

    }

}
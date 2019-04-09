// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.winreg {
    using org.jinterop.dcom.common;
    using org.jinterop.winreg.smb;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Factory to get an implementation of <code>IJIWinReg</code>.
    /// This interface uses "Windows Remote Registry" and "Server" 
    /// services and these must be running on target workstation.
    /// based upon the transport intended to be used this factory 
    /// provides either the smb impl or the tcp/ip one.
    /// </summary>
    public class JIWinRegFactory {

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIWinRegFactory() {
        }

        /// <summary>
        /// Instantiates the Factory.
        /// </summary>
        public static JIWinRegFactory SingleTon {
            get {
                if (_factory == null) {
                    lock (typeof(JIWinRegFactory)) {
                        if (_factory == null) {
                            _factory = new JIWinRegFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        /// <summary>
        /// Gets an Implementation of WinReg interface, 
        /// currently only SMB transport is supported.
        /// </summary>
        /// <param name="authInfo"> credentials for access
        /// to Windows Remote Registry service </param>
        /// <param name="serverName"> target server </param>
        /// <param name="smbTransport"> true if SMB transport
        /// is required, false will return null.
        /// </param>
        /// <exception cref="UnknownHostException"> </exception>
        public virtual IJIWinReg GetWinreg(IJIAuthInfo authInfo, 
            string serverName, bool smbTransport) {
            if (smbTransport) {
                return new JIWinRegStub(authInfo, serverName);
            }
            return null;
        }

        /// <summary>
        /// Gets an Implementation of WinReg interface, 
        /// currently only SMB transport is supported.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="smbTransport"> true if SMB transport 
        /// is required, false will return null.
        /// </param>
        /// <exception cref="UnknownHostException"> </exception>
        public virtual IJIWinReg GetWinreg(string serverName, 
            bool smbTransport) {
            if (smbTransport) {
                return new JIWinRegStub(serverName);
            }
            return null;
        }

        private static JIWinRegFactory _factory;
    }
}
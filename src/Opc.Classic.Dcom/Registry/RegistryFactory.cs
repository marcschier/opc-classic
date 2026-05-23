//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Registry {
    using SharpInterop.Common;
    using SharpInterop.Registry.Smb;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Factory to get an implementation of <code>IRegistry</code>.
    /// This interface uses "Windows Remote Registry" and "Server"
    /// services and these must be running on target workstation.
    /// based upon the transport intended to be used this factory
    /// provides either the smb impl or the tcp/ip one.
    /// </summary>
    public class RegistryFactory {

        /// <summary>
        /// Private constructor
        /// </summary>
        private RegistryFactory() {
        }

        /// <summary>
        /// Instantiates the Factory.
        /// </summary>
        public static RegistryFactory Instance {
            get {
                if (_factory == null) {
                    lock (typeof(RegistryFactory)) {
                        if (_factory == null) {
                            _factory = new RegistryFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        /// <summary>
        /// Gets an Implementation of IRegistry interface,
        /// currently only SMB transport is supported.
        /// </summary>
        /// <param name="authInfo"> credentials for access
        /// to Windows Remote Registry service </param>
        /// <param name="serverName"> target server </param>
        /// <param name="smbTransport"> true if SMB transport
        /// is required, false will return null.
        /// </param>
        /// <exception cref="UnknownHostException"> </exception>
        public IRegistry GetRegistryClient(IAuthInfo authInfo, string serverName,
            bool smbTransport) {
            if (smbTransport) {
                return new RegistryStub(authInfo, serverName);
            }
            return null;
        }

        /// <summary>
        /// Gets an Implementation of IRegistry interface,
        /// currently only SMB transport is supported.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="smbTransport"> true if SMB transport
        /// is required, false will return null.
        /// </param>
        /// <exception cref="UnknownHostException"> </exception>
        public IRegistry GetRegistryClient(string serverName, bool smbTransport) {
            if (smbTransport) {
                return new RegistryStub(serverName);
            }
            return null;
        }

        private static RegistryFactory _factory;
    }
}
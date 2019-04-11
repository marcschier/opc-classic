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

namespace org.jinterop.winreg {

    using IJIAuthInfo = org.jinterop.dcom.common.IJIAuthInfo;
    using JIWinRegStub = org.jinterop.winreg.smb.JIWinRegStub;



    /// <summary>
    /// Factory to get an implementation of <code>IJIWinReg</code>.
    /// <para>This interface uses "Windows Remote Registry" and "Server" services and these must be running on target workstation.
    /// 
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    //based upon the transport intended to be used this
    //factory provides either the smb impl of ijiwinreg or the tcp/ip one.
    public class JIWinRegFactory {

        private JIWinRegFactory() {
        };

        private static JIWinRegFactory Factory = null;

        /// <summary>
        /// Instantiates the Factory.
        /// 
        /// @return
        /// </summary>
        public static JIWinRegFactory SingleTon {
            get {
                if (Factory == null) {
                    lock (typeof(JIWinRegFactory)) {
                        if (Factory == null) {
                            Factory = new JIWinRegFactory();
                        }
                    }
                }
    
                return Factory;
            }
        }

        /// <summary>
        /// Gets an Implementation of WinReg interface, currently only SMB transport is supported.
        /// </summary>
        /// <param name="authInfo"> credentials for access to Windows Remote Registry service </param>
        /// <param name="serverName"> target server </param>
        /// <param name="smbTransport"> true if SMB transport is required , false will return null.
        /// @return </param>
        /// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIWinReg getWinreg(org.jinterop.dcom.common.IJIAuthInfo authInfo,String serverName, boolean smbTransport) throws java.net.UnknownHostException
        public virtual IJIWinReg GetWinreg(IJIAuthInfo authInfo, string serverName, bool smbTransport) {
            if (smbTransport) {
                return new JIWinRegStub(authInfo,serverName);
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Gets an Implementation of WinReg interface, currently only SMB transport is supported.
        /// </summary>
        /// <param name="smbTransport"> true if SMB transport is required , false will return null.
        /// @return </param>
        /// <exception cref="UnknownHostException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIWinReg getWinreg(String serverName, boolean smbTransport) throws java.net.UnknownHostException
        public virtual IJIWinReg GetWinreg(string serverName, bool smbTransport) {
            if (smbTransport) {
                return new JIWinRegStub(serverName);
            }
            else {
                return null;
            }
        }
    }

}
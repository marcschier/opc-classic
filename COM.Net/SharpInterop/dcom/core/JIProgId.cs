//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//
namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using org.jinterop.winreg;
    using SharpCifs.Util.Sharpen;
    using System;

    /// <summary>
    /// Wrapper class used to define user friendly <code>ProgID</code>.
    /// Definition from MSDN:
    /// <i>
    /// A ProgID, or programmatic identifier, is a registry entry that can be associated
    /// with a CLSID. The format of a ProgID is &lt;Vendor&gt;.&lt;Component&gt;.&lt;Version&gt;,
    /// separated by periods and with no spaces, as in Word.Document.6. Like the CLSID, the ProgID
    /// identifies a class, but with less precision.
    /// </i>
    /// This class uses the <code>WINREG</code> service to get the mapping between the <code>ProgId</code>
    /// and the <code>CLSID</code>.
    /// The <code>WINREG</code> package of j-Interop is capable of querying the Windows registry in a
    /// platform independent way using SMB. The internal database is looked up first before
    /// making calls to <code>WINREG</code> service.
    /// </summary>
    public class JIProgId {

        /// <summary>
        /// Indicates to the framework, if Windows Registry settings for DLL\OCX
        /// component identified by this object should be modified to add a <code>Surrogate</code>
        /// automatically. A <code>Surrogate</code> is a process which provides resources
        /// such as memory and cpu for a DLL\OCX to execute.
        /// </summary>
        /// <remarks> <code>true</code> if auto registration should be done by the framework. </remarks>
        public virtual bool AutoRegistration {
            set => autoRegister = value;
        }

        /// <summary>
        /// Returns the status of the auto registration flag for the component identified
        /// by this object.
        /// </summary>
        /// <returns> <code>true</code> if the auto registration flag is set. </returns>
        public virtual bool AutoRegistrationSet => autoRegister;

        /// <summary>
        /// Create prog id
        /// </summary>
        /// <param name="progId"></param>
        private JIProgId(string progId) {
            this.progId = progId;
            clsid = JIClsid.ValueOf(JISystem.GetClsidFromProgId(progId));
        }

        /// <summary>
        /// Set server
        /// </summary>
		internal virtual string Server {
            set => server = value;
        }

        /// <summary>
        /// Get id from winreg
        /// </summary>
        /// <exception cref="JIException"></exception>
        private void GetIdFromWinReg() {
            IJIWinReg winreg;
            //winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),session.getPassword()),server,true);
            //System.out.println("Encoding the password...");

            //		try {
            //			winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),URLEncoder.encode(session.getPassword(),"UTF-8")),server,true);
            //		} catch (UnsupportedEncodingException e) {
            //			try {
            //				winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),URLEncoder.encode(session.getPassword(),System.getProperty("file.encoding"))),server,true);
            //			} catch (UnsupportedEncodingException e1) {
            //				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION2);
            //			}catch (UnknownHostException e2)
            //			{
            //				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
            //			}
            //		} catch (UnknownHostException e)
            //		{
            //			throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
            //		}

            if (server == null) {
                server = session.TargetServer;
            }

            try {
                if (session.SSOEnabled) {
                    winreg = JIWinRegFactory.SingleTon.GetWinreg(
                        server, true);
                }
                else {
                    winreg = JIWinRegFactory.SingleTon.GetWinreg(
                        new JIDefaultAuthInfoImpl(session.Domain, session.UserName, session.Password), server, true);
                }

            }
            catch (UnknownHostException) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
            }
            var handle = winreg.winreg_OpenHKLM();
            var handle2 = winreg.winreg_OpenKey(handle, "SOFTWARE\\Classes\\" + progId + "\\CLSID", IJIWinReg_Fields.KEY_READ);
            var key = StringHelperClass.NewString(winreg.winreg_QueryValue(handle2, 255));
            winreg.winreg_CloseKey(handle2);
            winreg.winreg_CloseKey(handle);
            winreg.CloseConnection();
            //seperate the {}
            clsid = JIClsid.ValueOf(StringHelperClass.SubstringSpecial(key,
                key.IndexOf("{", StringComparison.Ordinal) + 1, key.IndexOf("}", StringComparison.Ordinal)));
            clsid.AutoRegistration = autoRegister;
            JISystem.Internal_setClsidtoProgId(progId, clsid.CLSID);
        }

        /// <summary>
        /// Factory method returning an instance of this class.
        /// </summary>
        /// <param name="progId"> user-friendly string representation such as "Excel.Application"
        /// </param>
        public static JIProgId ValueOf(string progId) {
            return new JIProgId(progId);
        }

        /// <summary>
        /// Returns the <code>CLSID</code> for this <code>ProgId</code>.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        public virtual JIClsid CorrespondingCLSID {
            get {
                if (clsid == null) {
                    GetIdFromWinReg();
                }
                return clsid;
            }
        }

        /// <summary>
        /// Set session
        /// </summary>
        internal virtual JISession Session {
            set => session = value;
        }

        private readonly string progId;
        private JIClsid clsid;
        private JISession session;
        private string server;
        private bool autoRegister;
    }
}
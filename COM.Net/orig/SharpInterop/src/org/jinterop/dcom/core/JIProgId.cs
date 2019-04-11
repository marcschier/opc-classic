using System;

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

namespace org.jinterop.dcom.core {


    using JIDefaultAuthInfoImpl = org.jinterop.dcom.common.JIDefaultAuthInfoImpl;
    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIWinReg = org.jinterop.winreg.IJIWinReg;
    using JIPolicyHandle = org.jinterop.winreg.JIPolicyHandle;
    using JIWinRegFactory = org.jinterop.winreg.JIWinRegFactory;


    /// <summary>
    ///<para>Wrapper class used to define user friendly <code>ProgID</code>.
    /// </para>
    /// <para> Definition from MSDN: <i>
    ///  A ProgID, or programmatic identifier, is a registry entry that can be associated
    ///  with a CLSID. The format of a ProgID is <Vendor>.<Component>.<Version>, separated
    ///  by periods and with no spaces, as in Word.Document.6. Like the CLSID, the ProgID
    ///  identifies a class, but with less precision.
    ///  </i>
    /// </para>
    /// <para>
    /// This class uses the <code>WINREG</code> service to get the mapping between the <code>ProgId</code>
    /// and the <code>CLSID</code>.
    /// </para>
    /// <para>
    /// The <code>WINREG</code> package of j-Interop is capable of querying the Windows registry in a
    /// platform independent way using SMB. The internal database is looked up first before
    /// making calls to <code>WINREG</code> service.
    /// </para>
    /// @since 1.0
    /// </summary>
    public class JIProgId {

        private string ProgId = null;
        private JIClsid Clsid = null;
        private JISession Session_Renamed = null;
        private string Server_Renamed = null;
        private bool AutoRegister = false;

        /// <summary>
        /// Indicates to the framework, if Windows Registry settings for DLL\OCX
        /// component identified by this object should be modified to add a <code>Surrogate</code>
        /// automatically. A <code>Surrogate</code> is a process which provides resources
        /// such as memory and cpu for a DLL\OCX to execute.
        /// </summary>
        /// <param name="autoRegister"> <code>true</code> if auto registration should be done by the framework. </param>
        public virtual bool AutoRegistration {
            set {
                this.AutoRegister = value;
            }
        }

        /// <summary>
        ///Returns the status of the auto registration flag for the component identified by this object.
        /// </summary>
        /// <returns> <code>true</code> if the auto registration flag is set. </returns>
        public virtual bool AutoRegistrationSet {
            get {
                return AutoRegister;
            }
        }

        private JIProgId(string progId) {
            this.ProgId = progId;
            Clsid = JIClsid.ValueOf(JISystem.GetClsidFromProgId(progId));
        }

        public virtual string Server {
            set {
                this.Server_Renamed = value;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void getIdFromWinReg() throws org.jinterop.dcom.common.JIException
        private void GetIdFromWinReg() {
            IJIWinReg winreg;
            //winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),session.getPassword()),server,true);
            //System.out.println("Encoding the password...");

    //        try {
    //            winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),URLEncoder.encode(session.getPassword(),"UTF-8")),server,true);
    //        } catch (UnsupportedEncodingException e) {
    //            try {
    //                winreg = JIWinRegFactory.getSingleTon().getWinreg(new JIDefaultAuthInfoImpl(session.getDomain(),session.getUserName(),URLEncoder.encode(session.getPassword(),System.getProperty("file.encoding"))),server,true);
    //            } catch (UnsupportedEncodingException e1) {
    //                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION2);
    //            }catch (UnknownHostException e2)
    //            {
    //                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
    //            }
    //        } catch (UnknownHostException e)
    //        {
    //            throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
    //        }

            if (Server_Renamed == null) {
                Server_Renamed = Session_Renamed.TargetServer;
            }

            try {
                if (Session_Renamed.SSOEnabled) {
                    winreg = JIWinRegFactory.SingleTon.GetWinreg(Server_Renamed,true);
                }
                else {
                    winreg = JIWinRegFactory.SingleTon.GetWinreg(new JIDefaultAuthInfoImpl(Session_Renamed.Domain,Session_Renamed.UserName,Session_Renamed.Password),Server_Renamed,true);
                }

            }
            catch (UnknownHostException) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION3);
            }
            JIPolicyHandle handle = winreg.Winreg_OpenHKLM();
            JIPolicyHandle handle2 = winreg.Winreg_OpenKey(handle,"SOFTWARE\\Classes\\" + ProgId + "\\CLSID",org.jinterop.winreg.IJIWinReg_Fields.KEY_READ);
            string key = StringHelperClass.NewString(winreg.Winreg_QueryValue(handle2,255));
            winreg.Winreg_CloseKey(handle2);
            winreg.Winreg_CloseKey(handle);
            winreg.CloseConnection();
            //seperate the {}
            Clsid = JIClsid.ValueOf(StringHelperClass.SubstringSpecial(key, key.IndexOf("{", StringComparison.Ordinal) + 1,key.IndexOf("}", StringComparison.Ordinal)));
            Clsid.AutoRegistration = AutoRegister;
            JISystem.Internal_setClsidtoProgId(ProgId,Clsid.CLSID);

        }

        /// <summary>
        /// Factory method returning an instance of this class.
        /// </summary>
        /// <param name="progId"> user-friendly string representation such as "Excel.Application"
        /// @return </param>
        public static JIProgId ValueOf(string progId) {
            return new JIProgId(progId);
        }

        /// <summary>
        /// Returns the <code>CLSID</code> for this <code>ProgId</code>.
        /// 
        /// @return </summary>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIClsid getCorrespondingCLSID() throws org.jinterop.dcom.common.JIException
        public virtual JIClsid CorrespondingCLSID {
            get {
                if (Clsid == null) {
                    IdFromWinReg;
                }
                return Clsid;
            }
        }

        public virtual JISession Session {
            set {
                this.Session_Renamed = value;
            }
        }
    }

}
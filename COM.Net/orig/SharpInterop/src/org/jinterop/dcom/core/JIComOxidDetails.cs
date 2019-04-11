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

    using JISystem = org.jinterop.dcom.common.JISystem;


    /// <summary>
    ///Stores the oxid details in memory.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    internal sealed class JIComOxidDetails {

        private JILocalCoClass Referent_Renamed = null;
        private string Ipid_Renamed = null;
        private string RemUnknownIpid_Renamed = null;
        private JIOxid Oxid_Renamed = null;
        private JIObjectId Oid_Renamed = null;
        private string Iid = null;
        private JIComOxidRuntimeHelper ComRuntimeHelper = null;
        private int PortForRemUnknown_Renamed = -1;
        private int ProtectionLevel_Renamed = 2;
        private ThreadGroup RemUnknownThread = null;

        public JIComOxidDetails(JILocalCoClass javaInstance, JIOxid oxid, JIObjectId oid, string iid, string ipid, JIInterfacePointer ptr, JIComOxidRuntimeHelper helper, int protectionLevel) {
            Referent_Renamed = javaInstance;
            this.Ipid_Renamed = ipid;
            this.Oxid_Renamed = oxid;
            this.Oid_Renamed = oid;
            this.Iid = iid;
            this.ProtectionLevel_Renamed = protectionLevel;
            ComRuntimeHelper = helper;
        }

        public int PortForRemUnknown {
            set {
                PortForRemUnknown_Renamed = value;
            }
            get {
                return PortForRemUnknown_Renamed;
            }
        }


        public string IID {
            get {
                return Iid;
            }
        }

        public string Ipid {
            get {
                return Ipid_Renamed;
            }
        }

        public string RemUnknownIpid {
            get {
                return RemUnknownIpid_Renamed;
            }
            set {
                this.RemUnknownIpid_Renamed = value;
            }
        }


        public JIObjectId Oid {
            get {
                return Oid_Renamed;
            }
        }

        public JIOxid Oxid {
            get {
                return Oxid_Renamed;
            }
        }

        public JILocalCoClass Referent {
            get {
                return Referent_Renamed;
            }
        }


        public JIComOxidRuntimeHelper COMRuntimeHelper {
            get {
                return ComRuntimeHelper;
            }
        }

        public int ProtectionLevel {
            get {
                return ProtectionLevel_Renamed;
            }
        }

        public ThreadGroup RemUnknownThreadGroup {
            set {
                this.RemUnknownThread = value;
            }
        }

        public void InterruptRemUnknownThreadGroup() {
            if (RemUnknownThread != null) {
                try {
                    RemUnknownThread.interrupt();
    //                remUnknownThread.destroy();
                }
                catch (Exception e) {
                    JISystem.Logger.info("JIComOxidDetails interruptRemUnknownThreadGroup " + e.ToString());
                }
            }
        }
    }

}
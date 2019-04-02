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
    using System;


    /// <summary>
    /// Stores the oxid details in memory.
    /// </summary>
    internal sealed class JIComOxidDetails {
        private ThreadGroup remUnknownThread;

        internal JIComOxidDetails(JILocalCoClass javaInstance, JIOxid oxid, 
            JIObjectId oid, string iid, string ipid, JIInterfacePointer ptr,
            JIComOxidRuntimeHelper helper, int protectionLevel) {
            Referent = javaInstance;
            Ipid = ipid;
            Oxid = oxid;
            Oid = oid;
            IID = iid;
            ProtectionLevel = protectionLevel;
            COMRuntimeHelper = helper;
        }

        internal int PortForRemUnknown { set; get; } = -1;

        internal string IID { get; } = null;

        internal string Ipid { get; } = null;

        internal string RemUnknownIpid { get; set; } = null;

        internal JIObjectId Oid { get; } = null;

        internal JIOxid Oxid { get; } = null;

        internal JILocalCoClass Referent { get; } = null;


        internal JIComOxidRuntimeHelper COMRuntimeHelper { get; } = null;

        internal int ProtectionLevel { get; } = 2;

        internal ThreadGroup RemUnknownThreadGroup {
            set => remUnknownThread = value;
        }

        internal void interruptRemUnknownThreadGroup() {
            if (remUnknownThread != null) {
                try {
                    remUnknownThread.interrupt();
                    // remUnknownThread.destroy();
                }
                catch (Exception e) {
                    Log.Logger.Information("JIComOxidDetails interruptRemUnknownThreadGroup " + e);
                }
            }
        }
    }

}
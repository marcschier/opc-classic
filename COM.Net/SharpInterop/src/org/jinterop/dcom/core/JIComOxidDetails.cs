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
    using Serilog;
    using System;

    /// <summary>
    /// Stores the oxid details in memory.
    /// </summary>
    internal sealed class JIComOxidDetails {
        private ThreadGroup remUnknownThread;

        /// <summary>
        /// Create details
        /// </summary>
        /// <param name="javaInstance"></param>
        /// <param name="oxid"></param>
        /// <param name="oid"></param>
        /// <param name="iid"></param>
        /// <param name="ipid"></param>
        /// <param name="ptr"></param>
        /// <param name="helper"></param>
        /// <param name="protectionLevel"></param>
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

        /// <summary>
        /// Port for unknown
        /// </summary>
        internal int PortForRemUnknown { set; get; } = -1;

        /// <summary>
        /// IId
        /// </summary>
        internal string IID { get; }

        /// <summary>
        /// Ipid
        /// </summary>
        internal string Ipid { get; }

        /// <summary>
        /// Unknown ipid
        /// </summary>
        internal string RemUnknownIpid { get; set; }

        /// <summary>
        /// Oid
        /// </summary>
        internal JIObjectId Oid { get; }

        /// <summary>
        /// Oxid
        /// </summary>
        internal JIOxid Oxid { get; }

        /// <summary>
        /// Referent
        /// </summary>
        internal JILocalCoClass Referent { get; }

        /// <summary>
        /// Runtime helper
        /// </summary>
        internal JIComOxidRuntimeHelper COMRuntimeHelper { get; }

        /// <summary>
        /// Protection level
        /// </summary>
        internal int ProtectionLevel { get; } = 2;

        internal ThreadGroup RemUnknownThreadGroup {
            set => remUnknownThread = value;
        }

        /// <summary>
        /// Interrupt unknown thread group thread
        /// </summary>
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
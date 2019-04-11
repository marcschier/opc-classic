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
    using rpc.core;
    using Serilog;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    /// Representation of an active session with a COM server.
    /// All interface references being given out by
    /// the framework for a particular COM server are maintained by
    /// the session and an <code>IJIComObject</code>
    /// is associated with a single session only. Sessions are
    /// also responsible for the clean up once the system
    /// shuts down or <code>IJIComObject</code> go out of
    /// reference scope. Please make sure that you call
    /// <seealso cref="DestroySession(JISession)"/>
    /// after you are done using the session. This will ensure
    /// that any open sockets to COM server are closed.
    /// </summary>
    public sealed class JISession {
        /// <summary>
        /// Local host
        /// </summary>
        internal static byte[] LocalhostAddressAsIPbytes { get; private set; } =
            new byte[] { 127, 0, 0, 1 };

        /// <summary>
        /// Local host
        /// </summary>
        internal static string LocalhostAddressAsIPString { get; private set; } = "127.0.0.1";

        /// <summary>
        /// Localhost
        /// </summary>
        internal static string LocalhostCanonicalAddressAsString { get; private set; } = "LOCALHOST";

        /// <summary>
        /// Resolver port
        /// </summary>
        internal static int OxidResolverPort { get; private set; } = -1;




        /// <summary>
        /// Initialize static session info
        /// </summary>
        static JISession() {
            try {
                LocalhostCanonicalAddressAsString = Dns.GetHostName();
                var localhostAddr = Dns.GetHostAddresses(
                    LocalhostCanonicalAddressAsString).FirstOrDefault();
                LocalhostAddressAsIPbytes = localhostAddr.GetAddressBytes();
                LocalhostAddressAsIPString = localhostAddr.ToString();
            }
            catch (UnknownHostException) {
            }

           // System.setProperty("SharpCifs.smb.client.domain", "JIDomain"); //is being put in for completing type2 message
           //                                                                //somehow windows is not taking empty domain name.

            JIComOxidRuntime.Instance.StartResolver();
            OxidResolverPort = JIComOxidRuntime.Instance.OxidResolverPort;
            // This schedule used to be every 2 mins.
            kReleaseRefsTimer = new Timer(_ => Release_References_TimerTask(), null, 0, 2 * 60 * 1000);

            AppDomain.CurrentDomain.DomainUnload += (_, args) => {
                var i = 0;
                while (i < kListOfSessions.Count) {
                    var session = (JISession)kListOfSessions[i];
                    try {
                        DestroySession(session);
                    }
                    catch (JIException e) {
                        Log.Logger.Error(e, "JISession shutdown");
                    }
                    i++;
                }
                JISystem.Internal_writeProgIdsToFile();
                JIComOxidRuntime.Instance.StopResolver();
                kReleaseRefsTimer.Dispose();
                kMapOfSessionIdsVsSessions.Clear();
                lock (kMapOfObjects) {
                    kMapOfObjects.Clear();
                }
                kListOfSessions.Clear();
            };
        }

        /// <summary>
        /// Cancels the existing timer used to schedule collection of un-referenced COM Objects
        /// and then restarts the same with the new frequency. Default timer schedules the GC task
        /// every 2 mins.
        /// </summary>
        public static void SetReleaseRefTimerFrequency(int value) =>
            kReleaseRefsTimer.Change(0, value);

        /// <summary>
        /// Release references on timer
        /// </summary>
        private static void Release_References_TimerTask() {
            try {
                // Use a clone so we dont hold on to the mutex for longer than required.
                List<object> listOfSessionsClone = null;
                lock (kMutex) {
                    listOfSessionsClone = kListOfSessions.ToList();
                }

                var i = 0;
                while (i < listOfSessionsClone.Count) {
                    var session = (JISession)listOfSessionsClone[i];
                    Log.Logger.Information("Release_References_TimerTask:[RUN] Ipid Vs Count Map size " +
                        session._mapOfIPIDsVsRefcounts.Count +
                        " listOfDeferencedIpids size " + session._listOfDeferencedIpids.Count);
                    Log.Logger.Information("Release_References_TimerTask:[RUN] Session:  " +
                        session.SessionIdentifier +
                        ", listOfDeferencedIpids: " + session._listOfDeferencedIpids);

                    // now iterate over each sessions listOfDereferencedIpids and send a
                    // call to release for the entire lot.
                    var listToKill = new List<object>();
                    List<object> dereferencedIpids = null;

                    // Use a clone so we dont hold on to the mutex for longer than required.
                    lock (kMutex) {
                        dereferencedIpids = session._listOfDeferencedIpids.ToList();
                    }
                    for (var j = 0; j < dereferencedIpids.Count; j++) {
                        try {
                            var ipid = (string)dereferencedIpids[j];
                            listToKill.Add(session.PrepareForReleaseRef(ipid));
                        }
                        catch (JIException e) {
                            //eaten, will never get thrown from the try block.
                            Log.Logger.Information(e,
                                "Release_References_TimerTask:[RUN] Exception preparing for release ");
                        }
                    }
                    lock (kMutex) {
                        session._listOfDeferencedIpids.RemoveAll(dereferencedIpids);
                    }

                    dereferencedIpids.Clear();
                    Log.Logger.Information(
                        "Release_References_TimerTask:[RUN] Ipid Vs Count Map size after preparing release " +
                        session._mapOfIPIDsVsRefcounts.Count);

                    if (listToKill.Count > 0) {
                        var array = new JIArray(listToKill.Cast<JIStruct>().ToArray(), true);
                        try {
                            session.ReleaseRefs(array, false);
                        }
                        catch (JIException e) {
                            //This release cycle has to go on.
                            Log.Logger.Error(e,
                                "JISession Release_References_TimerTask:run() Exception in internal GC");
                        }
                    }
                    i++;
                }
            }
            catch (Exception e) {
                //This release cycle has to go on.
                Log.Logger.Error(e,
                    "JISession Release_References_TimerTask:run() Exception in internal GC");
            }
        }


        /// <summary>
        /// Creates a session with the <code>authInfo</code> of the user.
        /// This session is not yet attached to a COM server.
        /// </summary>
        /// <param name="authInfo"></param>
        /// <exception cref="ArgumentException"> if <code>authInfo</code>
        /// is <code>null</code>. </exception>
        /// <seealso cref="JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession CreateSession(IJIAuthInfo authInfo) {
            if (authInfo == null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
            }
            lock (kMutex) {
                int id;
                do {
                    id = kRandomGen.Next();
                }
                while (kMapOfSessionIdsVsSessions.ContainsKey(id));
                var session = new JISession {
                    AuthInfo = authInfo,
                    SessionIdentifier = id
                };
                kMapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                kListOfSessions.Add(session);
                Log.Logger.Information("Created Session: " + id);
                return session;
            }
        }

        /// <summary>
        /// Creates a session. This session is not yet attached to a
        /// COM server.
        /// </summary>
        /// <param name="domain"> domain of the user. </param>
        /// <param name="username"> name of the user </param>
        /// <param name="password"> password of the user.
        /// </param>
        /// <exception cref="ArgumentException"> if any parameter is
        /// <code>null</code>. </exception>
        /// <seealso cref="JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession CreateSession(string domain, string username,
            string password) {
            if (username == null || password == null || domain == null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
            }
            lock (kMutex) {
                int id;
                do {
                    id = kRandomGen.Next();
                }
                while (kMapOfSessionIdsVsSessions.ContainsKey(id));
                var session = new JISession {
                    _username = username,
                    _password = password,
                    _domain = domain,
                    SessionIdentifier = id
                };
                kMapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                kListOfSessions.Add(session);
                Log.Logger.Information("Created Session: " + id);
                return session;
            }
        }


        /// <summary>
        /// Creates a new session using credentials of the <code>session</code>parameter.
        /// The new session is not yet attached to a COM server.
        /// </summary>
        /// <param name="session">
        /// </param>
        /// <seealso cref="JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession CreateSession(JISession session) {
            var newSession = CreateSession(session.Domain, session.UserName,
                session.Password);
            newSession.AuthInfo = session.AuthInfo;
            return newSession;
        }

        /// <summary>
        /// <b>Native</b> Single Sign On capable session.
        /// <b>Warning:</b> <ul><li>This method works <b>only</b>
        /// on Microsoft Windows Platform.</li>
        /// <li>It does <b>not</b> support NTLMv2 or NTLM1 Session
        /// Security.</li>
        /// <li>It supports only NTLM1 Authentication.</li>
        /// <li>This session <b>cannot</b> be used with
        /// <code>JIComServer(ProgId,...)</code> ctors. JCIFS will
        /// fail to setup a connection with Windows Registry
        /// if GUEST account is disabled.</li></ul>
        /// </summary>
        /// <seealso cref="JIComServer(JIClsid, JISession)"></seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"></seealso>
        public static JISession CreateSession() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_WIN_ONLY));
            }
            lock (kMutex) {
                int id;
                do {
                    id = kRandomGen.Next();
                }
                while (kMapOfSessionIdsVsSessions.ContainsKey(id));

                var session = new JISession {
                    SessionIdentifier = id,
                    SSOEnabled = true
                };
                kMapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                kListOfSessions.Add(session);
                Log.Logger.Information("Created Session for SSO: " + id);
                return session;
            }
        }

        /// <summary>
        /// Used to destroy the <code>session</code>, this release all references
        /// of the COM server and it's interfaces.
        /// It should be called in the end after the developer is done with the
        /// COM server.
        /// Note that all interface references belonging to sessions linked to this
        /// session will also be destroyed.
        /// </summary>
        /// <param name="session"> </param>
        /// <exception cref="JIException"> </exception>
        public static void DestroySession(JISession session) {
            //null session
            if (session == null) {
                return;
            }

            //if stub is null then cleanup datastructures holding the session object only
            if (session._stub == null) {
                lock (kMutex) {
                    kMapOfSessionIdsVsSessions.Remove(session.SessionIdentifier);
                    kListOfSessions.Remove(session);
                }

                //now remove the links and the OIDs
                PostDestroy(session);
                return;
            }

            try {
                //session may have been destroyed and this call is from finalize.
                var list = new List<object>();
                var listOfFreeIPIDs = new List<object>();
                lock (kMutex) {
                    if (session.SessionInDestroy) {
                        return;
                    }
                    session.SessionInDestroy = true;
                    //list of dereferenced IPIDs
                    for (var j = 0; j < session._listOfDeferencedIpids.Count; j++) {
                        list.Add(session.PrepareForReleaseRef((string)session._listOfDeferencedIpids[j]));
                    }
                    listOfFreeIPIDs.AddRange(session._listOfDeferencedIpids);
                    session._listOfDeferencedIpids.Clear();
                }

                lock (kMapOfObjects) {
                    var iterator = kMapOfObjects.Iterator();
                    while (iterator.HasNext()) {
                        var entry = iterator.Next();
                        var holder = (IPID_SessionID_Holder)entry.Value;
                        if (session.SessionIdentifier != holder.SessionID) {
                            continue;
                        }
                        var ipid = holder.IPID;
                        if (ipid == null) {
                            continue;
                        }

                        list.Add(session.PrepareForReleaseRef(ipid));
                        listOfFreeIPIDs.Add(ipid);
                        iterator.Remove();
                    }
                }

                //now to kill the stub itself
                if (session._stub.ServerInterfacePointer != null) {
                    if (!listOfFreeIPIDs.Contains(session._stub.ServerInterfacePointer.IPID)) {
                        list.Add(session.PrepareForReleaseRef(session._stub.ServerInterfacePointer.IPID));
                        listOfFreeIPIDs.Add(session._stub.ServerInterfacePointer.IPID);
                    }
                }

                listOfFreeIPIDs.Clear();
                //release is performed if only something is in the session.
                if (list.Count > 0) {
                    var array = new JIArray(list.Cast<JIStruct>().ToArray(), true);
                    try {
                        session._stub.CloseStub(); //close the existing connection
                        session.ReleaseRefs(array, true);
                    }
                    catch (JIException e) {
                        //This release cycle has to go on.
                        Log.Logger.Error(e, "JISession", "destroySession", e);
                    }
                }

                JIComOxidRuntime.Instance.ClearIPIDsforSession(session);
                Log.Logger.Information("Destroyed Session: " + session.SessionIdentifier);
            }
            finally {
                lock (kMutex) {
                    kMapOfSessionIdsVsSessions.Remove(session.SessionIdentifier);
                    kListOfSessions.Remove(session);
                    // and remove its entry from the map
                    if (session._stub.ServerInterfacePointer != null) {
                        kMapOfOxidsVsJISessions.Remove(new JIOxid(session._stub.ServerInterfacePointer.OXID));
                    }
                }
                session._stub.CloseStub();
                session.Stub2.CloseStub();
            }

            PostDestroy(session);
            session._stub = null; //setting it null in the end.
            session.Stub2 = null;
        }

        /// <summary>
        /// Post destroy
        /// </summary>
        /// <param name="session"></param>
        /// <exception cref="JIException"></exception>
        private static void PostDestroy(JISession session) {
            //now destroy all linked sessions
            Log.Logger.Information("About to destroy links for Session: " +
                session.SessionIdentifier + ", size of which is " + session._links.Count);

            for (var i = 0; i < session._links.Count; i++) {
                DestroySession((JISession)session._links[i]);
            }

            session._links.Clear();
            //finally any oids exported by this session.
            JIComOxidRuntime.Instance.DestroySessionOIDs(session.SessionIdentifier);
        }

        /// <summary>
        /// Session tracking reference
        /// </summary>
        internal class IPID_SessionID_Holder {

            /// <summary>
            /// Ipid
            /// </summary>
            public string IPID { get; }

            /// <summary>
            /// Sessionid
            /// </summary>
            public int SessionID { get; }

            /// <summary>
            /// Only session id
            /// </summary>
            public bool IsOnlySessionIDPresent { get; }

            /// <summary>
            /// oid
            /// </summary>
            public byte[] Oid { get; }

            /// <summary>
            /// Create tracking reference
            /// </summary>
            /// <param name="ipid"></param>
            /// <param name="sessionID"></param>
            /// <param name="isOnlySessionId"></param>
            /// <param name="oid"></param>
            internal IPID_SessionID_Holder(string ipid, int sessionID,
                bool isOnlySessionId, byte[] oid) {
                IPID = ipid;
                IsOnlySessionIDPresent = isOnlySessionId;
                SessionID = sessionID;
                Oid = oid;
            }

            /// <summary>
            /// Finalize
            /// </summary>
            ~IPID_SessionID_Holder() {
                GcCollectSession(this);
            }
        }

        /// <summary>
        /// Called when the session id holder is garbage collected because
        /// the com object was garbage collected
        /// </summary>
        /// <param name="holder"></param>
        private static void GcCollectSession(IPID_SessionID_Holder holder) {
            try {
                if (holder == null) {
                    return;
                }

                JISession session = null;
                lock (kMutex) {
                    session = (JISession)kMapOfSessionIdsVsSessions[holder.SessionID];
                }

                // this means that the session got lost...but this logic
                // does not work, since session is strongly referenced from
                // multiple places
                if (holder.IsOnlySessionIDPresent) {
                    try {
                        DestroySession(session);
                    }
                    catch (Exception e) {
                        Log.Logger.Verbose("exception from destroy session in clean up thread: " + e.Message);
                    }
                }
                else {
                    //session may have been "destroySession"...
                    if (session == null) {
                        return;
                    }
                    try {
                        var IPID = holder.IPID;

                        // Since we are freeing up all references for the given IPID together, ensure
                        // that all weak-references for this IPID have been dereferenced before it to
                        // the list of Dereferenced IPIDs. The Reference Queue mechanism ensures that
                        // any reference only comes here once.

                        var weakRefsRemaining = session.RemoveWeakReference(IPID);

                        // Decrement the ref-count for the oid too.
                        // Will call the JIComOxidRuntime, and that is synched, but that will not
                        // cause a deadlock, since it or rather any method of JIComOxidRuntime does
                        // not call back into JISession.
                        JIComOxidRuntime.Instance.DelIPIDReference(IPID,
                            new JIObjectId(holder.Oid, false), session);

                        // Only proceed to de-list this IPID for clearance if all weak-references were
                        // released.
                        if (weakRefsRemaining > 0) {
                            return;
                        }

                        // session.releaseRef(IPID); Not doing release anymore, this causes a lot of calls to
                        // go across, so will save these in this list and then the cleanup thread will deal with
                        // this every 3 minutes.
                        Log.Logger.Verbose("Adding Dereferenced IPID " + IPID +
                            " session " + session.SessionIdentifier);

                        session.AddDereferencedIpids(IPID);
                        holder = null;
                        var unreferenced = session.GetUnreferencedHandler(IPID);
                        if (unreferenced != null) {
                            unreferenced.UnReferenced();
                        }
                        session.UnregisterUnreferencedHandler(IPID);
                    }
                    catch (Exception e) {
                        Log.Logger.Information(
                            "exception from removing a IPID from session in clean up thread: " + e.Message);
                    }
                }
            }
            catch (Exception e) {
                Log.Logger.Error(e, "JISession", "CleanupThread:run()", e);
            }
        }

        private static readonly Timer kReleaseRefsTimer;
        private static readonly Random kRandomGen = new Random();
        private static readonly Hashtable kMapOfObjects = new Hashtable();
        private static readonly object kMutex = new object();
        private static readonly Hashtable kMapOfOxidsVsJISessions = new Hashtable();
        private static readonly IDictionary<string, JIComCustomMarshallerUnMarshaller> kMapOfCustomCLSIDs =
            new Dictionary<string, JIComCustomMarshallerUnMarshaller>();
        private static readonly Hashtable kMapOfSessionIdsVsSessions = new Hashtable();
        private static readonly List<object> kListOfSessions = new List<object>();
        private static readonly ConditionalWeakTable<object, IPID_SessionID_Holder> kWeakTable =
            new ConditionalWeakTable<object, IPID_SessionID_Holder>();














        /// <summary>
        /// Returns the <code>IJIAuthInfo</code> (if any) associated with this session.
        /// </summary>
        public IJIAuthInfo AuthInfo { get; private set; } = null;

        /// <summary>
        /// Returns whether this session is SSO or not.
        /// </summary>
        public bool SSOEnabled { get; private set; }

        /// <summary>
        /// Gets the user name associated with this session.
        /// </summary>
        public string UserName => AuthInfo == null ? _username : AuthInfo.UserName;

        /// <summary>
        /// Password
        /// </summary>
        internal string Password => AuthInfo == null ? _password : AuthInfo.Password;

        /// <summary>
        /// Gets the domain of the user associated with this session.
        /// </summary>
        public string Domain => AuthInfo == null ? _domain : AuthInfo.Domain;

        /// <summary>
        /// Returns a unique identifier for this session.
        /// </summary>
        public int SessionIdentifier { get; private set; } = -1;

        /// <summary>
        ///<para> Sets the timeout for all sockets opened to (not fro) the
        ///COM server for this session. Default value is 0 (no timeout).
        /// The class level and the method level settings in case of
        /// <code>IJIComObject</code> override this timeout. </para>
        /// </summary>
        /// <seealso cref="IComObject.InstanceLevelSocketTimeout"> </seealso>
        /// <seealso cref="IComObject.Call(JICallBuilder, int)"> </seealso>
        public int GlobalSocketTimeout { set; get; }

        /// <summary>
        /// Sets the use of NTLM2 Session Security. Framework will use
        /// NTLM Packet Level Privacy and Sign\Seal all packets.
        /// Once the <code>JIComServer</code> is bound to this session
        /// (using any of the <code>JIComServer</code> constructors)
        /// the use of session security <b>cannot</b> be enabled or disabled.
        /// Please note that session security can come at any available
        /// level of authentication (LM\NTLM\LMv2\NTLMv2). The framework
        /// currently only supports sign and seal at NTLMv1 level.
        /// Whether to use NTLM1 or not is dictated by this field in the
        /// Windows Registry.
        /// <code>
        /// HKLM\System\CurrentControlSet\Control\Lsa\LmCompatibilityLevel
        /// </code>
        /// This article on MSDN talks more about it
        /// http://support.microsoft.com/default.aspx?scid=KB;en-us;239869
        /// </summary>
        /// <param name="enable"> <code>true</code> to enable,
        /// <code>false</code> to disable. </param>
        public void UseSessionSecurity(bool enable) => _useSessionSecurity = enable;

        /// <summary>
        /// <para> Sets the use of NTLMv2 Security (default is NTLM1). This
        /// can be used in combination with <code>useSessionSecurity</code> method.
        /// Once the <code>JIComServer</code> is bound to this session
        /// (using any of the <code>JIComServer</code> constructors)
        /// the use of NTLMv2 security <b>cannot</b> be enabled or disabled.
        /// </para>
        /// <para>
        ///
        /// </para>
        /// </summary>
        /// <param name="enable"> <code>true</code> to enable. </param>
        public void UseNTLMv2(bool enable) => _useNTLMv2 = enable;

        /// <summary>
        /// Flag indicating whether session security is enabled.
        /// </summary>
        /// <returns> <code>true</code> for enabled. </returns>
        public bool SessionSecurityEnabled => !SSOEnabled & _useSessionSecurity;

        /// <summary>
        /// Flag indicating whether NTLMv2 security is enabled.
        /// </summary>
        /// <returns> <code>true</code> for enabled. </returns>
        public bool NTLMv2Enabled => !SSOEnabled & _useNTLMv2;

        /// <summary>
        /// Destroying
        /// </summary>
        internal bool SessionInDestroy { get; private set; } = false;

        /// <summary>
        /// Target server
        /// </summary>
        internal string TargetServer {
            set {
                if (value.Equals("127.0.0.1", StringComparison.CurrentCultureIgnoreCase) ||
                    value.Equals("localhost", StringComparison.CurrentCultureIgnoreCase)) {
                    //Replace with it's actual bindings, otherwise does not work for authentication
                    _targetServer = LocalhostAddressAsIPString;
                }
                else {
                    _targetServer = value;
                }
            }
            get => _targetServer;
        }

        /// <summary>
        /// each session is associated with 1 and only 1 stub.
        /// adding something new now another stub for IRemUnknown operations
        /// </summary>
        internal JIComServer Stub {
            set {
                _stub = value;
                lock (kMutex) {
                    kMapOfOxidsVsJISessions[new JIOxid(value.ServerInterfacePointer.OXID)] = this;
                }
            }
            get => _stub;
        }

        /// <summary>
        /// Unknown stub
        /// </summary>
        internal JIRemUnknownServer Stub2 { set; get; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private JISession() {
        }

        /// <summary>
        /// Add to session
        /// </summary>
        /// <param name="comObject"></param>
        /// <param name="oid"></param>
        internal void AddToSession(IComObject comObject, byte[] oid) {
            //nothing will be done if the session is being destroyed.
            if (SessionInDestroy) {
                return;
            }
            AddWeakReference(comObject, oid);

            // setting if NO PING flag has been set to true.
            AddToSession(comObject.Ipid, oid, ((JIStdObjRef)comObject.Internal_getInterfacePointer()
                .GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Flags == 0x00001000);
            Log.Logger.Information(" for IID: " + comObject.InterfaceIdentifier + " session: " + SessionIdentifier);

            var refcount = ((JIStdObjRef)comObject.Internal_getInterfacePointer()
                .GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
            UpdateReferenceForIPID(comObject.Ipid, refcount);
        }

        /// <summary>
        /// Addref release
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="obj"></param>
        /// <param name="refcount"></param>
        /// <exception cref="JIException"></exception>
        internal void AddRef_ReleaseRef(string IPID, JICallBuilder obj, int refcount) {
            UpdateReferenceForIPID(IPID, refcount);
            Stub2.AddRef_ReleaseRef(obj);
        }

        /// <summary>
        /// Update reference
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="refcount"></param>
        private void UpdateReferenceForIPID(string ipid, int refcount) {
            if (!_mapOfIPIDsVsRefcounts.TryGetValue(ipid, out var value)) {
                // Were we asked to release a ref that wasnt in our map?
                if (refcount < 0) {
                    Log.Logger.Information("[updateReferenceForIPID] Released IPID not found: " + ipid);
                    return;
                }
                value = 0;
            }
            var newCount = (int)value + refcount;
            if (newCount > 0) {
                _mapOfIPIDsVsRefcounts[ipid] = newCount; // TODO AddOrUpdate
            }
            else {
                _mapOfIPIDsVsRefcounts.Remove(ipid);
            }
        }

        /// <summary>
        /// Add weak reference
        /// </summary>
        /// <param name="comObject"></param>
        /// <param name="oid"></param>
        internal void AddWeakReference(IComObject comObject, byte[] oid) {
            var holder = new IPID_SessionID_Holder(comObject.Ipid, SessionIdentifier, false, oid);
            lock (kMapOfObjects) {
                // Add to finalizer table - it will finalize the holder
                kWeakTable.Add(comObject, holder);
                // Add to weak object map - gives us a view of all objects around
                kMapOfObjects[new WeakReference(comObject)] = Tuple.Create(comObject.Ipid, SessionIdentifier);
            }
            // Increment the count for the number of weak-references for this IPID
            lock (_mapOfIPIDsVsWeakReferences) {
                // Count all weak-references for a given IPID.
                if (!_mapOfIPIDsVsWeakReferences.TryGetValue(comObject.Ipid, out var count)) {
                    count = 0;
                }
                _mapOfIPIDsVsWeakReferences[comObject.Ipid] = (int)count + 1;
            }
        }

        /// <summary>
        /// Reduce the count of weak-references stored in mapOfIPIDsVsWeakReferences and return the same.
        /// </summary>
        /// <param name="ipid"></param>
        /// <returns></returns>
        internal int RemoveWeakReference(string ipid) {
            Log.Logger.Verbose("Dumping mapOfIPIDsVsWeakReferences " +
                _mapOfIPIDsVsWeakReferences.ToString());
            var weakRefsRemaining = 0;
            lock (_mapOfIPIDsVsWeakReferences) {
                if (!_mapOfIPIDsVsWeakReferences.TryGetValue(ipid, out var count)) {
                    weakRefsRemaining = 0;
                }
                else {
                    weakRefsRemaining = (int)count - 1;
                    if (weakRefsRemaining > 0) {
                        _mapOfIPIDsVsWeakReferences[ipid] = weakRefsRemaining;
                    }
                    else {
                        _mapOfIPIDsVsWeakReferences.Remove(ipid);
                    }
                }
            }
            return weakRefsRemaining;
        }

        /// <summary>
        /// Add to session
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="oid"></param>
        /// <param name="dontping"></param>
        private void AddToSession(string IPID, byte[] oid, bool dontping) {
            // Weak reference of the object
            // mapOfObjects.put(new WeakReference(IPID,referenceQueueOfCOMObjects),IPID);
            // it does not matter if we create a new OID here, the OxidCOMRunttime
            // API uses the OID in the MAP, and not this one.
            var joid = new JIObjectId(oid, dontping);
            JIComOxidRuntime.Instance.AddUpdateOXIDs(this, IPID, joid);
            Log.Logger.Information("[addToSession] Adding IPID: " + IPID +
                " to session: " + SessionIdentifier);
        }

        /// <summary>
        /// this gets called from the cleanupthread and no place else
        /// and it calls the releaseRef of session which internally calls
        /// the <see cref="JIComServer.AddRef_ReleaseRef(JICallBuilder)"/>
        /// of the JIComServer, that method is synched at the instance level.
        /// I was worried about a deadlock with destroySession, since
        /// that also ultimately calls the add_releaseRef, but
        /// this will not happen since under a simultaneous destroy
        /// and removefromsession call, the "mutex" object will get synch.
        /// If suppose a comServer.getInterface(...) is being done
        /// (which also calls releaseRef), then that is synched at instance level
        /// and so is add_releaseRef (on the same instance), so deadlock
        /// won't happen there. If a simulataneous remove and getInterface call comes
        /// then getInterface(which internally calls releaseRef) will go through,
        /// since releaseRef is not synched but the api it calls i.e. add_releaseRef
        /// is synched with the same lock as getInterface. The remove will
        /// have to wait till that call gets over.
        /// </summary>
        /// <param name="IPID"></param>
        /// <exception cref="JIException"></exception>
        internal void ReleaseRef(string IPID) => ReleaseRef(IPID, 5);

        /// <summary>
        /// Release reference
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="numinstances"></param>
        /// <exception cref="JIException"></exception>
        internal void ReleaseRef(string IPID, int numinstances) {
            Log.Logger.Information("releaseRef:Reclaiming from Session: " +
                SessionIdentifier + ", the IPID: " + IPID + ", numinstances is " + numinstances);
            var obj = new JICallBuilder(true) {
                ParentIpid = IPID,
                Opnum = 2 //release
            };
            //length
            obj.AddInParamAsShort(1, JIFlags.FLAG_NULL);
            //ipid to addfref on
            var array = new JIArray(new UUID[] { new UUID(IPID) }, true);
            obj.AddInParamAsArray(array, JIFlags.FLAG_NULL);
            // TODO requesting 5 for now, will later build caching mechnaism to
            // exhaust 5 refs first before asking for more
            // same with release.
            obj.AddInParamAsInt(numinstances, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0, JIFlags.FLAG_NULL); //private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("releaseRef: Releasing numinstances " + numinstances +
                    " references of IPID: " + IPID + " session: " + SessionIdentifier);
                // debug_delIpids(IPID, numinstances);
            }
            AddRef_ReleaseRef(IPID, obj, -5);
        }

        /// <summary>
        /// Dreference
        /// </summary>
        /// <param name="IPID"></param>
        private void AddDereferencedIpids(string IPID) {
            Log.Logger.Information("addDereferencedIpids for session : " +
                SessionIdentifier + ", IPID is: " + IPID);
            lock (kMutex) {
                if (!_listOfDeferencedIpids.Contains(IPID)) {
                    _listOfDeferencedIpids.Add(IPID);
                }
            }
        }

        /// <summary>
        /// Release
        /// </summary>
        /// <param name="arrayOfStructs"></param>
        /// <param name="fromDestroy"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private void ReleaseRefs(JIArray arrayOfStructs, bool fromDestroy) {
            Log.Logger.Information("In releaseRefs for session : " + SessionIdentifier +
                ", array length is: " + (short)((object[])arrayOfStructs.ArrayInstance).Length);
            var obj = new JICallBuilder(true) {
                Opnum = 2 //release
            };
            //length
            obj.AddInParamAsShort((short)((object[])arrayOfStructs.ArrayInstance).Length, JIFlags.FLAG_NULL);
            obj.AddInParamAsArray(arrayOfStructs, JIFlags.FLAG_NULL);
            obj.FromDestroySession = fromDestroy;
            _stub.AddRef_ReleaseRef(obj);
            //ignore the results
        }

        /// <summary>
        /// Prepare for release
        /// </summary>
        /// <param name="IPID"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private JIStruct PrepareForReleaseRef(string IPID) {
            var releaseCount = 5 + 5; // 5 of the original and 5 for the addRef done later on.
            if (_mapOfIPIDsVsRefcounts.TryGetValue(IPID, out var refcount)) {
                releaseCount = (int)refcount;
            }
            return PrepareForReleaseRef(IPID, releaseCount);
        }

        /// <summary>
        /// Prepare for release
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="refcount"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private JIStruct PrepareForReleaseRef(string IPID, int refcount) {
            var remInterface = new JIStruct();
            remInterface.AddMember(new UUID(IPID));
            remInterface.AddMember(refcount);
            remInterface.AddMember(0); //private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("prepareForReleaseRef: Releasing " + refcount +
                    "references of IPID: " + IPID + " session: " + SessionIdentifier);
                // debug_delIpids(IPID, refcount);
            }
            UpdateReferenceForIPID(IPID, -1 * refcount);

            return remInterface;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JISession other)) {
                return false;
            }
            return other.SessionIdentifier == SessionIdentifier;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => SessionIdentifier;

        /// <summary>
        /// TODO: IDisposable
        /// </summary>
        ~JISession() {
            try {
                DestroySession(this);
            }
            catch (JIException e) {
                Log.Logger.Verbose("Exception in finalize when destroying session " + e.Message);
            }
        }

        /// <summary>
        /// Get unreferenced handler
        /// </summary>
        /// <param name="ipid"></param>
        /// <returns></returns>
        internal IJIUnreferenced GetUnreferencedHandler(string ipid) {
            lock (this) {
                return (IJIUnreferenced)_mapOfUnreferencedHandlers[ipid];
            }
        }

        /// <summary>
        /// Register unreferenced handler
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="unreferenced"></param>
        internal void RegisterUnreferencedHandler(string ipid, IJIUnreferenced unreferenced) {
            lock (this) {
                _mapOfUnreferencedHandlers[ipid] = unreferenced;
            }
        }

        /// <summary>
        /// Unregister unreferenced handler
        /// </summary>
        /// <param name="ipid"></param>
        internal void UnregisterUnreferencedHandler(string ipid) {
            lock (this) {
                _mapOfUnreferencedHandlers.Remove(ipid);
            }
        }

        /// <summary>
        /// Links the src with target. These two sessions can now be destroyed in a cascade effect.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        internal static void LinkTwoSessions(JISession src, JISession target) {
            if (src.SessionInDestroy || target.SessionInDestroy) {
                return;
            }
            if (src.Equals(target)) {
                return;
            }
            lock (kMutex) {
                if (!src._links.Contains(target)) {
                    src._links.Add(target);
                }
            }
        }

        /// <summary>
        /// Removes session from src sessions list.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="tobeunlinked"></param>
        internal static void UnLinkSession(JISession src, JISession tobeunlinked) {
            if (src.SessionInDestroy) {
                return;
            }
            if (src.Equals(tobeunlinked)) {
                return;
            }
            lock (kMutex) {
                src._links.Remove(tobeunlinked);
            }
        }

        /// <summary>
        /// Based on the oxid returns the JISession (and thus the COM Server)
        /// associated with it. This is required, since there are
        /// cases where a different JISession may be passed in JIObjectFactory
        /// for an JIInterfacePointer which does not belong to this JISession.
        /// Under those scenarios, the COM factory will create a new instance
        /// of a JISession and associate that Interface pointer with the session.
        /// But that is not the right approach as a COM Server for that interface
        /// and thus a session might already exist and these have to be tied
        /// together.
        /// </summary>
        internal static JISession ResolveSessionForOxid(JIOxid oxid) {
            lock (kMutex) {
                return (JISession)kMapOfOxidsVsJISessions[oxid];
            }
        }

        /// <summary>
        /// Register handlers for OBJREF_CUSTOM. customClass only serves as
        /// a Template and is of no real consequence.
        /// A new copy is returned from customClass.decode(...) and that
        /// is used by framework internally.
        /// </summary>
        /// <param name="CLSID"> </param>
        /// <param name="customClass"> </param>
        public void RegisterCustomMarshallerUnMarshallerTemplate(string CLSID,
            JIComCustomMarshallerUnMarshaller customClass) =>
            kMapOfCustomCLSIDs[CLSID.ToUpper()] = customClass;

        /// <summary>
        /// Get template
        /// </summary>
        /// <param name="CLSID"></param>
        /// <returns></returns>
        internal JIComCustomMarshallerUnMarshaller GetCustomMarshallerUnMarshallerTemplate(
            string CLSID) => kMapOfCustomCLSIDs[CLSID.ToUpper()];

        private string _username;
        private string _password;
        private string _domain;
        private string _targetServer;
        private JIComServer _stub;
        private bool _useSessionSecurity;
        private bool _useNTLMv2;
        private readonly List<object> _listOfDeferencedIpids = new List<object>();
        private readonly List<object> _links = new List<object>();
        private readonly Hashtable _mapOfUnreferencedHandlers = new Hashtable();
        private readonly Hashtable _mapOfIPIDsVsRefcounts = new Hashtable();
        private readonly Hashtable _mapOfIPIDsVsWeakReferences = new Hashtable();
    }
}
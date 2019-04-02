// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using ndr;
    using rpc.core;
    using org.jinterop.dcom.common;
    using org.jinterop.winreg;
    using Serilog;
    using System;
    using System.Collections;
    using System.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// Representation of an active session with a COM server. All interface references being given out by
    /// the framework for a particular COM server are maintained by the session and an <code>IJIComObject</code>
    /// is associated with a single session only. Sessions are also responsible for the clean up once the system
    /// shuts down or <code>IJIComObject</code> go out of reference scope.
    /// Please make sure that you call <seealso cref="destroySession(JISession)"/> after you are done using the session.
    /// This will ensure that any open sockets to COM server are closed.
    /// </summary>
    public sealed class JISession {

        private class RunnableAnonymousInnerClassHelper : Runnable {
            public RunnableAnonymousInnerClassHelper() {
            }

            public virtual void run() {
                try {
                    while (true) {
                        Reference r = _referenceQueueOfCOMObjects.remove();
                        if (r != null) {
                            // Object is no longer referenced.
                            //get from hash map and call release ref on that object
                            IPID_SessionID_Holder holder = null;
                            lock (_mapOfObjects) {
                                holder = (IPID_SessionID_Holder)_mapOfObjects.Remove(r);
                                if (holder == null) {
                                    continue;
                                }
                            }

                            JISession session = null;
                            lock (_mutex) {
                                session = (JISession)_mapOfSessionIdsVsSessions[holder.sessionID];
                            }
                            //this means that the session got lost...but this logic does not work, since
                            //session is strongly referenced from mapOfSessionIdsVsSessions and listOfSessions and even putting
                            //WeakReference for JISession when adding it to the mapOfSessionIdsVsSessions/listOfSessions does not
                            //make a difference as we always loose the session to GC before it come here.
                            if (holder.isOnlySessionIDPresent) {
                                try {
                                    destroySession(session);
                                }
                                catch (Exception e) {
                                    Log.Logger.Verbose("exception from destroy session in clean up thread: " + e.Message);
                                }
                            }
                            else {
                                //session may have been "destroySession"...
                                if (session == null) {
                                    continue;
                                }

                                try {
                                    var IPID = holder.IPID;

                                    // Since we are freeing up all references for the given IPID together, ensure
                                    // that all weak-references for this IPID have been dereferenced before it to
                                    // the list of Dereferenced IPIDs. The Reference Queue mechanism ensures that
                                    // any reference only comes here once.

                                    var weakRefsRemaining = session.removeWeakReference(IPID);

                                    // Decrement the ref-count for the oid too.
                                    //Will call the JIComOxidRuntime, and that is synched on mutex3, but that will not cause a deadlock, since
                                    //it or rather any method of JIComOxidRuntime does not call back into JISession.
                                    JIComOxidRuntime.delIPIDReference(IPID, new JIObjectId(holder.oid, false), session);

                                    // Only proceed to de-list this IPID for clearance if all weak-references were
                                    // released.
                                    if (weakRefsRemaining > 0) {
                                        continue;
                                    }

                                    //JIComOxidRuntime.delIPIDReference(IPID);
                                    //session.releaseRef(IPID); Not doing release anymore, this causes a lot of calls to
                                    //go across, so will save these in this list and then the cleanup thread will deal with
                                    //this every 3 minutes.
                                    Log.Logger.Verbose("Adding Dereferenced IPID " + IPID + " session " + session.SessionIdentifier);

                                    session.addDereferencedIpids(IPID);
                                    holder = null;
                                    var unreferenced = (IJIUnreferenced)session.getUnreferencedHandler(IPID);
                                    if (unreferenced != null) {
                                        unreferenced.unReferenced();
                                    }
                                    session.unregisterUnreferencedHandler(IPID);
                                }
                                catch (Exception e) {
                                    Log.Logger.Information("exception from removing a IPID from session in clean up thread: " + e.Message);
                                }
                            }
                        }


                    }
                }
                catch (Exception e) {
                    Log.Logger.Error(e, "JISession", "CleanupThread:run()", e);
                }
            }
        }

        //from JDK bug http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4665037
        private static string getLocalHost(string destination) {
            DatagramSocket sock;
            InetAddress intendedDestination;
            try {
                sock = new DatagramSocket();
                intendedDestination = InetAddress.getByName(destination);
            }
            catch (Exception) {
                return "127.0.0.1";
            }
            sock.connect(intendedDestination, sock.LocalPort);
            return sock.LocalAddress.HostAddress;
        }

        /// <summary>
        /// Initialize static session info
        /// </summary>
        static JISession() {
            JISystem.internal_initLogger();
            try {
                InetAddress localhostAddr = InetAddress.LocalHost;
                LocalhostAddressAsIPbytes = localhostAddr.Address;
                LocalhostAddressAsIPString = localhostAddr.HostAddress;
                LocalhostCanonicalAddressAsString = localhostAddr.CanonicalHostName;
            }
            catch (UnknownHostException) {
            }

            System.setProperty("jcifs.smb.client.domain", "JIDomain"); //is being put in for completing type2 message
                                                                       //somehow windows is not taking empty domain name.

            //start the cleanup thread.
            // and create a shutdown hook also.
            _cleanUpThread.Daemon = true;
            //cleanUpThread.setPriority(Thread.MIN_PRIORITY);
            _cleanUpThread.start();

            JIComOxidRuntime.startResolver();
            JIComOxidRuntime.startResolverTimer();
            OxidResolverPort = JIComOxidRuntime.OxidResolverPort;
            // This schedule used to be every 2 mins. 
            _releaseRefsTimer.scheduleAtFixedRate(new Release_References_TimerTask(), 0, 2 * 60 * 1000);

            Runtime.Runtime.addShutdownHook(new Thread(new RunnableAnonymousInnerClassHelper2(), "jI_ShutdownHook"));
        }

        private class RunnableAnonymousInnerClassHelper2 : Runnable {
            public RunnableAnonymousInnerClassHelper2() {
            }

            public virtual void run() {
                var i = 0;
                while (i < _listOfSessions.Count) {
                    var session = (JISession)_listOfSessions[i];
                    try {
                        destroySession(session);
                    }
                    catch (JIException e) {
                        Log.Logger.Error(e, "JISession", "addShutDownHook Thread:run()", e);
                    }
                    i++;
                }
                JISystem.internal_writeProgIdsToFile();
                JIComOxidRuntime.stopResolver();
                _releaseRefsTimer.cancel();
                _mapOfSessionIdsVsSessions.Clear();
                _mapOfObjects.Clear();
                _listOfSessions.Clear();
            }
        }

        /// <summary>
        /// Cancels the existing timer used to schedule collection of un-referenced COM Objects 
        /// and then restarts the same with the new frequency. Default timer schedules the GC task 
        /// every 2 mins.  
        /// </summary>
        public static int ReleaseRefTimerFrequency {
            set {
                _releaseRefsTimer.cancel();
                _releaseRefsTimer = new Timer(true);
                _releaseRefsTimer.scheduleAtFixedRate(new Release_References_TimerTask(), 0, value);
            }
        }

        private class Release_References_TimerTask : TimerTask {
            public virtual void run() {
                try {
                    // Use a clone so we dont hold on to the mutex for longer than required.
                    IList listOfSessionsClone = null;
                    lock (_mutex) {
                        listOfSessionsClone = (IList)_listOfSessions.clone();
                    }

                    var i = 0;

                    while (i < listOfSessionsClone.Count) {
                        var session = (JISession)listOfSessionsClone[i];
                        Log.Logger.Information("Release_References_TimerTask:[RUN] Ipid Vs Count Map size " +
                            session._mapOfIPIDsVsRefcounts.Count + " listOfDeferencedIpids size " + session._listOfDeferencedIpids.Count);
                        Log.Logger.Information("Release_References_TimerTask:[RUN] Session:  " + 
                            session.SessionIdentifier + " , listOfDeferencedIpids: " + session._listOfDeferencedIpids);

                        //now iterate over each sessions listOfDereferencedIpids and send a call to release for the entire lot.
                        var listToKill = new ArrayList();
                        IList dereferencedIpids = null;

                        // Use a clone so we dont hold on to the mutex for longer than required.
                        lock (_mutex) {
                            dereferencedIpids = (IList)((ArrayList)session._listOfDeferencedIpids).clone();
                        }

                        for (var j = 0; j < dereferencedIpids.Count; j++) {
                            try {
                                var ipid = (string)dereferencedIpids[j];
                                listToKill.Add(session.prepareForReleaseRef(ipid));
                            }
                            catch (JIException e) {
                                //eaten, will never get thrown from the try block.
                                Log.Logger.Information("Release_References_TimerTask:[RUN] Exception preparing for release " + e);
                            }
                        }
                        lock (_mutex) {
                            //JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
                            session._listOfDeferencedIpids.removeAll(dereferencedIpids);
                        }

                        dereferencedIpids.Clear();

                        Log.Logger.Information("Release_References_TimerTask:[RUN] Ipid Vs Count Map size after preparing release " + session._mapOfIPIDsVsRefcounts.Count);

                        if (listToKill.Count > 0) {
                            var array = new JIArray(listToKill.ToArray(typeof(JIStruct)), true);
                            try {
                                session.releaseRefs(array, false);
                            }
                            catch (JIException e) {
                                //This release cycle has to go on.
                                Log.Logger.Error(e, "JISession Release_References_TimerTask:run() Exception in internal GC");
                            }
                        }

                        i++;
                    }
                }
                catch (Exception e) {
                    //This release cycle has to go on.
                    Log.Logger.Error(e, "JISession Release_References_TimerTask:run() Exception in internal GC");
                }
            }
        }

        /// <summary>
        /// Target server
        /// </summary>
        internal string TargetServer {
            set {
                if (value.Equals("127.0.0.1", StringComparison.CurrentCultureIgnoreCase)) {
                    //Replace with it's actual bindings, otherwise does not work for JCIFS authentication
                    _targetServer = LocalhostAddressAsIPString;
                }
                else {
                    _targetServer = value;
                    //will change the localhost to the actual address as well
                    if (LocalhostAddressAsIPString.Equals("127.0.0.1", StringComparison.CurrentCultureIgnoreCase) ||
                        LocalhostAddressAsIPString.Equals("0.0.0.0", StringComparison.CurrentCultureIgnoreCase)) { 
                        //TODO: Bug in JDK , time to find alternate logic.
                        LocalhostAddressAsIPString = getLocalHost(value);
                    }
                }
            }
            get => _targetServer;
        }

        /// <summary>
        /// Local host
        /// </summary>
        internal static sbyte[] LocalhostAddressAsIPbytes { get; private set; } = new sbyte[] { 127, 0, 0, 1 };

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
        /// Returns the <code>IJIAuthInfo</code> (if any) associated with this session.
        /// </summary>
        public IJIAuthInfo AuthInfo { get; private set; } = null;

        /// <summary>
        /// Private constructor
        /// </summary>
        private JISession() { }

        /// <summary>
        /// Creates a session with the <code>authInfo</code> of the user. 
        /// This session is not yet attached to a COM server.
        /// </summary>
        /// <param name="authInfo"></param>
        /// <exception cref="ArgumentException"> if <code>authInfo</code> is <code>null</code>. </exception>
        /// <seealso cref="JIComServer.JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer.JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession createSession(IJIAuthInfo authInfo) {
            if (authInfo == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
            }

            var session = new JISession {
                AuthInfo = authInfo,

                SessionIdentifier = authInfo.UserName.GetHashCode() ^ authInfo.Password.GetHashCode() ^ authInfo.Domain.GetHashCode() ^ new object().GetHashCode() ^ (int)Runtime.Runtime.freeMemory() ^ _randomGen.Next()
            };


            lock (_mutex) {
                _mapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                _listOfSessions.Add(session);
            }

            Log.Logger.Information("Created Session: " + session.SessionIdentifier);
            return session;
        }

        /// <summary>
        /// Creates a session. This session is not yet attached to a
        /// COM server.
        /// </summary>
        /// <param name="domain"> domain of the user. </param>
        /// <param name="username"> name of the user </param>
        /// <param name="password"> password of the user.
        /// </param>
        /// <exception cref="ArgumentException"> if any parameter is <code>null</code>. </exception>
        /// <seealso cref="JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession createSession(string domain, string username, string password) {
            if (username == null || password == null || domain == null) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
            }
            var session = new JISession {
                _username = username,
                _password = password,
                _domain = domain,
                SessionIdentifier = username.GetHashCode() ^ password.GetHashCode() ^ domain.GetHashCode() ^ 
                    new object().GetHashCode() ^ _randomGen.Next()
            };
            lock (_mutex) {
                _mapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                _listOfSessions.Add(session);
            }
            Log.Logger.Information("Created Session: " + session.SessionIdentifier);
            return session;
        }


        /// <summary>
        /// Creates a new session using credentials of the <code>session</code>parameter.
        /// The new session is not yet attached to a COM server.
        /// </summary>
        /// <param name="session">
        /// </param>
        /// <seealso cref="JIComServer(JIClsid, JISession)"> </seealso>
        /// <seealso cref="JIComServer(JIProgId, JISession)"> </seealso>
        public static JISession createSession(JISession session) {
            var newSession = createSession(session.Domain, session.UserName, session.Password);
            newSession.AuthInfo = session.AuthInfo;
            return newSession;
        }

        /// <summary>
        /// <b>Native</b> Single Sign On capable session. 
        /// <b>Warning:</b> <ul><li>This method works <b>only</b> on Microsoft Windows Platform.</li>
        /// <li>It does <b>not</b> support NTLMv2 or NTLM1 Session Security.</li>
        /// <li>It supports only NTLM1 Authentication.</li>
        /// <li>This session <b>cannot</b> be used with <code>JIComServer(ProgId,...)</code> ctors. JCIFS will
        /// fail to setup a connection with Windows Registry if GUEST account is disabled.</li></ul> 
        /// </summary>
        /// <seealso cref="JIComServer.JIComServer(JIClsid, JISession)"></seealso>
        /// <seealso cref="JIComServer.JIComServer(JIProgId, JISession)"></seealso>
        public static JISession createSession() {
            if (!System.getProperty("os.name").ToLower().StartsWith("windows", StringComparison.Ordinal)) {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_WIN_ONLY));
            }

            var session = new JISession {
                SessionIdentifier = new object().GetHashCode() ^ (int)Runtime.Runtime.freeMemory() ^ _randomGen.Next(),
                SSOEnabled = true
            };

            lock (_mutex) {
                _mapOfSessionIdsVsSessions[session.SessionIdentifier] = session;
                _listOfSessions.Add(session);
            }

            Log.Logger.Information("Created Session for SSO: " + session.SessionIdentifier);

            return session;
        }

        /// <summary>
        /// Returns whether this session is SSO or not.
        /// </summary>
        public bool SSOEnabled { get; private set; }

        /// <summary>
        /// Used to destroy the <code>session</code>, this release all references of the COM server and it's interfaces.
        /// It should be called in the end after the developer is done with the COM server.
        /// Note that all interface references belonging to sessions linked to this session will also be destroyed.
        /// </summary>
        /// <param name="session"> </param>
        /// <exception cref="JIException"> </exception>
        public static void destroySession(JISession session) {
            //null session
            if (session == null) {
                return;
            }

            //if stub is null then cleanup datastructures holding the session object only
            if (session._stub == null) {
                lock (_mutex) {
                    _mapOfSessionIdsVsSessions.Remove(session.SessionIdentifier);
                    _listOfSessions.Remove(session);
                }

                //now remove the links and the OIDs
                postDestroy(session);
                return;
            }

            try {
                //session may have been destroyed and this call is from finalize.
                var list = new ArrayList();
                var listOfFreeIPIDs = new ArrayList();
                lock (_mutex) {
                    if (session.SessionInDestroy) {
                        return;
                    }
                    session.SessionInDestroy = true;
                    //list of dereferenced IPIDs
                    for (var j = 0; j < session._listOfDeferencedIpids.Count; j++) {
                        list.Add(session.prepareForReleaseRef((string)session._listOfDeferencedIpids[j]));
                    }
                    listOfFreeIPIDs.AddRange(session._listOfDeferencedIpids);
                    session._listOfDeferencedIpids.Clear();
                }

                lock (_mapOfObjects) {
                    //now take all the objects registered with this session and call release on them.
                    //				Iterator iterator = mapOfObjects.keySet().iterator();
                    IEnumerator iterator = _mapOfObjects.SetOfKeyValuePairs().GetEnumerator();
                    while (iterator.hasNext()) {
                        //String ipid = (String)session.mapOfObjects.get(iterator.next());
                        var entry = (DictionaryEntry)iterator.next();
                        //					IPID_SessionID_Holder holder = (IPID_SessionID_Holder)mapOfObjects.get(iterator.next());
                        var holder = (IPID_SessionID_Holder)entry.Value;
                        if (session.SessionIdentifier != (int)holder.sessionID) {
                            continue;
                        }
                        var ipid = holder.IPID;
                        if (ipid == null) {
                            continue;
                        }

                        //Commenting the line below since there could be more than one reference of a COM object taken in by
                        //j-Interop (via the client of j-Interop) and mapOfObjects will contain two references in this case.
                        //This was identified for the issue reported by Aquafold in sql dbg.
                        //					if (!listOfFreeIPIDs.contains(ipid))
                        {
                            list.Add(session.prepareForReleaseRef(ipid));
                            listOfFreeIPIDs.Add(ipid);
                        }
                        iterator.remove();
                    }
                }

                //now to kill the stub itself
                if (session._stub.ServerInterfacePointer != null) {
                    if (!listOfFreeIPIDs.Contains(session._stub.ServerInterfacePointer.IPID)) {
                        list.Add(session.prepareForReleaseRef(session._stub.ServerInterfacePointer.IPID));
                        listOfFreeIPIDs.Add(session._stub.ServerInterfacePointer.IPID);
                    }
                }

                listOfFreeIPIDs.Clear();
                //release is performed if only something is in the session.
                if (list.Count > 0) {
                    var array = new JIArray(list.ToArray(typeof(JIStruct)), true);
                    try {
                        session._stub.closeStub(); //close the existing connection
                        session.releaseRefs(array, true);
                    }
                    catch (JIException e) {
                        //This release cycle has to go on.
                        Log.Logger.Error(e, "JISession", "destroySession", e);
                    }
                }

                JIComOxidRuntime.clearIPIDsforSession(session);
                Log.Logger.Information("Destroyed Session: " + session.SessionIdentifier);
            }
            finally {
                lock (_mutex) {
                    _mapOfSessionIdsVsSessions.Remove(session.SessionIdentifier);
                    _listOfSessions.Remove(session);
                    // and remove its entry from the map
                    if (session._stub.ServerInterfacePointer != null) {
                        _mapOfOxidsVsJISessions.Remove(new JIOxid(session._stub.ServerInterfacePointer.OXID));
                    }
                }
                session._stub.closeStub();
                session.Stub2.closeStub();
            }

            postDestroy(session);
            session._stub = null; //setting it null in the end.
            session.Stub2 = null;
        }

        /// <summary>
        /// Post destroy
        /// </summary>
        /// <param name="session"></param>
        /// <exception cref="JIException"></exception>
        private static void postDestroy(JISession session) {
            //now destroy all linked sessions
            Log.Logger.Information("About to destroy links for Session: " + session.SessionIdentifier + " , size of which is " + session._links.Count);

            for (var i = 0; i < session._links.Count; i++) {
                destroySession((JISession)session._links[i]);
            }

            session._links.Clear();
            //finally any oids exported by this session.
            JIComOxidRuntime.destroySessionOIDs(session.SessionIdentifier);
        }

        /// <summary>
        /// each session is associated with 1 and only 1 stub.
        /// adding something new now another stub for IRemUnknown operations
        /// </summary>
        internal JIComServer Stub {
            set {
                _stub = value;
                lock (_mutex) {
                    _mapOfOxidsVsJISessions[new JIOxid(value.ServerInterfacePointer.OXID)] = this;
                }
            }
            get => _stub;
        }

        /// <summary>
        /// Unknown stub
        /// </summary>
        internal JIRemUnknownServer Stub2 { set; get; }

        /// <summary>
        /// Add to session
        /// </summary>
        /// <param name="comObject"></param>
        /// <param name="oid"></param>
        internal void addToSession(IJIComObject comObject, sbyte[] oid) {
            //nothing will be done if the session is being destroyed.
            if (SessionInDestroy) {
                return;
            }
            addWeakReference(comObject, oid);

            // setting if NO PING flag has been set to true.
            addToSession(comObject.Ipid, oid, ((JIStdObjRef)comObject.internal_getInterfacePointer()
                .getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).Flags == 0x00001000);
            Log.Logger.Information(" for IID: " + comObject.InterfaceIdentifier + " session: " + SessionIdentifier);

            var refcount = ((JIStdObjRef)comObject.internal_getInterfacePointer()
                .getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
            updateReferenceForIPID(comObject.Ipid, refcount);
        }

        /// <summary>
        /// Addref release
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="obj"></param>
        /// <param name="refcount"></param>
        /// <exception cref="JIException"></exception>
        internal void addRef_ReleaseRef(string IPID, JICallBuilder obj, int refcount) {
            updateReferenceForIPID(IPID, refcount);
            Stub2.addRef_ReleaseRef(obj);
        }

        /// <summary>
        /// Update reference
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="refcount"></param>
        private void updateReferenceForIPID(string ipid, int refcount) {
            var value = (int?)_mapOfIPIDsVsRefcounts[ipid];
            if (value == null) {
                // Were we asked to release a ref that wasnt in our map?
                if (refcount < 0) {
                    Log.Logger.Information("[updateReferenceForIPID] Released IPID not found: " + ipid);
                    return;
                }
                value = 0;
            }
            var newCount = (int)value + refcount;
            if (newCount > 0) {
                _mapOfIPIDsVsRefcounts[ipid] = newCount;
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
        internal void addWeakReference(IJIComObject comObject, sbyte[] oid) {
            var holder = new IPID_SessionID_Holder(comObject.Ipid, SessionIdentifier, false, oid);
            lock (_mapOfObjects) {
                _mapOfObjects[new WeakReference(comObject, _referenceQueueOfCOMObjects)] = holder;
            }
            // Increment the count for the number of weak-references for this IPID
            lock (_mapOfIPIDsVsWeakReferences) {
                // Count all weak-references for a given IPID.
                var count = (int?)_mapOfIPIDsVsWeakReferences[comObject.Ipid];
                if (count == null) {
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
        internal int removeWeakReference(string ipid) {
            Log.Logger.Verbose("Dumping mapOfIPIDsVsWeakReferences " + _mapOfIPIDsVsWeakReferences.ToString());
            var weakRefsRemaining = 0;
            lock (_mapOfIPIDsVsWeakReferences) {
                var count = (int?)_mapOfIPIDsVsWeakReferences[ipid];
                if (count == null) {
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
        private void addToSession(string IPID, sbyte[] oid, bool dontping) {
            //Weak reference of the object
            //mapOfObjects.put(new WeakReference(IPID,referenceQueueOfCOMObjects),IPID);
            //it does not matter if we create a new OID here, the OxidCOMRunttime API uses the OID in the MAP , and not this one.
            var joid = new JIObjectId(oid, dontping);
            JIComOxidRuntime.addUpdateOXIDs(this, IPID, joid);
            Log.Logger.Information("[addToSession] Adding IPID: " + IPID + " to session: " + SessionIdentifier);
        }


        /// <summary>
        /// this gets called from the cleanupthread and no place else and it calls the releaseRef of session which
        /// internally calls the add_releaseRef of the JIComServer, that method is synched at the instance level.
        /// I was worried about a deadlock with destroySession , since that also ultimately calls the add_releaseRef, but
        /// this will not happen since under a simultaneous destroy and removefromsession call , the "mutex" object will get synch.
        /// If suppose a comServer.getInterface(...) is being done (which also calls releaseRef), then that is synched at instance level
        /// and so is add_releaseRef (on the same instance), so deadlock won't happen there. If a simulataneous remove and getInterface call comes
        /// then getInterface(which internally calls releaseRef) will go through, since releaseRef is not synched but the api it calls i.e. add_releaseRef is synched with the same lock
        /// as getInterface. The remove will have to wait till that call gets over.
        /// </summary>
        /// <param name="IPID"></param>
        /// <exception cref="JIException"></exception>
        internal void releaseRef(string IPID) {
            releaseRef(IPID, 5);
        }

        /// <summary>
        /// Release reference
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="numinstances"></param>
        /// <exception cref="JIException"></exception>
        internal void releaseRef(string IPID, int numinstances) {
            Log.Logger.Information("releaseRef:Reclaiming from Session: " + SessionIdentifier + " , the IPID: " + IPID + ", numinstances is " + numinstances);
            var obj = new JICallBuilder(true) {
                ParentIpid = IPID,
                Opnum = 2 //release
            };
            //length
            obj.addInParamAsShort((short)1, JIFlags.FLAG_NULL);
            //ipid to addfref on
            var array = new JIArray(new rpc.core.UUID[] { new rpc.core.UUID(IPID) }, true);
            obj.addInParamAsArray(array, JIFlags.FLAG_NULL);
            //TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.addInParamAsInt(numinstances, JIFlags.FLAG_NULL);
            obj.addInParamAsInt(0, JIFlags.FLAG_NULL); //private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("releaseRef: Releasing numinstances " + numinstances + " references of IPID: " + IPID + " session: " + SessionIdentifier);
                debug_delIpids(IPID, numinstances);
            }
            addRef_ReleaseRef(IPID, obj, -5);
        }

        /// <summary>
        /// Dreference 
        /// </summary>
        /// <param name="IPID"></param>
        private void addDereferencedIpids(string IPID) {
            Log.Logger.Information("addDereferencedIpids for session : " + 
                SessionIdentifier + " , IPID is: " + IPID);
            lock (_mutex) {
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
        private void releaseRefs(JIArray arrayOfStructs, bool fromDestroy) {
            Log.Logger.Information("In releaseRefs for session : " + SessionIdentifier + 
                " , array length is: " + (short)((object[])arrayOfStructs.ArrayInstance).Length);
            var obj = new JICallBuilder(true) {
                Opnum = 2 //release
            };
            //length
            obj.addInParamAsShort((short)((object[])arrayOfStructs.ArrayInstance).Length, JIFlags.FLAG_NULL);
            obj.addInParamAsArray(arrayOfStructs, JIFlags.FLAG_NULL);
            obj._fromDestroySession = fromDestroy;
            _stub.addRef_ReleaseRef(obj);
            //ignore the results
        }

        /// <summary>
        /// Prepare for release
        /// </summary>
        /// <param name="IPID"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private JIStruct prepareForReleaseRef(string IPID) {
            var refcount = (int?)_mapOfIPIDsVsRefcounts[IPID];
            var releaseCount = 5 + 5; // 5 of the original and 5 for the addRef done later on.
            if (refcount != null) {
                releaseCount = (int)refcount;
            }

            return prepareForReleaseRef(IPID, releaseCount);
        }

        /// <summary>
        /// Prepare for release
        /// </summary>
        /// <param name="IPID"></param>
        /// <param name="refcount"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        private JIStruct prepareForReleaseRef(string IPID, int refcount) {
            var remInterface = new JIStruct();
            remInterface.addMember(new rpc.core.UUID(IPID));
            remInterface.addMember(refcount);
            remInterface.addMember(0); //private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("prepareForReleaseRef: Releasing " + refcount +
                    "references of IPID: " + IPID + " session: " + SessionIdentifier);
                debug_delIpids(IPID, refcount);
            }
            updateReferenceForIPID(IPID, -1 * refcount);

            return remInterface;
        }

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

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JISession other)) {
                return false;
            }
            return other.SessionIdentifier == SessionIdentifier;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return SessionIdentifier;
        }

        ~JISession() {
            try {
                destroySession(this);
            }
            catch (JIException e) {
                Log.Logger.Verbose("Exception in finalize when destroying session " + e.Message);
            }
        }

        internal IJIUnreferenced getUnreferencedHandler(string ipid) {
            lock (this) {
                return (IJIUnreferenced)_mapOfUnreferencedHandlers[ipid];
            }
        }

        internal void registerUnreferencedHandler(string ipid, IJIUnreferenced unreferenced) {
            lock (this) {
                _mapOfUnreferencedHandlers[ipid] = unreferenced;
            }
        }

        internal void unregisterUnreferencedHandler(string ipid) {
            lock (this) {
                _mapOfUnreferencedHandlers.Remove(ipid);
            }
        }

        /// <summary>
        ///<para> Sets the timeout for all sockets opened to (not fro) the COM server for this session. Default value is 0 (no timeout).
        /// The class level and the method level settings in case of <code>IJIComObject</code> override this timeout. </para>
        /// </summary>
        /// <seealso cref="IJIComObject.setInstanceLevelSocketTimeout(int)"> </seealso>
        /// <seealso cref="IJIComObject.call(JICallBuilder, int)"> </seealso>
        public int GlobalSocketTimeout { set; get; }

        /// <summary>
        /// Sets the use of NTLM2 Session Security. Framework will use NTLM Packet Level Privacy and Sign\Seal all packets.
        /// Once the <code>JIComServer</code> is bound to this session (using any of the <code>JIComServer</code> constructors)
        /// the use of session security <b>cannot</b> be enabled or disabled.
        /// Please note that session security can come at any available level of authentication (LM\NTLM\LMv2\NTLMv2). The framework
        /// currently only supports sign and seal at NTLMv1 level.
        /// Whether to use NTLM1 or not is dictated by this field in the Windows Registry.
        /// <code>
        /// HKLM\System\CurrentControlSet\Control\Lsa\LmCompatibilityLevel
        /// </code>
        /// This article on MSDN talks more about it http://support.microsoft.com/default.aspx?scid=KB;en-us;239869
        /// </summary>
        /// <param name="enable"> <code>true</code> to enable, <code>false</code> to disable. </param>
        public void useSessionSecurity(bool enable) {
            _useSessionSecurity_Renamed = enable;
            //		if (enable)
            //		{
            //			useNTLMv2 = enable;
            //		}
        }

        /// <summary>
        /// <para> Sets the use of NTLMv2 Security (default is NTLM1). This can be used in combination with <code>useSessionSecurity</code> method.
        /// Once the <code>JIComServer</code> is bound to this session (using any of the <code>JIComServer</code> constructors)
        /// the use of NTLMv2 security <b>cannot</b> be enabled or disabled.
        /// </para>
        /// <para>
        /// 
        /// </para>
        /// </summary>
        /// <param name="enable"> <code>true</code> to enable. </param>
        public void useNTLMv2(bool enable) {
            _useNTLMv2_Renamed = enable;
        }

        /// <summary>
        ///<para> Flag indicating whether session security is enabled. </para>
        /// </summary>
        /// <returns> <code>true</code> for enabled. </returns>
        public bool SessionSecurityEnabled => !SSOEnabled & _useSessionSecurity_Renamed;

        /// <summary>
        ///<para> Flag indicating whether NTLMv2 security is enabled. </para>
        /// </summary>
        /// <returns> <code>true</code> for enabled. </returns>
        public bool NTLMv2Enabled => !SSOEnabled & _useNTLMv2_Renamed;

        /// <summary>
        /// Links the src with target. These two sessions can now be destroyed in a cascade effect.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        internal static void linkTwoSessions(JISession src, JISession target) {
            if (src.SessionInDestroy || target.SessionInDestroy) {
                return;
            }
            if (src.Equals(target)) {
                return;
            }
            lock (_mutex) {
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
        internal static void unLinkSession(JISession src, JISession tobeunlinked) {
            if (src.SessionInDestroy) {
                return;
            }
            if (src.Equals(tobeunlinked)) {
                return;
            }
            lock (_mutex) {
                src._links.Remove(tobeunlinked);
            }
        }

        /// <summary>
        /// Based on the oxid returns the JISession (and thus the COM Server) associated with it. This is required, since there are
        /// cases where a different JISession may be passed in JIObjectFactory for an JIInterfacePointer which does not belong to this JISession.
        /// Under those scenarios, the COM factory will create a new instance of a JISession and associate that Interface pointer with the session.
        /// But that is not the right approach as a COM Server for that interface and thus a session might already exist and these have to be tied together.
        /// </summary>
        internal static JISession resolveSessionForOxid(JIOxid oxid) {
            lock (_mutex) {
                return (JISession)_mapOfOxidsVsJISessions[oxid];
            }
        }

        internal bool SessionInDestroy { get; private set; } = false;

        /// <summary>
        /// Register handlers for OBJREF_CUSTOM. customClass only serves as a Template and is of no real consequence.
        /// A new copy is returned from customClass.decode(...) and that is used by framework internally.
        /// </summary>
        /// <param name="CLSID"> </param>
        /// <param name="customClass"> </param>
        public void registerCustomMarshallerUnMarshallerTemplate(string CLSID, JIComCustomMarshallerUnMarshaller customClass) {
            _mapOfCustomCLSIDs[CLSID.ToLower()] = customClass;
        }

        internal JIComCustomMarshallerUnMarshaller getCustomMarshallerUnMarshallerTemplate(string CLSID) {
            return _mapOfCustomCLSIDs[CLSID.ToLower()];
        }

        private static Random _randomGen = new Random();
        private string _username;
        private string _password;
        private string _domain;
        private string _targetServer;
        private static IDictionary _mapOfObjects = Collections.synchronizedMap(new Hashtable());
        private static readonly object _mutex = new object();
        private JIComServer _stub;
        private static IDictionary _mapOfSessionIdsVsSessions = new Hashtable();
        private static ArrayList _listOfSessions = new ArrayList();
        private IList _listOfDeferencedIpids = new ArrayList();
        private static Timer _releaseRefsTimer = new Timer(true);
        private IDictionary _mapOfUnreferencedHandlers = new Hashtable();
        private bool _useSessionSecurity_Renamed;
        private bool _useNTLMv2_Renamed;
        private ArrayList _links = new ArrayList();
        private static readonly IDictionary _mapOfOxidsVsJISessions = new Hashtable();
        private static readonly IDictionary<string, JIComCustomMarshallerUnMarshaller> _mapOfCustomCLSIDs = new Dictionary<string, JIComCustomMarshallerUnMarshaller>();
        private IDictionary _mapOfIPIDsVsRefcounts = new Hashtable();
        private IDictionary _mapOfIPIDsVsWeakReferences = new Hashtable();
        internal static ReferenceQueue _referenceQueueOfCOMObjects = new ReferenceQueue();
        internal static Thread _cleanUpThread = new Thread(new RunnableAnonymousInnerClassHelper(), "jI_GarbageCollector");

        private class IPID_SessionID_Holder {
            public readonly string IPID;
            public readonly int? sessionID;
            public readonly bool isOnlySessionIDPresent;
            public readonly sbyte[] oid;
            internal IPID_SessionID_Holder(string IPID, int sessionID, bool isOnlySessionId, sbyte[] oid) {
                this.IPID = IPID;
                isOnlySessionIDPresent = isOnlySessionId;
                this.sessionID = sessionID;
                this.oid = oid;
            }
        }
    }
}
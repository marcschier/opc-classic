//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using Serilog;
    using System;
    using System.Threading;
    using SharpCifs.Util.Sharpen;
    using org.jinterop.dcom.common;
    using rpc;
    using System.Collections.Generic;

    /// <summary>
    /// Thread for Oxid Resolver. Creates and accepts socket
    /// connections for resolving oxids. Gets started once for each instance
    /// of the library.
    /// Please note that the <b>"Server"</b> Service should be running on the
    /// machine where the COM server is running.
    /// </summary>
    internal sealed class JIComOxidRuntime {

        private static Properties defaults = new Properties();
        private static Properties defaults2 = new Properties();
        private static bool stopSystem;
        private static bool resolverStarted;
        private static Hashtable mapOfIPIDVsComponent = new Hashtable(); //java client , com server
        private static Hashtable mapOfJavaVsOxidDetails = new Hashtable(); //java client , com server
        private static Hashtable mapOfOxidVsOxidDetails = new Hashtable(); //java client , com server
        private static Hashtable mapOfOIDVsComponents = new Hashtable(); //java client , com server

        //list of all exported oids per session, all these oids have to be removed.
        private static Hashtable mapOfSessionIdsVsOIDs = new Hashtable(); //java server , com client

        private static readonly Hashtable mapOfSetIdVsListOfOIDs = new Hashtable(); //com client , java server
        private static Hashtable mapOfSessionVsPingSetHolder = new Hashtable(); //com client , java server
                                                                                //private static HashMap mapOfIPIDVsOID = new HashMap(); //com client , java server, //IPID vs JIObjectId, for increasing\decreasing references
        private static Hashtable mapOfAddressVsStub = new Hashtable(); //java client , com server, so that we don't have to keep doing bind everytime.


        private static List<object> listOfExportedJavaComponents = new List<object>();

        internal static readonly object mutex = new object(); //for access to the sockets
        private static readonly object mutex2 = new object(); //for access to the maps
        private static readonly object mutex3 = new object(); //for access to the AddressVsSession,Stub Map

        private static readonly object mutex4 = new object(); //for access to the mapOfAddressVsStub

        private static ServerSocket serverSocket;
        private static Random randomGen = new Random(double.doubleToRawLongBits(new Random(1).NextDouble()));
        private static Timer pingTimer_2minutes = new Timer(true);
        private static Timer pingTimer_8minutes = new Timer(true);


        //one per session.
        private class PingSetHolder {
            internal byte[] setId;
            internal string username;
            internal string password;
            internal string domain;
            internal bool modified;
            internal bool closed;
            internal bool useNTLMv2;
            internal bool isSSO;
            internal int seqNum = 1;
            //JISession session  = null;
            internal Hashtable currentSetOIDs = new Hashtable(); //list of JIObjectId, this list is iterated and if the IPID ref count is 0 ,
                                                                 //it is added as a delete in set and a complex ping is sent.
            internal Hashtable pingedOnce = new Hashtable();
            public override string ToString() {
                return "SetID[" + setId + "] , currentSetOIDs[" + currentSetOIDs + "]";
            }
        }

        //this task just checks for expired OIDs in the mapOfOIDVsComponents, each OID carries with itself, lastPingedTime,
        //if that (currenttime - thattime) is < ping interval...all is okay, otherwise , all it's details are erased, thus
        //removing any reference of the given java server from j-Interop library, after which if no one outside has references, this
        //object can be GCed.
        private class ServerPingTimerTask : TimerTask {
            public virtual void run() {

                lock (mutex2) {

                    Log.Logger.Information("Running ServerPingTimerTask !");

                    var itr = mapOfOIDVsComponents.Keys.GetEnumerator();

                    while (itr.hasNext()) {
                        var oid = (JIObjectId)itr.next();
                        if (oid.HasExpired()) {
                            //remove all
                            var component = (JILocalCoClass)mapOfOIDVsComponents[oid];
                            //this means the local system still has references and we cannot delete this object
                            //since the user may reuse it.
                            if (component.AssociatedReferenceAlive) {
                                continue;
                            }
                            var details = (JIComOxidDetails)mapOfJavaVsOxidDetails[component];
                            mapOfOxidVsOxidDetails.Remove(details.Oxid);
                            mapOfIPIDVsComponent.Remove(details.Ipid);
                            mapOfJavaVsOxidDetails.Remove(component);
                            listOfExportedJavaComponents.Remove(component);
                            itr.remove();

                            //the thread associated with this will also stop.
                            details.interruptRemUnknownThreadGroup();

                            component = null;
                            details = null;
                        }
                    }

                }

            }
        }


        // Helper method to force release of a local component, so we dont
        // wait until the session is destroyed.
        internal static void releaseLocalComponent(JISession session, JILocalCoClass component) {
            lock (mutex2) {
                Log.Logger.Information("releaseLocalComponent: " + component.CoClassIID);

                var details = (JIComOxidDetails)mapOfJavaVsOxidDetails[component];
                mapOfOIDVsComponents.Remove(details.Oid);
                mapOfOxidVsOxidDetails.Remove(details.Oxid);
                mapOfIPIDVsComponent.Remove(details.Ipid);
                mapOfJavaVsOxidDetails.Remove(component);
                listOfExportedJavaComponents.Remove(component);
                mapOfSessionIdsVsOIDs.Remove(session.SessionIdentifier);

                //the thread associated with this will also stop.
                details.interruptRemUnknownThreadGroup();

                component = null;
                details = null;
            }
        }


        internal static void destroySessionOIDs(int sessionId) {
            lock (mutex2) {
                Log.Logger.Information("destroySessionOIDs for session: " + sessionId);

                IList oids = (ArrayList)mapOfSessionIdsVsOIDs.GetAndRemove(sessionId);
                if (oids == null || oids.Count == 0) {
                    return;
                }

                for (var i = 0; i < oids.Count; i++) {
                    var oid = (JIObjectId)oids[i];
                    //remove all
                    var component = (JILocalCoClass)mapOfOIDVsComponents.GetAndRemove(oid);
                    var details = (JIComOxidDetails)mapOfJavaVsOxidDetails[component];
                    if (details != null) {
                        mapOfOxidVsOxidDetails.Remove(details.Oxid);
                        mapOfIPIDVsComponent.Remove(details.Ipid);
                    }
                    mapOfJavaVsOxidDetails.Remove(component);
                    listOfExportedJavaComponents.Remove(component);
                    //the thread associated with this will also stop.
                    if (details != null) {
                        details.interruptRemUnknownThreadGroup();
                    }
                    component = null;
                    details = null;
                    oid = null;
                }

                oids.Clear();
            }
        }

        private class ClientPingTimerTask : TimerTask {
            public virtual void run() {

                IEnumerator itr = null;
                lock (mutex3) {
                    itr = ((IDictionary)mapOfSessionVsPingSetHolder.clone()).SetOfKeyValuePairs().GetEnumerator();
                }


                Log.Logger.Information("Running ClientPingTimerTask !");
                //iterate over the map and get the corresponding stubs and use there sessions to
                //stub is created here and used per address

                //if set id is null send a complex ping to get back the set id for all the OIDs in the
                //PingSetHolder

                while (itr.hasNext()) {
                    var entry = (DictionaryEntry)itr.next();
                    var holder = (PingSetHolder)entry.Value;
                    var address = ((JISession)entry.Key).TargetServer;
                    //will get it from the cache, since it is getting called after every 4 minutes
                    //what if this stub has timed out, I guess I will have to ask the developers to increase the timeout for now.
                    JIComOxidStub stub = null;
                    lock (mutex4) {
                        stub = (JIComOxidStub)mapOfAddressVsStub[address];
                        if (stub == null) {
                            stub = new JIComOxidStub(address, holder.domain, holder.username, holder.password, holder.useNTLMv2, holder.isSSO);
                            mapOfAddressVsStub[address] = stub;
                        }
                    }

                    var listOfAddedOIDs = new ArrayList();
                    var listOfRemovedOIDs = new ArrayList();
                    //form a list if OID is 0 ref
                    lock (mutex3) {
                        for (var itr2 = holder.currentSetOIDs.Keys.GetEnumerator(); itr2.hasNext();) {
                            var oid = (JIObjectId)itr2.next();
                            if (oid.IPIDRefCount == 0) {
                                if (!oid._dontping) {
                                    listOfRemovedOIDs.Add(oid);
                                    holder.pingedOnce.Remove(oid);
                                    holder.modified = true;
                                }
                                itr2.remove();
                            }
                            else {
                                if (!oid._dontping && !holder.pingedOnce.Contains(oid)) {
                                    listOfAddedOIDs.Add(oid);
                                    holder.pingedOnce[oid] = oid;
                                    holder.modified = true;
                                }
                            }
                        }
                    }
                    Log.Logger.Information("Within ClientPingTimerTask: holder.currentSetOIDs, current size of which is " + holder.currentSetOIDs.Count);

                    //this is the first time this is going and objects with no references will not be added to ping set.
                    if (holder.setId == null) {
                        listOfRemovedOIDs.Clear();
                    }

                    var isSimplePing = false;

                    //No additions and no deletions
                    if (holder.setId != null && !holder.modified) {
                        //send simple set ping
                        isSimplePing = true;
                    }

                    //seqNum will be 0 for simple ping, but incremented for complex pings. seqNum is per setId. first one will be 0 and increments
                    //there on...
                    holder.setId = stub.call(isSimplePing, holder.setId, listOfAddedOIDs, listOfRemovedOIDs, isSimplePing ? 0 : holder.seqNum++);

                    Log.Logger.Verbose("Within ClientPingTimerTask: holder.seqNum " + holder.seqNum);

                    holder.modified = false;
                    //stub.close(); commenting this since we are caching the stub.
                    if (holder.closed) {
                        //this means that this set is empty and there is no need for it. The set has emptied  itself and
                        //will get removed from COM servers side as well.
                        Log.Logger.Information("Within ClientPingTimerTask: Holder " + holder + " is empty, will remove this from mapOfSessionVsPingSetHolder");
                        itr.remove();
                        lock (mutex3) {
                            mapOfSessionVsPingSetHolder.Remove(entry.Key);
                        }
                    }
                }
            }
        }

        static JIComOxidRuntime() {
            defaults2.put("rpc.ntlm.lanManagerKey", "false");
            defaults2.put("rpc.ntlm.sign", "false");
            defaults2.put("rpc.ntlm.seal", "false");
            defaults2.put("rpc.ntlm.keyExchange", "false");
            defaults2.put("rpc.connectionContext", "org.jinterop.dcom.transport.JIComRuntimeNTLMConnectionContext");
            defaults.put("rpc.connectionContext", "org.jinterop.dcom.transport.JIComRuntimeConnectionContext");
        }

        //ip address
        internal static void addUpdateOXIDs(JISession session, string IPID, JIObjectId oid) {
            lock (mutex3) {
                //make sure this is the IP address
                var holder = (PingSetHolder)mapOfSessionVsPingSetHolder[session];
                if (holder == null) {
                    //new
                    holder = new PingSetHolder {
                        username = session.UserName,
                        password = session.Password,
                        domain = session.Domain
                    };
                    holder.currentSetOIDs[oid] = oid;
                    holder.modified = true;
                    holder.seqNum = 0;
                    holder.useNTLMv2 = session.NTLMv2Enabled;
                    holder.isSSO = session.SSOEnabled;
                    mapOfSessionVsPingSetHolder[session] = holder;
                }
                else //found , means it is another call for a new IPID
                {
                    var oid2 = (JIObjectId)holder.currentSetOIDs[oid];
                    if (oid2 != null) {
                        //have to update this oid, since the one from parameters is a "new" one.
                        oid = oid2;
                    }
                    else {
                        Log.Logger.Information("addUpdateOXIDs: Adding OID to holder " + holder + ", current size of currentSetOIDs is " + holder.currentSetOIDs.Count);
                        holder.currentSetOIDs[oid] = oid;
                        holder.modified = true;
                    }
                }

                oid.IncrementIPIDRefCountBy1();
                Log.Logger.Information("addUpdateOXIDs: finally this oid is " + oid);
            }

        }

        internal static void delIPIDReference(string IPID, JIObjectId oid, JISession session) {
            lock (mutex3) {
                var holder = (PingSetHolder)mapOfSessionVsPingSetHolder[session];
                //this will be non-null, since we are trying to remove an IPID reference so the PingSet for its OID should exist
                if (holder != null) {
                    var oid2 = (JIObjectId)holder.currentSetOIDs[oid];
                    if (oid2 != null) {
                        //temp gets replaced by the real one.
                        oid = oid2;
                    }
                    else {
                        Log.Logger.Warning("In delIPIDReference: Could not find Original OID for this temp OID for session: " + session.SessionIdentifier + " , temp oid is " + oid + " , and IPID is " + IPID);
                        return;
                    }

                    //this is the same OID as in the PingSetHolder.
                    oid.DecrementIPIDRefCountBy1();
                    Log.Logger.Information("delIPIDReference: Decrementing reference count for IPID " + IPID + " on OID " + oid);

                    //should we retain this now ??? , we need not send a ping for this as well. It is being retained for the last ping only.
                    if (oid.IPIDRefCount <= 0) {
                        holder.currentSetOIDs.Remove(oid);
                        //everything is gone, remove the session
                        if (holder.currentSetOIDs.Count == 0) {
                            holder.closed = true;
                            mapOfSessionVsPingSetHolder.Remove(session);
                        }
                        Log.Logger.Information("delIPIDReference: sessionid " + session.SessionIdentifier + "Ref count is <= 0, for OID " + oid + ", holder status: " + holder.closed);
                    }
                }
                else {
                    Log.Logger.Warning("In delIPIDReference: Could not find PingSetHolder for this session: " + session.SessionIdentifier + " , temp oid is " + oid + " , and IPID is " + IPID);
                }
            }
        }

        internal static void clearIPIDsforSession(JISession session) {
            lock (mutex3) {
                //make sure this is the IP address
                var holder = (PingSetHolder)mapOfSessionVsPingSetHolder[session];
                if (holder != null) {
                    Log.Logger.Information("clearIPIDsforSession: holder.currentSetOIDs's size is " + holder.currentSetOIDs.Count);

                    //No need to do this we are clearing the map anyways.
                    //				for (Iterator itr2 = holder.currentSetOIDs.keySet().iterator();itr2.hasNext();)
                    //				{
                    //					JIObjectId oid = (JIObjectId)itr2.next();
                    //					oid.setIPIDRefCountTo0();
                    //				}

                    holder.modified = true;
                    holder.currentSetOIDs.Clear(); //being done since this session is being destroyed and the corresponding COM server
                                                   //need not be retained by us.
                    holder.closed = true;

                    //Should be not remove this entry ??? I think it is being retained only for the pings ... we should let this go.
                    mapOfSessionVsPingSetHolder.Remove(session);
                }
            }

            //remove the socket for this session associated with ping timer
            lock (mutex4) {
                var stub = (JIComOxidStub)mapOfAddressVsStub.GetAndRemove(session.TargetServer);
                if (stub != null) {
                    stub.Close();
                }
            }

        }

        internal static void startResolverTimer() {
            lock (typeof(JIComOxidRuntime)) {
                //schedule only 1 timer task , the task to ping the OIDs obtained.
                pingTimer_2minutes.scheduleAtFixedRate(new ClientPingTimerTask(), 0, (int)(4 * 60 * 1000));
                if (JISystem.JavaCoClassAutoCollectionSet) {
                    pingTimer_8minutes.scheduleAtFixedRate(new ServerPingTimerTask(), 0, 8 * 60 * 1000);
                }
            }
        }

        //only one thread , that is the main is expected to enter this one.
        internal static void startResolver() {
            lock (typeof(JIComOxidRuntime)) {
                if (resolverStarted) {
                    return;
                }

                Runnable thread = new RunnableAnonymousInnerClassHelper();

                var thread2 = new Thread(thread, "jI_OxidResolver") {
                    Daemon = true
                };
                thread2.Start();
                resolverStarted = true;
            }
        }

        private class RunnableAnonymousInnerClassHelper : Runnable {
            public RunnableAnonymousInnerClassHelper() {
            }

            public virtual void run() {

                try {
                    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                    //ORIGINAL LINE: final java.nio.channels.ServerSocketChannel serverSocketChannel = java.nio.channels.ServerSocketChannel.open();
                    ServerSocketChannel serverSocketChannel = ServerSocketChannel.open();
                    serverSocket = serverSocketChannel.socket(); //new ServerSocket(0); //bind on any free port
                    serverSocket.bind(null);
                    OxidResolverPort = serverSocket.LocalPort;
                    //System.err.println("VIKRAM: oxidResolverPort: " + oxidResolverPort);
                    // server infinite loop
                    while (!stopSystem) {
                        Socket socket = serverSocket.accept();
                        //listOfSockets.add(socket);
                        //System.err.println("VIKRAM: Accepting new Call from " + socket.getPort());
                        //in a multithreaded scenario this will be serialized.
                        lock (mutex) {
                            JISystem.internal_setSocket(socket);
                            //now create the JIComOxidRuntimeHelper Object and start it.
                            var properties = new Properties(defaults);
                            properties.put("IID", "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0".ToUpper()); //IOxidResolver
                            var oxidResolver = new JIComOxidRuntimeHelper(properties);
                            oxidResolver.startOxid(socket.LocalPort, socket.Port);
                        }

                    }
                }
                catch (IOException) {
                    //e.printStackTrace();
                }

                //close all sockets.
                //			    for (int i = 0; i < listOfSockets.size(); i++)
                //			    {
                //			    	Socket s = (Socket)listOfSockets.get(i);
                //			    	try {
                //						s.close();
                //					} catch (IOException e) {}
                //			    }
            }
        }

        internal static int OxidResolverPort { get; private set; } = -1;

        //Will be called from shutDownHook thread.
        internal static void stopResolver() {
            lock (typeof(JIComOxidRuntime)) {
                stopSystem = true;
                try {
                    serverSocket.close();
                }
                catch (IOException) {
                }

                pingTimer_2minutes.cancel();
                pingTimer_8minutes.cancel();

                var itr = mapOfAddressVsStub.Values.GetEnumerator();
                while (itr.hasNext()) {
                    var s = (JIComOxidStub)itr.next();
                    s.Close();
                }
                mapOfAddressVsStub.Clear(); //will clean up all the others as well
            }
        }

        /// <summary>
        /// Returns the MIP for the Java Instance, this will also have the OXID,OID,IPID
        /// for the same.
        /// </summary>
        /// <param name="javaInstance">
        /// </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: static JIInterfacePointer getInterfacePointer(JISession session,JILocalCoClass component) throws org.jinterop.dcom.common.JIException
        internal static JIInterfacePointer getInterfacePointer(JISession session, JILocalCoClass component) {
            JIInterfacePointer ptr = null;

            lock (mutex2) {
                if (component.AlreadyExported) {
                    throw new JIException(JIErrorCodes.JI_JAVACOCLASS_ALREADY_EXPORTED);
                }

                component.Session = session;
                //
                //			JIComOxidDetails details = 	(JIComOxidDetails)mapOfJavaVsOxidDetails.get(component);
                //
                //			if (details != null)
                //			{
                //				return details.getInterfacePtr();
                //			}

                //as the ID could be repeated, this is the ipid of the interface being requested.
                //			String ipid = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
                string ipid = UUID.randomUUID().ToString();
                var iid = component.CoClassUnderRealIID ? component.CoClassIID : JiIUnknown.IID; //has to be IUnknown's IID.
                var bytes = new byte[8];
                randomGen.NextBytes(bytes);
                var oxid = new JIOxid(bytes);
                var bytes2 = new byte[8];
                randomGen.NextBytes(bytes2);

                var oid = new JIObjectId(bytes2, false);

                component.ObjectId = oid.OID;

                //JIComOxidDetails details = new JIComOxidDetails();
                var objref = new JIStdObjRef(ipid, oxid, oid);
                ptr = new JIInterfacePointer(iid, OxidResolverPort, objref);

                var properties = new Properties(defaults2);
                properties.put("IID", "00000131-0000-0000-C000-000000000046:0.0".ToUpper()); //IRemUnknown

                properties.put("rpc.ntlm.domain", session.TargetServer);

                var protecttionLevel = 2;

                if (session.SessionSecurityEnabled) {
                    protecttionLevel = 6;
                    properties.setProperty("rpc.ntlm.seal", "true");
                    properties.setProperty("rpc.ntlm.sign", "true");
                    properties.setProperty("rpc.ntlm.keyExchange", "true");
                    properties.setProperty("rpc.ntlm.keyLength", "128");
                    properties.setProperty("rpc.ntlm.ntlm2", "true");
                    properties.setProperty(Security.USERNAME, session.UserName);
                    properties.setProperty(Security.PASSWORD, session.Password);
                    properties.setProperty("rpc.ntlm.ntlm2", "true");
                }

                if (session.NTLMv2Enabled) {
                    properties.setProperty("rpc.ntlm.ntlmv2", "true");
                }

                var remUnknown = new JIComOxidRuntimeHelper(properties);


                //now create a new JIComOxidDetails
                //this carries a reference to the javaInstance , incase we do not get pings from the client
                //at the right times, the cleaup thread will remove this entry and it's OXID as well from both the maps.
                var details = new JIComOxidDetails(component, oxid, oid, iid, ipid, ptr, remUnknown, protecttionLevel);


                mapOfJavaVsOxidDetails[component] = details;

                mapOfOxidVsOxidDetails[oxid] = details;

                mapOfOIDVsComponents[oid] = component;

                listOfExportedJavaComponents.Add(component);

                mapOfIPIDVsComponent[ipid] = details; //this is the ipid of the component.

                IList oids = (ArrayList)mapOfSessionIdsVsOIDs[session.SessionIdentifier];
                if (oids == null) {
                    oids = new ArrayList();
                    mapOfSessionIdsVsOIDs[session.SessionIdentifier] = oids;
                }
                oids.Add(oid);

                component.AssociatedInterfacePointer = ptr;
            }
            return ptr;
        }

        //will get called from OxidResolverImpl only
        internal static JIComOxidDetails getOxidDetails(JIOxid oxid) {
            lock (mutex2) {
                return (JIComOxidDetails)mapOfOxidVsOxidDetails[oxid];
            }
        }

        //Will get called from RemQueryInterface of IRemUnknown, when it gets the IPID
        //it will identify the correct component to act on.
        //on this component the IID (provided again by the client) will do a exportInstance, with a
        //randomly generated IPID and this IPID will be returned to the client.
        //The oid be the one present in details object.
        //Now , when the alter context call will come with the new IID (which was just QIed), the
        //state of RemUnknownObject will get set for the correct component using getJavaComponentForIID.
        //The next call of requestcopdu will contain the request along with the field object having the IPID of the
        //instance to call on. Pass this to the components (identified previously) invoke API., along with the rest of params
        //How will the request get decoded with out IDL info ??? Hard code for now for toString ??
        internal static JIComOxidDetails getComponentFromIPID(string ipid) {
            lock (mutex2) {
                return (JIComOxidDetails)mapOfIPIDVsComponent[ipid];
            }
        }


        internal static void addUpdateSets(JISetId setId, ArrayList objectIdsAdded, ArrayList objectIdsDel) {
            lock (mutex2) {


                var listOfOIDs = (ArrayList)mapOfSetIdVsListOfOIDs[setId];

                if (listOfOIDs == null) {
                    listOfOIDs = new ArrayList();
                    //first time
                    listOfOIDs.AddRange(objectIdsAdded);
                    mapOfSetIdVsListOfOIDs[setId] = listOfOIDs;
                    //del list would be empty I presume

                }
                else {
                    for (var i = 0; i < listOfOIDs.Count; i++) {
                        var oid = (JIObjectId)listOfOIDs[i];
                        if (!objectIdsDel.Contains(oid)) {
                            oid.UpdateLastPingTime();
                        }
                    }

                    listOfOIDs.AddRange(objectIdsAdded);
                }

            }
        }

        //since the IID is unique and we have to consider nested IIDs, this API will not work for component's IID
        //	static JILocalCoClass getJavaComponentForIID(String uniqueIID)
        //	{
        //		JILocalCoClass component = null;
        //		synchronized (mutex2) {
        //			for (int i = 0; i < listOfExportedJavaComponents.size(); i++ )
        //			{
        //				component = (JILocalCoClass)listOfExportedJavaComponents.get(i);
        //				if (component.isPresent(uniqueIID))
        //				{
        //					break;
        //				}
        //				component = null;
        //			}
        //		}
        //
        //		return component;
        //	}

        internal static JILocalCoClass getJavaComponentFromIPID(string ipid) {
            JILocalCoClass component = null;
            lock (mutex2) {
                for (var i = 0; i < listOfExportedJavaComponents.Count; i++) {
                    component = (JILocalCoClass)listOfExportedJavaComponents[i];
                    //this will be unique, no two components will ever have same IPID for an IID.They will have different IPIDs for same IIDs.
                    if (component.GetIIDFromIpid(ipid) != null) {
                        break;
                    }
                    component = null;
                }
            }

            return component;
        }

    }

}
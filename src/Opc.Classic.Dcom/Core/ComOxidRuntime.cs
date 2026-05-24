//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Common;
using SharpInterop.Rpc;
using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharpInterop.Core; 
/// <summary>
/// Thread for Oxid Resolver. Creates and accepts socket
/// connections for resolving oxids. Gets started once for each instance
/// of the library.
/// Please note that the <b>"Server"</b> Service should be running on the
/// machine where the COM server is running.
/// </summary>
internal sealed class ComOxidRuntime {
    private static ComOxidRuntime _instance;

    /// <summary>
    /// Singleton
    /// </summary>
    public static ComOxidRuntime Instance {
        get {
            // TODO N1.2-followup: register ComOxidRuntime as an IServiceProvider singleton
            // and retire this legacy global mutable accessor.
            lock (typeof(ComOxidRuntime)) {
                if (_instance == null) {
                    try {
                        _instance = new ComOxidRuntime();
                    }
                    catch (IOException e) {
                        throw new InteropException(-1, e);
                    }
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// Oxid resolver port
    /// </summary>
    internal int OxidResolverPort { get; private set; } = -1;

    /// <summary>
    /// Create runtime
    /// </summary>
    private ComOxidRuntime() {
        _defaults2.SetProperty("rpc.ntlm.lanManagerKey", "false");
        _defaults2.SetProperty("rpc.ntlm.sign", "false");
        _defaults2.SetProperty("rpc.ntlm.seal", "false");
        _defaults2.SetProperty("rpc.ntlm.keyExchange", "false");
        _defaults2.SetProperty("rpc.connectionContext", "SharpInterop.Transport.ComRuntimeNTLMConnectionContext");
        _defaults.SetProperty("rpc.connectionContext", "SharpInterop.Transport.ComRuntimeConnectionContext");
    }

    internal static ProtectionLevel ConfigureActivationProtection(
        PropertyBag properties, bool sessionSecurityEnabled,
        string username, string password) {
        // Phase 3B: default to INTEGRITY per Microsoft DCOM hardening (KB5004442);
        // patched Windows DCOM servers reject CONNECT-level activation requests.
        // SessionSecurityEnabled still escalates to PRIVACY for full seal.
        var protectionLevel = ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;
        properties.SetProperty("rpc.ntlm.sign", "true");
        // Phase 3C: prefer NTLMv2 plus extended session security by default.
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");

        if (sessionSecurityEnabled) {
            protectionLevel = ProtectionLevel.PROTECTION_LEVEL_PRIVACY;
            properties.SetProperty("rpc.ntlm.seal", "true");
            properties.SetProperty("rpc.ntlm.keyExchange", "true");
            properties.SetProperty("rpc.ntlm.keyLength", "128");
            properties.SetProperty("rpc.ntlm.ntlm2", "true");
            properties.SetProperty(Security.USERNAME, username);
            properties.SetProperty(Security.PASSWORD, password);
            properties.SetProperty("rpc.ntlm.ntlm2", "true");
        }

        return protectionLevel;
    }

    /// <summary>
    /// Start resolver
    /// </summary>
    public void StartResolver() {
        lock (typeof(ComOxidRuntime)) {
            if (_resolverStarted) {
                return;
            }
            // TODO N1.2-followup: route the OXID socket listener through
            // ComOxidRuntimeAcceptService after the legacy Socket transport has an IAsyncEndpoint adapter.
            _thread = new OxidResolverThread(this, "jI_OxidResolver");
            _thread.SetDaemon(true);
            _thread.Start();

            // schedule only the task to ping the OIDs obtained.
            _clientPing = new Timer(_ => ClientPingTimerTask(), null, TimeSpan.Zero, DcomTimings.PingPeriod);
            if (Interop.IsCoClassAutoCollection) {
                _serverPing = new Timer(_ => ServerPingTimerTask(), null, TimeSpan.Zero, DcomTimings.ObjectExpiryPeriod);
            }
            _resolverStarted = true;
        }
    }

    /// <summary>
    /// Stop resolver
    /// </summary>
    public void StopResolver() {
        lock (typeof(ComOxidRuntime)) {
            _thread.Interrupt();
            _thread.Join();
            _thread = null;
            _clientPing?.Dispose();
            _serverPing?.Dispose();

            var itr = _mapOfAddressVsStub.Values.Iterator();
            while (itr.HasNext()) {
                var s = itr.Next();
                s.Close();
            }
            _mapOfAddressVsStub.Clear(); // will clean up all the others as well
        }
    }



    /// <summary>
    /// Helper method to force release of a local component, so we dont
    /// wait until the session is destroyed.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="component"></param>
    internal void ReleaseLocalComponent(Session session, LocalCoClass component) {
        lock (_mapOfOIDVsComponentsLock) {
            Log.Logger.Information("releaseLocalComponent: " + component.CoClassIID);

            var details = _mapOfLocalVsOxidDetails.GetOrDefault(component);
            _mapOfOIDVsComponents.Remove(details.Oid);
            _mapOfOxidVsOxidDetails.Remove(details.Oxid);
            _mapOfIPIDVsComponent.Remove(details.Ipid);
            _mapOfLocalVsOxidDetails.Remove(component);
            _listOfExportedComponents.Remove(component);
            _mapOfSessionIdsVsOIDs.Remove(session.SessionIdentifier);

            // the thread associated with this will also stop.
            details.InterruptRemUnknownThreadGroup();

            component = null;
            details = null;
        }
    }

    /// <summary>
    /// Destroy session oids
    /// </summary>
    /// <param name="sessionId"></param>
    internal void DestroySessionOIDs(int sessionId) {
        lock (_mapOfOIDVsComponentsLock) {
            Log.Logger.Information("DestroySessionOIDs for session: " + sessionId);

            var oids = _mapOfSessionIdsVsOIDs.GetAndRemove(sessionId);
            if (oids == null || oids.Count == 0) {
                return;
            }

            foreach (var oid in oids) {
                // remove all
                var component = _mapOfOIDVsComponents.GetAndRemove(oid);
                var details = _mapOfLocalVsOxidDetails.GetOrDefault(component);
                if (details != null) {
                    _mapOfOxidVsOxidDetails.Remove(details.Oxid);
                    _mapOfIPIDVsComponent.Remove(details.Ipid);
                }
                _mapOfLocalVsOxidDetails.Remove(component);
                _listOfExportedComponents.Remove(component);
                // the thread associated with this will also stop.
                if (details != null) {
                    details.InterruptRemUnknownThreadGroup();
                }
                component = null;
                details = null;
            }

            oids.Clear();
        }
    }

    /// <summary>
    /// Add or update oxids
    /// </summary>
    /// <param name="session"></param>
    /// <param name="IPID"></param>
    /// <param name="oid"></param>
    internal void AddUpdateOXIDs(Session session, string IPID, ObjectId oid) {
        System.Diagnostics.Debug.Assert(IPID != null);
        lock (_mapOfSessionVsPingSetHolderLock) {
            // make sure this is the IP address
            var holder = _mapOfSessionVsPingSetHolder.GetOrDefault(session);
            if (holder == null) {
                // new
                holder = new PingSetHolder {
                    Username = session.UserName,
                    Password = session.Password,
                    Domain = session.Domain
                };
                holder.CurrentSetOIDs.AddOrUpdate(oid, oid);
                holder.Modified = true;
                holder.SeqNum = 0;
                holder.UseNTLMv2 = session.NTLMv2Enabled;
                holder.IsSSO = session.SSOEnabled;
                _mapOfSessionVsPingSetHolder.AddOrUpdate(session, holder);
            }
            else {
                // found, means it is another call for a new IPID
                var oid2 = holder.CurrentSetOIDs.GetOrDefault(oid);
                if (oid2 != null) {
                    // have to update this oid, since the one from parameters is a "new" one.
                    oid = oid2;
                }
                else {
                    Log.Logger.Information("addUpdateOXIDs: Adding OID to holder " +
                        holder + ", current size of currentSetOIDs is " + holder.CurrentSetOIDs.Count);
                    holder.CurrentSetOIDs.AddOrUpdate(oid, oid);
                    holder.Modified = true;
                }
            }

            oid.IncrementIPIDRefCountBy1();
            Log.Logger.Information("addUpdateOXIDs: finally this oid is " + oid);
        }
    }

    /// <summary>
    /// Delete reference
    /// </summary>
    /// <param name="IPID"></param>
    /// <param name="oid"></param>
    /// <param name="session"></param>
    internal void DelIPIDReference(string IPID, ObjectId oid, Session session) {
        lock (_mapOfSessionVsPingSetHolderLock) {
            var holder = _mapOfSessionVsPingSetHolder.GetOrDefault(session);
            // this will be non-null, since we are trying to remove an IPID reference so the PingSet for its OID should exist
            if (holder != null) {
                var oid2 = holder.CurrentSetOIDs.GetOrDefault(oid);
                if (oid2 != null) {
                    // temp gets replaced by the real one.
                    oid = oid2;
                }
                else {
                    Log.Logger.Warning("In delIPIDReference: Could not find Original OID for this temp OID for session: " +
                        session.SessionIdentifier + ", temp oid is " + oid + ", and IPID is " + IPID);
                    return;
                }

                // this is the same OID as in the PingSetHolder.
                oid.DecrementIPIDRefCountBy1();
                Log.Logger.Information("delIPIDReference: Decrementing reference count for IPID " +
                    IPID + " on OID " + oid);

                // should we retain this now ???, we need not send a ping for this as well.
                // It is being retained for the last ping only.
                if (oid.IPIDRefCount <= 0) {
                    holder.CurrentSetOIDs.Remove(oid);
                    // everything is gone, remove the session
                    if (holder.CurrentSetOIDs.Count == 0) {
                        holder.Closed = true;
                        _mapOfSessionVsPingSetHolder.Remove(session);
                    }
                    Log.Logger.Information("delIPIDReference: sessionid " + session.SessionIdentifier +
                        "Ref count is <= 0, for OID " + oid + ", holder status: " + holder.Closed);
                }
            }
            else {
                Log.Logger.Warning("In delIPIDReference: Could not find PingSetHolder for this session: " +
                    session.SessionIdentifier + ", temp oid is " + oid + ", and IPID is " + IPID);
            }
        }
    }

    /// <summary>
    /// Clear ipds
    /// </summary>
    /// <param name="session"></param>
    internal void ClearIPIDsforSession(Session session) {
        lock (_mapOfSessionVsPingSetHolderLock) {
            // make sure this is the IP address
            var holder = _mapOfSessionVsPingSetHolder.GetOrDefault(session);
            if (holder != null) {
                Log.Logger.Information("clearIPIDsforSession: holder.currentSetOIDs's size is " +
                    holder.CurrentSetOIDs.Count);

                holder.Modified = true;
                // being done since this session is being destroyed and the corresponding COM server
                // need not be retained by us.
                holder.CurrentSetOIDs.Clear();
                holder.Closed = true;

                // Should be not remove this entry ???
                // I think it is being retained only for the pings ... we should let this go.
                _mapOfSessionVsPingSetHolder.Remove(session);
            }
        }

        // remove the socket for this session associated with ping timer
        lock (_mapOfAddressVsStubLock) {
            var stub = _mapOfAddressVsStub.GetAndRemove(session.TargetServer);
            if (stub != null) {
                stub.Close();
            }
        }
    }

    /// <summary>
    /// Returns the MIP for the Java Instance, this will also have the OXID,OID,IPID
    /// for the same.
    /// </summary>
    /// <exception cref="InteropException"></exception>
    /// <param name="session"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    internal InterfacePointer GetInterfacePointer(Session session, LocalCoClass component) {
        InterfacePointer ptr = null;

        lock (_mapOfOIDVsComponentsLock) {
            if (component.AlreadyExported) {
                throw new InteropException(ErrorCode.INTEROP_JAVACOCLASS_ALREADY_EXPORTED);
            }

            component.Session = session;

            // as the ID could be repeated, this is the ipid of the interface being requested.
            var ipid = Guid.NewGuid().ToString();
            var iid = component.ICoClassUnderRealIID ?
                component.CoClassIID : Interfaces.IID_IUnknown; // has to be IUnknown's IID.
            var bytes = new byte[8];
            _randomGen.NextBytes(bytes);
            var oxid = new Oxid(bytes);
            var bytes2 = new byte[8];
            _randomGen.NextBytes(bytes2);

            var oid = new ObjectId(bytes2, false);

            component.ObjectId = oid.OID;

            var objref = new StdObjRef(ipid, oxid, oid);
            ptr = new InterfacePointer(iid, OxidResolverPort, objref);

            var properties = new PropertyBag(_defaults2);
            properties.SetProperty("IID", Interfaces.IID_IRemUnknown + ":0.0");
            properties.SetProperty("rpc.ntlm.domain", session.TargetServer);

            var sessionSecurityEnabled = session.SessionSecurityEnabled;
            var protectionLevel = ConfigureActivationProtection(
                properties, sessionSecurityEnabled,
                sessionSecurityEnabled ? session.UserName : null,
                sessionSecurityEnabled ? session.Password : null);

            if (session.NTLMv2Enabled) {
                properties.SetProperty("rpc.ntlm.ntlmv2", "true");
            }

            var remUnknown = new ComOxidRuntimeHelper(properties);


            // this carries a reference to the local Instance, incase we do not get pings from the client
            // at the right times, the cleaup thread will remove this entry and it's OXID as well from both the maps.
            var details = new ComOxidDetails(component, oxid, oid, iid, ipid, ptr, remUnknown, protectionLevel);

            _mapOfLocalVsOxidDetails.AddOrUpdate(component, details);
            _mapOfOxidVsOxidDetails.AddOrUpdate(oxid, details);
            _mapOfOIDVsComponents.AddOrUpdate(oid, component);
            _listOfExportedComponents.Add(component);
            _mapOfIPIDVsComponent.AddOrUpdate(ipid, details); // this is the ipid of the component.

            var oids = _mapOfSessionIdsVsOIDs.GetOrDefault(session.SessionIdentifier);
            if (oids == null) {
                oids = new List<ObjectId>();
                _mapOfSessionIdsVsOIDs.AddOrUpdate(session.SessionIdentifier, oids);
            }
            oids.Add(oid);

            component.AssociatedInterfacePointer = ptr;
        }
        return ptr;
    }

    // will get called from OxidResolverImpl only
    internal ComOxidDetails GetOxidDetails(Oxid oxid) {
        lock (_mapOfOIDVsComponentsLock) {
            return _mapOfOxidVsOxidDetails.GetOrDefault(oxid);
        }
    }

    /// <summary>
    /// Will get called from RemQueryInterface of IRemUnknown, when it gets the IPID
    /// it will identify the correct component to act on.
    /// on this component the IID (provided again by the client) will do a
    /// exportInstance, with a randomly generated IPID and this IPID will be
    /// returned to the client. The oid be the one present in details object.
    /// Now, when the alter context call will come with the new IID (which was
    /// just QIed), the state of RemUnknownObject will get set for the correct
    /// component
    /// The next call of requestcopdu will contain the request along with the
    /// field object having the IPID of the instance to call on. Pass this to the
    /// components (identified previously) invoke API., along with the rest of params
    /// </summary>
    /// <param name="ipid"></param>
    /// <returns></returns>
    internal ComOxidDetails GetComponentFromIPID(string ipid) {
        // How will the request get decoded without IDL info ??? Hard code for now for toString ??
        lock (_mapOfOIDVsComponentsLock) {
            return _mapOfIPIDVsComponent.GetOrDefault(ipid);
        }
    }

    /// <summary>
    /// Add update sets
    /// </summary>
    /// <param name="setId"></param>
    /// <param name="objectIdsAdded"></param>
    /// <param name="objectIdsDel"></param>
    internal void AddUpdateSets(SetId setId, List<ObjectId> objectIdsAdded,
        List<ObjectId> objectIdsDel) {
        lock (_mapOfOIDVsComponentsLock) {
            var listOfOIDs = _mapOfSetIdVsListOfOIDs.GetOrDefault(setId);
            if (listOfOIDs == null) {
                listOfOIDs = new List<ObjectId>();
                // first time
                listOfOIDs.AddRange(objectIdsAdded);
                _mapOfSetIdVsListOfOIDs.AddOrUpdate(setId, listOfOIDs);
                // del list would be empty I presume
            }
            else {
                foreach (var oid in listOfOIDs) {
                    if (!objectIdsDel.Contains(oid)) {
                        oid.UpdateLastPingTime();
                    }
                }
                listOfOIDs.AddRange(objectIdsAdded);
            }
        }
    }

    /// <summary>
    /// Get component from ipid
    /// </summary>
    /// <param name="ipid"></param>
    /// <returns></returns>
    internal LocalCoClass GetLocalComponentFromIPID(string ipid) {
        lock (_mapOfOIDVsComponentsLock) {
            foreach (var component in _listOfExportedComponents) {
                // this will be unique, no two components will ever have same IPID for
                // an IID.They will have different IPIDs for same IIDs.
                if (component.GetIIDFromIpid(ipid) != null) {
                    return component;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// this task just checks for expired OIDs in the mapOfOIDVsComponents,
    /// each OID carries with itself, lastPingedTime, if that (currenttime - thattime)
    /// is &lt; ping interval...all is okay, otherwise, all it's details are erased,
    /// thus removing any reference of the given server from the library, after which
    /// if no one outside has references, this object can be GCed.
    /// </summary>
    private void ServerPingTimerTask() {

        lock (_mapOfOIDVsComponentsLock) {
            Log.Logger.Information("Running ServerPingTimerTask !");
            var itr = _mapOfOIDVsComponents.Keys.Iterator();
            while (itr.HasNext()) {
                var oid = itr.Next();
                if (oid.HasExpired()) {
                    // remove all
                    var component = _mapOfOIDVsComponents.GetOrDefault(oid);
                    // this means the local system still has references and we cannot delete this object
                    // since the user may reuse it.
                    if (component.AssociatedReferenceAlive) {
                        continue;
                    }
                    var details = _mapOfLocalVsOxidDetails.GetOrDefault(component);
                    _mapOfOxidVsOxidDetails.Remove(details.Oxid);
                    _mapOfIPIDVsComponent.Remove(details.Ipid);
                    _mapOfLocalVsOxidDetails.Remove(component);
                    _listOfExportedComponents.Remove(component);
                    itr.Remove();

                    // the thread associated with this will also stop.
                    details.InterruptRemUnknownThreadGroup();

                    component = null;
                    details = null;
                }
            }
        }
    }

    /// <summary>
    /// Client point
    /// </summary>
    private void ClientPingTimerTask() {

        Iterator<KeyValuePair<Session, PingSetHolder>> itr = null;
        lock (_mapOfSessionVsPingSetHolderLock) {
            itr = _mapOfSessionVsPingSetHolder.ToList().Iterator();
        }

        Log.Logger.Information("Running ClientPingTimerTask !");
        // iterate over the map and get the corresponding stubs and use there sessions to
        // stub is created here and used per address

        // if set id is null send a complex ping to get back the set id for all the OIDs in the
        // PingSetHolder

        while (itr.HasNext()) {
            var entry = itr.Next();
            var holder = entry.Value;
            var address = entry.Key.TargetServer;
            // will get it from the cache, since it is getting called every OXID ping period
            // what if this stub has timed out, I guess I will have to ask the developers to increase the timeout for now.
            ComOxidStub stub = null;
            lock (_mapOfAddressVsStubLock) {
                stub = _mapOfAddressVsStub.GetOrDefault(address);
                if (stub == null) {
                    stub = new ComOxidStub(address, holder.Domain, holder.Username,
                        holder.Password, holder.UseNTLMv2, holder.IsSSO);
                    _mapOfAddressVsStub.AddOrUpdate(address, stub);
                }
            }

            var listOfAddedOIDs = new List<ObjectId>();
            var listOfRemovedOIDs = new List<ObjectId>();
            // form a list if OID is 0 ref
            lock (_mapOfSessionVsPingSetHolderLock) {
                for (var itr2 = holder.CurrentSetOIDs.Keys.Iterator(); itr2.HasNext();) {
                    var oid = itr2.Next();
                    if (oid.IPIDRefCount == 0) {
                        if (!oid.Dontping) {
                            listOfRemovedOIDs.Add(oid);
                            holder.PingedOnce.Remove(oid);
                            holder.Modified = true;
                        }
                        itr2.Remove();
                    }
                    else {
                        if (!oid.Dontping && !holder.PingedOnce.Contains(oid)) {
                            listOfAddedOIDs.Add(oid);
                            holder.PingedOnce.AddOrUpdate(oid, oid);
                            holder.Modified = true;
                        }
                    }
                }
            }
            Log.Logger.Information(
                "Within ClientPingTimerTask: holder.currentSetOIDs, current size of which is " +
                holder.CurrentSetOIDs.Count);

            // this is the first time this is going and objects with no references
            // will not be added to ping set.
            if (holder.SetId == null) {
                listOfRemovedOIDs.Clear();
            }

            var isSimplePing = holder.SetId != null && !holder.Modified;

            // seqNum will be 0 for simple ping, but incremented for complex pings.
            // seqNum is per setId. first one will be 0 and increments there on...
            holder.SetId = stub.Call(isSimplePing, holder.SetId, listOfAddedOIDs,
                listOfRemovedOIDs, isSimplePing ? 0 : holder.SeqNum++);

            Log.Logger.Verbose("Within ClientPingTimerTask: holder.seqNum " + holder.SeqNum);

            holder.Modified = false;
            // stub.close(); commenting this since we are caching the stub.
            if (holder.Closed) {
                // this means that this set is empty and there is no need for it.
                // The set has emptied  itself and will get removed from COM servers side as well.
                Log.Logger.Information("Within ClientPingTimerTask: Holder " + holder +
                    " is empty, will remove this from mapOfSessionVsPingSetHolder");
                itr.Remove();
                lock (_mapOfSessionVsPingSetHolderLock) {
                    _mapOfSessionVsPingSetHolder.Remove(entry.Key);
                }
            }
        }
    }

    /// <summary>
    /// Ping set holder - one per session.
    /// </summary>
    private class PingSetHolder {
        internal byte[] SetId { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal string Domain { get; set; }
        internal bool Modified { get; set; }
        internal bool Closed { get; set; }
        internal bool UseNTLMv2 { get; set; }
        internal bool IsSSO { get; set; }
        internal int SeqNum { get; set; } = 1;

        /// <summary>
        /// List of ObjectId, this list is iterated and if the IPID ref count is 0,
        /// it is added as a delete in set and a complex ping is sent.
        /// </summary>
        internal Dictionary<ObjectId, ObjectId> CurrentSetOIDs { get; } =
            new Dictionary<ObjectId, ObjectId>();
        internal Dictionary<ObjectId, ObjectId> PingedOnce { get; } =
            new Dictionary<ObjectId, ObjectId>();

        /// <inheritdoc/>
        public override string ToString() =>
            "SetID[" + SetId + "], currentSetOIDs[" + CurrentSetOIDs + "]";
    }

    /// <summary>
    /// Oxid resolver thread
    /// </summary>
    private class OxidResolverThread : SharpCifs.Util.Sharpen.Thread {

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="outerInstance"></param>
        /// <param name="name"></param>
        public OxidResolverThread(ComOxidRuntime outerInstance, string name) :
            base(name) => _outerInstance = outerInstance;

        /// <inheritdoc/>
        public override void Run() {
            var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 0));
            listener.Listen();
            _outerInstance.OxidResolverPort = listener.GetLocalPort();
            while (!IsCanceled) {
                var socket = listener.Accept();
                lock (_outerInstance.Mutex) {
                    Interop.Internal_setSocket(socket);
                    // now create the ComOxidRuntimeHelper Object and start it.
                    var properties = new PropertyBag(_outerInstance._defaults);
                    properties.SetProperty("IID",
                        "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0".ToUpper()); // IOxidResolver
                    var oxidResolver = new ComOxidRuntimeHelper(properties);
                    oxidResolver.StartOxid(socket.GetLocalPort(), socket.GetLocalPort());
                }
            }
            try {
                listener.Close();
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }
            finally {
                listener.Dispose();
            }
        }
        private readonly ComOxidRuntime _outerInstance;
    }

    // java client, com server
    private readonly Dictionary<string, ComOxidDetails> _mapOfIPIDVsComponent =
        new Dictionary<string, ComOxidDetails>();
    // java client, com server
    private readonly Dictionary<LocalCoClass, ComOxidDetails> _mapOfLocalVsOxidDetails =
        new Dictionary<LocalCoClass, ComOxidDetails>();
    // java client, com server
    private readonly Dictionary<Oxid, ComOxidDetails> _mapOfOxidVsOxidDetails =
        new Dictionary<Oxid, ComOxidDetails>();
    // java client, com server
    private readonly Dictionary<ObjectId, LocalCoClass> _mapOfOIDVsComponents =
        new Dictionary<ObjectId, LocalCoClass>();
    // list of all exported oids per session, all these oids have to be removed.
    // java server, com client
    private readonly Dictionary<int, List<ObjectId>> _mapOfSessionIdsVsOIDs =
        new Dictionary<int, List<ObjectId>>();
    // com client, java server
    private readonly Dictionary<SetId, List<ObjectId>> _mapOfSetIdVsListOfOIDs =
        new Dictionary<SetId, List<ObjectId>>();
    // com client, java server
    private readonly Dictionary<Session, PingSetHolder> _mapOfSessionVsPingSetHolder =
        new Dictionary<Session, PingSetHolder>();
    // java client, com server, so that we don't have to keep doing bind everytime.
    private readonly Dictionary<string, ComOxidStub> _mapOfAddressVsStub =
        new Dictionary<string, ComOxidStub>();
    private readonly List<LocalCoClass> _listOfExportedComponents =
        new List<LocalCoClass>();

    internal readonly object Mutex = new object(); // for access to the sockets
    // for access to the maps
    private readonly object _mapOfOIDVsComponentsLock = new object();
    // for access to the AddressVsSession,Stub Map
    private readonly object _mapOfSessionVsPingSetHolderLock = new object();
    // for access to the mapOfAddressVsStub
    private readonly object _mapOfAddressVsStubLock = new object();


    private readonly PropertyBag _defaults = new PropertyBag();
    private readonly PropertyBag _defaults2 = new PropertyBag();
    private readonly Random _randomGen = new Random();
    private Timer _clientPing;
    private Timer _serverPing;
    private OxidResolverThread _thread;
    private bool _resolverStarted;
}

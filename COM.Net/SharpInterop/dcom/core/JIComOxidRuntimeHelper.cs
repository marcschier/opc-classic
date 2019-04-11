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
    using org.jinterop.dcom.transport;
    using rpc;
    using rpc.core;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Smb;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Used to manipulate Oxid details. one instance is created per binding
    /// call to the oxid resolver.
    /// </summary>
    internal sealed class JIComOxidRuntimeHelper : Stub {

        /// <summary>
        /// Create runtime helper
        /// </summary>
        /// <param name="properties"></param>
        internal JIComOxidRuntimeHelper(Properties properties) {
            TransportFactory = JIComRuntimeTransportFactory.Instance;
            Properties = properties;
            // this is never consulted so, putting localhost here.
            Address = "127.0.0.1[135]";
        }

        /// <inheritdoc/>
        protected override string Syntax => UUID.NIL_UUID + ":0.0";
            // "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0"; // IOxidResolver IID

        /// <summary>
        /// Start
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <param name="portNumLocal"></param>
        /// <param name="portNumRemote"></param>
        internal void StartOxid(int portNumLocal, int portNumRemote) {
            var oxidResolverThread = new OxidResolverThread(this,
                "jI_OxidResolver_Client[" + portNumLocal + ", " + portNumRemote + "]");
            oxidResolverThread.SetDaemon(true);
            oxidResolverThread.Start();
        }

        /// <summary>
        /// Returns the port to which the server is listening.
        /// </summary>
        /// <param name="baseIID"></param>
        /// <param name="ipidOfRemUnknown"></param>
        /// <param name="ipidOfComponent"></param>
        /// <param name="listOfSupportedInterfaces"></param>
        /// <param name="remUnknownForThisListener"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        internal int StartRemUnknown(string baseIID, string ipidOfRemUnknown,
            string ipidOfComponent, List<object> listOfSupportedInterfaces,
            out ThreadGroup remUnknownForThisListener) {
            var serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            var remUnknownPort = serverSocket.GetLocalPort();
            // have to pick up a random name so adding the ipid of
            // remunknown this is a uuid so the string is quite random.
            remUnknownForThisListener =
                new ThreadGroup("ThreadGroup - " + baseIID + "[" + ipidOfRemUnknown + "]");
            var remUnknownThread = new RemUnknownListenerThread(this, baseIID,
                ipidOfRemUnknown, ipidOfComponent, listOfSupportedInterfaces, serverSocket,
                remUnknownForThisListener, "jI_RemUnknownListener[" +
                baseIID + ", " + remUnknownPort + "]");
            remUnknownThread.SetDaemon(true);
            remUnknownThread.Start();
            return remUnknownPort;
        }

        /// <summary>
        /// Oxid resolver thread
        /// </summary>
        private class OxidResolverThread : Thread {
            /// <summary>
            /// Create thrad
            /// </summary>
            /// <param name="outerInstance"></param>
            /// <param name="name"></param>
            public OxidResolverThread(JIComOxidRuntimeHelper outerInstance, string name) :
                base(name) => _outerInstance = outerInstance;

            /// <inheritdoc/>
            public override void Run() {
                try {
                    Log.Logger.Information("started startOxid thread: " + GetName());
                    _outerInstance.Attach();
                    ((JIComRuntimeEndpoint)_outerInstance.Endpoint).ProcessRequests(
                        new OxidResolverImpl(_outerInstance.Properties), null, new List<object>(), Canceller.Token);
                }
                catch (OperationCanceledException) {
                    Log.Logger.Information("Oxid Resolver Thread" +
                        GetName() + " is purposefully closed by cancellation.");
                }
                catch (Exception e) {
                    Log.Logger.Error(e, "Oxid Resolver Thread: " + e.Message + ", on thread Id: " + GetName());
                }
                finally {
                    try {
                        ((JIComRuntimeEndpoint)_outerInstance.Endpoint).Detach();
                    }
                    catch (IOException) {
                    }
                }
                Log.Logger.Information("terminating startOxid thread: " + GetName());
            }
            private readonly JIComOxidRuntimeHelper _outerInstance;
        }

        /// <summary>
        /// Listener
        /// </summary>
        private class RemUnknownListenerThread : Thread {

            /// <summary>
            /// Create thread
            /// </summary>
            /// <param name="outerInstance"></param>
            /// <param name="baseIID"></param>
            /// <param name="ipidOfRemUnknown"></param>
            /// <param name="ipidOfComponent"></param>
            /// <param name="listOfSupportedInterfaces"></param>
            /// <param name="serverSocket"></param>
            /// <param name="remUnknownForThisListener"></param>
            /// <param name="name"></param>
            public RemUnknownListenerThread(JIComOxidRuntimeHelper outerInstance,
                string baseIID, string ipidOfRemUnknown, string ipidOfComponent,
                List<object> listOfSupportedInterfaces, Socket serverSocket,
                ThreadGroup remUnknownForThisListener, string name) :
                base(remUnknownForThisListener, name) {
                _outerInstance = outerInstance;
                _baseIID = baseIID;
                _ipidOfRemUnknown = ipidOfRemUnknown;
                _ipidOfComponent = ipidOfComponent;
                _listOfSupportedInterfaces = listOfSupportedInterfaces;
                _serverSocket = serverSocket;
                _remUnknownForThisListener = remUnknownForThisListener;
            }

            /// <inheritdoc/>
            public override void Run() {
                Log.Logger.Information("started RemUnknown listener thread for : " + GetName());
                try {
                    while (!IsCanceled) {
                        var socket = _serverSocket.Accept();
                        if (socket == null) {
                            continue;
                        }
                        Log.Logger.Information("RemUnknown listener: Got Connection from " + socket.GetPort());

                        // now create the JIComOxidRuntimeHelper Object and start it.
                        // We need a new one since the old one is already attached to the listener.
                        var remUnknownHelper = new JIComOxidRuntimeHelper(_outerInstance.Properties);
                        lock (JIComOxidRuntime.Instance.Mutex) {
                            JISystem.Internal_setSocket(socket);
                            remUnknownHelper.Attach();
                        }

                        // now start a new thread with this socket
                        var remUnknown = new RemUnknownThread(this, remUnknownHelper,
                            _remUnknownForThisListener, "jI_RemUnknown[" + _baseIID + ", L(" +
                                socket.GetLocalPort() + "):R(" + socket.GetPort() + ")]");
                        remUnknown.SetDaemon(true);
                        remUnknown.Start();
                    }
                }
                catch (OperationCanceledException) {
                    Log.Logger.Information("JIComOxidRuntimeHelper RemUnknownListener" +
                        GetName() + " is purposefully closed by cancellation.");
                }
                catch (IOException e) {
                    Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownListener");
                    Log.Logger.Warning("RemUnknownListener Thread: " + e.Message +
                        ", on thread Id: " + GetName());
                }
                catch (Exception e) {
                    Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownListener");
                }
                Log.Logger.Information("terminating RemUnknownListener thread: " + GetName());
            }

            /// <summary>
            /// Inner thread
            /// </summary>
            private class RemUnknownThread : Thread {

                /// <summary>
                /// Create runner
                /// </summary>
                /// <param name="outerInstance"></param>
                /// <param name="remUnknownHelper"></param>
                /// <param name="remUnknownForThisListener"></param>
                /// <param name="name"></param>
                public RemUnknownThread(RemUnknownListenerThread outerInstance,
                    JIComOxidRuntimeHelper remUnknownHelper,
                    ThreadGroup remUnknownForThisListener,
                    string name) : base(remUnknownForThisListener, name) {
                    _outerInstance = outerInstance;
                    _remUnknownHelper = remUnknownHelper;
                }

                /// <inheritdoc/>
                public override void Run() {
                    try {
                        ((JIComRuntimeEndpoint)_remUnknownHelper.Endpoint).ProcessRequests(
                            new RemUnknownObject(_outerInstance._ipidOfRemUnknown, _outerInstance._ipidOfComponent),
                            _outerInstance._baseIID, _outerInstance._listOfSupportedInterfaces, Canceller.Token);
                    }
                    catch (SmbAuthException e) {
                        Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownThread (not listener)");
                        throw new JIRuntimeException((int)JIErrorCodes.JI_CALLBACK_AUTH_FAILURE);
                    }
                    catch (SmbException e) {
                        // System.out.println(e.getMessage());
                        Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownThread (not listener)");
                        throw new JIRuntimeException((int)JIErrorCodes.JI_CALLBACK_SMB_FAILURE);
                    }
                    catch (OperationCanceledException) {
                        Log.Logger.Information("JIComOxidRuntimeHelper RemUnknownThread (not listener)" +
                            GetName() + " is purposefully closed by cancellation.");
                    }
                    catch (IOException e) {
                        Log.Logger.Warning(e, "JIComOxidRuntimeHelper RemUnknownThread (not listener)");
                    }
                    finally {
                        try {
                            _remUnknownHelper.Detach();
                        }
                        catch (IOException) {
                        }
                    }
                }

                private readonly RemUnknownListenerThread _outerInstance;
                private readonly JIComOxidRuntimeHelper _remUnknownHelper;
            }


            private readonly JIComOxidRuntimeHelper _outerInstance;
            private readonly string _baseIID;
            private readonly string _ipidOfRemUnknown;
            private readonly string _ipidOfComponent;
            private readonly List<object> _listOfSupportedInterfaces;
            private readonly Socket _serverSocket;
            private readonly ThreadGroup _remUnknownForThisListener;
        }

        /// <summary>
        /// This object should have serialized access only, i.e
        /// at a time only 1 read --> write, cycle should happen
        /// it is not multithreaded safe.
        /// </summary>
        internal class OxidResolverImpl : NdrOp, IJICOMRuntimeWorker {

            /// <summary>
            /// Create resolver
            /// </summary>
            /// <param name="p"></param>
            /// <inheritdoc/>
            public OxidResolverImpl(Properties p) => _p = p;

            /// <inheritdoc/>
            public List<object> QIedIIDs => null;

            /// <inheritdoc/>
            public bool Resolver => true;

            /// <inheritdoc/>
            public string CurrentIID { get; set; }

            /// <inheritdoc/>
            public bool WorkerOver() => false;

            /// <inheritdoc/>
            public UUID CurrentObjectID { set; get; }

            /// <inheritdoc/>
            public override int Opnum { get; set; }

            /// <inheritdoc/>
            public override void Write(NdrCodec ndr) => ndr.Buffer = _buffer;

            /// <inheritdoc/>
            public override void Read(NdrCodec ndr) {
                // will read according to the opnum.
                // The Opnum should have been set called before this call.
                switch (Opnum) {
                    case 1:
                        _buffer = ProcessSimplePing(ndr);
                        break;
                    case 2:
                        _buffer = ProcessComplexPing(ndr);
                        break;
                    case 3: // ServerAlive
                        _buffer = ProcessServerAlive(ndr);
                        break;
                    case 5: // This is ServerAlive2
                        _buffer = ProcessServerAlive2(ndr);
                        break;
                    case 4: // This is ResolveOxid2
                        _buffer = ProcessResolveOxid2(ndr);
                        break;
                    default: // should not have arrived here.
                        Log.Logger.Warning("Oxid Object: DEFAULTED !!!");
                        throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
                }
            }

            /// <summary>
            /// Process simple ping
            /// </summary>
            /// <param name="ndr"></param>
            /// <returns></returns>
            private NdrBuffer ProcessSimplePing(NdrCodec ndr) {
                Log.Logger.Information("Oxid Object: SimplePing");
                var b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8); // setid
                JIComOxidRuntime.Instance.AddUpdateSets(
                    new JISetId(b), new List<object>(), new List<object>());
                _buffer = new NdrBuffer(new byte[16], 0);
                _buffer.Enc_ndr_long(0);
                _buffer.Enc_ndr_long(0);
                _buffer.Enc_ndr_long(0);
                _buffer.Enc_ndr_long(0);
                return _buffer;
            }

            /// <summary>
            /// Process complex ping
            /// </summary>
            /// <param name="ndr"></param>
            /// <returns></returns>
            private NdrBuffer ProcessComplexPing(NdrCodec ndr) {
                Log.Logger.Information("Oxid Object: ComplexPing");
                var b = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8); // setid
                JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(short), null, JIFlags.FLAG_NULL, null); // seqId.
                var lengthAdds = (short)JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(short), null, JIFlags.FLAG_NULL, null);
                var lengthDels = (short)JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(short), null, JIFlags.FLAG_NULL, null);
                JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(int), null, JIFlags.FLAG_NULL, null);

                JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(int), null, JIFlags.FLAG_NULL, null); // length
                var listOfAdds = new List<object>();
                for (var i = 0; i < lengthAdds; i++) {
                    listOfAdds.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8), false));
                }

                JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(int), null, JIFlags.FLAG_NULL, null); // length
                var listOfDels = new List<object>();
                for (var i = 0; i < lengthDels; i++) {
                    listOfDels.Add(new JIObjectId(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8), false));
                }

                if (Arrays.Equals(b, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })) {
                    _random.NextBytes(b);
                }

                JIComOxidRuntime.Instance.AddUpdateSets(new JISetId(b), listOfAdds, listOfDels);
                _buffer = new NdrBuffer(new byte[32], 0);
                var ndr2 = new NdrCodec {
                    Buffer = _buffer
                };

                JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr2, b);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(short), (short)0, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), 0, null, JIFlags.FLAG_NULL); // hresult
                return _buffer;
            }

            /// <summary>
            /// Process server alive
            /// </summary>
            /// <param name="ndr"></param>
            /// <returns></returns>
            private NdrBuffer ProcessServerAlive(NdrCodec ndr) {
                System.Diagnostics.Debug.Assert(ndr != null);
                Log.Logger.Information("Oxid Object: ServerAlive");
                var buf = new byte[32]; // 16 + 16=just in case
                var ndrBuffer = new NdrBuffer(buf, 0);
                ndrBuffer.Enc_ndr_long(0);
                ndrBuffer.Enc_ndr_long(0);
                ndrBuffer.Enc_ndr_long(0);
                ndrBuffer.Enc_ndr_long(0);
                return ndrBuffer;
            }

            /// <summary>
            /// Process server alive
            /// </summary>
            /// <param name="ndr"></param>
            /// <returns></returns>
            private NdrBuffer ProcessServerAlive2(NdrCodec ndr) {
                System.Diagnostics.Debug.Assert(ndr != null);
                Log.Logger.Information("Oxid Object: ServerAlive2");
                // there is no in params for this.
                // only out params
                var dualStringArray = new JIDualStringArray(-1);

                var buf = new byte[dualStringArray.Length + 4 + 16 + 16]; // just in case - 2 unknown 8 bytes - COMVERSION
                var ndrBuffer = new NdrBuffer(buf, 0);

                var ndr2 = new NdrCodec {
                    Buffer = ndrBuffer
                };

                // Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4.
                // This is so that we stay at 5.4 DCOM until we upgrade the
                // local server to 5.7 as well.
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(short), (short)5, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(short), (short)4, null, JIFlags.FLAG_NULL);

                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), 0, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), dualStringArray.Length, null, JIFlags.FLAG_NULL);
                dualStringArray.Encode(ndr2);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), 0, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), 0, null, JIFlags.FLAG_NULL);
                return ndrBuffer;
            }

            /// <summary>
            /// Process resolve
            /// </summary>
            /// <param name="ndr"></param>
            /// <returns></returns>
            private NdrBuffer ProcessResolveOxid2(NdrCodec ndr) {
                Log.Logger.Information("Oxid Object: ResolveOxid2");
                // first read the OXID, then consult the oxid master about it's details.
                var oxid = new JIOxid(JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8));
                // now get the RequestedProtoSeq length.
                var length = (int)(short)JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(short), null, JIFlags.FLAG_NULL, null);

                // now for the array.
                var array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(ndr,
                    new JIArray(typeof(short), null, 1, true), null, JIFlags.FLAG_REPRESENTATION_ARRAY, null);

                // now query the Resolver master for this data.
                var details = JIComOxidRuntime.Instance.GetOxidDetails(oxid);

                if (details == null) {
                    // not found, now throw an JIRuntimeException, so that a FaultPdu could be sent.
                    throw new JIRuntimeException(JIErrorCodes.RPC_E_INVALID_OXID);
                }

                // randomly create IPID and send, this is the ipid of the remunknown,
                // we store it with remunknown object
                var uuid = details.RemUnknownIpid == null ?
                    new UUID(Guid.NewGuid().ToString()) : new UUID(details.RemUnknownIpid);

                int port;

                // create the bindings for this Java Object.
                // this port will go in the new bindings sent to the COM client.
                try {
                    // this is so that repeated calls for Oxid resolution return the same rem unknwon.
                    port = details.PortForRemUnknown;
                    if (port == -1) {
                        var remunknownipid = uuid.ToString();
                        port = details.COMRuntimeHelper.StartRemUnknown(
                            details.IID, remunknownipid, details.Ipid,
                            details.Referent.SupportedInterfaces, out var threadGroup);
                        details.SetRemUnknownThreadGroup(threadGroup);
                        details.RemUnknownIpid = remunknownipid;
                    }
                    details.PortForRemUnknown = port;
                }
                catch (IOException) {
                    throw new JIRuntimeException((int)JIErrorCodes.E_UNEXPECTED);
                }

                // can support only TCP connections
                var dualStringArray = new JIDualStringArray(port);

                var authnHint = details.AuthHint;
                var buf = new byte[4 + 4 + dualStringArray.Length + 16 + 4 + 2 + 2 + 4 + 16];

                // have all data now prepare the response
                // the response expected here is defines the byte array size.
                var ndrBuffer = new NdrBuffer(buf, 0);

                var ndr2 = new NdrCodec {
                    Buffer = ndrBuffer
                };

                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), new object().GetHashCode(), null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), (dualStringArray.Length - 4) / 2, null, JIFlags.FLAG_NULL);
                dualStringArray.Encode(ndr2);

                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(UUID), uuid, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), authnHint, null, JIFlags.FLAG_NULL);

                // Vikram June 19th 2013: Forcing the JILocalCoClass's server to 5.4.
                // This is so that we stay at 5.4 DCOM until we upgrade the
                // local server to 5.7 as well.
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(short), (short)5, null, JIFlags.FLAG_NULL);
                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(short), (short)4, null, JIFlags.FLAG_NULL);

                JIMarshalUnMarshalHelper.Serialize(
                    ndr2, typeof(int), 0, null, JIFlags.FLAG_NULL); // hresult

                return ndrBuffer;
            }

            private readonly Random _random = new Random();
            private NdrBuffer _buffer;
#pragma warning disable IDE0052 // Remove unread private members
            private readonly Properties _p;
#pragma warning restore IDE0052 // Remove unread private members
        }

        /// <summary>
        /// This object should have serialized access only, i.e
        /// at a time only 1 read --> write, cycle should happen
        /// it is not multithreaded safe.
        /// </summary>
        internal class RemUnknownObject : NdrOp, IJICOMRuntimeWorker {

            internal RemUnknownObject(string ipidOfme, string ipidOfComponent) {
                _selfIPID = ipidOfme;
                _mapOfIpidsVsRef[ipidOfComponent.ToUpper()] = 5;
            }

            /// <inheritdoc/>
            public override int Opnum { get; set; }

            /// <inheritdoc/>
            public List<object> QIedIIDs { get; } = new List<object>();

            /// <inheritdoc/>
            public bool Resolver => false;

            /// <inheritdoc/>
            public UUID CurrentObjectID {
                set {
                    _objectId = value;
                    _component = JIComOxidRuntime.Instance.GetLocalComponentFromIPID(value.ToString());
                }
                get => _objectId;
            }

            /// <inheritdoc/>
            public string CurrentIID { get; set; }

            /// <inheritdoc/>
            public bool WorkerOver() => _workerOver;

            /// <inheritdoc/>
            public override void Write(NdrCodec ndr) => ndr.Buffer = _buffer; // this buffer is prepared via read.

            /// <inheritdoc/>
            public override void Read(NdrCodec ndr) {
                // will read according to the opnum. The setOpnum should have been called before this call.
                var ipid = _objectId.ToString();

                //        if (!mapOfIpidsVsRef.containsKey(ipid.toUpperCase()))
                //        {
                //            System.out.println(Thread.currentThread() + " -->> " + ipid.toUpperCase());
                //            // we always give 5 references
                //            mapOfIpidsVsRef.put(ipid.toUpperCase(),new Integer(5));
                //        }

                // this means the call came for IRemUnknown apis, since selfIpid is null or matches the objectID
                // if (selfIPID == null || selfIPID.equalsIgnoreCase(ipid))
                //        if ("00000131-0000-0000-C000-000000000046".equalsIgnoreCase(currentIID))
                if (_selfIPID.Equals(ipid, StringComparison.CurrentCultureIgnoreCase)) {
                    switch (Opnum) {
                        case 3: // IRemUnknown QI.
                            _buffer = QueryInterface(ndr);
                            break;
                        case 4: // addref
                            JIOrpcThis.Decode(ndr);
                            var length = ndr.ReadUnsignedShort();

                            var retvals = new int[length];
                            var array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(ndr, kRemInterfaceRefArray,
                                new List<object>(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
                            // saving the ipids with there references. considering public + private references together for now.
                            var structs = (JIStruct[])array.ArrayInstance;
                            for (var i = 0; i < length; i++) {
                                var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
                                var publicRefs = (int)structs[i].GetMember(1);
                                var privateRefs = (int)structs[i].GetMember(2);

                                if (!_mapOfIpidsVsRef.Contains(ipidref)) {
                                    // this would be strange, since all the ipids we give should be part of the map already.
                                    // have to set 0x80000003 (INVALID ARG here)
                                    retvals[i] = unchecked((int)0x80000003);
                                    continue;
                                }
                                // StoredIQ - Satwik - native C++ says 01 here
                                retvals[i] = 0x1;


                                var total = (int)_mapOfIpidsVsRef[ipidref] + publicRefs + privateRefs;
                                _mapOfIpidsVsRef[ipidref] = total;
                            }
                            // preparing the response
                            _buffer = new NdrBuffer(new byte[(length * 4) + 16], 0);
                            var ndr2 = new NdrCodec {
                                Buffer = _buffer
                            };
                            JIOrpcThat.Encode(ndr2);
                            for (var i = 0; i < length; i++) {
                                _buffer.Enc_ndr_long(retvals[i]);
                            }

                            _buffer.Enc_ndr_long(0);
                            _buffer.Enc_ndr_long(0);

                            break;
                        case 5: // release
                            JIOrpcThis.Decode(ndr);
                            length = ndr.ReadUnsignedShort();
                            array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(
                                ndr, kRemInterfaceRefArray, new List<object>(), JIFlags.FLAG_REPRESENTATION_ARRAY, new Hashtable());
                            // saving the ipids with there references. considering public + private references together for now.
                            structs = (JIStruct[])array.ArrayInstance;
                            for (var i = 0; i < length; i++) {
                                var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper();
                                var publicRefs = (int)structs[i].GetMember(1);
                                var privateRefs = (int)structs[i].GetMember(2);
                                if (!_mapOfIpidsVsRef.Contains(ipidref)) {
                                    continue;
                                }

                                var total = (int)_mapOfIpidsVsRef[ipidref] - publicRefs - privateRefs;
                                if (total == 0) {
                                    _mapOfIpidsVsRef.Remove(ipidref);
                                }
                                else {
                                    _mapOfIpidsVsRef[ipidref] = total;
                                }
                            }

                            // all references to all IPIDs exported are over, this is now done.
                            if (_mapOfIpidsVsRef.Count == 0) {
                                _workerOver = true;
                            }

                            // I have 1 OID == 1 IPID == 1 java instance.
                            _buffer = new NdrBuffer(new byte[32], 0);
                            ndr2 = new NdrCodec {
                                Buffer = _buffer
                            };
                            JIOrpcThat.Encode(ndr2);
                            _buffer.Enc_ndr_long(0);
                            _buffer.Enc_ndr_long(0);
                            break;
                        default:
                            throw new JIRuntimeException(JIErrorCodes.RPC_S_PROCNUM_OUT_OF_RANGE);
                    }
                }
                else {
                    // now use the objectId, just set in before this call to read. That objectId is the IPID on which the
                    // call is being made, and was previously exported during Q.I. The component value was filled during an
                    // alter context or bind, again made some calls before.
                    if (_component == null) {
                        Log.Logger.Error("JIComOxidRuntimeHelper RemUnknownObject read(): component is null, opnum is " +
                            Opnum + ", IPID is " + ipid + ", selfIpid is " + _selfIPID);
                    }
                    byte[] b = null;
                    object result = null;
                    var ndr2 = new NdrCodec();
                    var hresult = 0;
                    object[] retArray = null;
                    try {
                        result = _component.InvokeMethod(ipid, Opnum, ndr);
                    }
                    catch (JIException e) {
                        hresult = (int)e.ErrorCode;
                        Log.Logger.Error(e, "RemUnknownObject read. Exception occured: " + e.ErrorCode);
                    }

                    // now if opnum was 6 then this is a dispatch call, so response has to be dispatch response
                    // not the normal one.
                    if (_component.GetInterfaceDefinitionFromIPID(ipid).DispInterface && Opnum == 6) {
                        var result2 = result;
                        // orpcthat
                        // [out] VARIANT * pVarResult,
                        // [out] EXCEPINFO * pExcepInfo,
                        // [out] UINT * pArgErr,
                        // [in, out, size_is(cVarRef)] VARIANTARG * rgVarRef
                        result = new object[4]; // orpcthat gets filled outside
                        var excepInfo = new JIStruct();
                        try {
                            excepInfo.AddMember((short)0);
                            excepInfo.AddMember((short)0);
                            excepInfo.AddMember(new JIString(""));
                            excepInfo.AddMember(new JIString(""));
                            excepInfo.AddMember(new JIString(""));
                            excepInfo.AddMember(0);
                            excepInfo.AddMember(new JIPointer(null, true));
                            excepInfo.AddMember(new JIPointer(null, true));
                            excepInfo.AddMember(0);
                        }
                        catch (JIException e) { // not expecting any here
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }

                        if (result2 == null) {
                            ((object[])result)[0] = JIVariant.CreateEMPTY();
                        }
                        else {
                            // now check whether the variant is by ref or not.
                            var variant = (JIVariant)((object[])result2)[0];

                            try {
                                if (variant.IsByRef) {
                                    // add empty inplace of this.
                                    ((object[])result)[0] = JIVariant.CreateEMPTY();
                                    // now update the array at the end.
                                    ((object[])result)[3] = new JIArray(new JIVariant[] { variant }, true);

                                }
                                else {
                                    ((object[])result)[0] = ((object[])result2)[0]; // will have only a single index.
                                    ((object[])result)[3] = 0; // Array
                                }
                            }
                            catch (JIException e) {
                                throw new JIRuntimeException(e.ErrorCode);
                            }
                        }
                        ((object[])result)[1] = excepInfo;
                        ((object[])result)[2] = 0; // argErr is null, for now.
                        retArray = (object[])result;
                    }
                    _buffer = new NdrBuffer(b, 0);
                    ndr2.Buffer = _buffer;

                    // JIOrpcThat.encode(ndr2);
                    // have to create a call Object, since these return types could be structs, unions etc. having deffered pointers
                    var callObject = new JICallBuilder();
                    callObject.AttachSession(_component.Session);
                    if (result != null) {

                        if (retArray != null) {
                            // serialize all members sequentially.
                            for (var i = 0; i < retArray.Length; i++) {
                                callObject.AddInParamAsObject(retArray[i], JIFlags.FLAG_NULL);
                            }
                        }
                        else {
                            // serialize all members sequentially.
                            for (var i = 0; i < ((object[])result).Length; i++) {
                                callObject.AddInParamAsObject(((object[])result)[i], JIFlags.FLAG_NULL);
                            }
                        }
                    }
                    callObject.Write2(ndr2);
                    JIMarshalUnMarshalHelper.Serialize(ndr2, typeof(int), hresult, null, JIFlags.FLAG_NULL);
                }
            }


            private NdrBuffer QueryInterface(NdrCodec ndr) {
                // now to decompose all
                Log.Logger.Verbose("Within RemUnknownObject: QueryInterface");
                Log.Logger.Verbose("RemUnknownObject: [QI] Before call terminated listOfIIDsQIed are: " + QIedIIDs);
                JIOrpcThis.Decode(ndr);

                // now get the IPID and export the component with a new IPID and IID.
                var ipid = new UUID();
                try {
                    ipid.Decode(ndr, ndr.Buffer);
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "JIComOxidRuntimeHelper", "QueryInterface", e);
                }

                Log.Logger.Verbose("RemUnknownObject: [QI] IPID is " + ipid);
                // set the JILocalCoClass., the ipid should not be null in this call.
                var details = JIComOxidRuntime.Instance.GetComponentFromIPID(ipid.ToString());

                if (details == null) {
                    // not found, now throw an JIRuntimeException, so that a FaultPdu could be sent.
                    throw new JIRuntimeException(JIErrorCodes.RPC_E_INVALID_OXID);
                }

                var component = details.Referent;

                Log.Logger.Verbose("RemUnknownObject: [QI] JIJavcCoClass is " + component.CoClassIID);

                JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(int), null, JIFlags.FLAG_NULL, null); // refs, don't really care about this.

                int length = (short)JIMarshalUnMarshalHelper.Deserialize(
                    ndr, typeof(short), null, JIFlags.FLAG_NULL, null); // length of the requested Interfaces
                var array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(
                    ndr, new JIArray(typeof(UUID), null, 1, true), null, JIFlags.FLAG_REPRESENTATION_ARRAY, null);

                // now to build the buffer and export the IIDs with new IPIDs
                var b = new byte[8 + 4 + 4 + (length * (4 + 4 + 40)) + 16];
                var buffer = new NdrBuffer(b, 0);

                // start with response
                var ndr2 = new NdrCodec {
                    Buffer = buffer
                };

                JIOrpcThat.Encode(ndr2);

                // pointer
                JIMarshalUnMarshalHelper.Serialize(ndr2, typeof(int), new object().GetHashCode(), null, JIFlags.FLAG_NULL);
                // length of array
                JIMarshalUnMarshalHelper.Serialize(ndr2, typeof(int), length, null, JIFlags.FLAG_NULL);

                var arrayOfUUIDs = (object[])array.ArrayInstance;

                for (var i = 0; i < arrayOfUUIDs.Length; i++) {
                    var iid = (UUID)arrayOfUUIDs[i];
                    Log.Logger.Verbose("RemUnknownObject: [QI] Array iid[" + i + "] is " + iid);
                    // now for each QueryResult
                    try {
                        var hresult = 0;
                        var ipid2 = Guid.NewGuid().ToString();
                        if (!component.IsPresent(iid.ToString())) {
                            hresult = (int)JIErrorCodes.E_NOINTERFACE;
                            ipid2 = Guid.Empty.ToString();
                        }
                        else {
                            string tmpIpid = null;
                            try {
                                tmpIpid = component.GetIpidFromIID(iid.ToString());
                            }
                            catch (Exception e) {
                                Log.Logger.Error(e, "JIComOxidRuntimeHelper: QueryInterface");
                            }

                            if (tmpIpid == null) {
                                Log.Logger.Verbose("RemUnknownObject: [QI] tmpIpid is null for iid " + iid);
                                component.ExportInstance(iid.ToString(), ipid2);
                            }
                            else {
                                Log.Logger.Verbose("RemUnknownObject: [QI] tmpIpid is NOT null for iid "
                                    + iid + " and ipid sent back is " + ipid2);
                                ipid2 = tmpIpid;
                            }
                        }
                        // hresult
                        JIMarshalUnMarshalHelper.Serialize(
                            ndr2, typeof(int), hresult, null, JIFlags.FLAG_NULL);
                        JIMarshalUnMarshalHelper.Serialize(
                            ndr2, typeof(int), unchecked((int)0xCCCCCCCC), null, JIFlags.FLAG_NULL);

                        // now generate the IPID and export a java instance with this.
                        JIStdObjRef objRef = null;
                        if (hresult == 0) {
                            objRef = new JIStdObjRef(ipid2, details.Oxid, details.Oid);
                        }
                        else {
                            objRef = new JIStdObjRef(ipid2);
                        }
                        objRef.Encode(ndr2);

                        // add it to the exported Ipids map
                        if (hresult == 0) {
                            _mapOfIpidsVsRef[ipid2.ToUpper()] = objRef.PublicRefs;
                        }

                        Log.Logger.Verbose("RemUnknownObject: [QI] for which the stdObjRef is " + objRef);

                    }
                    catch (MemberAccessException e) {
                        Log.Logger.Error(e, "JIComOxidRuntimeHelper: QueryInterface");
                    }
                    catch (InstantiationException e) {
                        Log.Logger.Error(e, "JIComOxidRuntimeHelper: QueryInterface");
                    }

                    var iidtemp = iid.ToString().ToUpper() + ":0.0";
                    if (!QIedIIDs.Contains(iidtemp)) {
                        QIedIIDs.Add(iidtemp);
                    }
                }

                Log.Logger.Verbose("RemUnknownObject: [QI] After call terminated listOfIIDsQIed are: " +
                    QIedIIDs);
                return buffer;
            }

            static RemUnknownObject() {
                kRemInterfaceRef.AddMember(typeof(UUID));
                kRemInterfaceRef.AddMember(typeof(int));
                kRemInterfaceRef.AddMember(typeof(int)); // ??
            }

            private static readonly JIStruct kRemInterfaceRef =
                new JIStruct();
            private static readonly JIArray kRemInterfaceRefArray =
                new JIArray(kRemInterfaceRef, null, 1, true);
            private readonly Hashtable _mapOfIpidsVsRef = new Hashtable();
            private bool _workerOver;
            private NdrBuffer _buffer;
            // component tells you the JILocalCoClass to act on, sent via the AlterContext calls
            // for all Altercontexts with IRemUnknown, this will be null.
            // will hold the current instance to act on.
            // the component and object id duo work together.
            // 1 component could export many ipids.
            private JILocalCoClass _component;
            // ObjectID tells you the IPID to act on, sent via the Request calls
            private UUID _objectId;
            // this would be the ipid of this RemUnknownObject
            private readonly string _selfIPID;
        }
    }
}
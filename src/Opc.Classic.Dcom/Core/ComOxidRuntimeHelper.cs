// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Linq;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Used to manipulate Oxid details. one instance is created per binding
/// call to the oxid resolver.
/// </summary>
internal sealed class ComOxidRuntimeHelper : Stub {

    /// <summary>
    /// Create runtime helper
    /// </summary>
    /// <param name="properties"></param>
    internal ComOxidRuntimeHelper(PropertyBag properties) {
        TransportFactory = ComRuntimeTransportFactory.Instance;
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
        string ipidOfComponent, List<string> listOfSupportedInterfaces,
        out ThreadGroup remUnknownForThisListener) {
        var serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        var remUnknownPort = serverSocket.GetLocalPort();
        // have to pick up a random name so adding the ipid of
        // remunknown this is a uuid so the string is quite random.
        // TODO N1.2-followup: replace ThreadGroup-scoped RemUnknown lifetime with
        // an IAsyncDisposable lease owned by ComOxidRuntimeAcceptService workers.
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
    private sealed class OxidResolverThread : SharpCifs.Util.Sharpen.Thread {
#pragma warning disable RECS0154 // Parameter is never used
        /// <summary>
        /// Create thrad
        /// </summary>
        /// <param name="outerInstance"></param>
        /// <param name="name"></param>
        public OxidResolverThread(ComOxidRuntimeHelper outerInstance, string name) :
#pragma warning restore RECS0154 // Parameter is never used
            base(name) => _outerInstance = outerInstance;

        /// <inheritdoc/>
        public override void Run() {
            try {
                Log.Logger.Information("started startOxid thread: " + GetName());
                _outerInstance.Attach();
                ((ComRuntimeEndpoint)_outerInstance.Endpoint).ProcessRequests(
                    new OxidResolverImpl(_outerInstance.Properties), null, new List<string>(), Canceller.Token);
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
                    ((ComRuntimeEndpoint)_outerInstance.Endpoint).Detach();
                }
                catch (IOException) {
                }
            }
            Log.Logger.Information("terminating startOxid thread: " + GetName());
        }
        private readonly ComOxidRuntimeHelper _outerInstance;
    }

    /// <summary>
    /// Listener
    /// </summary>
    private sealed class RemUnknownListenerThread : SharpCifs.Util.Sharpen.Thread {

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
        public RemUnknownListenerThread(ComOxidRuntimeHelper outerInstance,
            string baseIID, string ipidOfRemUnknown, string ipidOfComponent,
            List<string> listOfSupportedInterfaces, Socket serverSocket,
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

                    // now create the ComOxidRuntimeHelper Object and start it.
                    // We need a new one since the old one is already attached to the listener.
                    var remUnknownHelper = new ComOxidRuntimeHelper(_outerInstance.Properties);
                    lock (ComOxidRuntime.Instance.Mutex) {
                        Interop.Internal_setSocket(socket);
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
                Log.Logger.Information("ComOxidRuntimeHelper RemUnknownListener" +
                    GetName() + " is purposefully closed by cancellation.");
            }
            catch (IOException e) {
                Log.Logger.Warning(e, "ComOxidRuntimeHelper RemUnknownListener");
                Log.Logger.Warning("RemUnknownListener Thread: " + e.Message +
                    ", on thread Id: " + GetName());
            }
            catch (Exception e) {
                Log.Logger.Warning(e, "ComOxidRuntimeHelper RemUnknownListener");
            }
            Log.Logger.Information("terminating RemUnknownListener thread: " + GetName());
        }

        /// <summary>
        /// Inner thread
        /// </summary>
        private sealed class RemUnknownThread : SharpCifs.Util.Sharpen.Thread {

            /// <summary>
            /// Create runner
            /// </summary>
            /// <param name="outerInstance"></param>
            /// <param name="remUnknownHelper"></param>
            /// <param name="remUnknownForThisListener"></param>
            /// <param name="name"></param>
            public RemUnknownThread(RemUnknownListenerThread outerInstance,
                ComOxidRuntimeHelper remUnknownHelper,
                ThreadGroup remUnknownForThisListener,
                string name) : base(remUnknownForThisListener, name) {
                _outerInstance = outerInstance;
                _remUnknownHelper = remUnknownHelper;
            }

            /// <inheritdoc/>
            public override void Run() {
                try {
                    ((ComRuntimeEndpoint)_remUnknownHelper.Endpoint).ProcessRequests(
                        new RemUnknownObject(_outerInstance._ipidOfRemUnknown, _outerInstance._ipidOfComponent),
                        _outerInstance._baseIID, _outerInstance._listOfSupportedInterfaces, Canceller.Token);
                }
                catch (SmbAuthException e) {
                    Log.Logger.Warning(e, "ComOxidRuntimeHelper RemUnknownThread (not listener)");
                    throw new InteropRuntimeException((int)ErrorCode.INTEROP_CALLBACK_AUTH_FAILURE);
                }
                catch (SmbException e) {
                    // System.out.println(e.getMessage());
                    Log.Logger.Warning(e, "ComOxidRuntimeHelper RemUnknownThread (not listener)");
                    throw new InteropRuntimeException((int)ErrorCode.INTEROP_CALLBACK_SMB_FAILURE);
                }
                catch (OperationCanceledException) {
                    Log.Logger.Information("ComOxidRuntimeHelper RemUnknownThread (not listener)" +
                        GetName() + " is purposefully closed by cancellation.");
                }
                catch (IOException e) {
                    Log.Logger.Warning(e, "ComOxidRuntimeHelper RemUnknownThread (not listener)");
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
            private readonly ComOxidRuntimeHelper _remUnknownHelper;
        }


        private readonly ComOxidRuntimeHelper _outerInstance;
        private readonly string _baseIID;
        private readonly string _ipidOfRemUnknown;
        private readonly string _ipidOfComponent;
        private readonly List<string> _listOfSupportedInterfaces;
        private readonly Socket _serverSocket;
        private readonly ThreadGroup _remUnknownForThisListener;
    }

    /// <summary>
    /// This object should have serialized access only, i.e
    /// at a time only 1 read --> write, cycle should happen
    /// it is not multithreaded safe.
    /// </summary>
    internal sealed class OxidResolverImpl : NdrOp, IComRuntimeWorker {

#pragma warning disable RECS0154 // Parameter is never used
        /// <summary>
        /// Create resolver
        /// </summary>
        /// <param name="p"></param>
        /// <inheritdoc/>
        public OxidResolverImpl(PropertyBag p) => _p = p;
#pragma warning restore RECS0154 // Parameter is never used

        /// <inheritdoc/>
        public List<string> QIedIIDs => null;

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
                    throw new InteropRuntimeException(ErrorCode.RPC_S_PROCNUM_OUT_OF_RANGE);
            }
        }

        /// <summary>
        /// Process simple ping
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        private NdrBuffer ProcessSimplePing(NdrCodec ndr) {
            Log.Logger.Information("Oxid Object: SimplePing");
            var b = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8); // setid
            ComOxidRuntime.Instance.AddUpdateSets(
                new SetId(b), new List<ObjectId>(), new List<ObjectId>());
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
            var b = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8); // setid
            MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(short)); // seqId.
            var lengthAdds = (short)MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(short));
            var lengthDels = (short)MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(short));
            MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(int));

            MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(int)); // length
            var listOfAdds = new List<ObjectId>();
            for (var i = 0; i < lengthAdds; i++) {
                listOfAdds.Add(new ObjectId(MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8), false));
            }

            MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(int)); // length
            var listOfDels = new List<ObjectId>();
            for (var i = 0; i < lengthDels; i++) {
                listOfDels.Add(new ObjectId(MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8), false));
            }

            if (Arrays.Equals(b, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })) {
                _random.NextBytes(b);
            }

            ComOxidRuntime.Instance.AddUpdateSets(new SetId(b), listOfAdds, listOfDels);
            _buffer = new NdrBuffer(new byte[32], 0);
            var ndr2 = new NdrCodec {
                Buffer = _buffer
            };

            MarshalUnMarshalHelper.WriteOctetArrayLE(ndr2, b);
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(short), (short)0);
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(int), 0); // hresult
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
            var dualStringArray = new DualStringArray(-1);

            var buf = new byte[dualStringArray.Length + 4 + 16 + 16]; // just in case - 2 unknown 8 bytes - COMVERSION
            var ndrBuffer = new NdrBuffer(buf, 0);

            var ndr2 = new NdrCodec {
                Buffer = ndrBuffer
            };

            // Forcing the LocalCoClass's server to 5.4.
            // This is so that we stay at 5.4 DCOM until we upgrade the
            // local server to 5.7 as well.
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(short), (short)5);
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(short), (short)4);
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), 0);
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), dualStringArray.Length);
            dualStringArray.Encode(ndr2);
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), 0);
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), 0);
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
            var oxid = new Oxid(MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8));
            // now get the RequestedProtoSeq length.
            var length = (int)(short)MarshalUnMarshalHelper.Deserialize(ndr, typeof(short));

            // now for the array.
            var array = (ComArray)MarshalUnMarshalHelper.Deserialize(ndr,
                new ComArray(typeof(short), null, 1, true), new CodecContext {
                    Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY
                });

            // now query the Resolver master for this data.
            var details = ComOxidRuntime.Instance.GetOxidDetails(oxid);

            if (details == null) {
                // not found, now throw an InteropRuntimeException, so that a FaultPdu is sent.
                throw new InteropRuntimeException(ErrorCode.RPC_E_INVALID_OXID);
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
                throw new InteropRuntimeException(ErrorCode.E_UNEXPECTED);
            }

            // can support only TCP connections
            var dualStringArray = new DualStringArray(port);

            var authnHint = details.AuthHint;
            var buf = new byte[4 + 4 + dualStringArray.Length + 16 + 4 + 2 + 2 + 4 + 16];

            // have all data now prepare the response
            // the response expected here is defines the byte array size.
            var ndrBuffer = new NdrBuffer(buf, 0);

            var ndr2 = new NdrCodec {
                Buffer = ndrBuffer
            };

            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(int), new object().GetHashCode());
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(int), (dualStringArray.Length - 4) / 2);
            dualStringArray.Encode(ndr2);

            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(UUID), uuid);
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(int), authnHint);

            // Forcing the LocalCoClass's server to 5.4.
            // This is so that we stay at 5.4 DCOM until we upgrade the
            // local server to 5.7 as well.
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(short), (short)5);
            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(short), (short)4);

            MarshalUnMarshalHelper.Serialize(
                ndr2, typeof(int), 0); // hresult

            return ndrBuffer;
        }

        private readonly Random _random = new Random();
        private NdrBuffer _buffer;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly PropertyBag _p;
#pragma warning restore IDE0052 // Remove unread private members
    }

    /// <summary>
    /// This object should have serialized access only, i.e
    /// at a time only 1 read --> write, cycle should happen
    /// it is not multithreaded safe.
    /// </summary>
    internal sealed class RemUnknownObject : NdrOp, IComRuntimeWorker {

        internal RemUnknownObject(string ipidOfme, string ipidOfComponent) {
            _selfIPID = ipidOfme;
            _mapOfIpidsVsRef.AddOrUpdate(ipidOfComponent.ToUpper(CultureInfo.InvariantCulture), 5);
        }

        /// <inheritdoc/>
        public override int Opnum { get; set; }

        /// <inheritdoc/>
        public List<string> QIedIIDs { get; } = new List<string>();

        /// <inheritdoc/>
        public bool Resolver => false;

        /// <inheritdoc/>
        public UUID CurrentObjectID {
            set {
                _objectId = value;
                _component = ComOxidRuntime.Instance.GetLocalComponentFromIPID(value.ToString());
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

            // this means the call came for IRemUnknown apis, since selfIpid is null or matches the objectID
            // if (selfIPID == null || selfIPID.equalsIgnoreCase(ipid))
            //        if (Interfaces.IID_IRemUnknown.EqualsIgnoreCase(currentIID))
            if (_selfIPID.Equals(ipid, StringComparison.CurrentCultureIgnoreCase)) {
                switch (Opnum) {
                    case 3: // IRemUnknown QI.
                        _buffer = QueryInterface(ndr);
                        break;
                    case 4: // addref
                        OrpcThis.Decode(ndr);
                        var length = ndr.ReadUnsignedShort();

                        var retvals = new int[length];
                        var array = (ComArray)MarshalUnMarshalHelper.Deserialize(ndr, kRemInterfaceRefArray,
                            new CodecContext {
                                Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY
                            });
                        // saving the ipids with there references. considering public + private references together for now.
                        var structs = (Struct[])array.ArrayInstance;
                        for (var i = 0; i < length; i++) {
                            var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper(CultureInfo.InvariantCulture);
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

                            var total = _mapOfIpidsVsRef.GetOrDefault(ipidref) + publicRefs + privateRefs;
                            _mapOfIpidsVsRef[ipidref] = total;
                        }
                        // preparing the response
                        _buffer = new NdrBuffer(new byte[(length * 4) + 16], 0);
                        var ndr2 = new NdrCodec {
                            Buffer = _buffer
                        };
                        OrpcThat.Encode(ndr2);
                        for (var i = 0; i < length; i++) {
                            _buffer.Enc_ndr_long(retvals[i]);
                        }

                        _buffer.Enc_ndr_long(0);
                        _buffer.Enc_ndr_long(0);

                        break;
                    case 5: // release
                        OrpcThis.Decode(ndr);
                        length = ndr.ReadUnsignedShort();
                        array = (ComArray)MarshalUnMarshalHelper.Deserialize(
                            ndr, kRemInterfaceRefArray, new CodecContext {
                                Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY
                            });
                        // saving the ipids with there references. considering public + private references together for now.
                        structs = (Struct[])array.ArrayInstance;
                        for (var i = 0; i < length; i++) {
                            var ipidref = ((UUID)structs[i].GetMember(0)).ToString().ToUpper(CultureInfo.InvariantCulture);
                            var publicRefs = (int)structs[i].GetMember(1);
                            var privateRefs = (int)structs[i].GetMember(2);
                            if (!_mapOfIpidsVsRef.Contains(ipidref)) {
                                continue;
                            }

                            var total = _mapOfIpidsVsRef.GetOrDefault(ipidref) - publicRefs - privateRefs;
                            if (total == 0) {
                                _mapOfIpidsVsRef.Remove(ipidref);
                            }
                            else {
                                _mapOfIpidsVsRef.AddOrUpdate(ipidref, total);
                            }
                        }

                        // all references to all IPIDs exported are over, this is now done.
                        _workerOver |= _mapOfIpidsVsRef.Count == 0;

                        // I have 1 OID == 1 IPID == 1 java instance.
                        _buffer = new NdrBuffer(new byte[32], 0);
                        ndr2 = new NdrCodec {
                            Buffer = _buffer
                        };
                        OrpcThat.Encode(ndr2);
                        _buffer.Enc_ndr_long(0);
                        _buffer.Enc_ndr_long(0);
                        break;
                    default:
                        throw new InteropRuntimeException(ErrorCode.RPC_S_PROCNUM_OUT_OF_RANGE);
                }
            }
            else {
                // now use the objectId, just set in before this call to read. That objectId is the IPID on which the
                // call is being made, and was previously exported during Q.I. The component value was filled during an
                // alter context or bind, again made some calls before.
                if (_component == null) {
                    Log.Logger.Error("ComOxidRuntimeHelper RemUnknownObject read(): component is null, opnum is " +
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
                catch (InteropException e) {
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
                    var excepInfo = new Struct();
                    try {
                        excepInfo.AddMember((short)0);
                        excepInfo.AddMember((short)0);
                        excepInfo.AddMember(new ComString(""));
                        excepInfo.AddMember(new ComString(""));
                        excepInfo.AddMember(new ComString(""));
                        excepInfo.AddMember(0);
                        excepInfo.AddMember(new ComPointer(null, true));
                        excepInfo.AddMember(new ComPointer(null, true));
                        excepInfo.AddMember(0);
                    }
                    catch (InteropException e) { // not expecting any here
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }

                    if (result2 == null) {
                        ((object[])result)[0] = Variant.CreateEMPTY();
                    }
                    else {
                        // now check whether the variant is by ref or not.
                        var variant = (Variant)((object[])result2)[0];

                        try {
                            if (variant.IsByRef) {
                                // add empty inplace of this.
                                ((object[])result)[0] = Variant.CreateEMPTY();
                                // now update the array at the end.
                                ((object[])result)[3] = new ComArray(new Variant[] { variant }, true);

                            }
                            else {
                                ((object[])result)[0] = ((object[])result2)[0]; // will have only a single index.
                                ((object[])result)[3] = 0; // Array
                            }
                        }
                        catch (InteropException e) {
                            throw new InteropRuntimeException(e.ErrorCode);
                        }
                    }
                    ((object[])result)[1] = excepInfo;
                    ((object[])result)[2] = 0; // argErr is null, for now.
                    retArray = (object[])result;
                }
                _buffer = new NdrBuffer(b, 0);
                ndr2.Buffer = _buffer;

                // have to create a call Object, since these return types could be structs, unions etc. having deffered pointers
                var callObject = new CallBuilder();
                callObject.AttachSession(_component.Session);
                if (result != null) {

                    if (retArray != null) {
                        // serialize all members sequentially.
                        for (var i = 0; i < retArray.Length; i++) {
                            callObject.AddInParamAsObject(retArray[i], InteropFlags.FLAG_NULL);
                        }
                    }
                    else {
                        // serialize all members sequentially.
                        for (var i = 0; i < ((object[])result).Length; i++) {
                            callObject.AddInParamAsObject(((object[])result)[i], InteropFlags.FLAG_NULL);
                        }
                    }
                }
                callObject.Write2(ndr2);
                MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), hresult);
            }
        }

        /// <summary>
        /// Query interface
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        private NdrBuffer QueryInterface(NdrCodec ndr) {
            // now to decompose all
            Log.Logger.Verbose("Within RemUnknownObject: QueryInterface");
            Log.Logger.Verbose("RemUnknownObject: [QI] Before call terminated listOfIIDsQIed are: " + QIedIIDs);
            OrpcThis.Decode(ndr);

            // now get the IPID and export the component with a new IPID and IID.
            var ipid = new UUID();
            try {
                ipid.Decode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "ComOxidRuntimeHelper", "QueryInterface", e);
            }

            Log.Logger.Verbose("RemUnknownObject: [QI] IPID is " + ipid);
            // set theLocalCoClass., the ipid should not be null in this call.
            var details = ComOxidRuntime.Instance.GetComponentFromIPID(ipid.ToString());

            if (details == null) {
                // not found, now throw an InteropRuntimeException, so that a FaultPdu is sent.
                throw new InteropRuntimeException(ErrorCode.RPC_E_INVALID_OXID);
            }

            var component = details.Referent;

            Log.Logger.Verbose("RemUnknownObject: [QI] LocalCoClass is " + component.CoClassIID);

            // refs, don't really care about this.
            MarshalUnMarshalHelper.Deserialize(ndr, typeof(int));

            int length = (short)MarshalUnMarshalHelper.Deserialize(
                ndr, typeof(short)); // length of the requested Interfaces
            var array = (ComArray)MarshalUnMarshalHelper.Deserialize(
                ndr, new ComArray(typeof(UUID), null, 1, true), new CodecContext {
                    Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY
                });

            // now to build the buffer and export the IIDs with new IPIDs
            var b = new byte[8 + 4 + 4 + (length * (4 + 4 + 40)) + 16];
            var buffer = new NdrBuffer(b, 0);

            // start with response
            var ndr2 = new NdrCodec {
                Buffer = buffer
            };

            OrpcThat.Encode(ndr2);

            // pointer
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), new object().GetHashCode());
            // length of array
            MarshalUnMarshalHelper.Serialize(ndr2, typeof(int), length);

            var arrayOfUUIDs = (object[])array.ArrayInstance;

            for (var i = 0; i < arrayOfUUIDs.Length; i++) {
                var iid = (UUID)arrayOfUUIDs[i];
                Log.Logger.Verbose("RemUnknownObject: [QI] Array iid[" + i + "] is " + iid);
                // now for each QueryResult
                try {
                    var hresult = ErrorCode.ERROR_SUCCESS;
                    var ipid2 = Guid.NewGuid().ToString();
                    if (!component.IsIIDPresent(iid.ToString())) {
                        hresult = ErrorCode.E_NOINTERFACE;
                        ipid2 = Guid.Empty.ToString();
                    }
                    else {
                        string tmpIpid = null;
                        try {
                            tmpIpid = component.GetIpidFromIID(iid.ToString());
                        }
                        catch (Exception e) {
                            Log.Logger.Error(e, "ComOxidRuntimeHelper: QueryInterface");
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
                    MarshalUnMarshalHelper.Serialize(
                        ndr2, typeof(uint), hresult);
                    MarshalUnMarshalHelper.Serialize(
                        ndr2, typeof(int), unchecked((int)0xCCCCCCCC));

                    // now generate the IPID and export a java instance with this.
                    StdObjRef objRef = null;
                    if (hresult == ErrorCode.ERROR_SUCCESS) {
                        objRef = new StdObjRef(ipid2, details.Oxid, details.Oid);
                    }
                    else {
                        objRef = new StdObjRef(ipid2);
                    }
                    objRef.Encode(ndr2);

                    // add it to the exported Ipids map
                    if (hresult == ErrorCode.ERROR_SUCCESS) {
                        _mapOfIpidsVsRef.AddOrUpdate(ipid2.ToUpper(CultureInfo.InvariantCulture), objRef.PublicRefs);
                    }

                    Log.Logger.Verbose("RemUnknownObject: [QI] for which the stdObjRef is " + objRef);

                }
                catch (MemberAccessException e) {
                    Log.Logger.Error(e, "ComOxidRuntimeHelper: QueryInterface");
                }
                catch (InstantiationException e) {
                    Log.Logger.Error(e, "ComOxidRuntimeHelper: QueryInterface");
                }

                var iidtemp = iid.ToString().ToUpper(CultureInfo.InvariantCulture) + ":0.0";
                if (!QIedIIDs.Contains(iidtemp, StringComparer.OrdinalIgnoreCase)) {
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
            kRemInterfaceRef.AddMember(typeof(int));
        }

        private static readonly Struct kRemInterfaceRef =
            new Struct();
        private static readonly ComArray kRemInterfaceRefArray =
            new ComArray(kRemInterfaceRef, null, 1, true);
        private readonly Dictionary<string, int> _mapOfIpidsVsRef =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private bool _workerOver;
        private NdrBuffer _buffer;
        // component tells you the LocalCoClass to act on, sent via the AlterContext calls
        // for all Altercontexts with IRemUnknown, this will be null.
        // will hold the current instance to act on.
        // the component and object id duo work together.
        // 1 component could export many ipids.
        private LocalCoClass _component;
        // ObjectID tells you the IPID to act on, sent via the Request calls
        private UUID _objectId;
        // this would be the ipid of this RemUnknownObject
        private readonly string _selfIPID;
    }
}

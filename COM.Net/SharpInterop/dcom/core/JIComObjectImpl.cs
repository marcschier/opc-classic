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

    /// <summary>
    /// Implementation for IJIComObject.
    /// There is a 1 to 1 mapping between this and a <code>COM</code> interface.
    /// </summary>
    [Serializable]
    internal sealed class JIComObjectImpl : IComObject {

        /// <summary>
        /// Dual interface
        /// </summary>
        internal bool IsDual {
            set {
                _dualInfo = true;
                _isDual = value;
            }
        }

        /// <inheritdoc/>
        public bool LocalReference { get; }

        /// <inheritdoc/>
        public JIComCustomMarshallerUnMarshaller CustomObject { get; set; }

        /// <inheritdoc/>
        public int LengthOfInterfacePointer => _ptr.Length;

        /// <inheritdoc/>
        public string Ipid => _ptr.IPID;

        /// <inheritdoc/>
        public JISession AssociatedSession => _session;

        /// <inheritdoc/>
        public string InterfaceIdentifier => _ptr.IID;

        /// <inheritdoc/>
        public bool DispatchSupported {
            get {
                lock (this) {
                    CheckLocal();
                    if (!_dualInfo) {
                        // query interface for it and then release it.
                        try {
                            var comObject = QueryInterface("00020400-0000-0000-c000-000000000046");
                            comObject.Release();
                            IsDual = true;
                        }
                        catch (JIException) {
                            IsDual = false;
                        }
                    }
                    return _isDual;
                }
            }
        }

        /// <inheritdoc/>
        public IJIUnreferenced UnreferencedHandler {
            get {
                CheckLocal();
                return _session.GetUnreferencedHandler(Ipid);
            }
        }

        /// <inheritdoc/>
        public int InstanceLevelSocketTimeout {
            get {
                CheckLocal();
                return _timeout;
            }
            set {
                CheckLocal();
                _timeout = value;
            }
        }

        /// <summary>
        /// Create object
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ptr"></param>
        internal JIComObjectImpl(JISession session, JIInterfacePointer ptr) :
            this(session, ptr, false) {
        }

        /// <summary>
        /// Create object
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ptr"></param>
        /// <param name="isLocal"></param>
        internal JIComObjectImpl(JISession session, JIInterfacePointer ptr, bool isLocal) {
            _session = session;
            _ptr = ptr;
            LocalReference = isLocal;
        }

        /// <inheritdoc/>
        public IComObject QueryInterface(string iid) {
            CheckLocal();
            return _session.Stub.GetInterface(iid, _ptr.IPID);
        }

        /// <inheritdoc/>
        public void AddRef() {
            CheckLocal();
            var obj = new JICallBuilder(true) {
                ParentIpid = _ptr.IPID,
                Opnum = 1 // addRef
            };

            // length
            obj.AddInParamAsShort(1, JIFlags.FLAG_NULL);
            // ipid to addfref on
            var array = new JIArray(new UUID[] { new UUID(_ptr.IPID) }, true);
            obj.AddInParamAsArray(array, JIFlags.FLAG_NULL);
            // TODO requesting 5 for now, will later build caching mechnaism to exhaust
            // 5 refs first before asking for more
            // same with release.
            obj.AddInParamAsInt(5, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0, JIFlags.FLAG_NULL); // private refs = 0

            obj.AddOutParamAsType(typeof(short), JIFlags.FLAG_NULL); // size
            obj.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL); // Hresult for size
            Log.Logger.Warning("addRef: Adding 5 references for " + _ptr.IPID + " session: " +
                _session.SessionIdentifier);

            // JISession.debug_addIpids(_ptr.IPID, 5);

            //        session.getStub2().addRef_ReleaseRef(obj);
            _session.AddRef_ReleaseRef(_ptr.IPID, obj, 5);

            if (obj.GetResultAsIntAt(1) != 0) {
                throw new JIException(obj.GetResultAsIntAt(1), (Exception)null);
            }
        }

        /// <inheritdoc/>
        public void Release() {
            CheckLocal();
            var obj = new JICallBuilder(true) {
                ParentIpid = _ptr.IPID,
                Opnum = 2 // release
            };
            // length
            obj.AddInParamAsShort(1, JIFlags.FLAG_NULL);
            // ipid to addfref on
            var array = new JIArray(new UUID[] { new UUID(_ptr.IPID) }, true);
            obj.AddInParamAsArray(array, JIFlags.FLAG_NULL);
            // TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.AddInParamAsInt(5, JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0, JIFlags.FLAG_NULL); // private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("RELEASE called directly ! removing 5 references for " + _ptr.IPID + " session: " + _session.SessionIdentifier);
                // JISession.debug_delIpids(_ptr.IPID, 5);
            }
            // TODO??
            //        session.getStub2().addRef_ReleaseRef(obj);
            _session.AddRef_ReleaseRef(_ptr.IPID, obj, -5);
        }

        /// <inheritdoc/>
        public object[] Call(JICallBuilder obj) {
            CheckLocal();
            return Call(obj, _timeout);
        }

        /// <inheritdoc/>
        public void RegisterUnreferencedHandler(IJIUnreferenced unreferenced) {
            CheckLocal();
            _session.RegisterUnreferencedHandler(Ipid, unreferenced);
        }

        /// <inheritdoc/>
        public void UnregisterUnreferencedHandler() {
            CheckLocal();
            _session.UnregisterUnreferencedHandler(Ipid);
        }

        /// <inheritdoc/>
        public object[] Call(JICallBuilder obj, int socketTimeout) {
            CheckLocal();
            obj.AttachSession(_session);
            obj.ParentIpid = _ptr.IPID;
            // Call is always made on your stub.
            if (socketTimeout != 0) { // using instance level timeout
                return _session.Stub.Call(obj, _ptr.IID, socketTimeout);
            }
            return _session.Stub.Call(obj, _ptr.IID);
        }

        /// <inheritdoc/>
        public JIInterfacePointer Internal_getInterfacePointer() => _ptr ?? _session.Stub.ServerInterfacePointer;

        /// <inheritdoc/>
        public string Internal_setConnectionInfo(IComObject connectionPoint, int? cookie) {
            lock (this) {
                CheckLocal();
                if (_connectionPointInfo == null) { // lazy creation, since this is used by event callbacks only.
                    _connectionPointInfo = new Hashtable();
                }
                var uniqueId = /*UUID.randomUUID()*/ Guid.NewGuid().ToString();
                _connectionPointInfo[uniqueId] = new object[] { connectionPoint, cookie };
                return uniqueId;
            }
        }

        /// <inheritdoc/>
        public object[] Internal_getConnectionInfo(string identifier) {
            lock (this) {
                CheckLocal();
                return (object[])_connectionPointInfo[identifier];
            }
        }

        /// <inheritdoc/>
        public object[] Internal_removeConnectionInfo(string identifier) {
            lock (this) {
                CheckLocal();
                var result = _connectionPointInfo[identifier];
                _connectionPointInfo.Remove(identifier);
                return (object[])result;
            }
        }

        /// <inheritdoc/>
        public void Internal_setDeffered(bool deffered) => _ptr.Deffered = deffered;

        /// <inheritdoc/>
        public override string ToString() => "IJIComObject[" + Internal_getInterfacePointer() + ", session: " +
                AssociatedSession.SessionIdentifier + ", isLocal: " + LocalReference + "]";

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JIComObjectImpl other)) {
                return false;
            }
            return _ptr.IPID.Equals(other.Ipid, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Ipid.GetHashCode();

        /// <summary>
        /// Replace members
        /// </summary>
        /// <param name="comObject"></param>
        internal void ReplaceMembers(IComObject comObject) {
            _session = comObject.AssociatedSession;
            _ptr = comObject.Internal_getInterfacePointer();
        }

        /// <summary>
        /// Check local
        /// </summary>
        private void CheckLocal() {
            if (_session == null) {
                throw new InvalidOperationException(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_SESSION_NOT_ATTACHED));
            }
            if (LocalReference) {
                throw new InvalidOperationException(
                    JISystem.GetLocalizedMessage(JIErrorCodes.E_NOTIMPL));
            }
        }

        private bool _isDual;
        private bool _dualInfo;
        [NonSerialized] private JISession _session;
        private JIInterfacePointer _ptr;
        private Hashtable _connectionPointInfo;
        private int _timeout;
    }
}
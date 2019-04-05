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
    using System;
    using System.Collections;

    /// <summary>
    /// Implementation for IJIComObject. 
    /// There is a 1 to 1 mapping between this and a <code>COM</code> interface. 
    /// </summary>
    [Serializable]
    internal sealed class JIComObjectImpl : IJIComObject {

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

        /// <summary>
        /// Replace members
        /// </summary>
        /// <param name="comObject"></param>
        internal void replaceMembers(IJIComObject comObject) {
            _session = comObject.AssociatedSession;
            _ptr = comObject.internal_getInterfacePointer();
        }

        /// <summary>
        /// Check local
        /// </summary>
        private void checkLocal() {
            if (_session == null) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_SESSION_NOT_ATTACHED));
            }
            if (LocalReference) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.E_NOTIMPL));
            }
        }

        /// <summary>
        /// Query interface 
        /// </summary>
        /// <param name="iid"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public IJIComObject queryInterface(string iid) {
            checkLocal();
            return _session.Stub.getInterface(iid, _ptr.IPID);
        }

        /// <summary>
        /// Add reference 
        /// </summary>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public void addRef() {
            checkLocal();
            var obj = new JICallBuilder(true) {
                ParentIpid = _ptr.IPID,
                Opnum = 1 //addRef
            };

            //length
            obj.addInParamAsShort((short)1, JIFlags.FLAG_NULL);
            //ipid to addfref on
            var array = new JIArray(new UUID[] { new UUID(_ptr.IPID) }, true);
            obj.addInParamAsArray(array, JIFlags.FLAG_NULL);
            //TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.addInParamAsInt(5, JIFlags.FLAG_NULL);
            obj.addInParamAsInt(0, JIFlags.FLAG_NULL); //private refs = 0

            obj.addOutParamAsType(typeof(short?), JIFlags.FLAG_NULL); //size
            obj.addOutParamAsType(typeof(int?), JIFlags.FLAG_NULL); //Hresult for size
            Log.Logger.Warning("addRef: Adding 5 references for " + _ptr.IPID + " session: " + _session.SessionIdentifier);

            JISession.debug_addIpids(_ptr.IPID, 5);

            //		session.getStub2().addRef_ReleaseRef(obj);
            _session.addRef_ReleaseRef(_ptr.IPID, obj, 5);

            if (obj.getResultAsIntAt(1) != 0) {
                throw new JIException(obj.getResultAsIntAt(1), (Exception)null);
            }
        }

        /// <summary>
        /// release 
        /// </summary>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public void release() {
            checkLocal();
            var obj = new JICallBuilder(true) {
                ParentIpid = _ptr.IPID,
                Opnum = 2 //release
            };
            //length
            obj.addInParamAsShort((short)1, JIFlags.FLAG_NULL);
            //ipid to addfref on
            var array = new JIArray(new UUID[] { new UUID(_ptr.IPID) }, true);
            obj.addInParamAsArray(array, JIFlags.FLAG_NULL);
            //TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.addInParamAsInt(5, JIFlags.FLAG_NULL);
            obj.addInParamAsInt(0, JIFlags.FLAG_NULL); //private refs = 0
            if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                Log.Logger.Warning("RELEASE called directly ! removing 5 references for " + _ptr.IPID + " session: " + _session.SessionIdentifier);
                JISession.debug_delIpids(_ptr.IPID, 5);
            }
            // TODO??
            //		session.getStub2().addRef_ReleaseRef(obj);
            _session.addRef_ReleaseRef(_ptr.IPID, obj, -5);
        }

        /// <summary>
        /// Call 
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public object[] call(JICallBuilder obj) {
            checkLocal();
            return call(obj, _timeout);
        }

        /// <summary>
        /// Get internal interface pointer
        /// </summary>
        /// <returns></returns>
        public JIInterfacePointer internal_getInterfacePointer() {
            return _ptr ?? _session.Stub.ServerInterfacePointer;
        }

        /// <summary>
        /// Ip id
        /// </summary>
        public string Ipid => _ptr.IPID;

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (!(obj is JIComObjectImpl other)) {
                return false;
            }
            return _ptr.IPID.Equals(other.Ipid, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return Ipid.GetHashCode();
        }

        /// <summary>
        /// Session
        /// </summary>
        public JISession AssociatedSession => _session;

        /// <summary>
        /// Interface id
        /// </summary>
        public string InterfaceIdentifier => _ptr.IID;

        /// <summary>
        /// Dispatch is supported
        /// </summary>
        public bool DispatchSupported {
            get {
                lock (this) {
                    checkLocal();
                    if (!_dualInfo) {
                        //query interface for it and then release it.
                        try {
                            var comObject = queryInterface("00020400-0000-0000-c000-000000000046");
                            comObject.release();
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

        /// <summary>
        /// Update connection info
        /// </summary>
        /// <param name="connectionPoint"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public string internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie) {
            lock (this) {
                checkLocal();
                if (_connectionPointInfo == null) //lazy creation, since this is used by event callbacks only.
                {
                    _connectionPointInfo = new Hashtable();
                }
                var uniqueId = /*UUID.randomUUID()*/ Guid.NewGuid().ToString();
                _connectionPointInfo[uniqueId] = new object[] { connectionPoint, cookie };
                return uniqueId;
            }
        }

        /// <summary>
        /// Get connection info
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public object[] internal_getConnectionInfo(string identifier) {
            lock (this) {
                checkLocal();
                return (object[])_connectionPointInfo[identifier];
            }
        }

        /// <summary>
        /// Remove connection info
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public object[] internal_removeConnectionInfo(string identifier) {
            lock (this) {
                checkLocal();
                var result = _connectionPointInfo[identifier];
                _connectionPointInfo.Remove(identifier);
                return (object[])result;
            }
        }

        public IJIUnreferenced UnreferencedHandler {
            get {
                checkLocal();
                return _session.getUnreferencedHandler(Ipid);
            }
        }

        public void registerUnreferencedHandler(IJIUnreferenced unreferenced) {
            checkLocal();
            _session.registerUnreferencedHandler(Ipid, unreferenced);
        }

        public void unregisterUnreferencedHandler() {
            checkLocal();
            _session.unregisterUnreferencedHandler(Ipid);
        }

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="socketTimeout"></param>
        /// <exception cref="JIException"></exception>
        /// <returns></returns>
        public object[] call(JICallBuilder obj, int socketTimeout) {
            checkLocal();
            obj.attachSession(_session);
            obj.ParentIpid = _ptr.IPID;
            // Call is always made on your stub.
            if (socketTimeout != 0) //using instance level timeout
            {
                return _session.Stub.call(obj, _ptr.IID, socketTimeout);
            }
            return _session.Stub.call(obj, _ptr.IID);
        }

        /// <summary>
        /// Timeout
        /// </summary>
        public int InstanceLevelSocketTimeout {
            get {
                checkLocal();
                return _timeout;
            }
            set {
                checkLocal();
                _timeout = value;
            }
        }

        /// <summary>
        /// Set deferred
        /// </summary>
        /// <param name="deffered"></param>
        public void internal_setDeffered(bool deffered) {
            _ptr.Deffered = deffered;
        }

        /// <summary>
        /// Local reference
        /// </summary>
        public bool LocalReference { get; }

        /// <summary>
        /// Custom object
        /// </summary>
        public JIComCustomMarshallerUnMarshaller CustomObject { get; set; } = null;

        /// <summary>
        /// Length of pointer
        /// </summary>
        public int LengthOfInterfacePointer => _ptr.Length;

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
        public override string ToString() {
            return "IJIComObject[" + internal_getInterfacePointer() + " , session: " +
                AssociatedSession.SessionIdentifier + ", isLocal: " + LocalReference + "]";
        }

        private bool _isDual;
        private bool _dualInfo;
        [NonSerialized] private JISession _session;
        private JIInterfacePointer _ptr;
        private IDictionary _connectionPointInfo;
        private int _timeout;
    }
}
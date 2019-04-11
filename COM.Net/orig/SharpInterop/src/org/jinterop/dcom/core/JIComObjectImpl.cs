using System;
using System.Collections;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {


    using IJIUnreferenced = org.jinterop.dcom.common.IJIUnreferenced;
    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;

    //import com.iwombat.foundation.IdentifierFactory;
    //import com.iwombat.util.GUIDUtil;





    /// <summary>
    /// Implementation for IJIComObject. There is a 1 to 1 mapping between this and a <code>COM</code> interface. 
    /// 
    /// @exclude
    /// @since 1.0
    /// </summary>
    [Serializable]
    internal sealed class JIComObjectImpl : IJIComObject {

        /// 
        private const long SerialVersionUID = -1661750453596032089L;
        private bool IsDual_Renamed = false;
        private bool DualInfo = false;
        [NonSerialized]
        private JISession Session = null;
        private JIInterfacePointer Ptr = null;
        private IDictionary ConnectionPointInfo = null;
        private int Timeout = 0;
        private readonly bool IsLocal;

        private JIComCustomMarshallerUnMarshaller CustomObject_Renamed = null;

        public JIComObjectImpl(JISession session, JIInterfacePointer ptr) : this(session,ptr,false) {
        }

        public JIComObjectImpl(JISession session, JIInterfacePointer ptr, bool isLocal) {
            this.Session = session;
            this.Ptr = ptr;
            this.IsLocal = isLocal;
        }

        public void ReplaceMembers(IJIComObject comObject) {
            this.Session = comObject.AssociatedSession;
            this.Ptr = comObject.Internal_getInterfacePointer();
        }

        private void CheckLocal() {
            if (Session == null) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_SESSION_NOT_ATTACHED));
            }

            if (LocalReference) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.E_NOTIMPL));
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIComObject queryInterface(String iid) throws org.jinterop.dcom.common.JIException
        public IJIComObject QueryInterface(string iid) {
            CheckLocal();
            return Session.Stub.GetInterface(iid,Ptr.IPID);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void addRef() throws org.jinterop.dcom.common.JIException
        public void AddRef() {
            CheckLocal();
            JICallBuilder obj = new JICallBuilder(true);
            obj.ParentIpid = Ptr.IPID;
            obj.Opnum = 1; //addRef

            //length
            obj.AddInParamAsShort((short)1,JIFlags.FLAG_NULL);
            //ipid to addfref on
            JIArray array = new JIArray(new rpc.core.UUID[]{ new rpc.core.UUID(Ptr.IPID) },true);
            obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
            //TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.AddInParamAsInt(5,JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0,JIFlags.FLAG_NULL); //private refs = 0

            obj.AddOutParamAsType(typeof(short?),JIFlags.FLAG_NULL); //size
            obj.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL); //Hresult for size
            if (JISystem.Logger.isLoggable(Level.INFO)) {
                JISystem.Logger.warning("addRef: Adding 5 references for " + Ptr.IPID + " session: " + Session.SessionIdentifier);
            }

            JISession.Debug_addIpids(Ptr.IPID, 5);

    //        session.getStub2().addRef_ReleaseRef(obj);
            Session.AddRef_ReleaseRef(Ptr.IPID, obj, 5);

            if (obj.GetResultAsIntAt(1) != 0) {
                throw new JIException(obj.GetResultAsIntAt(1),(Exception)null);
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void release() throws org.jinterop.dcom.common.JIException
        public void Release() {
            CheckLocal();
            JICallBuilder obj = new JICallBuilder(true);
            obj.ParentIpid = Ptr.IPID;
            obj.Opnum = 2; //release
            //length
            obj.AddInParamAsShort((short)1,JIFlags.FLAG_NULL);
            //ipid to addfref on
            JIArray array = new JIArray(new rpc.core.UUID[]{ new rpc.core.UUID(Ptr.IPID) },true);
            obj.AddInParamAsArray(array,JIFlags.FLAG_NULL);
            //TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
            // same with release.
            obj.AddInParamAsInt(5,JIFlags.FLAG_NULL);
            obj.AddInParamAsInt(0,JIFlags.FLAG_NULL); //private refs = 0
            if (JISystem.Logger.isLoggable(Level.INFO)) {
                JISystem.Logger.warning("RELEASE called directly ! removing 5 references for " + Ptr.IPID + " session: " + Session.SessionIdentifier);
                JISession.Debug_delIpids(Ptr.IPID, 5);
            }
    //        session.getStub2().addRef_ReleaseRef(obj);
            Session.AddRef_ReleaseRef(Ptr.IPID, obj, -5);
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj) throws org.jinterop.dcom.common.JIException
        public object[] Call(JICallBuilder obj) {
            CheckLocal();
            return Call(obj,Timeout);
        }




        public JIInterfacePointer Internal_getInterfacePointer() {
            return Ptr == null ? Session.Stub.ServerInterfacePointer : Ptr;
        }

        public string Ipid {
            get {
                return Ptr.IPID;
            }
        }

        public override bool Equals(object obj) {

            if (!(obj is JIComObjectImpl)) {
                return false;
            }

            return (this.Ptr.IPID.Equals(((IJIComObject)obj).Ipid, StringComparison.CurrentCultureIgnoreCase));
        }

        public override int GetHashCode() {
            return Ptr.IPID.GetHashCode();
        }

        public JISession AssociatedSession {
            get {
                return Session;
            }
        }

        public string InterfaceIdentifier {
            get {
                return Ptr.IID;
            }
        }

    //    public JIComServer getAssociatedComServer()
    //    {
    //        checkLocal();
    //        return session.getStub();
    //    }

        public bool DispatchSupported {
            get {
                lock (this) {
                    CheckLocal();
                    if (!DualInfo) {
                        //query interface for it and then release it.
                        try {
                            IJIComObject comObject = QueryInterface("00020400-0000-0000-c000-000000000046");
                            comObject.Release();
                            IsDual = true;
                        }
                        catch (JIException) {
                            IsDual = false;
                        }
                    }
                    return IsDual_Renamed;
                }
            }
        }

        public string Internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie) {
            lock (this) {
                CheckLocal();
                if (ConnectionPointInfo == null) { //lazy creation, since this is used by event callbacks only.
                    ConnectionPointInfo = new Hashtable();
                }
        
        //        String uniqueId = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
                string uniqueId = UUID.randomUUID().ToString();
                ConnectionPointInfo[uniqueId] = new object[]{ connectionPoint,cookie };
                return uniqueId;
            }
        }

        public object[] Internal_getConnectionInfo(string identifier) {
            lock (this) {
                CheckLocal();
                return (object[])ConnectionPointInfo.GetValueOrNull(identifier);
            }
        }

        public object[] Internal_removeConnectionInfo(string identifier) {
            lock (this) {
                CheckLocal();
                return (object[])ConnectionPointInfo.Remove(identifier);
            }
        }

        public IJIUnreferenced UnreferencedHandler {
            get {
                CheckLocal();
                return Session.GetUnreferencedHandler(Ipid);
            }
        }

        public void RegisterUnreferencedHandler(IJIUnreferenced unreferenced) {
            CheckLocal();
            Session.RegisterUnreferencedHandler(Ipid, unreferenced);
        }

        public void UnregisterUnreferencedHandler() {
            CheckLocal();
            Session.UnregisterUnreferencedHandler(Ipid);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] call(JICallBuilder obj, int socketTimeout) throws org.jinterop.dcom.common.JIException
        public object[] Call(JICallBuilder obj, int socketTimeout) {
            CheckLocal();
            obj.AttachSession(Session);
            obj.ParentIpid = Ptr.IPID;
            //Call is always made on your stub.

            if (socketTimeout != 0) { //using instance level timeout
                return Session.Stub.Call(obj,Ptr.IID,socketTimeout);
            }
            else {
                return Session.Stub.Call(obj,Ptr.IID);
            }
        }

        public int InstanceLevelSocketTimeout {
            get {
                CheckLocal();
                return Timeout;
            }
            set {
                CheckLocal();
                this.Timeout = value;
            }
        }


        public void Internal_setDeffered(bool deffered) {
            Ptr.Deffered = deffered;
        }

        public bool LocalReference {
            get {
                return IsLocal;
            }
        }

        public bool IsDual {
            set {
                this.DualInfo = true;
                this.IsDual_Renamed = value;
            }
        }

        public override string ToString() {
            return "IJIComObject[" + Internal_getInterfacePointer() + " , session: " + AssociatedSession.SessionIdentifier + ", isLocal: " + LocalReference + "]";
        }

        public JIComCustomMarshallerUnMarshaller CustomObject {
            get {
                return CustomObject_Renamed;
            }
            set {
                this.CustomObject_Renamed = value;
            }
        }


        public int LengthOfInterfacePointer {
            get {
                return Ptr.Length;
            }
        }
    }

}
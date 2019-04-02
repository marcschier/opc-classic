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
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System;
    using System.Collections;

    internal sealed class JIRemActivation : NdrObject, JIIServerActivation {

        /// <summary>
        /// That
        /// </summary>
        public JIOrpcThat ORPCThat { get; private set; }

        /// <summary>
        /// Oxid
        /// </summary>
        public sbyte[] Oxid { get; private set; }

        /// <summary>
        /// Authentication hint
        /// </summary>
        public int AuthenticationHint { get; private set; } = -1;

        /// <summary>
        /// Com version
        /// </summary>
        public JIComVersion ComVersion { get; private set; }

        /// <summary>
        /// Activation result
        /// </summary>
        public int Hresult => _hresult;

        /// <summary>
        /// Mode
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// Client impersonation level
        /// </summary>
        public int ClientImpersonationLevel { get; set; }

        /// <summary>
        /// Create activation
        /// </summary>
        /// <param name="clsid"></param>
		public JIRemActivation(string clsid) {
            ClientImpersonationLevel = JIIServerActivation_Fields.RPC_C_IMP_LEVEL_IMPERSONATE;
            // 10000002-0000-0000-0000-000000000001 Inside DCOM
            _clsid = new UUID(clsid);
        }

        /// <summary>
        /// Set file moniker
        /// </summary>
        /// <param name="name"></param>
        public void setfileMonikerAtServer(string name) {
            if (name != null && !name.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                _monikerName = name;
            }
        }

        /// <inheritdoc/>
        public override int Opnum => 0;

        /// <inheritdoc/>
        public override void write(NetworkDataRepresentation ndr) {
            var orpcThis = new JIOrpcThis();
            orpcThis.encode(ndr);

            //JIClsid of the component being activated.
            var uuid = new UUID();
            uuid.parse(_clsid.ToString());
            try {
                uuid.encode(ndr, ndr.buf);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation write", e);
            }
            if (_monikerName == null) {
                ndr.writeUnsignedLong(0);
            }
            else {
                ndr.writeCharacterArray(_monikerName.ToCharArray(), 0, _monikerName.Length); // Object Name
            }

            ndr.writeUnsignedLong(0); // Minterface pointer
            ndr.writeUnsignedLong(ClientImpersonationLevel); // impersonation level
            ndr.writeUnsignedLong(Mode); //mode, when object name , interface pointer are not null , this is passed directly to IPersistFile:Load
            ndr.writeUnsignedLong(2); //No. of IIDs requested.
            ndr.writeUnsignedLong(new object().GetHashCode());
            ndr.writeUnsignedLong(2); //Array length

            //IID of IUnknown , this is hard coded here, standard way of COM is to first get a handle to the IUnknown
            uuid.parse("00000000-0000-0000-c000-000000000046");
            try {
                uuid.encode(ndr, ndr.buf);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivatio write");
            }

            //checking for IDispatch support
            uuid.parse("00020400-0000-0000-c000-000000000046");
            try {
                uuid.encode(ndr, ndr.buf);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation write");
            }

            ndr.writeUnsignedLong(1); //Protocol Sequences available
            ndr.writeUnsignedLong(1); //Array length
            ndr.writeUnsignedShort(7); //TCP
            var address = JISession.LocalhostAddressAsIPbytes;
            ndr.writeUnsignedShort(address[0]);
            ndr.writeUnsignedShort(address[1]);
            ndr.writeUnsignedShort(address[2]);
            ndr.writeUnsignedShort(address[3]);
            ndr.writeUnsignedShort(0);
        }

        /// <inheritdoc/>
        public override void read(NetworkDataRepresentation ndr) {

            //first take out JIOrpcThat
            ORPCThat = JIOrpcThat.decode(ndr);

            //now fill the oxid
            Oxid = JIMarshalUnMarshalHelper.readOctetArrayLE(ndr, 8);

            var skipdual = ndr.readUnsignedLong();

            if (skipdual != 0) {
                ndr.readUnsignedLong();
                //now fill the dual string array for oxid bindings, the call to IRemUnknown will be
                //directed to this address and the port in that address.
                DualStringArrayForOxid = JIDualStringArray.decode(ndr);
            }

            //get the IPID which will be the "Object" in the call to IRemUknown. This is the IPID of the
            //component which has been specified as the JIClsid. This may differ in multiple invokations of
            //of remote activation as everytime a new object may be created at the server per call. This is all
            //server implementation dependent.
            try {
                var ipid2 = new UUID();
                ipid2.decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation read");
            }

            //read the auth hint
            AuthenticationHint = ndr.readUnsignedLong();

            ComVersion = new JIComVersion {
                MajorVersion = ndr.readUnsignedShort(),
                MinorVersion = ndr.readUnsignedShort()
            };

            _hresult = ndr.readUnsignedLong();

            if (_hresult != 0) {
                //System.out.println("EXCEPTION FROM SERVER ! --> " + "0x" + Long.toHexString(hresult).substring(8));
                throw new JIRuntimeException(_hresult);
            }

            //int numRet = ndr.readUnsignedLong();//Number of interface pointers returned. Currently only 2.

            var array = new JIArray(typeof(JIInterfacePointer), null, 1, true);
            var listOfDefferedPointers = new ArrayList();
            array = (JIArray)JIMarshalUnMarshalHelper.deSerialize(ndr, array, listOfDefferedPointers, JIFlags.FLAG_NULL, new Hashtable());
            var x = 0;

            while (x < listOfDefferedPointers.Count) {

                var newList = new ArrayList();
                var replacement = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, null);
                ((JIPointer)listOfDefferedPointers[x]).replaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
                x++;
                listOfDefferedPointers.AddRange(x, newList);
            }
            var arrayObjs = (JIInterfacePointer[])array.ArrayInstance;
            MInterfacePointer = arrayObjs[0];

            if (arrayObjs[1] != null) {
                //dual is supported since the IDispatch was obtained
                _isDual = true;
                //eat this keeping only the IPID for cleanup , let the user perform another queryInterface for this.
                var ptr = arrayObjs[1];
                _dispIpid = ptr.IPID;
                _dispOid = ptr.OID;
                _dispRefs = ((JIStdObjRef)ptr.getObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
            }

            array = new JIArray(typeof(int?), null, 1, true);
            //ignore the retvals
            JIMarshalUnMarshalHelper.deSerialize(ndr, array, null, JIFlags.FLAG_NULL, null);

            ActivationSuccessful = true;

        }

        /// <inheritdoc/>
        public bool ActivationSuccessful { get; private set; }

        /// <inheritdoc/>
        public JIDualStringArray DualStringArrayForOxid { get; private set; }
        /// <inheritdoc/>
        public JIInterfacePointer MInterfacePointer { get; private set; }

        /// <inheritdoc/>
        public string IPID { get; private set; }

        /// <inheritdoc/>
        public bool Dual => _isDual;

        /// <inheritdoc/>
        public string DispIpid {
            get => _dispIpid;
            set => _dispIpid = value;
        }

        /// <inheritdoc/>
        public int DispRefs => _dispRefs;

        private string _monikerName;
        private UUID _clsid;
        private int _hresult = -1;
        internal bool _isDual;
        internal string _dispIpid;
        internal int _dispRefs = 5;
        internal sbyte[] _dispOid;
    }
}
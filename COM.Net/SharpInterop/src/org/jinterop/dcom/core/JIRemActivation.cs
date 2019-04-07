//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using org.jinterop.dcom.common;
    using rpc.core;
    using Serilog;
    using System;
    using System.Collections.Generic;

    internal sealed class JIRemActivation : NdrOp, JIIServerActivation {

        /// <summary>
        /// That
        /// </summary>
        public JIOrpcThat ORPCThat { get; private set; }

        /// <summary>
        /// Oxid
        /// </summary>
        public byte[] Oxid { get; private set; }

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
        public void SetfileMonikerAtServer(string name) {
            if (name != null && !name.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                _monikerName = name;
            }
        }

        /// <inheritdoc/>
        public override int Opnum => 0;

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            var orpcThis = new JIOrpcThis();
            orpcThis.encode(ndr);

            //JIClsid of the component being activated.
            var uuid = new UUID();
            uuid.Parse(_clsid.ToString());
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation write", e);
            }
            if (_monikerName == null) {
                ndr.WriteUnsignedLong(0);
            }
            else {
                ndr.WriteCharacterArray(_monikerName.ToCharArray(), 0, _monikerName.Length); // Object Name
            }

            ndr.WriteUnsignedLong(0); // Minterface pointer
            ndr.WriteUnsignedLong(ClientImpersonationLevel); // impersonation level
            ndr.WriteUnsignedLong(Mode); //mode, when object name , interface pointer are not null , this is passed directly to IPersistFile:Load
            ndr.WriteUnsignedLong(2); //No. of IIDs requested.
            ndr.WriteUnsignedLong(new object().GetHashCode());
            ndr.WriteUnsignedLong(2); //Array length

            //IID of IUnknown , this is hard coded here, standard way of COM is to first get a handle to the IUnknown
            uuid.Parse("00000000-0000-0000-c000-000000000046");
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivatio write");
            }

            //checking for IDispatch support
            uuid.Parse("00020400-0000-0000-c000-000000000046");
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation write");
            }

            ndr.WriteUnsignedLong(1); //Protocol Sequences available
            ndr.WriteUnsignedLong(1); //Array length
            ndr.WriteUnsignedShort(7); //TCP
            var address = JISession.LocalhostAddressAsIPbytes;
            ndr.WriteUnsignedShort(address[0]);
            ndr.WriteUnsignedShort(address[1]);
            ndr.WriteUnsignedShort(address[2]);
            ndr.WriteUnsignedShort(address[3]);
            ndr.WriteUnsignedShort(0);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {

            //first take out JIOrpcThat
            ORPCThat = JIOrpcThat.decode(ndr);

            //now fill the oxid
            Oxid = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8);

            var skipdual = ndr.ReadUnsignedLong();

            if (skipdual != 0) {
                ndr.ReadUnsignedLong();
                //now fill the dual string array for oxid bindings, the call to IRemUnknown will be
                //directed to this address and the port in that address.
                DualStringArrayForOxid = JIDualStringArray.Decode(ndr);
            }

            //get the IPID which will be the "Object" in the call to IRemUknown. This is the IPID of the
            //component which has been specified as the JIClsid. This may differ in multiple invokations of
            //of remote activation as everytime a new object may be created at the server per call. This is all
            //server implementation dependent.
            try {
                var ipid2 = new UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIRemActivation read");
            }

            //read the auth hint
            AuthenticationHint = ndr.ReadUnsignedLong();

            ComVersion = new JIComVersion {
                MajorVersion = ndr.ReadUnsignedShort(),
                MinorVersion = ndr.ReadUnsignedShort()
            };

            _hresult = ndr.ReadUnsignedLong();

            if (_hresult != 0) {
                //System.out.println("EXCEPTION FROM SERVER ! --> " + "0x" + Long.toHexString(hresult).substring(8));
                throw new JIRuntimeException(_hresult);
            }

            //int numRet = ndr.readUnsignedLong();//Number of interface pointers returned. Currently only 2.

            var array = new JIArray(typeof(JIInterfacePointer), null, 1, true);
            var listOfDefferedPointers = new List<object>();
            array = (JIArray)JIMarshalUnMarshalHelper.Deserialize(ndr, array,
                listOfDefferedPointers, JIFlags.FLAG_NULL, new Hashtable());
            var x = 0;

            while (x < listOfDefferedPointers.Count) {

                var newList = new List<object>();
                var replacement = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, null);
                ((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
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
                _dispRefs = ((JIStdObjRef)ptr.GetObjectReference(JIInterfacePointer.OBJREF_STANDARD)).PublicRefs;
            }

            array = new JIArray(typeof(int?), null, 1, true);
            //ignore the retvals
            JIMarshalUnMarshalHelper.Deserialize(ndr, array, null, JIFlags.FLAG_NULL, null);

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
        internal byte[] _dispOid;
    }
}
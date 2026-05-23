//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using Opc.Classic.Dcom.Internal.LegacyNdr;
    using SharpCifs.Util.Sharpen;
    using SharpInterop.Common;
    using SharpInterop.Rpc.Core;
    using Opc.Classic.Dcom.Internal;
    using System;
    using System.Collections.Generic;
    using SharpInterop.Rpc;

    internal sealed class RemActivation : NdrOp, IServerActivation {

        /// <summary>
        /// That
        /// </summary>
        public OrpcThat ORPCThat { get; private set; }

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
        public ComVersion ComVersion { get; private set; }

        /// <summary>
        /// Activation result
        /// </summary>
        public int Hresult { get; private set; } = -1;

        /// <summary>
        /// Mode
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// Client impersonation level
        /// </summary>
        public RpcImpersonationLevel ClientImpersonationLevel { get; set; }

        /// <summary>
        /// Create activation
        /// </summary>
        /// <param name="clsid"></param>
        public RemActivation(string clsid) {
            ClientImpersonationLevel = RpcImpersonationLevel.RPC_C_IMP_LEVEL_IMPERSONATE;
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
            var orpcThis = new OrpcThis();
            orpcThis.Encode(ndr);

            // Clsid of the component being activated.
            var uuid = new UUID();
            uuid.Parse(_clsid.ToString());
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "RemActivation write");
            }
            if (_monikerName == null) {
                ndr.WriteUnsignedLong(0);
            }
            else {
                ndr.WriteCharacterArray(_monikerName.ToCharArray(), 0,
                    _monikerName.Length); // Object Name
            }

            ndr.WriteUnsignedLong(0); // Minterface pointer
            ndr.WriteUnsignedLong((int)ClientImpersonationLevel); // impersonation level
            ndr.WriteUnsignedLong(Mode); // mode, when object name, interface pointer are not null, this is passed directly to IPersistFile:Load
            ndr.WriteUnsignedLong(2); // No. of IIDs requested.
            ndr.WriteUnsignedLong(new object().GetHashCode());
            ndr.WriteUnsignedLong(2); // Array length

            // IID of IUnknown, this is hard coded here, standard way of COM 
            // is to first get a handle to the IUnknown
            uuid.Parse(Interfaces.IID_IUnknown);
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "RemActivation write");
            }

            // checking for IDispatch support
            uuid.Parse(Interfaces.IID_IDispatch);
            try {
                uuid.Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "RemActivation write");
            }

            ndr.WriteUnsignedLong(1); // Protocol Sequences available
            ndr.WriteUnsignedLong(1); // Array length
            ndr.WriteUnsignedShort(7); // TCP
            var address = Session.LocalhostAddressAsIPbytes;
            ndr.WriteUnsignedShort(address[0]);
            ndr.WriteUnsignedShort(address[1]);
            ndr.WriteUnsignedShort(address[2]);
            ndr.WriteUnsignedShort(address[3]);
            ndr.WriteUnsignedShort(0);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {

            // first take out OrpcThat
            ORPCThat = OrpcThat.Decode(ndr);

            // now fill the oxid
            Oxid = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8);

            var skipdual = ndr.ReadUnsignedLong();

            if (skipdual != 0) {
                ndr.ReadUnsignedLong();
                // now fill the dual string array for oxid bindings, the call to IRemUnknown will be
                // directed to this address and the port in that address.
                DualStringArrayForOxid = DualStringArray.Decode(ndr);
            }

            // get the IPID which will be the "Object" in the call to IRemUknown. This is the IPID of the
            // component which has been specified as the Clsid. This may differ in multiple invokations of
            // of remote activation as everytime a new object may be created at the server per call. This is all
            // server implementation dependent.
            try {
                var ipid2 = new UUID();
                ipid2.Decode(ndr, ndr.Buffer);
                IPID = ipid2.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "RemActivation read");
            }

            // read the auth hint
            AuthenticationHint = ndr.ReadUnsignedLong();

            ComVersion = new ComVersion {
                MajorVersion = ndr.ReadUnsignedShort(),
                MinorVersion = ndr.ReadUnsignedShort()
            };

            Hresult = ndr.ReadUnsignedLong();
            if (Hresult != 0) {
                throw new InteropRuntimeException(Hresult);
            }

            var array = new ComArray(typeof(InterfacePointer), null, 1, true);

            var context = new CodecContext();
            array = (ComArray)MarshalUnMarshalHelper.Deserialize(ndr, array, context);
            context.DecodeDeferredPointers(ndr);

            var arrayObjs = (InterfacePointer[])array.ArrayInstance;
            MInterfacePointer = arrayObjs[0];

            if (arrayObjs[1] != null) {
                // dual is supported since the IDispatch was obtained
                _isDual = true;
                // eat this keeping only the IPID for cleanup, let the user perform another queryInterface for this.
                var ptr = arrayObjs[1];
                _dispIpid = ptr.IPID;
                _dispOid = ptr.OID;
                _dispRefs = ((StdObjRef)ptr.GetObjectReference(InterfacePointer.OBJREF_STANDARD)).PublicRefs;
            }

            array = new ComArray(typeof(int), null, 1, true);
            // ignore the retvals
            MarshalUnMarshalHelper.Deserialize(ndr, array);

            ActivationSuccessful = true;

        }

        /// <inheritdoc/>
        public bool ActivationSuccessful { get; private set; }

        /// <inheritdoc/>
        public DualStringArray DualStringArrayForOxid { get; private set; }
        /// <inheritdoc/>
        public InterfacePointer MInterfacePointer { get; private set; }

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
        private readonly UUID _clsid;
        internal bool _isDual;
        internal string _dispIpid;
        internal int _dispRefs = 5;
        internal byte[] _dispOid;
    }
}
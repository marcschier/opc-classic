using System.Collections;
using System.Collections.Generic;

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


    using NdrException = ndr.NdrException;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
    using JISystem = org.jinterop.dcom.common.JISystem;

    using UUID = rpc.core.UUID;


    internal sealed class JIRemUnknown : NdrObject {

        public const string IID_IUnknown = "00000143-0000-0000-c000-000000000046";
    //    public static final String IID_IDispatch = "00020400-0000-0000-c000-000000000046";
        private string IpidOfIUnknown = null;
        private string RequestedIID = null;
        private JIInterfacePointer IidPtr = null;


        public JIRemUnknown(string ipidOfIUnknown, string requestedIID) {
            this.IpidOfIUnknown = ipidOfIUnknown;
            this.RequestedIID = requestedIID;
        }

        public int Opnum {
            get {
                //opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
                //3,4,5 by IRemUnknown and we are going to call IRemUnknown2.QI so that we get MIPs.
                return 6;
            }
        }

        public void Write(NetworkDataRepresentation ndr) {


            JIOrpcThis orpcthis = new JIOrpcThis();
            orpcthis.Encode(ndr);

            //now write the IPID
            UUID uuid = new UUID(IpidOfIUnknown);
            try {
                uuid.encode(ndr,ndr.buf);
            }
            catch (NdrException e) {

                JISystem.Logger.throwing("JIRemUnknown","write",e);
            }

            ndr.writeUnsignedShort(1); //1 interfaces. (requested IID)
            ndr.writeUnsignedShort(0); //byte alignment
            ndr.writeUnsignedLong(1); //length of the array
            uuid = new UUID(RequestedIID);
            try {
                uuid.encode(ndr,ndr.buf);
            }
            catch (NdrException e) {

                JISystem.Logger.throwing("JIRemUnknown","Performing a QueryInterface for " + RequestedIID,e);
            }

            ndr.writeUnsignedLong(0); //TODO Index Matching , there seems to be a bug in
                                        // the jarapac system, it only reads upto (length - 6) bytes and one has to have another
                                        // call after that or incomplete request will go. in case no param is present just put an unsigned long = 0.
        }

        public void Read(NetworkDataRepresentation ndr) {
            JIOrpcThat.Decode(ndr);
            ndr.readUnsignedLong(); //size will be one
            int hresult1 = ndr.readUnsignedLong();
            if (hresult1 != 0) {
                //something happened.
                throw new JIRuntimeException(hresult1);
            }

            //array length
            ndr.readUnsignedLong();

            //and now the JIInterfacePointer itself.
            IidPtr = JIInterfacePointer.Decode(ndr, new List<object>(), JIFlags.FLAG_NULL, new Hashtable());
            //final hresult
            hresult1 = ndr.readUnsignedLong();
            if (hresult1 != 0) {
                //something happened.
                throw new JIRuntimeException(hresult1);
            }
        }




        public JIInterfacePointer InterfacePointer {
            get {
                return IidPtr;
            }
        }


    }

}
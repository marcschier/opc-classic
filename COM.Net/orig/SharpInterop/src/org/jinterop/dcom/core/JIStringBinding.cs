using System;
using System.Text;

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

    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    [Serializable]
    internal sealed class JIStringBinding {


        private const long SerialVersionUID = -5797400235890434880L;

        private JIStringBinding() {
        }

        private int TowerId_Renamed = -1;

        //IP or resolved name follwed by port in []
        private string NetworkAddress_Renamed = null;

        private int Length_Renamed = -1;

        public int Length {
            get {
                return Length_Renamed;
            }
        }

        //private static boolean test = false;
        public JIStringBinding(int port, bool hostname) {
            string hostaddress = null;
            if (!hostname) {
                //single binding with our IP address
                hostaddress = JISession.LocalhostAddressAsIPString;
            }
            else {
                hostaddress = JISession.LocalhostCanonicalAddressAsString;
            }

            if (port == -1) {
                NetworkAddress_Renamed = hostaddress;
            }
            else {
                NetworkAddress_Renamed = hostaddress + "[" + Convert.ToString(port) + "]";
            }

            Length_Renamed = 2 + NetworkAddress_Renamed.Length * 2 + 2;
            TowerId_Renamed = 0x7; //TCP_IP
        }

        public JIStringBinding(int port) : this(port,false) {
        }

        internal static JIStringBinding Decode(NetworkDataRepresentation ndr) {
            JIStringBinding stringBinding = new JIStringBinding();

            stringBinding.TowerId_Renamed = ndr.readUnsignedShort();

            //hit the end , security bindings start.
            if (stringBinding.TowerId_Renamed == 0) {
                return null;
            }

            //now to read the String till a null termination character.
            // a '0' will be represented as 30
            int retVal = -1;
            StringBuilder buffer = new StringBuilder();
            while ((retVal = ndr.readUnsignedShort()) != 0) {
                //even though this is a unicode string , but will not have anything else
                //other than ascii charset, which is supported by all encodings.
                buffer.Append(StringHelperClass.NewString(new sbyte[]{ (sbyte)retVal }));
            }

            stringBinding.NetworkAddress_Renamed = buffer.ToString();

            // 2 bytes for tower id, each character is 2 bytes (short) and last 2 bytes for null termination
            stringBinding.Length_Renamed = 2 + stringBinding.NetworkAddress_Renamed.Length * 2 + 2;



            return stringBinding;
        }

        public int TowerId {
            get {
                return TowerId_Renamed;
            }
        }

        public string NetworkAddress {
            get {
                return NetworkAddress_Renamed;
            }
        }

        public void Encode(NetworkDataRepresentation ndr) {
            ndr.writeUnsignedShort(TowerId_Renamed);

            //now to write the network address.
            int i = 0;
            while (i < NetworkAddress_Renamed.Length) {
                ndr.writeUnsignedShort(NetworkAddress_Renamed[i]);
                i++;
            }

    //        //TODO testing only.
    //        if (networkAddress.length()%2 != 0)
    //        {
    //            ndr.writeUnsignedShort(0);
    //        }
            ndr.writeUnsignedShort(0); //null termination

        }

    }

}
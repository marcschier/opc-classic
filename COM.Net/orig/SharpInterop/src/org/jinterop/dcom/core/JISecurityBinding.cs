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
    internal sealed class JISecurityBinding {


        private const long SerialVersionUID = 2100264431889577123L;

        private JISecurityBinding() {
        }

        public const int COM_C_AUTHZ_NONE = 0xffff;
        private int AuthnSvc = 0; // Cannot be zero.
        private int AuthzSvc = 0; // Must not be zero.
        private string PrincName = null; // Zero terminated.
        private int Length_Renamed = -1;

        public int Length {
            get {
                return Length_Renamed;
            }
        }


        public JISecurityBinding(int authnSvc, int authzSvc, string princName) {
            this.AuthnSvc = authnSvc;
            this.AuthzSvc = authzSvc;
            this.PrincName = princName;
            if (princName.Equals("")) {
                Length_Renamed = 2 + 2 + 2;
            }
            else {
                Length_Renamed = 2 + 2 + princName.Length * 2 + 2;
            }
        }

        internal static JISecurityBinding Decode(NetworkDataRepresentation ndr) {
            JISecurityBinding securityBinding = new JISecurityBinding();

            securityBinding.AuthnSvc = ndr.readUnsignedShort();

            if (securityBinding.AuthnSvc == 0) {
                //security binding over.
                return null;
            }

            securityBinding.AuthzSvc = ndr.readUnsignedShort();

            //now to read the String till a null termination character.
            // a '0' will be represented as 30
            int retVal = -1;
            StringBuilder buffer = new StringBuilder();
            while ((retVal = ndr.readUnsignedShort()) != 0) {
                //even though this is a unicode string , but will not have anything else
                //other than ascii charset, which is supported by all encodings.
                buffer.Append(StringHelperClass.NewString(new sbyte[]{ (sbyte)retVal }));
            }


            securityBinding.PrincName = buffer.ToString();



            // 2 bytes for authnsvc, 2 for authzsvc , each character is 2 bytes (short) and last 2 bytes for null termination
            securityBinding.Length_Renamed = 2 + 2 + securityBinding.PrincName.Length * 2 + 2;

            return securityBinding;
        }

        public void Encode(NetworkDataRepresentation ndr) {
            ndr.writeUnsignedShort(AuthnSvc);
            ndr.writeUnsignedShort(AuthzSvc);

            //now to write the network address.
            int i = 0;
            while (i < PrincName.Length) {
                ndr.writeUnsignedShort(PrincName[i]);
                i++;
            }

            ndr.writeUnsignedShort(0); //null termination

        }
    }

}
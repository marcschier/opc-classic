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
    using rpc.core;
    using org.jinterop.dcom.common;
    using org.jinterop.winreg;
    using System;
    using System.Text;

    /// <summary>
    /// Represents a string binding
    /// </summary>
    [Serializable]
    internal sealed class JIStringBinding {

        /// <summary>
        /// Tower id
        /// </summary>
        public int TowerId { get; private set; } = -1;

        /// <summary>
        /// Network address
        /// </summary>
        public string NetworkAddress { get; private set; }

        /// <summary>
        /// Length
        /// </summary>
        public int Length { get; private set; } = -1;

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIStringBinding() {
        }

        /// <summary>
        /// Create string binding
        /// </summary>
        /// <param name="port"></param>
        /// <param name="hostname"></param>
        internal JIStringBinding(int port, bool hostname) {
            string hostaddress = null;
            if (!hostname) {
                //single binding with our IP address
                hostaddress = JISession.LocalhostAddressAsIPString;
            }
            else {
                hostaddress = JISession.LocalhostCanonicalAddressAsString;
            }
            if (port == -1) {
                NetworkAddress = hostaddress;
            }
            else {
                NetworkAddress = hostaddress + "[" + Convert.ToString(port) + "]";
            }
            Length = 2 + NetworkAddress.Length * 2 + 2;
            TowerId = 0x7; // TCP_IP
        }

        /// <summary>
        /// Create string binding
        /// </summary>
        /// <param name="port"></param>
        internal JIStringBinding(int port) : this(port, false) {
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static JIStringBinding Decode(NdrCodec ndr) {
            var stringBinding = new JIStringBinding {
                TowerId = ndr.ReadUnsignedShort()
            };

            //hit the end , security bindings start.
            if (stringBinding.TowerId == 0) {
                return null;
            }

            //now to read the String till a null termination character.
            // a '0' will be represented as 30
            var retVal = -1;
            var buffer = new StringBuilder();
            while ((retVal = ndr.ReadUnsignedShort()) != 0) {
                //even though this is a unicode string, but will not have anything else
                //other than ascii charset, which is supported by all encodings.
                buffer.Append(StringHelperClass.NewString(new byte[] { (byte)retVal }));
            }

            stringBinding.NetworkAddress = buffer.ToString();
            // 2 bytes for tower id, each character is 2 bytes (short) and last 2 bytes for null termination
            stringBinding.Length = 2 + stringBinding.NetworkAddress.Length * 2 + 2;
            return stringBinding;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        public void Encode(NdrCodec ndr) {
            ndr.WriteUnsignedShort(TowerId);
            //now to write the network address.
            var i = 0;
            while (i < NetworkAddress.Length) {
                ndr.WriteUnsignedShort(NetworkAddress[i]);
                i++;
            }
            ndr.WriteUnsignedShort(0); //null termination
        }
    }
}
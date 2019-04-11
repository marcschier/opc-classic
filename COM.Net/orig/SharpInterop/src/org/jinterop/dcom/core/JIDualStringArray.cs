using System;
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


    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    /// <summary>
    /// Represents array of network address and security bindings.
    /// 
    /// @exclude
    /// @since 1.0
    /// </summary>
    [Serializable]
    internal sealed class JIDualStringArray {


        private const long SerialVersionUID = -3351948896808028565L;

        private JIDualStringArray() {
        }

         //static boolean test = false;
        //Will get called from Oxid Resolver
        public JIDualStringArray(int port) {
            //create bindings here.
            StringBinding = new JIStringBinding[2]; //only 1
            StringBinding[0] = new JIStringBinding(port,false);

            Length_Renamed = StringBinding[0].Length;

            StringBinding[1] = new JIStringBinding(port,true);

            Length_Renamed = Length_Renamed + StringBinding[1].Length + 2; //null termination

            SecOffset = Length_Renamed;

            SecurityBinding = new JISecurityBinding[1]; //support only winnt NTLM
            SecurityBinding[0] = new JISecurityBinding(0x0a,0xffff,"");
            Length_Renamed = Length_Renamed + SecurityBinding[0].Length;

            Length_Renamed = Length_Renamed + 2 + 2 + 2; //null termination, 2 bytes for num entries and 2 bytes for sec offset.
        }

        private JIStringBinding[] StringBinding = null;
        private JISecurityBinding[] SecurityBinding = null;
        private int Length_Renamed = 0;
        private int SecOffset = 0;

        internal static JIDualStringArray Decode(NetworkDataRepresentation ndr) {
            JIDualStringArray dualStringArray = new JIDualStringArray();

            //first extract number of entries
            int numEntries = ndr.readUnsignedShort();

            //return empty
            if (numEntries == 0) {
                return dualStringArray;
            }

            //extract security offset
            int securityOffset = ndr.readUnsignedShort();

            List<object> listOfStringBindings = new List<object>();
            List<object> listOfSecurityBindings = new List<object>();

            bool stringbinding = true;
            while (true) {
                if (stringbinding) {
                    JIStringBinding s = JIStringBinding.Decode(ndr);
                    if (s == null) {
                        stringbinding = false;
                        //null termination
                        dualStringArray.Length_Renamed = dualStringArray.Length_Renamed + 2;
                        dualStringArray.SecOffset = dualStringArray.Length_Renamed;
                        continue;
                    }

                    listOfStringBindings.Add(s);
                    dualStringArray.Length_Renamed = dualStringArray.Length_Renamed + s.Length;
                }
                else {
                    JISecurityBinding s = JISecurityBinding.Decode(ndr);
                    if (s == null) {
                        //null termination
                        dualStringArray.Length_Renamed = dualStringArray.Length_Renamed + 2;
                        break;
                    }

                    listOfSecurityBindings.Add(s);
                    dualStringArray.Length_Renamed = dualStringArray.Length_Renamed + s.Length;
                }

            }

            // 2 bytes for num entries and 2 bytes for sec offset.
            dualStringArray.Length_Renamed = dualStringArray.Length_Renamed + 2 + 2;

            dualStringArray.StringBinding = (JIStringBinding[])listOfStringBindings.ToArray(typeof(JIStringBinding));
            dualStringArray.SecurityBinding = (JISecurityBinding[])listOfSecurityBindings.ToArray(typeof(JISecurityBinding));
            return dualStringArray;
        }

        public JIStringBinding[] StringBindings {
            get {
                return StringBinding;
            }
        }

        public JISecurityBinding[] SecurityBindings {
            get {
                return SecurityBinding;
            }
        }

        public int Length {
            get {
                return Length_Renamed;
            }
        }

        public void Encode(NetworkDataRepresentation ndr) {
            //fill num entries
            //this is total length/2. since they are all shorts
            ndr.writeUnsignedShort((Length_Renamed - 4) / 2);
            ndr.writeUnsignedShort((SecOffset) / 2);

            int i = 0;
            if (StringBinding != null) {
                while (i < StringBinding.Length) {
                    StringBinding[i].Encode(ndr);
                    i++;
                }
                ndr.writeUnsignedShort(0);
            }




            i = 0;

            if (SecurityBinding != null) {
                while (i < SecurityBinding.Length) {
                    SecurityBinding[i].Encode(ndr);
                    i++;
                }
                ndr.writeUnsignedShort(0);
            }

        }

    }


}
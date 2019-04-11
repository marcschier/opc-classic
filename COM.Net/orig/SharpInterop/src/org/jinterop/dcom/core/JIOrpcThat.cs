using System;
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


    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    using JIException = org.jinterop.dcom.common.JIException;
    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;

    using UUID = rpc.core.UUID;

    [Serializable]
    internal sealed class JIOrpcThat {


        private const long SerialVersionUID = -9167101165773840248L;
        private int Flags_Renamed = -1;
        private JIOrpcExtentArray[] Arry = null;

        private JIOrpcThat() {
        }

        private int Flags {
            set {
                Flags_Renamed = value;
            }
        }

        //Returns an array of flags present (JIOrpcFlags).
        //For now only 2 flags are returned to the user
        // 0 and 1. Reserved flags are not returned.
        public int[] SupportedFlags {
            get {
    
                if (Flags_Renamed == -1) {
                    return null;
                }
    
                if ((Flags_Renamed & 1) == 1) {
                    return new int[]{ 1 };
                }
                else {
                    return new int[]{ 0 };
                }
            }
        }

        private JIOrpcExtentArray[] ExtentArray {
            set {
                this.Arry = value;
            }
            get {
                return Arry;
            }
        }


        internal static void Encode(NetworkDataRepresentation ndr) {
            ndr.writeUnsignedLong(0);
            ndr.writeUnsignedLong(0);
        }

        internal static JIOrpcThat Decode(NetworkDataRepresentation ndr) {
            JIOrpcThat orpcthat = new JIOrpcThat();
            orpcthat.Flags = ndr.readUnsignedLong();

            //to throw JIRuntimeException from here.
            if (orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_NULL && orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_LOCAL && orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_RESERVED1 && orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_RESERVED2 && orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_RESERVED3 && orpcthat.Flags_Renamed != JIOrpcFlags.ORPCF_RESERVED4) {
                throw new JIRuntimeException(orpcthat.Flags_Renamed);
            }

            JIStruct orpcextentarray = new JIStruct();
            try {
            //create the orpcextent struct
            /*
             *  typedef struct tagORPC_EXTENT
        {
            GUID                    id;          // Extension identifier.
            unsigned long           size;        // Extension size.
            [size_is((size+7)&~7)]  byte data[]; // Extension data.
        } ORPC_EXTENT;
    
             */

                JIStruct orpcextent = new JIStruct();
                orpcextent.AddMember(typeof(UUID));
                orpcextent.AddMember(typeof(int?)); //length
                orpcextent.AddMember(new JIArray(typeof(sbyte?),null,1,true));
            //create the orpcextentarray struct
            /*
             *    typedef struct tagORPC_EXTENT_ARRAY
        {
            unsigned long size;     // Num extents.
            unsigned long reserved; // Must be zero.
            [size_is((size+1)&~1,), unique] ORPC_EXTENT **extent; // extents
        } ORPC_EXTENT_ARRAY;
    
             */


                orpcextentarray.AddMember(typeof(int?));
                orpcextentarray.AddMember(typeof(int?));
                //this is since the pointer is [unique]
                orpcextentarray.AddMember(new JIPointer(new JIArray(new JIPointer(orpcextent),null,1,true)));
            }
            catch (JIException) {
                //this won't fail...i am certain :)...
            }

            IDictionary map = new Hashtable();
            IList listOfDefferedPointers = new List<object>();
            JIPointer orpcextentarrayptr = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,new JIPointer(orpcextentarray),listOfDefferedPointers,JIFlags.FLAG_NULL,map);
            int x = 0;

            while (x < listOfDefferedPointers.Count) {
                List<object> newList = new List<object>();
                JIPointer replacement = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,(JIPointer)listOfDefferedPointers[x],newList,JIFlags.FLAG_NULL,map);
                ((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); //this should replace the value in the original place.
                x++;
                listOfDefferedPointers.AddRange(x,newList);
            }

            List<object> extentArrays = new List<object>();
            //now read whether extend array exists or not
            //int ptr = ndr.readUnsignedLong();
            if (!orpcextentarrayptr.Null) {
                JIPointer[] pointers = (JIPointer[])((JIArray)((JIPointer)((JIStruct)orpcextentarrayptr.GetReferent()).GetMember(2)).GetReferent()).ArrayInstance;
                for (int i = 0;i < pointers.Length;i++) {
                    if (pointers[i].Null) {
                        continue;
                    }

                    JIStruct orpcextent2 = (JIStruct)pointers[i].GetReferent();
                    sbyte?[] byteArray = (sbyte?[])((JIArray)orpcextent2.GetMember(2)).ArrayInstance;

                    extentArrays.Add(new JIOrpcExtentArray(((UUID)orpcextent2.GetMember(0)).ToString(),byteArray.Length,byteArray));
                }

            }

            orpcthat.ExtentArray = (JIOrpcExtentArray[])extentArrays.ToArray(typeof(JIOrpcExtentArray));

            return orpcthat;
        }
    }

}
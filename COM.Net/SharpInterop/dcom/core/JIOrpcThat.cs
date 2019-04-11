//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using rpc.core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    internal sealed class JIOrpcThat {

        /// <summary>
        /// Create that
        /// </summary>
        private JIOrpcThat() {
        }

        /// <summary>
        /// Extent array
        /// </summary>
        public JIOrpcExtentArray[] ExtentArray { get; private set; }

        /// <summary>
        /// Returns an array of flags present (JIOrpcFlags).
        /// For now only 2 flags are returned to the user
        /// 0 and 1. Reserved flags are not returned.
        /// </summary>
        public int[] SupportedFlags {
            get {
                if (_flags == -1) {
                    return null;
                }
                if ((_flags & 1) == 1) {
                    return new int[] { 1 };
                }
                return new int[] { 0 };
            }
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        internal static void Encode(NdrCodec ndr) {
            ndr.WriteUnsignedLong(0);
            ndr.WriteUnsignedLong(0);
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static JIOrpcThat Decode(NdrCodec ndr) {
            var orpcthat = new JIOrpcThat {
                _flags = ndr.ReadUnsignedLong()
            };

            // to throw JIRuntimeException from here.
            if (orpcthat._flags != (int)JIOrpcFlags.ORPCF_NULL &&
                orpcthat._flags != (int)JIOrpcFlags.ORPCF_LOCAL &&
                orpcthat._flags != (int)JIOrpcFlags.ORPCF_RESERVED1 &&
                orpcthat._flags != (int)JIOrpcFlags.ORPCF_RESERVED2 &&
                orpcthat._flags != (int)JIOrpcFlags.ORPCF_RESERVED3 &&
                orpcthat._flags != (int)JIOrpcFlags.ORPCF_RESERVED4) {
                throw new JIRuntimeException(orpcthat._flags);
            }

            var orpcextentarray = new JIStruct();
            try {
                // create the orpcextent struct
                /*
                 *  typedef struct tagORPC_EXTENT
            {
                GUID                    id;          // Extension identifier.
                unsigned long           size;        // Extension size.
                [size_is((size+7)&~7)]  byte data[]; // Extension data.
            } ORPC_EXTENT;

                 */

                var orpcextent = new JIStruct();
                orpcextent.AddMember(typeof(UUID));
                orpcextent.AddMember(typeof(int)); // length
                orpcextent.AddMember(new JIArray(typeof(sbyte), null, 1, true));
                // create the orpcextentarray struct
                /*
                 *    typedef struct tagORPC_EXTENT_ARRAY
            {
                unsigned long size;     // Num extents.
                unsigned long reserved; // Must be zero.
                [size_is((size+1)&~1,), unique] ORPC_EXTENT **extent; // extents
            } ORPC_EXTENT_ARRAY;

                 */


                orpcextentarray.AddMember(typeof(int));
                orpcextentarray.AddMember(typeof(int));
                // this is since the pointer is [unique]
                orpcextentarray.AddMember(new JIPointer(new JIArray(new JIPointer(orpcextent), null, 1, true)));
            }
            catch (JIException) {
                // this won't fail...i am certain :)...
            }

            var map = new Hashtable();
            var listOfDefferedPointers = new List<object>();
            var orpcextentarrayptr = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr, new JIPointer(orpcextentarray), listOfDefferedPointers, JIFlags.FLAG_NULL, map);
            var x = 0;

            while (x < listOfDefferedPointers.Count) {
                var newList = new List<object>();
                var replacement = (JIPointer)JIMarshalUnMarshalHelper.Deserialize(ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, map);
                ((JIPointer)listOfDefferedPointers[x]).ReplaceSelfWithNewPointer(replacement); // this should replace the value in the original place.
                x++;
                listOfDefferedPointers.InsertRange(x, newList);
            }

            var extentArrays = new List<object>();
            // now read whether extend array exists or not
            // int ptr = ndr.readUnsignedLong();
            if (!orpcextentarrayptr.IsNull) {
                var pointers = (JIPointer[])((JIArray)((JIPointer)((JIStruct)orpcextentarrayptr.Referent).GetMember(2)).Referent).ArrayInstance;
                for (var i = 0; i < pointers.Length; i++) {
                    if (pointers[i].IsNull) {
                        continue;
                    }

                    var orpcextent2 = (JIStruct)pointers[i].Referent;
                    var byteArray = (byte[])((JIArray)orpcextent2.GetMember(2)).ArrayInstance;

                    extentArrays.Add(new JIOrpcExtentArray(((UUID)orpcextent2.GetMember(0)).ToString(), byteArray.Length, byteArray));
                }

            }

            orpcthat.ExtentArray = extentArrays.Cast<JIOrpcExtentArray>().ToArray();
            return orpcthat;
        }

        private int _flags = -1;
    }
}
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

    [Serializable]
    internal sealed class JIOrpcThis {


        private static ThreadLocal cidForCallback = new ThreadLocal();
        private JIComVersion version = JISystem.COMVersion;

        public JIOrpcThis() {
            //		cid = GUIDUtil.guidStringFromHexString(IdentifierFactory.createUniqueIdentifier().toHexString());
            CasualityIdentifier = Guid.NewGuid().ToString(); //  java.util.UUID.randomUUID().ToString();
        }

        public JIOrpcThis(UUID casualityIdentifier) {
            CasualityIdentifier = casualityIdentifier.ToString();
        }

        public int ORPCFlags { set; get; } = 0;


        public JIOrpcExtentArray[] ExtentArray { set; get; } = null;


        public string CasualityIdentifier { get; private set; } = null;

        public void encode(NetworkDataRepresentation ndr) {
            ndr.writeUnsignedShort(version.MajorVersion); //COM Major version
            ndr.writeUnsignedShort(version.MinorVersion); //COM minor version
            ndr.writeUnsignedLong(ORPCFlags); // No Flags
            ndr.writeUnsignedLong(0); // Reserved ...always 0.

            //the order here is important since the cid is always filled from the ctor hence will never be null.
            var cid2 = cidForCallback.get() == null ? CasualityIdentifier : (string)cidForCallback.get();
            var uuid = new UUID(cid2);
            try {
                uuid.encode(ndr, ndr.Buffer);
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIOrpcThis", "encode", e);
            }

            var i = 0;
            if (ExtentArray != null && ExtentArray.Length != 0) {
                ndr.writeUnsignedLong(ExtentArray.Length);
                ndr.writeUnsignedLong(0);
                while (i < ExtentArray.Length) {
                    var arryy = ExtentArray[i];
                    uuid = new UUID(arryy.GUID);
                    try {
                        uuid.encode(ndr, ndr.Buffer);
                    }
                    catch (NdrException e) {
                        Log.Logger.Error(e, "JIOrpcThis", "encode", e);
                    }

                    ndr.writeUnsignedLong(arryy.SizeOfData);
                    ndr.writeOctetArray(arryy.Data, 0, arryy.SizeOfData);
                    i++;
                }
            }
            else {
                ndr.writeUnsignedLong(0);
            }
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <returns></returns>
        internal static JIOrpcThis decode(NetworkDataRepresentation ndr) {
            var retval = new JIOrpcThis();
            IDictionary map = new Hashtable();
            var majorVersion = (int)(short?)JIMarshalUnMarshalHelper.deSerialize(
                ndr, typeof(short?), null, JIFlags.FLAG_NULL, map);
            var minorVersion = (int)(short?)JIMarshalUnMarshalHelper.deSerialize(
                ndr, typeof(short?), null, JIFlags.FLAG_NULL, map);

            retval.version = new JIComVersion(majorVersion, minorVersion);
            retval.ORPCFlags = (int)(int?)JIMarshalUnMarshalHelper.deSerialize(
                ndr, typeof(int?), null, JIFlags.FLAG_NULL, map);

            JIMarshalUnMarshalHelper.deSerialize(
                ndr, typeof(int?), null, JIFlags.FLAG_NULL, map); //reserved.

            var uuid = new UUID();
            try {
                uuid.decode(ndr, ndr.Buffer);
                retval.CasualityIdentifier = uuid.ToString();
            }
            catch (NdrException e) {
                Log.Logger.Error(e, "JIOrpcThis", "decode", e);
            }


            var orpcextentarray = new JIStruct();
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

                var orpcextent = new JIStruct();
                orpcextent.addMember(typeof(UUID));
                orpcextent.addMember(typeof(int?)); //length
                orpcextent.addMember(new JIArray(typeof(sbyte?), null, 1, true));
                //create the orpcextentarray struct
                /*
                 *    typedef struct tagORPC_EXTENT_ARRAY
            {
                unsigned long size;     // Num extents.
                unsigned long reserved; // Must be zero.
                [size_is((size+1)&~1,), unique] ORPC_EXTENT **extent; // extents
            } ORPC_EXTENT_ARRAY;

                 */


                orpcextentarray.addMember(typeof(int?));
                orpcextentarray.addMember(typeof(int?));
                //this is since the pointer is [unique]
                orpcextentarray.addMember(new JIPointer(new JIArray(new JIPointer(orpcextent), null, 1, true)));
            }
            catch (JIException) {
                //this won't fail...i am certain :)...
            }

            IList listOfDefferedPointers = new ArrayList();
            var orpcextentarrayptr = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(
                ndr, new JIPointer(orpcextentarray), listOfDefferedPointers, JIFlags.FLAG_NULL, map);
            var x = 0;

            while (x < listOfDefferedPointers.Count) {
                var newList = new ArrayList();
                var replacement = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(
                    ndr, (JIPointer)listOfDefferedPointers[x], newList, JIFlags.FLAG_NULL, map);
                //this should replace the value in the original place.
                ((JIPointer)listOfDefferedPointers[x]).replaceSelfWithNewPointer(replacement); 
                x++;
                listOfDefferedPointers.AddRange(x, newList);
            }

            var extentArrays = new ArrayList();
            //now read whether extend array exists or not
            if (!orpcextentarrayptr.Null) {
                var pointers = (JIPointer[])((JIArray)((JIPointer)((JIStruct)orpcextentarrayptr.getReferent()).getMember(2)).getReferent()).ArrayInstance;
                for (var i = 0; i < pointers.Length; i++) {
                    if (pointers[i].Null) {
                        continue;
                    }

                    var orpcextent2 = (JIStruct)pointers[i].getReferent();
                    var byteArray = (sbyte?[])((JIArray)orpcextent2.getMember(2)).ArrayInstance;

                    extentArrays.Add(new JIOrpcExtentArray(((UUID)orpcextent2.getMember(0)).ToString(), byteArray.Length, byteArray));
                }

            }

            retval.ExtentArray = (JIOrpcExtentArray[])extentArrays.ToArray(typeof(JIOrpcExtentArray));

            //decode can only be executed incase of a request made from the server side in case of a callback. so the thread making this
            //callback will store the cid from the decode operation in the threadlocal variable. In case an encode is performed using the
            //same thread then we know that this is a nested call. Hence will replace the cid with the thread local cid. For the calls being in
            //case of encode this value will not be used if the encode thread is of the client and not of JIComOxidRuntimeHelper.
            cidForCallback.set(retval.CasualityIdentifier);
            return retval;
        }
    }
}
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
    using org.jinterop.dcom.impls.automation;
    using rpc.core;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Marshal helper
    /// </summary>
    internal sealed class JIMarshalUnMarshalHelper {

        private static readonly Hashtable kMapOfSerializers;

        // TODO This is very important, please note that arrays in C++ have a fixed size and unlike Java have to be
        // declared with there Max index right in the beginning. therefore all arrays (of any type), will
        // already come padded here to there Max size., this has to be ensured by the caller
        // Basically the index on COMs side should match with the array length here...otherwise exception
        // will come. This has to be managed by IDL generator.
        static JIMarshalUnMarshalHelper() => kMapOfSerializers = new Hashtable {
            [typeof(DateTime)] = new DateImpl(),
            [typeof(JICurrency)] = new JICurrencyImpl(),
            [typeof(JIVariantBody)] = new JIVariant2Impl(),
            [typeof(JIVariant)] = new JIVariantImpl(),
            [typeof(double)] = new DoubleImpl(),
            [typeof(bool)] = new BooleanImpl(),
            [typeof(float)] = new FloatImpl(),
            [typeof(string)] = new StringImpl(),
            [typeof(UUID)] = new UUIDImpl(),
            [typeof(JIUnsignedByte)] = new JIUnsignedByteImpl(),
            [typeof(sbyte)] = new SByteImpl(),
            [typeof(JIUnsignedShort)] = new JIUnsignedShortImpl(),
            [typeof(short)] = new ShortImpl(),
            [typeof(JIUnsignedInteger)] = new JIUnsignedIntImpl(),
            [typeof(int)] = new IntegerImpl(),
            [typeof(long)] = new LongImpl(),
            [typeof(ulong)] = new LongImpl(),
            [typeof(char)] = new CharacterImpl(),
            [typeof(JIInterfacePointer)] = new MInterfacePointerImpl(),
            [typeof(JIInterfacePointerBody)] = new MInterfacePointerImpl2(),
            [typeof(IJIDispatch)] = new IJIComObjectSerDer(),
            [typeof(IJIComObject)] = new IJIComObjectSerDer(),
            [typeof(JIPointer)] = new PointerImpl(),
            [typeof(JIStruct)] = new StructImpl(),
            [typeof(JIUnion)] = new UnionImpl(),
            [typeof(JIString)] = new JIStringImpl(),
            [typeof(JIDualStringArray)] = new JIDualStringArrayImpl(),
            // [typeof(IJIUnsigned)] = new JIUnsignedImpl()
        };

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        internal static void Serialize(NdrCodec ndr, Type c, object value,
            List<object> defferedPointers, int flag) {
            if (c.Equals(typeof(JIArray))) {
                ((JIArray)value).Encode(ndr, ((JIArray)value).ArrayInstance, defferedPointers, flag);
            }
            else {
                if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && value is IJIComObject) {
                    c = typeof(IJIComObject);
                }

                AlignMemberWhileEncoding(ndr, c, value);

                if (c.Equals(typeof(JIString))) {
                    ((JIString)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                if (c.Equals(typeof(JIPointer))) {
                    ((JIPointer)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                if (c.Equals(typeof(JIStruct))) {
                    ((JIStruct)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                if (c.Equals(typeof(JIUnion))) {
                    ((JIUnion)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                //			if (c.equals(JIDispatchImpl.class) || c.equals(IJIDispatch.class))
                //			{
                //				IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
                //				JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
                //				interfacePointer.encode(ndr,defferedPointers,flag);
                //				return ;
                //			}
                //
                //			if (c.equals(JIComObjectImpl.class) || c.equals(IJIComObject.class) || c.equals(IJIUnknown.class))
                //			{
                //				JIInterfacePointer interfacePointer = ((IJIComObject)value).getInterfacePointer();
                //				interfacePointer.encode(ndr,defferedPointers,flag);
                //				return ;
                //			}


                if (c.Equals(typeof(JIInterfacePointer))) {
                    ((JIInterfacePointer)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                if (c.Equals(typeof(JIVariant))) {
                    ((JIVariant)value).Encode(ndr, defferedPointers, flag);
                    return;
                }

                if (c.Equals(typeof(JIVariantBody))) {
                    ((JIVariantBody)value).Encode(ndr, defferedPointers, flag);
                    return;
                }
                if (!kMapOfSerializers.ContainsKey(c)) {
                    throw new InvalidOperationException(
                        string.Format(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), c));
                }
                ((ISerializerDeserializer)kMapOfSerializers[c]).SerializeData(ndr, value, defferedPointers, flag);
            }
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="obj"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="flag"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal static object Deserialize(NdrCodec ndr, object obj,
            List<object> defferedPointers, int flag, IDictionary<object, object> additionalData) {
            var c = obj is Type ? (Type)obj : obj.GetType();
            if (c.Equals(typeof(JIArray))) {
                return ((JIArray)obj).Decode(ndr, ((JIArray)obj).ArrayClass,
                    ((JIArray)obj).Dimensions, defferedPointers, flag, additionalData);
            }

            AlignMemberWhileDecoding(ndr, c, obj);

            if (c.Equals(typeof(JIPointer))) {
                var retVal = ((JIPointer)obj).Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIStruct))) {
                var retVal = ((JIStruct)obj).Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIUnion))) {
                var retVal = ((JIUnion)obj).Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIString))) {
                var retVal = ((JIString)obj).Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            //This will always be a class
            if (c.Equals(typeof(JIInterfacePointer))) {
                var retVal = JIInterfacePointer.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            //This will always be a class
            if (c.Equals(typeof(JIVariant))) {
                var retVal = JIVariant.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            //This will always be a class
            if (c.Equals(typeof(JIVariantBody))) {
                var retVal = JIVariantBody.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            if (!kMapOfSerializers.ContainsKey(c)) {
                throw new InvalidOperationException(string.Format(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), obj));
            }
            return ((ISerializerDeserializer)kMapOfSerializers[c]).DeserializeData(
                ndr, defferedPointers, additionalData, flag);

        }

        /// <summary>
        /// Get length in bytes
        /// </summary>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        internal static int GetLengthInBytes(Type c, object obj, int flag) {
            if (obj != null && obj.GetType().Equals(typeof(JIArray))) {
                return ((JIArray)obj).SizeOfAllElementsInBytes;
            }
            if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && obj is IJIComObject) {
                c = typeof(IJIComObject);
            }

            if (((ISerializerDeserializer)kMapOfSerializers[c]) == null) {
                throw new InvalidOperationException(string.Format(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), c));
            }
            return ((ISerializerDeserializer)kMapOfSerializers[c]).GetLengthInBytes(obj, flag);
        }


        /// <summary>
        /// Align on write
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        private static void AlignMemberWhileEncoding(NdrCodec ndr, Type c, object obj) {
            var index = (double)ndr.Buffer.Index;
            if (c.Equals(typeof(JIStruct))) {
                ndr.FillAligned(((JIStruct)obj).Alignment);
            }
            else if (c.Equals(typeof(JIUnion))) {
                ndr.FillAligned(((JIUnion)obj).Alignment);
            }
            else if (c.Equals(typeof(int)) ||
                    c.Equals(typeof(float)) ||
                    c.Equals(typeof(JIVariant)) ||
                    c.Equals(typeof(string)) ||
                    c.Equals(typeof(JIPointer))) {
                // align with 4 
                ndr.FillAligned(4);
            }
            else if (c.Equals(typeof(double))) {
                // align with 8
                ndr.FillAligned(8);
            }
            else if (c.Equals(typeof(short))) {
                ndr.FillAligned(2);
            }
        }

        /// <summary>
        /// Align to read
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="c"></param>
        /// <param name="obj"></param>
        private static void AlignMemberWhileDecoding(NdrCodec ndr, Type c, object obj) {
            if (c.Equals(typeof(JIStruct))) {
                ndr.SkipAligned(((JIStruct)obj).Alignment);
            }
            else if (c.Equals(typeof(JIUnion))) {
                ndr.SkipAligned(((JIUnion)obj).Alignment);
            }
            else if (c.Equals(typeof(int)) ||
                    c.Equals(typeof(float)) ||
                    c.Equals(typeof(JIVariant)) ||
                    c.Equals(typeof(string)) ||
                    c.Equals(typeof(JIPointer))) {
                // align with 4 
                ndr.SkipAligned(4);
            }
            else if (c.Equals(typeof(double))) {
                // align with 8
                ndr.SkipAligned(8);
            }
            else if (c.Equals(typeof(short))) {
                ndr.SkipAligned(2);
            }
        }

        /// <summary>
        /// Read buffer
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static byte[] ReadOctetArrayLE(NdrCodec ndr, int length) {
            System.Diagnostics.Debug.Assert(length == 8); // TODO: Should be generic.
            var bytes = new byte[8];
            ndr.ReadOctetArray(bytes, 0, 8);
            for (var i = 0; i < 4; i++) {
                var t = bytes[i];
                bytes[i] = bytes[7 - i];
                bytes[7 - i] = t;
            }
            return bytes;
        }

        /// <summary>
        /// Write buffer
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="b"></param>
        internal static void WriteOctetArrayLE(NdrCodec ndr, byte[] b) {
            for (var i = 0; i < b.Length; i++) {
                ndr.WriteUnsignedSmall(b[b.Length - i - 1]);
            }
        }

        /// <summary>
        /// Serializer interface
        /// </summary>
        private interface ISerializerDeserializer {

            /// <summary>
            /// Serialize data
            /// </summary>
            /// <param name="ndr"></param>
            /// <param name="value"></param>
            /// <param name="defferedPointers"></param>
            /// <param name="flag"></param>
            void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag);

            /// <summary>
            /// Deserialize
            /// </summary>
            /// <param name="ndr"></param>
            /// <param name="defferedPointers"></param>
            /// <param name="additionalData"></param>
            /// <param name="flag"></param>
            /// <returns></returns>
            object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag);

            /// <summary>
            /// Get length in bytes
            /// </summary>
            /// <param name="value"></param>
            /// <param name="flag"></param>
            /// <returns></returns>
            int GetLengthInBytes(object value, int flag);
        }


        /// <inheritdoc/>
        private class PointerImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIPointer)value).Length;
        }

        /// <inheritdoc/>
        private class JIUnsignedIntImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                Serialize(ndr, typeof(int), (int)((JIUnsignedInteger)value).Value, null, flag);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var b = (int)Deserialize(ndr, typeof(int), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((int)b & 0xFFFFFFFFL, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 4;

        }

        /// <inheritdoc/>
        private class JIDualStringArrayImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) => JIDualStringArray.Decode(ndr);

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIDualStringArray)value).Length;

        }

        /// <inheritdoc/>
        private class JIUnsignedByteImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                Serialize(ndr, typeof(sbyte), (sbyte)((JIUnsignedByte)value).Value, null, flag);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                var b = (byte)Deserialize(ndr, typeof(byte), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((short)((sbyte)b & 0xFF), JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 1;

        }

        /// <inheritdoc/>
        private class JIUnsignedShortImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                Serialize(ndr, typeof(short), (short)((JIUnsignedShort)value).Value, null, flag);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                var b = (short)Deserialize(ndr, typeof(short), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((short)b & 0xFFFF, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 2;
        }

        /// <inheritdoc/>
        private class StructImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIStruct)value).Length;
        }

        /// <inheritdoc/>
        private class UnionImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIUnion)value).Length;
        }

        /// <inheritdoc/>
        private class IJIComObjectSerDer : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                var ptr = ((IJIComObject)value).Internal_getInterfacePointer();
                Serialize(ndr, typeof(JIInterfacePointer), ptr, defferedPointers, flag);
                if (ptr.IsCustomObjRef) {
                    // ask the session now for its marshaller unmarshaller and that should 
                    // write the object down into the JIInterfacePointer.
                    // Where we are right now is where our object needs to be written.

                    // TODO we have just written a "reserved" member (before we write the body), 
                    // it has been observed in WMIO that this reserved member
                    // is the total length of the block, if this is so then the Custom Marshaller
                    // for WMIO should overwrite this with the full length.

                    //First write the custom marshaller unmarshaller CLSID. Then the object definition.
                    var index = ndr.Buffer.Index;
                    ((IJIComObject)value).CustomObject.Encode(ndr, defferedPointers, flag);
                    var currentIndex = ndr.Buffer.Index;
                    var totalLength = currentIndex - index + 48;
                    ndr.Buffer.Index = ndr.Buffer.Index - totalLength - 8;
                    ndr.WriteUnsignedLong(totalLength + 4);
                    ndr.WriteUnsignedLong(totalLength + 4);
                    ndr.Buffer.Index = currentIndex;
                }
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                var session = (JISession)additionalData[JICallBuilder.CURRENTSESSION];
                var ptr = (JIInterfacePointer)Deserialize(ndr, typeof(JIInterfacePointer),
                    defferedPointers, flag, additionalData);
                IJIComObject comObject = new JIComObjectImpl(session, ptr);
                if (ptr != null &&
                    ((JIFlags.FLAG_REPRESENTATION_ARRAY & flag) != JIFlags.FLAG_REPRESENTATION_ARRAY) &&
                    ptr.IsCustomObjRef) {
                    //now we need to ask the session for its marshaller unmarshaller based on the CLSID
                    ((JIComObjectImpl)comObject).CustomObject = session.GetCustomMarshallerUnMarshallerTemplate(
                        ptr.CustomCLSID).Decode(comObject, ndr, defferedPointers, flag, additionalData);
                }
                ((List<object>)additionalData[JICallBuilder.COMOBJECTS]).Add(comObject);
                return comObject;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) {
                var interfacePointer = ((IJIComObject)value).Internal_getInterfacePointer();
                return interfacePointer.Length;
            }
        }

        /// <inheritdoc/>
        private class JIVariant2Impl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIVariantBody)value).LengthInBytes;
        }

        /// <inheritdoc/>
        private class JIVariantImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) {
                // 4 for pointer and rest for variant2
                try {
                    return ((JIVariant)value).GetLengthInBytes(flag);
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
            }
        }

        /// <inheritdoc/>
        private class CharacterImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                ndr.WriteUnsignedSmall((char)value);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) => (char)ndr.ReadUnsignedSmall();

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 1;
        }

        /// <inheritdoc/>
        private class SByteImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                ndr.WriteUnsignedSmall((sbyte)value);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) => (sbyte)ndr.ReadUnsignedSmall();

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 1;
        }

        /// <inheritdoc/>
        private class ShortImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = short.MinValue;
                }
                ndr.WriteUnsignedShort((short)value);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var s = (short)ndr.ReadUnsignedShort();
                return s;
            }

            public int GetLengthInBytes(object value, int flag) =>
                2 + 2; //????
        }

        /// <inheritdoc/>
        private class BooleanImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = false;
                }
                if ((flag & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    ndr.WriteUnsignedShort((bool)value == true ? 0xFFFF : 0x0000);
                }
                else {
                    ndr.WriteBoolean((bool)(bool)value);
                }
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                bool? b = null;
                if ((flag & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    var s = ndr.ReadUnsignedShort();
                    b = s != 0 ? true : false;
                }
                else {
                    b = Convert.ToBoolean(ndr.ReadBoolean());
                }
                return b;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    return 2;
                }
                return 1;
            }
        }

        /// <inheritdoc/>
        private class IntegerImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = int.MinValue;
                }
                ndr.WriteUnsignedLong((int)value);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) => ndr.ReadUnsignedLong();

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 4;
        }

        /// <inheritdoc/>
        private class LongImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = long.MinValue;
                }
                ndr.Buffer.Align(8);
                Encdec.Enc_uint64le((long)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = Encdec.Dec_uint64le(ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);
                return b;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 8;
        }

        /// <inheritdoc/>
        private class DoubleImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = double.NaN;
                }
                ndr.Buffer.Align(8);
                Encdec.Enc_doublele((double)(double)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = new double?(Encdec.Dec_doublele(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(8);
                return b;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 8;

        }

        /// <inheritdoc/>
        private class JICurrencyImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                var currency = (JICurrency)value;

                var units = currency.Units;
                var fractionalUnits = currency.FractionalUnits;

                double p = units + (fractionalUnits / 10000);

                //scale the units by 10000 to remove the decimal and take two's compliment.
                var toSend = ~(int)(p * 10000.00) + 1;
                var toSend2 = toSend.ToString("x");
                var hibytes = 0;
                var lowbytes = 0;
                if (toSend2.Length > 8) {
                    lowbytes = Convert.ToInt32(toSend2.Substring(8), 16);
                    hibytes = Convert.ToInt32(toSend2.Substring(0, 8), 16);
                }
                else {
                    lowbytes = toSend;
                    if (toSend < 0) {
                        hibytes = -1;
                    }
                }

                // now align by 8 bytes, since this is struct has a hyper, which I don't support yet
                ndr.FillAligned(8);

                var @struct = new JIStruct();
                try {
                    @struct.AddMember(lowbytes);
                    @struct.AddMember(hibytes);
                }
                catch (JIException) {
                }
                Serialize(ndr, typeof(JIStruct), @struct, null, flag);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                //first align
                ndr.SkipAligned(8);

                //now read the low byte
                var lowbyte = ndr.ReadUnsignedLong();
                //hibyte
                var hibyte = ndr.ReadUnsignedLong();
                if (hibyte < 0) {
                    lowbyte = -1 * Math.Abs(lowbyte);
                }

                //String newValue = Integer.toHexString(hibyte) + Integer.toHexString(lowbyte);
                //long value = Long.parseLong(newValue,16);
                return new JICurrency((lowbyte - (lowbyte % 10000)) / 10000, lowbyte % 10000);
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 4 + 4;
        }

        /// <inheritdoc/>
        private class DateImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                ndr.Buffer.Align(8);
                Encdec.Enc_doublele(((DateTime)value).ToOADate(), ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);

            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = DateTime.FromOADate(Encdec.Dec_doublele(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(8);
                return b;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 8;
        }

        /// <inheritdoc/>
        private class FloatImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = float.NaN;
                }
                ndr.Buffer.Align(4);
                Encdec.Enc_floatle((float)(float)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(4);
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(4);
                var b = new float?(Encdec.Dec_floatle(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(4);
                return b;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 4;

        }

        /// <inheritdoc/>
        private class StringImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException((int)JIErrorCodes.JI_UTIL_STRING_INVALID);
                }
                var str = (string)value;
                if (str == null) {
                    str = "";
                }
                //BSTR encoding
                if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                    byte[] strBytes = null;
                    try {
                        strBytes = str.GetBytes("UTF-16LE");
                    }
                    catch (UnsupportedEncodingException) {
                        throw new JIRuntimeException((int)JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                    }
                    //NDR representation Max count, then offset, then, actual count
                    //length of String (Maximum count)
                    ndr.WriteUnsignedLong(strBytes.Length / 2);
                    //last index of String (length in bytes)
                    ndr.WriteUnsignedLong(strBytes.Length);
                    //length of String Again !! (Actual count)
                    ndr.WriteUnsignedLong(strBytes.Length / 2);
                    //write an array of unsigned shorts
                    var i = 0;
                    while (i < strBytes.Length) {
                        //ndr.writeUnsignedShort(str.charAt(i));
                        ndr.WriteUnsignedSmall(strBytes[i]);
                        i++;
                    }

                }
                else //Normal String
                {
                    if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                        // the String is written as "short" so length is strlen/2+1
                        var strlen = (int)Math.Round(str.Length / 2.0);

                        ndr.WriteUnsignedLong(strlen + 1);
                        ndr.WriteUnsignedLong(0);
                        ndr.WriteUnsignedLong(strlen + 1);
                        if (str.Length != 0) {
                            ndr.WriteCharacterArray(str.ToCharArray(), 0, str.Length);
                            //odd length
                            if (str.Length % 2 != 0) {
                                //add a 0
                                ndr.WriteUnsignedSmall(0);
                            }
                        }

                        //null termination
                        ndr.WriteUnsignedShort(0);
                    }
                    else {
                        if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {

                            byte[] strBytes = null;
                            try {
                                strBytes = str.GetBytes("UTF-16LE");
                            }
                            catch (UnsupportedEncodingException) {
                                throw new JIRuntimeException((int)JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                            }

                            //bytes + 1
                            ndr.WriteUnsignedLong((strBytes.Length / 2) + 1);
                            ndr.WriteUnsignedLong(0);
                            ndr.WriteUnsignedLong((strBytes.Length / 2) + 1);
                            //write an array of unsigned shorts
                            var i = 0;
                            while (i < strBytes.Length) {
                                //ndr.writeUnsignedShort(str.charAt(i));
                                ndr.WriteUnsignedSmall(strBytes[i]);
                                i++;
                            }

                            //					int strlen = str.length();
                            //					ndr.writeUnsignedLong(strlen + 1);
                            //					ndr.writeUnsignedLong(0);
                            //					ndr.writeUnsignedLong(strlen + 1);
                            //
                            //					int i = 0;
                            //					while (i < str.length())
                            //					{
                            //						ndr.writeUnsignedShort(str.charAt(i));
                            //						i++;
                            //					}

                            //null termination
                            ndr.WriteUnsignedShort(0);

                        }
                    }
                }
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException((int)JIErrorCodes.JI_UTIL_STRING_INVALID);
                }
                var retVal = -1;
                //StringBuffer buffer = new StringBuffer();
                string retString = null;
                try {

                    //BSTR Decoding
                    if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                        //Read for user
                        ndr.ReadUnsignedLong(); //eating max length
                        ndr.ReadUnsignedLong(); //eating length in bytes
                        var actuallength = ndr.ReadUnsignedLong() * 2;
                        var buffer = new byte[actuallength];
                        var i = 0;
                        while (i < actuallength) {
                            retVal = ndr.ReadUnsignedSmall();
                            buffer[i] = (byte)retVal;
                            i++;
                        }

                        retString = StringHelperClass.NewString(buffer, "UTF-16LE");
                    }
                    else //Normal String
                    {
                        if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                            {
                                var actuallength = ndr.ReadUnsignedLong(); //max length
                                if (actuallength == 0) {
                                    return null;
                                }

                                ndr.ReadUnsignedLong(); //eating offset
                                ndr.ReadUnsignedLong(); //eating actuallength again
                                                        //now read array.
                                var ret = new char[(actuallength * 2) - 2];
                                //read including the unsigned short (null chars)
                                ndr.ReadCharacterArray(ret, 0, (actuallength * 2) - 2);
                                if (ret[ret.Length - 1] == '0') {
                                    retString = new string(ret, 0, ret.Length - 1);
                                }
                                else {
                                    retString = new string(ret);
                                }

                                ndr.ReadUnsignedShort();
                            }
                        }
                        else {
                            if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {

                                {
                                    var maxlength = ndr.ReadUnsignedLong();
                                    if (maxlength == 0) {
                                        return null;
                                    }
                                    ndr.ReadUnsignedLong(); //eating offset
                                    var actuallength = ndr.ReadUnsignedLong() * 2;
                                    var buffer = new byte[actuallength - 2];
                                    var i = 0;
                                    //last 2 bytes, null termination will be eaten outside the loop
                                    while (i < actuallength - 2) {
                                        retVal = ndr.ReadUnsignedSmall();
                                        buffer[i] = (byte)retVal;
                                        i++;
                                    }
                                    if (actuallength != 0) {
                                        ndr.ReadUnsignedShort();
                                    }

                                    retString = StringHelperClass.NewString(buffer, "UTF-16LE");

                                }

                            }
                        }
                    }
                }
                catch (UnsupportedEncodingException) {
                    throw new JIRuntimeException((int)JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                }
                return retString;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) {
                //rough estimate, this will vary from string to string

                var length = 4 + 4 + 4; //max len, offset,actual length
                if (!((flag & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR)) {
                    length += 2; //adding null termination
                }

                if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                    length += ((string)value).Length; //this is only a character array, no unicode, each char is writen in 1 byte "abcd" --> ab, cd,00 ; "abcde" --> ab,cd,e0, 00
                    if (!(((string)value).Length % 2 == 0)) //odd
                    {
                        length++;
                    }
                }
                else {
                    //				if (value == null)
                    //				{
                    //					int i = 0;
                    //				}
                    length += ((string)value).Length * 2; //these are both unicode (utf-16le)
                }
                return length;
            }
        }

        /// <inheritdoc/>
        private class JIStringImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) {
                var length = 4;

                if (((JIString)value).String == null) {
                    return length;
                }
                //for LPWSTR and BSTR adding 2 for the null character.
                length += ((JIString)value).Type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ? 0 : 2;
                //Pointer referentId --> USER
                return length + JIMarshalUnMarshalHelper.GetLengthInBytes(typeof(string), ((JIString)value).String, ((JIString)value).Type | flag);
            }
        }


        /// <inheritdoc/>
        private class UUIDImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                try {
                    ((UUID)value).Encode(ndr, ndr.Buffer);
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "UUIDImpl serializeData");
                }
            }

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                var ret = new UUID();
                try {
                    ret.Decode(ndr, ndr.Buffer);
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "UUIDImpl deserializeData", e);
                    ret = null;
                }
                return ret;
            }

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => 16;
        }


        /// <inheritdoc/>
        private class MInterfacePointerImpl : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) =>
                throw new InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIInterfacePointer)value).Length;
        }

        /// <inheritdoc/>
        private class MInterfacePointerImpl2 : ISerializerDeserializer {

            /// <inheritdoc/>
            public void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) =>
                ((JIInterfacePointerBody)value).Encode(ndr, flag);

            /// <inheritdoc/>
            public object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) => JIInterfacePointerBody.Decode(ndr, flag);

            /// <inheritdoc/>
            public int GetLengthInBytes(object value, int flag) => ((JIInterfacePointerBody)value).Length;
        }
    }
}
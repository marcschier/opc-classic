//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//
namespace org.jinterop.dcom.core {
    using SharpCifs.Util;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.impls.automation;
    using rpc.core;
    using System;
    using System.Collections.Generic;
    using Serilog;

    /// <summary>
    /// Marshal helper
    /// </summary>
    internal sealed class JIMarshalUnMarshalHelper {

        private static Hashtable mapOfSerializers = new Hashtable();

        // TODO This is very important , please note that arrays in C++ have a fixed size and unlike Java have to be
        // declared with there Max index right in the beginning. therefore all arrays (of any type) , will
        // already come padded here to there Max size., this has to be ensured by the caller
        // Basically the index on COMs side should match with the array length here...otherwise exception
        // will come. This has to be managed by IDL generator.
        static JIMarshalUnMarshalHelper() {
            mapOfSerializers[typeof(DateTime)] = new DateImpl();
            mapOfSerializers[typeof(JICurrency)] = new JICurrencyImpl();
            mapOfSerializers[typeof(JIVariantBody)] = new JIVariant2Impl();
            mapOfSerializers[typeof(JIVariant)] = new JIVariantImpl();
            mapOfSerializers[typeof(double?)] = new DoubleImpl();
            mapOfSerializers[typeof(bool?)] = new BooleanImpl();
            mapOfSerializers[typeof(short?)] = new ShortImpl();
            mapOfSerializers[typeof(int?)] = new IntegerImpl();
            mapOfSerializers[typeof(float?)] = new FloatImpl();
            mapOfSerializers[typeof(string)] = new StringImpl();
            mapOfSerializers[typeof(UUID)] = new UUIDImpl();
            mapOfSerializers[typeof(byte?)] = new ByteImpl();
            mapOfSerializers[typeof(long?)] = new LongImpl(); //LONG , 8 bytes, written as 4+4 in LE.
            mapOfSerializers[typeof(char?)] = new CharacterImpl();
            mapOfSerializers[typeof(JIInterfacePointer)] = new MInterfacePointerImpl();
            mapOfSerializers[typeof(JIInterfacePointerBody)] = new MInterfacePointerImpl2();
            mapOfSerializers[typeof(IJIDispatch)] = new IJIComObjectSerDer();
            mapOfSerializers[typeof(IJIComObject)] = new IJIComObjectSerDer();
            mapOfSerializers[typeof(JIPointer)] = new PointerImpl();
            mapOfSerializers[typeof(JIStruct)] = new StructImpl();
            mapOfSerializers[typeof(JIUnion)] = new UnionImpl();
            mapOfSerializers[typeof(JIString)] = new JIStringImpl();
            mapOfSerializers[typeof(JIUnsignedByte)] = new JIUnsignedByteImpl();
            mapOfSerializers[typeof(JIUnsignedShort)] = new JIUnsignedShortImpl();
            mapOfSerializers[typeof(JIUnsignedInteger)] = new JIUnsignedIntImpl();
            mapOfSerializers[typeof(JIDualStringArray)] = new JIDualStringArrayImpl();
            //		mapOfSerializers.put(IJIUnsigned.class,new JIMarshalUnMarshalHelper.JIUnsignedImpl());
        }

        /// <summary>
        /// Read buffer
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        internal static byte[] ReadOctetArrayLE(NdrCodec ndr, int length) {
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

                alignMemberWhileEncoding(ndr, c, value);

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
                    ((JIVariantBody)value).encode(ndr, defferedPointers, flag);
                    return;
                }


                if (mapOfSerializers[c] == null) {
                    throw new InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), new string[] { c.ToString() }));
                } ((ISerializerDeserializer)mapOfSerializers.get(c)).SerializeData(ndr, value, defferedPointers, flag);
            }
        }

        internal static void alignMemberWhileEncoding(NdrCodec ndr, Type c, object obj) {
            var index = (double)ndr.Buffer.Index;
            if (c.Equals(typeof(JIStruct))) {
                var align = (double)((JIStruct)obj).Alignment;
                var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.writeOctetArray(new sbyte[(int)i], 0, (int)i);
            }
            else if (c.Equals(typeof(JIUnion))) {
                var align = (double)((JIUnion)obj).Alignment;
                var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.writeOctetArray(new sbyte[(int)i], 0, (int)i);
            }
            else {
                if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(JIVariant)) || c.Equals(typeof(string)) || c.Equals(typeof(JIPointer))) //c.equals(Character.class) || c.equals(Byte.class) ||
                {
                    //align with 4 bytes
                    long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
                    ndr.writeOctetArray(new sbyte[(int)i], 0, (int)i);
                }
                else if (c.Equals(typeof(double?))) {
                    //align with 8
                    long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                    ndr.writeOctetArray(new sbyte[(int)i], 0, (int)i);
                }
                else if (c.Equals(typeof(short?))) {
                    long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
                    ndr.writeOctetArray(new sbyte[(int)i], 0, (int)i);
                }
            }
        }

        internal static void AlignMemberWhileDecoding(NdrCodec ndr, Type c, object obj) {
            var index = ndr.Buffer.Index;
            if (c.Equals(typeof(JIStruct))) {
                var align = ((JIStruct)obj).Alignment;
                var i = index % align;
                i = i == 0 ? 0 : align - i;
                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
            }
            else if (c.Equals(typeof(JIUnion))) {
                var align = ((JIUnion)obj).Alignment;
                var i = index % align;
                i = i == 0 ? 0 : align - i;
                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
            }
            else {
                if (c.Equals(typeof(int?)) ||
                    c.Equals(typeof(float?)) ||
                    c.Equals(typeof(JIVariant)) ||
                    c.Equals(typeof(string)) ||
                    c.Equals(typeof(JIPointer))) {
                    //align with 4 bytes
                    long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
                    ndr.readOctetArray(new sbyte[(int)i], 0, (int)i);
                }
                else if (c.Equals(typeof(double?))) {
                    //align with 8
                    long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                    ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
                }
                else if (c.Equals(typeof(short?))) {
                    long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
                    ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);
                }
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
            if (obj.Equals(typeof(JIInterfacePointer))) {
                var retVal = JIInterfacePointer.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            //This will always be a class
            if (obj.Equals(typeof(JIVariant))) {
                var retVal = JIVariant.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            //This will always be a class
            if (obj.Equals(typeof(JIVariantBody))) {
                var retVal = JIVariantBody.Decode(ndr, defferedPointers, flag, additionalData);
                return retVal;
            }

            if (mapOfSerializers[obj] == null) {
                throw new InvalidOperationException(string.Format(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), obj));
            }
            return ((ISerializerDeserializer)mapOfSerializers[obj]).DeserializeData(
                ndr, defferedPointers, additionalData, flag);

        }

        internal static int GetLengthInBytes(Type c, object obj, int flag) {
            if (obj != null && obj.GetType().Equals(typeof(JIArray))) {
                return ((JIArray)obj).SizeOfAllElementsInBytes;
            }
            if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && obj is IJIComObject) {
                c = typeof(IJIComObject);
            }

            if (((ISerializerDeserializer)mapOfSerializers[c]) == null) {
                throw new InvalidOperationException(string.Format(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), c)));
            }
            return ((ISerializerDeserializer)mapOfSerializers[c]).GetLengthInBytes(obj, flag);
        }

        private interface ISerializerDeserializer {
            void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag);
            object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag);
            int GetLengthInBytes(object value, int flag);
        }


        private class PointerImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIPointer)value).Length;
            }

        }

        private class JIUnsignedIntImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                Serialize(ndr, typeof(int?), (int)((JIUnsignedInteger)value).Value, null, flag);
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var b = (int?)Deserialize(ndr, typeof(int?), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((long)((int)b & 0xFFFFFFFFL), JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 4;
            }

        }

        private class JIDualStringArrayImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                return JIDualStringArray.Decode(ndr);
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIDualStringArray)value).Length;
            }

        }

        private class JIUnsignedByteImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                Serialize(ndr, typeof(sbyte?), (sbyte)((JIUnsignedByte)value).Value, null, flag);
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var b = (sbyte?)Deserialize(ndr, typeof(sbyte?), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((short)((sbyte)b & 0xFF), JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 1;
            }

        }

        private class JIUnsignedShortImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                Serialize(ndr, typeof(short?), (short)((JIUnsignedShort)value).Value, null, flag);
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var b = (short?)Deserialize(ndr, typeof(short?), null, flag, additionalData);
                return JIUnsignedFactory.GetUnsigned((int)((short)b & 0xFFFF), JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 2;
            }
        }

        //	private static class JIUnsignedImpl implements SerializerDeserializer {
        //
        //		public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int flag)
        //		{
        //			IJIUnsigned unsigned = (IJIUnsigned)value;
        //			switch(unsigned.getType())
        //			{
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
        //					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedByte.class,value,defferedPointers,flag);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
        //					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedShort.class,value,defferedPointers,flag);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
        //					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedInteger.class,value,defferedPointers,flag);
        //					break;
        //
        //				default:
        //					throw new System.InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
        //			}
        //
        //		}
        //
        //		public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int flag)
        //		{
        //			IJIUnsigned unsigned = null;
        //			int type = JIFlags.FLAG_NULL;
        //			if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
        //			{
        //				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
        //			}
        //			else
        //			if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
        //			{
        //				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
        //			}
        //			else
        //			if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
        //			{
        //				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
        //			}
        //
        //			switch(type)
        //			{
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
        //					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedByte.class, defferedPointers, flag, additionalData);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
        //					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedShort.class, defferedPointers, flag, additionalData);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
        //					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedInteger.class, defferedPointers, flag, additionalData);
        //					break;
        //
        //				default:
        //					throw new System.InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
        //			}
        //
        //			return unsigned;
        //		}
        //
        //		public int getLengthInBytes(Object value,int flag)
        //		{
        //			IJIUnsigned unsigned = (IJIUnsigned)value;
        //			int length = 0;
        //			int type = JIFlags.FLAG_NULL;
        //			if (unsigned != null)
        //			{
        //				type = unsigned.getType();
        //			}
        //			else
        //			{
        //				if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
        //				{
        //					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
        //				}
        //				else
        //				if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
        //				{
        //					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
        //				}
        //				else
        //				if ((flag & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
        //				{
        //					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
        //				}
        //			}
        //
        //			switch(type)
        //			{
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
        //					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedByte.class,value,flag);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
        //					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedShort.class,value,flag);
        //					break;
        //
        //				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
        //					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedInteger.class,value,flag);
        //					break;
        //
        //				default:
        //					throw new System.InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
        //			}
        //
        //			return length;
        //		}
        //
        //	}

        private class StructImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIStruct)value).Length;
            }

        }

        private class UnionImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIUnion)value).Length;
            }

        }


        private class IJIComObjectSerDer : ISerializerDeserializer {


            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                var ptr = ((IJIComObject)value).Internal_getInterfacePointer();
                Serialize(ndr, typeof(JIInterfacePointer), ptr, defferedPointers, flag);
                if (ptr.CustomObjRef) {
                    //ask the session now for its marshaller unmarshaller and that should write the object down into the JIInterfacePointer.
                    //Where we are right now is where our object needs to be written.

                    //TODO we have just written a "reserved" member (before we write the body), it has been observed in WMIO that this reserved member
                    //is the total length of the block, if this is so then the Custom Marshaller for WMIO should overwrite this with the full length.

                    //First write the custom marshaller unmarshaller CLSID. Then the object definition.
                    var index = ndr.Buffer.Index;
                    ((IJIComObject)value).CustomObject.Encode(ndr, defferedPointers, flag);
                    var currentIndex = ndr.Buffer.Index;
                    var totalLength = currentIndex - index + 48;
                    ndr.Buffer.Index = ndr.Buffer.Index - totalLength - 8;
                    ndr.WriteUnsignedLong(totalLength + 4);
                    ndr.WriteUnsignedLong(totalLength + 4);
                    ndr.Buffer.Index = currentIndex;
                    //				Hexdump.hexdump(System.out, ndr.getBuffer().getBuffer(), 0, ndr.getBuffer().getIndex());
                }
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers,
                IDictionary<object, object> additionalData, int flag) {
                var session = (JISession)additionalData[JICallBuilder.CURRENTSESSION];
                var ptr = (JIInterfacePointer)Deserialize(ndr, typeof(JIInterfacePointer),
                    defferedPointers, flag, additionalData);
                IJIComObject comObject = new JIComObjectImpl(session, ptr);
                if (ptr != null &&
                    ((JIFlags.FLAG_REPRESENTATION_ARRAY & flag) != JIFlags.FLAG_REPRESENTATION_ARRAY) &&
                    ptr.CustomObjRef) {
                    //now we need to ask the session for its marshaller unmarshaller based on the CLSID
                    ((JIComObjectImpl)comObject).CustomObject = session.getCustomMarshallerUnMarshallerTemplate(
                        ptr.CustomCLSID).Decode(comObject, ndr, defferedPointers, flag, additionalData);
                }
                ((List<object>)additionalData[JICallBuilder.COMOBJECTS]).Add(comObject);
                return comObject;
            }


            public virtual int GetLengthInBytes(object value, int flag) {
                var interfacePointer = ((IJIComObject)value).Internal_getInterfacePointer();
                return ((JIInterfacePointer)interfacePointer).Length;
            }

        }

        //	private static class IJIDispatchImpl implements SerializerDeserializer {
        //
        //
        //		public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int flag)
        //		{
        //			throw new System.InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
        //		}
        //
        //		public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int flag)
        //		{
        //			throw new System.InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
        //		}
        //
        //
        //		public int getLengthInBytes(Object value,int flag)
        //		{
        //			IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
        //			JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
        //			return ((JIInterfacePointer)interfacePointer).getLength();
        //		}
        //
        //
        //	}

        private class JIVariant2Impl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIVariantBody)value).LengthInBytes;
            }

        }

        private class JIVariantImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                //4 for pointer and rest for variant2
                try {
                    return ((JIVariant)value).getLengthInBytes(flag);
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
            }

        }

        private class CharacterImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                ndr.WriteUnsignedSmall((char)(char?)value);
            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var c = new char?((char)ndr.ReadUnsignedSmall());
                return c;
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 1;
            }

        }

        private class ByteImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                ndr.WriteUnsignedSmall((sbyte)(sbyte?)value);
            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var c = new sbyte?((sbyte)ndr.ReadUnsignedSmall());
                return c;
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 1;
            }

        }

        private class ShortImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = short.MinValue;
                }
                ndr.WriteUnsignedShort((short)(short?)value);


            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                var s = new short?((short)ndr.ReadUnsignedShort());
                return s;
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 2 + 2;
                }
            }

        }

        private class BooleanImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = false;
                }

                if ((flag & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    ndr.WriteUnsignedShort((bool)(bool?)value == true ? 0xFFFF : 0x0000);
                }
                else {
                    ndr.WriteBoolean((bool)(bool?)value);
                }

            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
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
            public virtual int GetLengthInBytes(object value, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    return 2;
                }
                return 1;
            }
        }

        private class IntegerImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = int.MinValue;
                }
                ndr.WriteUnsignedLong((int)(int?)value);
            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                return ndr.ReadUnsignedLong();
            }
            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 4;
                }
            }

        }

        private class LongImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = long.MinValue;
                }
                ndr.Buffer.Align(8);
                Encdec.Enc_uint64le((long)(long?)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = new long?(Encdec.Dec_uint64le(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(8);
                return b;
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return 8;
            }

        }

        private class DoubleImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = double.NaN;
                }

                ndr.Buffer.Align(8);
                Encdec.Enc_doublele((double)(double?)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);

            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = new double?(Encdec.Dec_doublele(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(8);


                return b;
            }
            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 8;
                }
            }

        }

        private class JICurrencyImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                var currency = (JICurrency)value;

                var units = currency.Units;
                var fractionalUnits = currency.FractionalUnits;

                double p = units + fractionalUnits / 10000;

                //scale the units by 10000 to remove the decimal and take two's compliment.
                var toSend = ~(int)(p * 10000.00) + 1;

                var toSend2 = toSend.ToString("x");
                var hibytes = 0;
                var lowbytes = 0;
                if (toSend2.Length > 8) {
                    lowbytes = (int)Convert.ToInt32(toSend2.Substring(8), 16);
                    hibytes = (int)Convert.ToInt32(toSend2.Substring(0, 8), 16);
                }
                else {
                    lowbytes = toSend;
                    if (toSend < 0) {
                        hibytes = -1;
                    }
                }

                //			now align by 8 bytes, since this is struct has a hyper, which I don't support yet
                var index = (double)ndr.Buffer.Index;
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.WriteOctetArray(new byte[(int)i], 0, (int)i);

                var @struct = new JIStruct();
                try {
                    @struct.AddMember(lowbytes);
                    @struct.AddMember(hibytes);
                }
                catch (JIException) {

                }
                Serialize(ndr, typeof(JIStruct), @struct, null, flag);

            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                //first align
                var index = (double)ndr.Buffer.Index;
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.ReadOctetArray(new byte[(int)i], 0, (int)i);

                //now read the low byte
                var lowbyte = ndr.ReadUnsignedLong();
                //hibyte
                var hibyte = ndr.ReadUnsignedLong();
                if (hibyte < 0) {
                    lowbyte = -1 * Math.Abs(lowbyte);
                }

                //String newValue = Integer.toHexString(hibyte) + Integer.toHexString(lowbyte);
                //long value = Long.parseLong(newValue,16);
                return new JICurrency((int)((lowbyte - lowbyte % 10000) / 10000), (int)(lowbyte % 10000));


            }
            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 4 + 4;
                }
            }
        }
        //will only get called from a variant.
        private class DateImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                //			if (value == null && flag == JIFlags.FLAG_REPRESENTATION_ARRAY)
                //			{
                //				value = new Double(Double.NaN);
                //			}

                ndr.Buffer.Align(8);
                Encdec.Enc_doublele(convertMillisecondsToWindowsTime(((DateTime)value).Ticks), ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(8);

            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(8);
                var b = new DateTime(convertWindowsTimeToMilliseconds(Encdec.Dec_doublele(ndr.Buffer.Buf, ndr.Buffer.Index)));
                ndr.Buffer.Advance(8);
                return b;
            }
            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 8;
                }
            }

            /// <summary>
            ///FROM JACAOB 1.10. www.danadler.com.
            /// Convert a COM time from functions Date(), Time(), Now() to a
            /// Java time (milliseconds). Visual Basic time values are based to
            /// 30.12.1899, Java time values are based to 1.1.1970 (= 0
            /// milliseconds). The difference is added to the Visual Basic value to
            /// get the corresponding Java value. The Visual Basic double value
            /// reads: &lt;day count delta since 30.12.1899&gt;.&lt;1 day percentage
            /// fraction&gt;, e.g. "38100.6453" means: 38100 days since 30.12.1899 plus
            /// (24 hours * 0.6453). Example usage:
            /// <code>Date javaDate = new Date(toMilliseconds (vbDate));</code>.
            /// </summary>
            /// <param name="comTime">
            ///            COM time. </param>
            /// <returns> Java time. </returns>
            internal virtual long convertWindowsTimeToMilliseconds(double comTime) {
                long result = 0;

                // code from jacobgen:
                comTime -= 25569D;
                var cal = new DateTime();
                result = Math.Round(86400000L * comTime) - cal.get(DateTime.ZONE_OFFSET);
                cal = new DateTime(new DateTime(result));
                result -= cal.get(DateTime.DST_OFFSET);

                return result;
            } // convertWindowsTimeToMilliseconds()


            /// <summary>
            ///FROM JACAOB 1.10. www.danadler.com.
            /// Convert a Java time to a COM time.
            /// </summary>
            /// <param name="milliseconds">
            ///            Java time. </param>
            /// <returns> COM time. </returns>
            internal virtual double convertMillisecondsToWindowsTime(long milliseconds) {
                var result = 0.0;

                // code from jacobgen:
                var cal = new DateTime {
                    TimeInMillis = milliseconds
                };
                milliseconds += cal.get(DateTime.ZONE_OFFSET) + cal.get(DateTime.DST_OFFSET); // add GMT offset
                result = (milliseconds / 86400000D) + 25569D;

                return result;
            } //convertMillisecondsToWindowsTime()

        }

        private class FloatImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if (value == null) {
                    value = float.NaN;
                }
                ndr.Buffer.Align(4);
                Encdec.Enc_floatle((float)(float?)value, ndr.Buffer.Buf, ndr.Buffer.Index);
                ndr.Buffer.Advance(4);

            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                ndr.Buffer.Align(4);
                var b = new float?(Encdec.Dec_floatle(ndr.Buffer.Buf, ndr.Buffer.Index));
                ndr.Buffer.Advance(4);

                return b;
            }
            public virtual int GetLengthInBytes(object value, int flag) {
                {
                    return 4;
                }

            }

        }

        private class StringImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
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
                        throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                    }
                    //NDR representation Max count , then offset, then, actual count
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
                                throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                            }

                            //bytes + 1
                            ndr.WriteUnsignedLong(strBytes.Length / 2 + 1);
                            ndr.WriteUnsignedLong(0);
                            ndr.WriteUnsignedLong(strBytes.Length / 2 + 1);
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

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                if ((flag & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
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
                                var ret = new char[actuallength * 2 - 2];
                                //read including the unsigned short (null chars)
                                ndr.ReadCharacterArray(ret, 0, actuallength * 2 - 2);
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
                                    //last 2 bytes , null termination will be eaten outside the loop
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
                    throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                }

                return retString;
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                //rough estimate, this will vary from string to string

                var length = 4 + 4 + 4; //max len, offset ,actual length

                if (!((flag & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR)) {
                    length += 2; //adding null termination
                }

                if ((flag & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                    length += ((string)value).Length; //this is only a character array, no unicode, each char is writen in 1 byte "abcd" --> ab, cd ,00 ; "abcde" --> ab,cd,e0, 00
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


        private class JIStringImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                var length = 4;

                if (((JIString)value).String == null) {
                    return length;
                }


                //for LPWSTR and BSTR adding 2 for the null character.
                length += (((JIString)value).Type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ? 0 : 2);
                //Pointer referentId --> USER
                return length + JIMarshalUnMarshalHelper.GetLengthInBytes(typeof(string), ((JIString)value).String, ((JIString)value).Type | flag);
            }


        }


        private class UUIDImpl : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                try {
                    ((UUID)value).Encode(ndr, ndr.Buffer);
                }
                catch (NdrException e) {
                    Log.Logger.Error(e, "UUIDImpl serializeData");
                }
            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
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

            public virtual int GetLengthInBytes(object value, int flag) {
                return 16;
            }

        }

        private class MInterfacePointerImpl : ISerializerDeserializer {

            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIInterfacePointer)value).Length;
            }
        }

        private class MInterfacePointerImpl2 : ISerializerDeserializer {
            public virtual void SerializeData(NdrCodec ndr, object value, List<object> defferedPointers, int flag) {
                ((JIInterfacePointerBody)value).Encode(ndr, flag);
            }
            public virtual object DeserializeData(NdrCodec ndr, List<object> defferedPointers, IDictionary<object, object> additionalData, int flag) {
                return JIInterfacePointerBody.Decode(ndr, flag);
            }
            public virtual int GetLengthInBytes(object value, int flag) {
                return ((JIInterfacePointerBody)value).Length;
            }
        }
    }

}
using System;
using System.Collections;

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





    using Encdec = jcifs.util.Encdec;
    using NdrException = ndr.NdrException;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JIException = org.jinterop.dcom.common.JIException;
    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    using UUID = rpc.core.UUID;

    internal sealed class JIMarshalUnMarshalHelper {

        private static IDictionary MapOfSerializers = new Hashtable();




        //TODO This is very important , please note that arrays in C++ have a fixed size and unlike Java have to be
        //declared with there Max index right in the beginning. therefore all arrays (of any type) , will
        //already come padded here to there Max size., this has to be ensured by the caller
        // Basically the index on COMs side should match with the array length here...otherwise exception
        // will come. This has to be managed by IDL generator.
        static JIMarshalUnMarshalHelper() {

            MapOfSerializers[typeof(DateTime?)] = new JIMarshalUnMarshalHelper.DateImpl();
            MapOfSerializers[typeof(JICurrency)] = new JIMarshalUnMarshalHelper.JICurrencyImpl();
            MapOfSerializers[typeof(VariantBody)] = new JIMarshalUnMarshalHelper.JIVariant2Impl();
            MapOfSerializers[typeof(JIVariant)] = new JIMarshalUnMarshalHelper.JIVariantImpl();
            MapOfSerializers[typeof(double?)] = new JIMarshalUnMarshalHelper.DoubleImpl();
            MapOfSerializers[typeof(bool?)] = new JIMarshalUnMarshalHelper.BooleanImpl();
            MapOfSerializers[typeof(short?)] = new JIMarshalUnMarshalHelper.ShortImpl();
            MapOfSerializers[typeof(int?)] = new JIMarshalUnMarshalHelper.IntegerImpl();
            MapOfSerializers[typeof(float?)] = new JIMarshalUnMarshalHelper.FloatImpl();
            MapOfSerializers[typeof(string)] = new JIMarshalUnMarshalHelper.StringImpl();
            MapOfSerializers[typeof(UUID)] = new JIMarshalUnMarshalHelper.UUIDImpl();
            MapOfSerializers[typeof(sbyte?)] = new JIMarshalUnMarshalHelper.ByteImpl();
            MapOfSerializers[typeof(long?)] = new JIMarshalUnMarshalHelper.LongImpl(); //LONG , 8 bytes, written as 4+4 in LE.
            MapOfSerializers[typeof(char?)] = new JIMarshalUnMarshalHelper.CharacterImpl();
            MapOfSerializers[typeof(JIInterfacePointer)] = new JIMarshalUnMarshalHelper.MInterfacePointerImpl();
            MapOfSerializers[typeof(JIInterfacePointerBody)] = new JIMarshalUnMarshalHelper.MInterfacePointerImpl2();
            MapOfSerializers[typeof(IJIDispatch)] = new JIMarshalUnMarshalHelper.IJIComObjectSerDer();
            MapOfSerializers[typeof(IJIComObject)] = new JIMarshalUnMarshalHelper.IJIComObjectSerDer();
            MapOfSerializers[typeof(JIPointer)] = new JIMarshalUnMarshalHelper.PointerImpl();
            MapOfSerializers[typeof(JIStruct)] = new JIMarshalUnMarshalHelper.StructImpl();
            MapOfSerializers[typeof(JIUnion)] = new JIMarshalUnMarshalHelper.UnionImpl();
            MapOfSerializers[typeof(JIString)] = new JIMarshalUnMarshalHelper.JIStringImpl();
            MapOfSerializers[typeof(JIUnsignedByte)] = new JIMarshalUnMarshalHelper.JIUnsignedByteImpl();
            MapOfSerializers[typeof(JIUnsignedShort)] = new JIMarshalUnMarshalHelper.JIUnsignedShortImpl();
            MapOfSerializers[typeof(JIUnsignedInteger)] = new JIMarshalUnMarshalHelper.JIUnsignedIntImpl();
            MapOfSerializers[typeof(JIDualStringArray)] = new JIMarshalUnMarshalHelper.JIDualStringArrayImpl();
    //        mapOfSerializers.put(IJIUnsigned.class,new JIMarshalUnMarshalHelper.JIUnsignedImpl());

        }

        internal static sbyte[] ReadOctetArrayLE(NetworkDataRepresentation ndr, int length) {
            sbyte[] bytes = new sbyte[8];
            ndr.readOctetArray(bytes,0,8);
            for (int i = 0;i < 4; i++) {
                sbyte t = bytes[i];
                bytes[i] = bytes[7 - i];
                bytes[7 - i] = t;
            }
            return bytes;
        }

        internal static void WriteOctetArrayLE(NetworkDataRepresentation ndr, sbyte[] b) {
            for (int i = 0;i < b.Length; i++) {
                ndr.writeUnsignedSmall(b[b.Length - i - 1]);
            }
        }

        internal static void Serialize(NetworkDataRepresentation ndr, Type c, object value, IList defferedPointers, int FLAG) {
            if (c.Equals(typeof(JIArray))) {
                ((JIArray)value).Encode(ndr,((JIArray)value).ArrayInstance,defferedPointers,FLAG);
            }
            else {
                if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && value is IJIComObject) {
                    c = typeof(IJIComObject);
                }

                AlignMemberWhileEncoding(ndr,c,value);

                if (c.Equals(typeof(JIString))) {
                    ((JIString)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

                if (c.Equals(typeof(JIPointer))) {
                    ((JIPointer)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

                if (c.Equals(typeof(JIStruct))) {
                    ((JIStruct)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

                if (c.Equals(typeof(JIUnion))) {
                    ((JIUnion)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

    //            if (c.equals(JIDispatchImpl.class) || c.equals(IJIDispatch.class))
    //            {
    //                IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
    //                JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
    //                interfacePointer.encode(ndr,defferedPointers,FLAG);
    //                return ;
    //            }
    //
    //            if (c.equals(JIComObjectImpl.class) || c.equals(IJIComObject.class) || c.equals(IJIUnknown.class))
    //            {
    //                JIInterfacePointer interfacePointer = ((IJIComObject)value).getInterfacePointer();
    //                interfacePointer.encode(ndr,defferedPointers,FLAG);
    //                return ;
    //            }


                if (c.Equals(typeof(JIInterfacePointer))) {
                    ((JIInterfacePointer)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

                if (c.Equals(typeof(JIVariant))) {
                    ((JIVariant)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }

                if (c.Equals(typeof(VariantBody))) {
                    ((VariantBody)value).Encode(ndr,defferedPointers,FLAG);
                    return;
                }


                if (MapOfSerializers.GetValueOrNull(c) == null) {
                    throw new System.InvalidOperationException(MessageFormat.format(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new string[]{ c.ToString() }));
                }((SerializerDeserializer)mapOfSerializers.get(c)).serializeData(ndr,value,defferedPointers,FLAG);
            }
        }

        internal static void AlignMemberWhileEncoding(NetworkDataRepresentation ndr, Type c, object obj) {
            double index = (double)(new int?(ndr.Buffer.Index));
            if (c.Equals(typeof(JIStruct))) {
                double align = (double)(new int?(((JIStruct)obj).Alignment));
                long i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(JIUnion))) {
                double align = (double)(new int?(((JIUnion)obj).Alignment));
                long i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else {
            if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(JIVariant)) || c.Equals(typeof(string)) || c.Equals(typeof(JIPointer))) { //c.equals(Character.class) || c.equals(Byte.class) ||
                //align with 4 bytes
                long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(double?))) {
                //align with 8
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(short?))) {
                long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
            }
            }
        }

        internal static void AlignMemberWhileDecoding(NetworkDataRepresentation ndr, Type c, object obj) {
            double index = (double)(new int?(ndr.Buffer.Index));
            if (c.Equals(typeof(JIStruct))) {
                double align = (double)(new int?(((JIStruct)obj).Alignment));
                long i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(JIUnion))) {
                double align = (double)(new int?(((JIUnion)obj).Alignment));
                long i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else {
            if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(JIVariant)) || c.Equals(typeof(string)) || c.Equals(typeof(JIPointer))) {
                //align with 4 bytes
                long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(double?))) {
                //align with 8
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
            }
            else if (c.Equals(typeof(short?))) {
                long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
            }
            }
        }


        internal static object DeSerialize(NetworkDataRepresentation ndr, object obj, IList defferedPointers, int FLAG, IDictionary additionalData) {
            Type c = obj is Type ? (Type)obj : obj.GetType();
            if (c.Equals(typeof(JIArray))) {
                return ((JIArray)obj).Decode(ndr,((JIArray)obj).ArrayClass,((JIArray)obj).Dimensions,defferedPointers,FLAG,additionalData);
            }
            else {

                AlignMemberWhileDecoding(ndr,c,obj);


                if (c.Equals(typeof(JIPointer))) {
                    JIPointer retVal = ((JIPointer)obj).Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                if (c.Equals(typeof(JIStruct))) {
                    JIStruct retVal = ((JIStruct)obj).Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                if (c.Equals(typeof(JIUnion))) {
                    JIUnion retVal = ((JIUnion)obj).Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                if (c.Equals(typeof(JIString))) {
                    JIString retVal = ((JIString)obj).Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                //This will always be a class
                if (obj.Equals(typeof(JIInterfacePointer))) {
                    JIInterfacePointer retVal = JIInterfacePointer.Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                //This will always be a class
                if (obj.Equals(typeof(JIVariant))) {
                    JIVariant retVal = JIVariant.Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                //This will always be a class
                if (obj.Equals(typeof(VariantBody))) {
                    VariantBody retVal = VariantBody.Decode(ndr,defferedPointers,FLAG,additionalData);
                    return retVal;
                }

                if (MapOfSerializers.GetValueOrNull(obj) == null) {
                    throw new System.InvalidOperationException(MessageFormat.format(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new string[]{ obj.ToString() }));
                }
                return ((SerializerDeserializer)MapOfSerializers.GetValueOrNull(obj)).DeserializeData(ndr,defferedPointers,additionalData,FLAG);
            }

        }



        internal static int GetLengthInBytes(Type c, object obj, int FLAG) {
            if (obj != null && obj.GetType().Equals(typeof(JIArray))) {
                return ((JIArray)obj).SizeOfAllElementsInBytes;
            }
            else {
                if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && obj is IJIComObject) {
                    c = typeof(IJIComObject);
                }

                if (((SerializerDeserializer)MapOfSerializers.GetValueOrNull(c)) == null) {
                    throw new System.InvalidOperationException(MessageFormat.format(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new string[]{ c.ToString() }));
                }
                return ((SerializerDeserializer)MapOfSerializers.GetValueOrNull(c)).GetLengthInBytes(obj,FLAG);
            }

        }

        private interface SerializerDeserializer {
            void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG);
            object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG);
            int GetLengthInBytes(object value, int FLAG);
        }


        private class PointerImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIPointer)value).Length;
            }

        }

        private class JIUnsignedIntImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?((int)((JIUnsignedInteger)value).Value),null,FLAG);
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                int? b = (int?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,FLAG,additionalData);
                return JIUnsignedFactory.GetUnsigned(new long?((long)((int)b & 0xFFFFFFFFL)),JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 4;
            }

        }

        private class JIDualStringArrayImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                return JIDualStringArray.Decode(ndr);
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIDualStringArray)value).Length;
            }

        }

        private class JIUnsignedByteImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(sbyte?),new sbyte?((sbyte)((JIUnsignedByte)value).Value),null,FLAG);
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                sbyte? b = (sbyte?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(sbyte?),null,FLAG,additionalData);
                return JIUnsignedFactory.GetUnsigned(new short?((short)((sbyte)b & 0xFF)),JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 1;
            }

        }

        private class JIUnsignedShortImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                JIMarshalUnMarshalHelper.Serialize(ndr,typeof(short?),new short?((short)((JIUnsignedShort)value).Value),null,FLAG);
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                short? b = (short?)JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,FLAG,additionalData);
                return JIUnsignedFactory.GetUnsigned(new int?((int)((short)b & 0xFFFF)), JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 2;
            }
        }

    //    private static class JIUnsignedImpl implements SerializerDeserializer {
    //
    //        public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int FLAG)
    //        {
    //            IJIUnsigned unsigned = (IJIUnsigned)value;
    //            switch(unsigned.getType())
    //            {
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
    //                    JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedByte.class,value,defferedPointers,FLAG);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
    //                    JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedShort.class,value,defferedPointers,FLAG);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
    //                    JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedInteger.class,value,defferedPointers,FLAG);
    //                    break;
    //
    //                default:
    //                    throw new IllegalStateException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
    //            }
    //
    //        }
    //
    //        public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int FLAG)
    //        {
    //            IJIUnsigned unsigned = null;
    //            int type = JIFlags.FLAG_NULL;
    //            if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
    //            {
    //                type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
    //            }
    //            else
    //            if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
    //            {
    //                type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
    //            }
    //            else
    //            if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
    //            {
    //                type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
    //            }
    //
    //            switch(type)
    //            {
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
    //                    unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedByte.class, defferedPointers, FLAG, additionalData);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
    //                    unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedShort.class, defferedPointers, FLAG, additionalData);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
    //                    unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedInteger.class, defferedPointers, FLAG, additionalData);
    //                    break;
    //
    //                default:
    //                    throw new IllegalStateException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
    //            }
    //
    //            return unsigned;
    //        }
    //
    //        public int getLengthInBytes(Object value,int FLAG)
    //        {
    //            IJIUnsigned unsigned = (IJIUnsigned)value;
    //            int length = 0;
    //            int type = JIFlags.FLAG_NULL;
    //            if (unsigned != null)
    //            {
    //                type = unsigned.getType();
    //            }
    //            else
    //            {
    //                if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
    //                {
    //                    type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
    //                }
    //                else
    //                if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
    //                {
    //                    type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
    //                }
    //                else
    //                if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
    //                {
    //                    type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
    //                }
    //            }
    //
    //            switch(type)
    //            {
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
    //                    length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedByte.class,value,FLAG);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
    //                    length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedShort.class,value,FLAG);
    //                    break;
    //
    //                case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
    //                    length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedInteger.class,value,FLAG);
    //                    break;
    //
    //                default:
    //                    throw new IllegalStateException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
    //            }
    //
    //            return length;
    //        }
    //
    //    }

        private class StructImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIStruct)value).Length;
            }

        }

        private class UnionImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIUnion)value).Length;
            }

        }


        private class IJIComObjectSerDer : SerializerDeserializer {


            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                JIInterfacePointer ptr = ((IJIComObject)value).Internal_getInterfacePointer();
                Serialize(ndr, typeof(JIInterfacePointer), ptr, defferedPointers, FLAG);
                if (ptr.CustomObjRef) {
                    //ask the session now for its marshaller unmarshaller and that should write the object down into the JIInterfacePointer.
                    //Where we are right now is where our object needs to be written.

                    //TODO we have just written a "reserved" member (before we write the body), it has been observed in WMIO that this reserved member 
                    //is the total length of the block, if this is so then the Custom Marshaller for WMIO should overwrite this with the full length.

                    //First write the custom marshaller unmarshaller CLSID. Then the object definition.
                    int index = ndr.Buffer.Index;
                    ((IJIComObject)value).CustomObject.Encode(ndr, defferedPointers, FLAG);
                    int currentIndex = ndr.Buffer.Index;
                    int totalLength = (currentIndex - index) + 48;
                    ndr.Buffer.Index = ndr.Buffer.Index - totalLength - 8;
                    ndr.writeUnsignedLong(totalLength + 4);
                    ndr.writeUnsignedLong(totalLength + 4);
                    ndr.Buffer.Index = currentIndex;
    //                Hexdump.hexdump(System.out, ndr.getBuffer().getBuffer(), 0, ndr.getBuffer().getIndex());
                }
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                JISession session = (JISession)additionalData.GetValueOrNull(JICallBuilder.CURRENTSESSION);
                JIInterfacePointer ptr = (JIInterfacePointer)DeSerialize(ndr, typeof(JIInterfacePointer), defferedPointers, FLAG, additionalData);
                IJIComObject comObject = new JIComObjectImpl(session, ptr);
                if (ptr != null && ((JIFlags.FLAG_REPRESENTATION_ARRAY & FLAG) != JIFlags.FLAG_REPRESENTATION_ARRAY) && ptr.CustomObjRef) {
                    //now we need to ask the session for its marshaller unmarshaller based on the CLSID 
                    ((JIComObjectImpl)comObject).CustomObject = session.GetCustomMarshallerUnMarshallerTemplate(ptr.CustomCLSID).Decode(comObject, ndr, defferedPointers, FLAG, additionalData);
                }((List<object>)additionalData.get(JICallBuilder.COMOBJECTS)).add(comObject);
                return comObject;
            }


            public virtual int GetLengthInBytes(object value, int FLAG) {
                JIInterfacePointer interfacePointer = ((IJIComObject)value).Internal_getInterfacePointer();
                return ((JIInterfacePointer)interfacePointer).Length;
            }

        }

    //    private static class IJIDispatchImpl implements SerializerDeserializer {
    //
    //
    //        public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int FLAG)
    //        {
    //            throw new IllegalStateException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
    //        }
    //
    //        public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int FLAG)
    //        {
    //            throw new IllegalStateException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
    //        }
    //
    //
    //        public int getLengthInBytes(Object value,int FLAG)
    //        {
    //            IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
    //            JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
    //            return ((JIInterfacePointer)interfacePointer).getLength();
    //        }
    //
    //
    //    }

        private class JIVariant2Impl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((VariantBody)value).LengthInBytes;
            }

        }

        private class JIVariantImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                //4 for pointer and rest for variant2
                try {
                    return ((JIVariant)value).GetLengthInBytes(FLAG);
                }
                catch (JIException e) {
                    throw new JIRuntimeException(e.ErrorCode);
                }
            }

        }

        private class CharacterImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                ndr.writeUnsignedSmall((char)((char?)value));
            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                char? c = new char?((char)ndr.readUnsignedSmall());
                return c;
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 1;
            }

        }

        private class ByteImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                ndr.writeUnsignedSmall((sbyte)((sbyte?)value));
            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                sbyte? c = new sbyte?((sbyte)ndr.readUnsignedSmall());
                return c;
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 1;
            }

        }

        private class ShortImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = new short?(short.MinValue);
                }
                ndr.writeUnsignedShort((short)((short?)value));


            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                short? s = new short?((short)ndr.readUnsignedShort());
                return s;
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
            {
                    return 2 + 2;
            }
            }

        }

        private class BooleanImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = false;
                }

                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    ndr.writeUnsignedShort((bool)((bool?)value) == true ? 0xFFFF: 0x0000);
                }
                else {
                    ndr.writeBoolean((bool)((bool?)value));
                }

            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                bool? b = null;
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    int s = ndr.readUnsignedShort();
                    b = s != 0 ? true:false;
                }
                else {
                    b = Convert.ToBoolean(ndr.readBoolean());
                }

                return b;
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    return 2;
                }
                else {
                    return 1;
                }
            }
        }

        private class IntegerImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = new int?(int.MinValue);
                }
                ndr.writeUnsignedLong((int)((int?)value));
            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                return new int?(ndr.readUnsignedLong());
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
            {
                    return 4;
            }
            }

        }

        private class LongImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = new long?(long.MinValue);
                }
                ndr.Buffer.align(8);
                Encdec.enc_uint64le((long)((long?)value),ndr.Buffer.Buffer,ndr.Buffer.Index);
                ndr.Buffer.advance(8);
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                ndr.Buffer.align(8);
                long? b = new long?(Encdec.dec_uint64le(ndr.Buffer.Buffer,ndr.Buffer.Index));
                ndr.Buffer.advance(8);
                return b;
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 8;
            }

        }

        private class DoubleImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = new double?(double.NaN);
                }

                ndr.Buffer.align(8);
                Encdec.enc_doublele((double)((double?)value), ndr.Buffer.Buffer,ndr.Buffer.Index);
                ndr.Buffer.advance(8);

            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                ndr.Buffer.align(8);
                double? b = new double?(Encdec.dec_doublele(ndr.Buffer.Buffer,ndr.Buffer.Index));
                ndr.Buffer.advance(8);


                return b;
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
            {
                    return 8;
            }
            }

        }

        private class JICurrencyImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                JICurrency currency = (JICurrency)value;

                int units = currency.Units;
                int fractionalUnits = currency.FractionalUnits;

                double p = units + fractionalUnits / 10000;

                //scale the units by 10000 to remove the decimal and take two's compliment.
                int toSend = ~((int)(p * 10000.00)) + 1;

                string toSend2 = (toSend.ToString("x"));
                int hibytes = 0;
                int lowbytes = 0;
                if (toSend2.Length > 8) {
                    lowbytes = (int)Convert.ToInt32(toSend2.Substring(8),16);
                    hibytes = (int)Convert.ToInt32(toSend2.Substring(0,8),16);
                }
                else {
                    lowbytes = toSend;
                    if (toSend < 0) {
                        hibytes = -1;
                    }
                }

    //            now align by 8 bytes, since this is struct has a hyper, which I don't support yet
                double index = (double)(new int?(ndr.Buffer.Index));
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);

                JIStruct @struct = new JIStruct();
                try {
                    @struct.AddMember(new int?(lowbytes));
                    @struct.AddMember(new int?(hibytes));
                }
                catch (JIException) {

                }
                Serialize(ndr,typeof(JIStruct),@struct,null,FLAG);

            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                //first align
                double index = (double)(new int?(ndr.Buffer.Index));
                long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
                ndr.readOctetArray(new sbyte[(int)i],0,(int)i);

                //now read the low byte
                int lowbyte = ndr.readUnsignedLong();
                //hibyte
                int hibyte = ndr.readUnsignedLong();
                if (hibyte < 0) {
                    lowbyte = -1 * Math.Abs(lowbyte);
                }

                //String newValue = Integer.toHexString(hibyte) + Integer.toHexString(lowbyte);
                //long value = Long.parseLong(newValue,16);
                return new JICurrency((int)((lowbyte - lowbyte % 10000) / 10000),(int)(lowbyte % 10000));


            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
            {
                    return 4 + 4;
            }
            }
        }
        //will only get called from a variant.
        private class DateImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
    //            if (value == null && FLAG == JIFlags.FLAG_REPRESENTATION_ARRAY)
    //            {
    //                value = new Double(Double.NaN);
    //            }

                ndr.Buffer.align(8);
                Encdec.enc_doublele(ConvertMillisecondsToWindowsTime(((DateTime?)value).Value.Ticks), ndr.Buffer.Buffer,ndr.Buffer.Index);
                ndr.Buffer.advance(8);

            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                ndr.Buffer.align(8);
                DateTime? b = new DateTime?(ConvertWindowsTimeToMilliseconds(Encdec.dec_doublele(ndr.Buffer.Buffer,ndr.Buffer.Index)));
                ndr.Buffer.advance(8);
                return b;
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
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
             /// reads: <day count delta since 30.12.1899>.<1 day percentage
             /// fraction>, e.g. "38100.6453" means: 38100 days since 30.12.1899 plus
             /// (24 hours * 0.6453). Example usage:
             /// <code>Date javaDate = new Date(toMilliseconds (vbDate));</code>.
             /// </summary>
             /// <param name="comTime">
             ///            COM time. </param>
             /// <returns> Java time. </returns>
            public virtual long ConvertWindowsTimeToMilliseconds(double comTime) {
                long result = 0;

                // code from jacobgen:
                comTime = comTime - 25569D;
                DateTime? cal = new DateTime();
                result = Math.Round(86400000L * comTime) - cal.Value.get(DateTime.ZONE_OFFSET);
                cal.Value = new DateTime(new DateTime?(result));
                result -= cal.Value.get(DateTime.DST_OFFSET);

                return result;
            } // convertWindowsTimeToMilliseconds()


            /// <summary>
            ///FROM JACAOB 1.10. www.danadler.com.
            /// Convert a Java time to a COM time.
            /// </summary>
            /// <param name="milliseconds">
            ///            Java time. </param>
            /// <returns> COM time. </returns>
            public virtual double ConvertMillisecondsToWindowsTime(long milliseconds) {
                double result = 0.0;

                // code from jacobgen:
                DateTime? cal = new DateTime();
                cal.Value.TimeInMillis = milliseconds;
                milliseconds += (cal.Value.get(DateTime.ZONE_OFFSET) + cal.Value.get(DateTime.DST_OFFSET)); // add GMT offset
                result = (milliseconds / 86400000D) + 25569D;

                return result;
            } //convertMillisecondsToWindowsTime()

        }

        private class FloatImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if (value == null) {
                    value = new float?(float.NaN);
                }
                ndr.Buffer.align(4);
                Encdec.enc_floatle((float)((float?)value), ndr.Buffer.Buffer,ndr.Buffer.Index);
                ndr.Buffer.advance(4);

            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                ndr.Buffer.align(4);
                float? b = new float?(Encdec.dec_floatle(ndr.Buffer.Buffer,ndr.Buffer.Index));
                ndr.Buffer.advance(4);

                return b;
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
            {
                    return 4;
            }

            }

        }

        private class StringImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
                }

                string str = ((string)value);
                if (str == null) {
                    str = "";
                }
                //BSTR encoding
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                    sbyte[] strBytes = null;
                    try {
                        strBytes = str.GetBytes("UTF-16LE");
                    }
                    catch (UnsupportedEncodingException) {
                        throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                    }
                    //NDR representation Max count , then offset, then, actual count
                    //length of String (Maximum count)
                    ndr.writeUnsignedLong(strBytes.Length / 2);
                    //last index of String (length in bytes)
                    ndr.writeUnsignedLong(strBytes.Length);
                    //length of String Again !! (Actual count)
                    ndr.writeUnsignedLong(strBytes.Length / 2);
                    //write an array of unsigned shorts
                    int i = 0;
                    while (i < strBytes.Length) {
                        //ndr.writeUnsignedShort(str.charAt(i));
                        ndr.writeUnsignedSmall(strBytes[i]);
                        i++;
                    }

                }
                else { //Normal String
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                    // the String is written as "short" so length is strlen/2+1
                    int strlen = (int)Math.Round(str.Length / 2.0);

                    ndr.writeUnsignedLong(strlen + 1);
                    ndr.writeUnsignedLong(0);
                    ndr.writeUnsignedLong(strlen + 1);
                    if (str.Length != 0) {
                        ndr.writeCharacterArray(str.ToCharArray(),0,str.Length);
                        //odd length
                        if (str.Length % 2 != 0) {
                            //add a 0
                            ndr.writeUnsignedSmall(0);
                        }
                    }

                    //null termination
                    ndr.writeUnsignedShort(0);
                }
                else {
                    if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {

                        sbyte[] strBytes = null;
                        try {
                            strBytes = str.GetBytes("UTF-16LE");
                        }
                        catch (UnsupportedEncodingException) {
                            throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
                        }

                        //bytes + 1
                        ndr.writeUnsignedLong(strBytes.Length / 2 + 1);
                        ndr.writeUnsignedLong(0);
                        ndr.writeUnsignedLong(strBytes.Length / 2 + 1);
                        //write an array of unsigned shorts
                        int i = 0;
                        while (i < strBytes.Length) {
                            //ndr.writeUnsignedShort(str.charAt(i));
                            ndr.writeUnsignedSmall(strBytes[i]);
                            i++;
                        }

    //                    int strlen = str.length();
    //                    ndr.writeUnsignedLong(strlen + 1);
    //                    ndr.writeUnsignedLong(0);
    //                    ndr.writeUnsignedLong(strlen + 1);
    //
    //                    int i = 0;
    //                    while (i < str.length())
    //                    {
    //                        ndr.writeUnsignedShort(str.charAt(i));
    //                        i++;
    //                    }

                        //null termination
                        ndr.writeUnsignedShort(0);

                    }
                }
                }

            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING) {
                    throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
                }
                int retVal = -1;
                //StringBuffer buffer = new StringBuffer();
                string retString = null;
                try {

                    //BSTR Decoding
                    if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                        //Read for user
                        ndr.readUnsignedLong(); //eating max length
                        ndr.readUnsignedLong(); //eating length in bytes
                        int actuallength = ndr.readUnsignedLong() * 2;
                        sbyte[] buffer = new sbyte[actuallength];
                        int i = 0;
                        while (i < actuallength) {
                            retVal = ndr.readUnsignedSmall();
                            buffer[i] = (sbyte)retVal;
                            i++;
                        }

                        retString = StringHelperClass.NewString(buffer, "UTF-16LE");

                    }
                    else { //Normal String
                    if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                    {
                            int actuallength = ndr.readUnsignedLong(); //max length
                            if (actuallength == 0) {
                                return null;
                            }

                            ndr.readUnsignedLong(); //eating offset
                            ndr.readUnsignedLong(); //eating actuallength again
                            //now read array.
                            char[] ret = new char[actuallength * 2 - 2];
                            //read including the unsigned short (null chars)
                            ndr.readCharacterArray(ret,0,actuallength * 2 - 2);
                            if (ret[ret.Length - 1] == '0') {
                                retString = new string(ret,0,ret.Length - 1);
                            }
                            else {
                                retString = new string(ret);
                            }

                            ndr.readUnsignedShort();
                    }
                    }
                    else {
                    if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {

                    {
                            int maxlength = ndr.readUnsignedLong();
                            if (maxlength == 0) {
                                return null;
                            }
                            ndr.readUnsignedLong(); //eating offset
                            int actuallength = ndr.readUnsignedLong() * 2;
                            sbyte[] buffer = new sbyte[actuallength - 2];
                            int i = 0;
                            //last 2 bytes , null termination will be eaten outside the loop
                            while (i < actuallength - 2) {
                                retVal = ndr.readUnsignedSmall();
                                buffer[i] = (sbyte)retVal;
                                i++;
                            }
                            if (actuallength != 0) {
                                ndr.readUnsignedShort();
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

            public virtual int GetLengthInBytes(object value, int FLAG) {
                //rough estimate, this will vary from string to string

                int length = 4 + 4 + 4; //max len, offset ,actual length

                if (!((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR)) {
                    length = length + 2; //adding null termination
                }

                if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) {
                    length = length + ((string)value).Length; //this is only a character array, no unicode, each char is writen in 1 byte "abcd" --> ab, cd ,00 ; "abcde" --> ab,cd,e0, 00
                    if (!(((string)value).Length % 2 == 0)) { //odd
                        length++;
                    }
                }
                else {
    //                if (value == null)
    //                {
    //                    int i = 0;
    //                }
                    length = length + ((string)value).Length * 2; //these are both unicode (utf-16le)
                }


                return length;
            }

        }


        private class JIStringImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                int length = 4;

                if (((JIString)value).String == null) {
                    return length;
                }


                //for LPWSTR and BSTR adding 2 for the null character.
                length = length + (((JIString)value).Type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ? 0 : 2);
                //Pointer referentId --> USER
                return length + JIMarshalUnMarshalHelper.GetLengthInBytes(typeof(string),((JIString)value).String,((JIString)value).Type | FLAG);
            }


        }


        private class UUIDImpl : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                try {
                    ((UUID)value).encode(ndr,ndr.Buffer);
                }
                catch (NdrException e) {
                    JISystem.Logger.throwing("UUIDImpl","serializeData",e);
                }
            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                UUID ret = new UUID();
                try {
                    ret.decode(ndr,ndr.Buffer);
                }
                catch (NdrException e) {
                    JISystem.Logger.throwing("UUIDImpl","deserializeData",e);
                    ret = null;
                }
                return ret;
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return 16;
            }

        }

        private class MInterfacePointerImpl : SerializerDeserializer {

            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                throw new System.InvalidOperationException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
            }

            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIInterfacePointer)value).Length;
            }
        }

        private class MInterfacePointerImpl2 : SerializerDeserializer {
            public virtual void SerializeData(NetworkDataRepresentation ndr, object value, IList defferedPointers, int FLAG) {
                ((JIInterfacePointerBody)value).Encode(ndr,FLAG);
            }
            public virtual object DeserializeData(NetworkDataRepresentation ndr, IList defferedPointers, IDictionary additionalData, int FLAG) {
                return JIInterfacePointerBody.Decode(ndr,FLAG);
            }
            public virtual int GetLengthInBytes(object value, int FLAG) {
                return ((JIInterfacePointerBody)value).Length;
            }
        }
    }

}
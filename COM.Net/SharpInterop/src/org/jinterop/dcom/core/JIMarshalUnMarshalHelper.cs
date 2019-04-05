// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 
namespace org.jinterop.dcom.core {





    using Encdec = SharpCifs.util.Encdec;
    using NdrException = SharpCifs.Dcerpc.Ndr.NdrException;
    using NdrCodec = SharpCifs.Dcerpc.Ndr.NdrCodec;

    using JIErrorCodes = common.JIErrorCodes;
    using JIException = common.JIException;
    using JIRuntimeException = common.JIRuntimeException;
    using JISystem = common.JISystem;
    using IJIDispatch = impls.automation.IJIDispatch;

    using UUID = rpc.core.UUID;
    using System.Collections;
    using System;

    internal sealed class JIMarshalUnMarshalHelper
	{

		private static IDictionary mapOfSerializers = new Hashtable();




		//TODO This is very important , please note that arrays in C++ have a fixed size and unlike Java have to be
		//declared with there Max index right in the beginning. therefore all arrays (of any type) , will
		//already come padded here to there Max size., this has to be ensured by the caller
		// Basically the index on COMs side should match with the array length here...otherwise exception
		// will come. This has to be managed by IDL generator.
		static JIMarshalUnMarshalHelper()
		{

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

		internal static byte[] readOctetArrayLE(NdrCodec ndr, int length)
		{
			var bytes = new byte[8];
			ndr.ReadOctetArray(bytes,0,8);
			for (var i = 0;i < 4; i++)
			{
				var t = bytes[i];
				bytes[i] = bytes[7 - i];
				bytes[7 - i] = t;
			}
			return bytes;
		}

		internal static void writeOctetArrayLE(NdrCodec ndr, byte[] b)
		{
			for (var i = 0;i < b.Length; i++)
			{
				ndr.WriteUnsignedSmall(b[b.Length - i - 1]);
			}
		}

		internal static void serialize(NdrCodec ndr, Type c, object value, IList defferedPointers, int FLAG)
		{
			if (c.Equals(typeof(JIArray)))
			{
				((JIArray)value).encode(ndr,((JIArray)value).ArrayInstance,defferedPointers,FLAG);
			}
			else
			{
				if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && value is IJIComObject)
				{
					c = typeof(IJIComObject);
				}

				alignMemberWhileEncoding(ndr,c,value);

				if (c.Equals(typeof(JIString)))
				{
					((JIString)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

				if (c.Equals(typeof(JIPointer)))
				{
					((JIPointer)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

				if (c.Equals(typeof(JIStruct)))
				{
					((JIStruct)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

				if (c.Equals(typeof(JIUnion)))
				{
					((JIUnion)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

	//			if (c.equals(JIDispatchImpl.class) || c.equals(IJIDispatch.class))
	//			{
	//				IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
	//				JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
	//				interfacePointer.encode(ndr,defferedPointers,FLAG);
	//				return ;
	//			}
	//
	//			if (c.equals(JIComObjectImpl.class) || c.equals(IJIComObject.class) || c.equals(IJIUnknown.class))
	//			{
	//				JIInterfacePointer interfacePointer = ((IJIComObject)value).getInterfacePointer();
	//				interfacePointer.encode(ndr,defferedPointers,FLAG);
	//				return ;
	//			}


				if (c.Equals(typeof(JIInterfacePointer)))
				{
					((JIInterfacePointer)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

				if (c.Equals(typeof(JIVariant)))
				{
					((JIVariant)value).encode(ndr,defferedPointers,FLAG);
					return;
				}

				if (c.Equals(typeof(JIVariantBody)))
				{
					((JIVariantBody)value).encode(ndr,defferedPointers,FLAG);
					return;
				}


				if (mapOfSerializers[c] == null)
				{
					throw new InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new string[]{c.ToString()}));
				}((SerializerDeserializer)mapOfSerializers.get(c)).serializeData(ndr,value,defferedPointers,FLAG);
			}
		}

		internal static void alignMemberWhileEncoding(NdrCodec ndr, Type c, object obj)
		{
			var index = (double)ndr.Buffer.Index;
			if (c.Equals(typeof(JIStruct)))
			{
				var align = (double)((JIStruct)obj).Alignment;
				var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(JIUnion)))
			{
				var align = (double)((JIUnion)obj).Alignment;
				var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else
			{
			if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(JIVariant)) || c.Equals(typeof(string)) || c.Equals(typeof(JIPointer))) //c.equals(Character.class) || c.equals(Byte.class) ||
			{
				//align with 4 bytes
				long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(double?)))
			{
				//align with 8
				long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(short?)))
			{
				long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);
			}
			}
		}

		internal static void alignMemberWhileDecoding(NdrCodec ndr, Type c, object obj)
		{
			var index = (double)ndr.Buffer.Index;
			if (c.Equals(typeof(JIStruct)))
			{
				var align = (double)((JIStruct)obj).Alignment;
				var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(JIUnion)))
			{
				var align = (double)((JIUnion)obj).Alignment;
				var i = (long)((i = Math.Round(index % align)) == 0 ? 0 : align - i);
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else
			{
			if (c.Equals(typeof(int?)) || c.Equals(typeof(float?)) || c.Equals(typeof(JIVariant)) || c.Equals(typeof(string)) || c.Equals(typeof(JIPointer)))
			{
				//align with 4 bytes
				long i = (i = Math.Round(index % 4.0)) == 0 ? 0 : 4 - i;
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(double?)))
			{
				//align with 8
				long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}
			else if (c.Equals(typeof(short?)))
			{
				long i = (i = Math.Round(index % 2.0)) == 0 ? 0 : 2 - i;
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);
			}
			}
		}


		internal static object deSerialize(NdrCodec ndr, object obj, IList defferedPointers, int FLAG, IDictionary additionalData)
		{
			var c = obj is Type ? (Type)obj : obj.GetType();
            if (c.Equals(typeof(JIArray))) {
                return ((JIArray)obj).decode(ndr, ((JIArray)obj).ArrayClass, ((JIArray)obj).Dimensions, defferedPointers, FLAG, additionalData);
            }

            alignMemberWhileDecoding(ndr, c, obj);


            if (c.Equals(typeof(JIPointer))) {
                var retVal = ((JIPointer)obj).decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIStruct))) {
                var retVal = ((JIStruct)obj).decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIUnion))) {
                var retVal = ((JIUnion)obj).decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            if (c.Equals(typeof(JIString))) {
                var retVal = ((JIString)obj).decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            //This will always be a class
            if (obj.Equals(typeof(JIInterfacePointer))) {
                var retVal = JIInterfacePointer.decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            //This will always be a class
            if (obj.Equals(typeof(JIVariant))) {
                var retVal = JIVariant.decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            //This will always be a class
            if (obj.Equals(typeof(JIVariantBody))) {
                var retVal = JIVariantBody.decode(ndr, defferedPointers, FLAG, additionalData);
                return retVal;
            }

            if (mapOfSerializers[obj] == null) {
                throw new InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), new string[] { obj.ToString() }));
            }
            return ((SerializerDeserializer)mapOfSerializers[obj]).deserializeData(ndr, defferedPointers, additionalData, FLAG);

        }



		internal static int getLengthInBytes(Type c, object obj, int FLAG)
		{
            if (obj != null && obj.GetType().Equals(typeof(JIArray))) {
                return ((JIArray)obj).SizeOfAllElementsInBytes;
            }
            if ((c != typeof(IJIComObject) || c != typeof(IJIDispatch)) && obj is IJIComObject) {
                c = typeof(IJIComObject);
            }

            if (((SerializerDeserializer)mapOfSerializers[c]) == null) {
                throw new InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND), new string[] { c.ToString() }));
            }
            return ((SerializerDeserializer)mapOfSerializers[c]).getLengthInBytes(obj, FLAG);

        }

		private interface SerializerDeserializer
		{
			void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG);
			object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG);
			int getLengthInBytes(object value, int FLAG);
		}


		private class PointerImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIPointer)value).Length;
			}

		}

		private class JIUnsignedIntImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
                serialize(ndr,typeof(int?), (int)((JIUnsignedInteger)value).Value, null,FLAG);
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var b = (int?)deSerialize(ndr,typeof(int?),null,FLAG,additionalData);
				return JIUnsignedFactory.getUnsigned((long)((int)b & 0xFFFFFFFFL), JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 4;
			}

		}

		private class JIDualStringArrayImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				return JIDualStringArray.decode(ndr);
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIDualStringArray)value).Length;
			}

		}

		private class JIUnsignedByteImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
                serialize(ndr,typeof(sbyte?), (sbyte)((JIUnsignedByte)value).Value, null,FLAG);
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var b = (sbyte?)deSerialize(ndr,typeof(sbyte?),null,FLAG,additionalData);
				return JIUnsignedFactory.getUnsigned((short)((sbyte)b & 0xFF), JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE);
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 1;
			}

		}

		private class JIUnsignedShortImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
                serialize(ndr,typeof(short?), (short)((JIUnsignedShort)value).Value, null,FLAG);
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var b = (short?)deSerialize(ndr,typeof(short?),null,FLAG,additionalData);
				return JIUnsignedFactory.getUnsigned((int)((short)b & 0xFFFF), JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 2;
			}
		}

	//	private static class JIUnsignedImpl implements SerializerDeserializer {
	//
	//		public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int FLAG)
	//		{
	//			IJIUnsigned unsigned = (IJIUnsigned)value;
	//			switch(unsigned.getType())
	//			{
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
	//					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedByte.class,value,defferedPointers,FLAG);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
	//					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedShort.class,value,defferedPointers,FLAG);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
	//					JIMarshalUnMarshalHelper.serialize(ndr,JIUnsignedInteger.class,value,defferedPointers,FLAG);
	//					break;
	//
	//				default:
	//					throw new System.InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
	//			}
	//
	//		}
	//
	//		public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int FLAG)
	//		{
	//			IJIUnsigned unsigned = null;
	//			int type = JIFlags.FLAG_NULL;
	//			if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
	//			{
	//				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
	//			}
	//			else
	//			if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
	//			{
	//				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
	//			}
	//			else
	//			if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
	//			{
	//				type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
	//			}
	//
	//			switch(type)
	//			{
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
	//					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedByte.class, defferedPointers, FLAG, additionalData);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
	//					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedShort.class, defferedPointers, FLAG, additionalData);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
	//					unsigned = (IJIUnsigned)JIMarshalUnMarshalHelper.deSerialize(ndr, JIUnsignedInteger.class, defferedPointers, FLAG, additionalData);
	//					break;
	//
	//				default:
	//					throw new System.InvalidOperationException(MessageFormat.format(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_SERDESER_NOT_FOUND),new String[]{"IJIUnsigned#" + unsigned.getType()}));
	//			}
	//
	//			return unsigned;
	//		}
	//
	//		public int getLengthInBytes(Object value,int FLAG)
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
	//				if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE)
	//				{
	//					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE;
	//				}
	//				else
	//				if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT)
	//				{
	//					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
	//				}
	//				else
	//				if ((FLAG & JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT) == JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT)
	//				{
	//					type = JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT;
	//				}
	//			}
	//
	//			switch(type)
	//			{
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_BYTE:
	//					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedByte.class,value,FLAG);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT:
	//					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedShort.class,value,FLAG);
	//					break;
	//
	//				case JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT:
	//					length = JIMarshalUnMarshalHelper.getLengthInBytes(JIUnsignedInteger.class,value,FLAG);
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

		private class StructImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIStruct)value).Length;
			}

		}

		private class UnionImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIUnion)value).Length;
			}

		}


		private class IJIComObjectSerDer : SerializerDeserializer
		{


			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				var ptr = ((IJIComObject)value).internal_getInterfacePointer();
				serialize(ndr, typeof(JIInterfacePointer), ptr, defferedPointers, FLAG);
				if (ptr.CustomObjRef)
				{
					//ask the session now for its marshaller unmarshaller and that should write the object down into the JIInterfacePointer.
					//Where we are right now is where our object needs to be written.

					//TODO we have just written a "reserved" member (before we write the body), it has been observed in WMIO that this reserved member 
					//is the total length of the block, if this is so then the Custom Marshaller for WMIO should overwrite this with the full length.

					//First write the custom marshaller unmarshaller CLSID. Then the object definition.
					var index = ndr.Buffer.Index;
					((IJIComObject)value).CustomObject.encode(ndr, defferedPointers, FLAG);
					var currentIndex = ndr.Buffer.Index;
					var totalLength = currentIndex - index + 48;
					ndr.Buffer.Index = ndr.Buffer.Index - totalLength - 8;
					ndr.WriteUnsignedLong(totalLength + 4);
					ndr.WriteUnsignedLong(totalLength + 4);
					ndr.Buffer.Index = currentIndex;
	//				Hexdump.hexdump(System.out, ndr.getBuffer().getBuffer(), 0, ndr.getBuffer().getIndex());
				}
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var session = (JISession)additionalData[JICallBuilder.CURRENTSESSION];
				var ptr = (JIInterfacePointer)deSerialize(ndr, typeof(JIInterfacePointer), defferedPointers, FLAG, additionalData);
				IJIComObject comObject = new JIComObjectImpl(session, ptr);
				if (ptr != null && ((JIFlags.FLAG_REPRESENTATION_ARRAY & FLAG) != JIFlags.FLAG_REPRESENTATION_ARRAY) && ptr.CustomObjRef)
				{
					//now we need to ask the session for its marshaller unmarshaller based on the CLSID 
					((JIComObjectImpl)comObject).CustomObject = session.getCustomMarshallerUnMarshallerTemplate(ptr.CustomCLSID).decode(comObject, ndr, defferedPointers, FLAG, additionalData);
				}((ArrayList)additionalData.get(JICallBuilder.COMOBJECTS)).add(comObject);
				return comObject;
			}


			public virtual int getLengthInBytes(object value, int FLAG)
			{
				var interfacePointer = ((IJIComObject)value).internal_getInterfacePointer();
				return ((JIInterfacePointer)interfacePointer).Length;
			}

		}

	//	private static class IJIDispatchImpl implements SerializerDeserializer {
	//
	//
	//		public void serializeData(NetworkDataRepresentation ndr,Object value,List defferedPointers,int FLAG)
	//		{
	//			throw new System.InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
	//		}
	//
	//		public Object deserializeData(NetworkDataRepresentation ndr,List defferedPointers, Map additionalData, int FLAG)
	//		{
	//			throw new System.InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
	//		}
	//
	//
	//		public int getLengthInBytes(Object value,int FLAG)
	//		{
	//			IJIComObject unknown = ((JIDispatchImpl)value).getCOMObject();
	//			JIInterfacePointer interfacePointer = new JIInterfacePointer(IJIDispatch.IID,unknown.getInterfacePointer());
	//			return ((JIInterfacePointer)interfacePointer).getLength();
	//		}
	//
	//
	//	}

		private class JIVariant2Impl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIVariantBody)value).LengthInBytes;
			}

		}

		private class JIVariantImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				//4 for pointer and rest for variant2
				try
				{
					return ((JIVariant)value).getLengthInBytes(FLAG);
				}
				catch (JIException e)
				{
					throw new JIRuntimeException(e.ErrorCode);
				}
			}

		}

		private class CharacterImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				ndr.WriteUnsignedSmall((char)(char?)value);
			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var c = new char?((char)ndr.ReadUnsignedSmall());
				return c;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 1;
			}

		}

		private class ByteImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				ndr.WriteUnsignedSmall((sbyte)(sbyte?)value);
			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var c = new sbyte?((sbyte)ndr.ReadUnsignedSmall());
				return c;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 1;
			}

		}

		private class ShortImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = short.MinValue;
				}
				ndr.WriteUnsignedShort((short)(short?)value);


			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var s = new short?((short)ndr.ReadUnsignedShort());
				return s;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				{
					return 2 + 2;
				}
			}

		}

		private class BooleanImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = false;
				}

				if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL)
				{
					ndr.WriteUnsignedShort((bool)(bool?)value == true ? 0xFFFF: 0x0000);
				}
				else
				{
					ndr.WriteBoolean((bool)(bool?)value);
				}

			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				bool? b = null;
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL)
				{
					var s = ndr.ReadUnsignedShort();
					b = s != 0 ? true:false;
				}
				else
				{
					b = Convert.ToBoolean(ndr.ReadBoolean());
				}

				return b;
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
                if ((FLAG & JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == JIFlags.FLAG_REPRESENTATION_VARIANT_BOOL) {
                    return 2;
                }
                return 1;
            }
		}

		private class IntegerImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = int.MinValue;
				}
				ndr.WriteUnsignedLong((int)(int?)value);
			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				return ndr.ReadUnsignedLong();
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
				{
					return 4;
				}
			}

		}

		private class LongImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = long.MinValue;
				}
				ndr.Buffer.align(8);
				Encdec.enc_uint64le((long)(long?)value,ndr.Buffer.Buffer,ndr.Buffer.Index);
				ndr.Buffer.advance(8);
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				ndr.Buffer.align(8);
				var b = new long?(Encdec.dec_uint64le(ndr.Buffer.Buffer,ndr.Buffer.Index));
				ndr.Buffer.advance(8);
				return b;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 8;
			}

		}

		private class DoubleImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = double.NaN;
				}

				ndr.Buffer.align(8);
				Encdec.enc_doublele((double)(double?)value, ndr.Buffer.Buffer,ndr.Buffer.Index);
				ndr.Buffer.advance(8);

			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				ndr.Buffer.align(8);
				var b = new double?(Encdec.dec_doublele(ndr.Buffer.Buffer,ndr.Buffer.Index));
				ndr.Buffer.advance(8);


				return b;
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
				{
					return 8;
				}
			}

		}

		private class JICurrencyImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				var currency = (JICurrency)value;

				var units = currency.Units;
				var fractionalUnits = currency.FractionalUnits;

				double p = units + fractionalUnits / 10000;

				//scale the units by 10000 to remove the decimal and take two's compliment.
				var toSend = ~(int)(p * 10000.00) + 1;

				var toSend2 = toSend.ToString("x");
				var hibytes = 0;
				var lowbytes = 0;
				if (toSend2.Length > 8)
				{
					lowbytes = (int)Convert.ToInt32(toSend2.Substring(8),16);
					hibytes = (int)Convert.ToInt32(toSend2.Substring(0,8),16);
				}
				else
				{
					lowbytes = toSend;
					if (toSend < 0)
					{
						hibytes = -1;
					}
				}

	//			now align by 8 bytes, since this is struct has a hyper, which I don't support yet
				var index = (double)ndr.Buffer.Index;
				long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
				ndr.writeOctetArray(new sbyte[(int)i],0,(int)i);

				var @struct = new JIStruct();
				try
				{
					@struct.addMember(lowbytes);
					@struct.addMember(hibytes);
				}
				catch (JIException)
				{

				}
				serialize(ndr,typeof(JIStruct),@struct,null,FLAG);

			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				//first align
				var index = (double)ndr.Buffer.Index;
				long i = (i = Math.Round(index % 8.0)) == 0 ? 0 : 8 - i;
				ndr.readOctetArray(new sbyte[(int)i],0,(int)i);

				//now read the low byte
				var lowbyte = ndr.ReadUnsignedLong();
				//hibyte
				var hibyte = ndr.ReadUnsignedLong();
				if (hibyte < 0)
				{
					lowbyte = -1 * Math.Abs(lowbyte);
				}

				//String newValue = Integer.toHexString(hibyte) + Integer.toHexString(lowbyte);
				//long value = Long.parseLong(newValue,16);
				return new JICurrency((int)((lowbyte - lowbyte % 10000) / 10000),(int)(lowbyte % 10000));


			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
				{
					return 4 + 4;
				}
			}
		}
		//will only get called from a variant.
		private class DateImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
	//			if (value == null && FLAG == JIFlags.FLAG_REPRESENTATION_ARRAY)
	//			{
	//				value = new Double(Double.NaN);
	//			}

				ndr.Buffer.align(8);
				Encdec.enc_doublele(convertMillisecondsToWindowsTime(((DateTime)value).Ticks), ndr.Buffer.Buffer,ndr.Buffer.Index);
				ndr.Buffer.advance(8);

			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				ndr.Buffer.align(8);
				var b = new DateTime(convertWindowsTimeToMilliseconds(Encdec.dec_doublele(ndr.Buffer.Buffer,ndr.Buffer.Index)));
				ndr.Buffer.advance(8);
				return b;
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
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
            internal virtual long convertWindowsTimeToMilliseconds(double comTime)
			{
				long result = 0;

				// code from jacobgen:
				comTime = comTime - 25569D;
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
			internal virtual double convertMillisecondsToWindowsTime(long milliseconds)
			{
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

		private class FloatImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if (value == null)
				{
					value = float.NaN;
				}
				ndr.Buffer.align(4);
				Encdec.enc_floatle((float)(float?)value, ndr.Buffer.Buffer,ndr.Buffer.Index);
				ndr.Buffer.advance(4);

			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				ndr.Buffer.align(4);
				var b = new float?(Encdec.dec_floatle(ndr.Buffer.Buffer,ndr.Buffer.Index));
				ndr.Buffer.advance(4);

				return b;
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
				{
					return 4;
				}

			}

		}

		private class StringImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING)
				{
					throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
				}

				var str = (string)value;
				if (str == null)
				{
					str = "";
				}
				//BSTR encoding
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR)
				{
					sbyte[] strBytes = null;
					try
					{
						strBytes = str.GetBytes("UTF-16LE");
					}
					catch (UnsupportedEncodingException)
					{
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
					while (i < strBytes.Length)
					{
						//ndr.writeUnsignedShort(str.charAt(i));
						ndr.WriteUnsignedSmall(strBytes[i]);
						i++;
					}

				}
				else //Normal String
				{
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
				{
					// the String is written as "short" so length is strlen/2+1
					var strlen = (int)Math.Round(str.Length / 2.0);

					ndr.WriteUnsignedLong(strlen + 1);
					ndr.WriteUnsignedLong(0);
					ndr.WriteUnsignedLong(strlen + 1);
					if (str.Length != 0)
					{
						ndr.WriteCharacterArray(str.ToCharArray(),0,str.Length);
						//odd length
						if (str.Length % 2 != 0)
						{
							//add a 0
							ndr.WriteUnsignedSmall(0);
						}
					}

					//null termination
					ndr.WriteUnsignedShort(0);
				}
				else
				{
					if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
					{

						sbyte[] strBytes = null;
						try
						{
							strBytes = str.GetBytes("UTF-16LE");
						}
						catch (UnsupportedEncodingException)
						{
							throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
						}

						//bytes + 1
						ndr.WriteUnsignedLong(strBytes.Length / 2 + 1);
						ndr.WriteUnsignedLong(0);
						ndr.WriteUnsignedLong(strBytes.Length / 2 + 1);
						//write an array of unsigned shorts
						var i = 0;
						while (i < strBytes.Length)
						{
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

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				if ((FLAG & JIFlags.FLAG_REPRESENTATION_VALID_STRING) != JIFlags.FLAG_REPRESENTATION_VALID_STRING)
				{
					throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_INVALID);
				}
				var retVal = -1;
				//StringBuffer buffer = new StringBuffer();
				string retString = null;
				try
				{

					//BSTR Decoding
					if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR)
					{
						//Read for user
						ndr.ReadUnsignedLong(); //eating max length
						ndr.ReadUnsignedLong(); //eating length in bytes
						var actuallength = ndr.ReadUnsignedLong() * 2;
						var buffer = new sbyte[actuallength];
						var i = 0;
						while (i < actuallength)
						{
							retVal = ndr.ReadUnsignedSmall();
							buffer[i] = (sbyte)retVal;
							i++;
						}

						retString = StringHelperClass.NewString(buffer, "UTF-16LE");

					}
					else //Normal String
					{
					if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
					{
						{
							var actuallength = ndr.ReadUnsignedLong(); //max length
							if (actuallength == 0)
							{
								return null;
							}

							ndr.ReadUnsignedLong(); //eating offset
							ndr.ReadUnsignedLong(); //eating actuallength again
							//now read array.
							var ret = new char[actuallength * 2 - 2];
							//read including the unsigned short (null chars)
							ndr.ReadCharacterArray(ret,0,actuallength * 2 - 2);
							if (ret[ret.Length - 1] == '0')
							{
								retString = new string(ret,0,ret.Length - 1);
							}
							else
							{
								retString = new string(ret);
							}

							ndr.ReadUnsignedShort();
						}
					}
					else
					{
					if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
					{

						{
							var maxlength = ndr.ReadUnsignedLong();
							if (maxlength == 0)
							{
								return null;
							}
							ndr.ReadUnsignedLong(); //eating offset
							var actuallength = ndr.ReadUnsignedLong() * 2;
							var buffer = new sbyte[actuallength - 2];
							var i = 0;
							//last 2 bytes , null termination will be eaten outside the loop
							while (i < actuallength - 2)
							{
								retVal = ndr.ReadUnsignedSmall();
								buffer[i] = (sbyte)retVal;
								i++;
							}
							if (actuallength != 0)
							{
								ndr.ReadUnsignedShort();
							}

							retString = StringHelperClass.NewString(buffer, "UTF-16LE");

						}

					}
					}
					}
				}
				catch (UnsupportedEncodingException)
				{
					throw new JIRuntimeException(JIErrorCodes.JI_UTIL_STRING_DECODE_CHARSET);
				}

				return retString;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				//rough estimate, this will vary from string to string

				var length = 4 + 4 + 4; //max len, offset ,actual length

				if (!((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_BSTR) == JIFlags.FLAG_REPRESENTATION_STRING_BSTR))
				{
					length = length + 2; //adding null termination
				}

				if ((FLAG & JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
				{
					length = length + ((string)value).Length; //this is only a character array, no unicode, each char is writen in 1 byte "abcd" --> ab, cd ,00 ; "abcde" --> ab,cd,e0, 00
					if (!(((string)value).Length % 2 == 0)) //odd
					{
						length++;
					}
				}
				else
				{
	//				if (value == null)
	//				{
	//					int i = 0;
	//				}
					length = length + ((string)value).Length * 2; //these are both unicode (utf-16le)
				}


				return length;
			}

		}


		private class JIStringImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				var length = 4;

				if (((JIString)value).String == null)
				{
					return length;
				}


				//for LPWSTR and BSTR adding 2 for the null character.
				length = length + (((JIString)value).Type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ? 0 : 2);
				//Pointer referentId --> USER
				return length + JIMarshalUnMarshalHelper.getLengthInBytes(typeof(string),((JIString)value).String,((JIString)value).Type | FLAG);
			}


		}


		private class UUIDImpl : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				try
				{
					((UUID)value).Encode(ndr,ndr.Buffer);
				}
				catch (NdrException e)
				{
					Log.Logger.Error(e, "UUIDImpl","serializeData",e);
				}
			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				var ret = new UUID();
				try
				{
					ret.Decode(ndr,ndr.Buffer);
				}
				catch (NdrException e)
				{
					Log.Logger.Error(e, "UUIDImpl","deserializeData",e);
					ret = null;
				}
				return ret;
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return 16;
			}

		}

		private class MInterfacePointerImpl : SerializerDeserializer
		{

			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				throw new InvalidOperationException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_INCORRECT_CALL));
			}

			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIInterfacePointer)value).Length;
			}
		}

		private class MInterfacePointerImpl2 : SerializerDeserializer
		{
			public virtual void serializeData(NdrCodec ndr, object value, IList defferedPointers, int FLAG)
			{
				((JIInterfacePointerBody)value).encode(ndr,FLAG);
			}
			public virtual object deserializeData(NdrCodec ndr, IList defferedPointers, IDictionary additionalData, int FLAG)
			{
				return JIInterfacePointerBody.decode(ndr,FLAG);
			}
			public virtual int getLengthInBytes(object value, int FLAG)
			{
				return ((JIInterfacePointerBody)value).Length;
			}
		}
	}

}
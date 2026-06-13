// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Automation;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Buffers.Binary;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Marshal helper
/// </summary>
internal static class MarshalUnMarshalHelper
{
    /// <summary>
    /// Serialize
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    internal static void Serialize(NdrCodec ndr, Type c, object value, CodecContext context = null)
    {
        if (context == null)
        {
            context = new CodecContext();
        }
        if (c.Equals(typeof(ComArray)))
        {
            ((ComArray)value).Encode(ndr, ((ComArray)value).ArrayInstance, context);
        }
        else
        {
            if ((c != typeof(IComObject) || c != typeof(IDispatch)) && value is IComObject)
            {
                c = typeof(IComObject);
            }

            AlignMemberWhileEncoding(ndr, c, value);

            if (c.Equals(typeof(ComString)))
            {
                ((ComString)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(ComPointer)))
            {
                ((ComPointer)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(Struct)))
            {
                ((Struct)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(Union)))
            {
                ((Union)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(InterfacePointer)))
            {
                ((InterfacePointer)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(Variant)))
            {
                ((Variant)value).Encode(ndr, context);
                return;
            }

            if (c.Equals(typeof(VariantBody)))
            {
                ((VariantBody)value).Encode(ndr, context);
                return;
            }
            if (!kMapOfSerializers.ContainsKey(c))
            {
                throw new InvalidOperationException(
                    string.Format(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_SERDESER_NOT_FOUND), c));
            }
            kMapOfSerializers[c].SerializeData(ndr, value, context);
        }
    }

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>The object reconstructed from the serialized COM wire representation.</returns>
    internal static object Deserialize(NdrCodec ndr, object obj, CodecContext context = null)
    {
        if (context == null)
        {
            context = new CodecContext();
        }

        var c = obj is Type t ? t : obj.GetType();
        if (c.Equals(typeof(ComArray)))
        {
            return ((ComArray)obj).Decode(ndr, ((ComArray)obj).ArrayType,
                ((ComArray)obj).Dimensions, context);
        }

        AlignMemberWhileDecoding(ndr, c, obj);

        if (c.Equals(typeof(ComPointer)))
        {
            var retVal = ((ComPointer)obj).Decode(ndr, context);
            return retVal;
        }

        if (c.Equals(typeof(Struct)))
        {
            var retVal = ((Struct)obj).Decode(ndr, context);
            return retVal;
        }

        if (c.Equals(typeof(Union)))
        {
            var retVal = ((Union)obj).Decode(ndr, context);
            return retVal;
        }

        if (c.Equals(typeof(ComString)))
        {
            var retVal = ((ComString)obj).Decode(ndr, context);
            return retVal;
        }

        // This will always be a class
        if (c.Equals(typeof(InterfacePointer)))
        {
            var retVal = InterfacePointer.Decode(ndr, context);
            return retVal;
        }

        // This will always be a class
        if (c.Equals(typeof(Variant)))
        {
            var retVal = Variant.Decode(ndr, context);
            return retVal;
        }

        // This will always be a class
        if (c.Equals(typeof(VariantBody)))
        {
            var retVal = VariantBody.Decode(ndr, context);
            return retVal;
        }

        if (!kMapOfSerializers.ContainsKey(c))
        {
            throw new InvalidOperationException(string.Format(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_SERDESER_NOT_FOUND), obj));
        }
        return kMapOfSerializers[c].DeserializeData(ndr, context);
    }

    /// <summary>
    /// Get length in bytes
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    /// <returns>The requested length in bytes value.</returns>
    internal static int GetLengthInBytes(Type c, object obj, int flag = InteropFlags.FLAG_NULL)
    {
        if (obj != null && obj.GetType().Equals(typeof(ComArray)))
        {
            return ((ComArray)obj).SizeOfAllElementsInBytes;
        }
        if ((c != typeof(IComObject) || c != typeof(IDispatch)) && obj is IComObject)
        {
            c = typeof(IComObject);
        }

        if (kMapOfSerializers[c] == null)
        {
            throw new InvalidOperationException(string.Format(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_SERDESER_NOT_FOUND), c));
        }
        return kMapOfSerializers[c].GetDataLengthInBytes(obj, flag);
    }

    /// <summary>
    /// Align on write
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    private static void AlignMemberWhileEncoding(NdrCodec ndr, Type c, object obj)
    {
        var index = (double)ndr.Buffer.Index;
        if (c.Equals(typeof(Struct)))
        {
            ndr.FillAligned(((Struct)obj).Alignment);
        }
        else if (c.Equals(typeof(Union)))
        {
            ndr.FillAligned(((Union)obj).Alignment);
        }
        else if (c.Equals(typeof(int)) ||
                c.Equals(typeof(float)) ||
                c.Equals(typeof(Variant)) ||
                c.Equals(typeof(string)) ||
                c.Equals(typeof(ComPointer)))
        {
            // align with 4
            ndr.FillAligned(4);
        }
        else if (c.Equals(typeof(double)))
        {
            // align with 8
            ndr.FillAligned(8);
        }
        else if (c.Equals(typeof(short)))
        {
            ndr.FillAligned(2);
        }
    }

    /// <summary>
    /// Align to read
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    private static void AlignMemberWhileDecoding(NdrCodec ndr, Type c, object obj)
    {
        if (c.Equals(typeof(Struct)))
        {
            ndr.SkipAligned(((Struct)obj).Alignment);
        }
        else if (c.Equals(typeof(Union)))
        {
            ndr.SkipAligned(((Union)obj).Alignment);
        }
        else if (c.Equals(typeof(int)) ||
                c.Equals(typeof(float)) ||
                c.Equals(typeof(Variant)) ||
                c.Equals(typeof(string)) ||
                c.Equals(typeof(ComPointer)))
        {
            // align with 4
            ndr.SkipAligned(4);
        }
        else if (c.Equals(typeof(double)))
        {
            // align with 8
            ndr.SkipAligned(8);
        }
        else if (c.Equals(typeof(short)))
        {
            ndr.SkipAligned(2);
        }
    }

    /// <summary>
    /// Read buffer
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="length">Number of bytes or elements to process.</param>
    /// <returns>The sequence of octet array le values produced by the operation.</returns>
    internal static byte[] ReadOctetArrayLE(NdrCodec ndr, int length)
    {
        System.Diagnostics.Debug.Assert(length == 8); // TODO: Should be generic.
        var bytes = new byte[8];
        ndr.ReadOctetArray(bytes, 0, 8);
        for (var i = 0; i < 4; i++)
        {
            var t = bytes[i];
            bytes[i] = bytes[7 - i];
            bytes[7 - i] = t;
        }
        return bytes;
    }

    /// <summary>
    /// Write buffer
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="b">Wire-format bytes consumed or produced by the operation.</param>
    internal static void WriteOctetArrayLE(NdrCodec ndr, byte[] b)
    {
        for (var i = 0; i < b.Length; i++)
        {
            ndr.WriteUnsignedSmall(b[b.Length - i - 1]);
        }
    }

    /// <summary>
    /// Serializer interface
    /// </summary>
    private interface ISerializerDeserializer
    {
        /// <summary>
        /// Serialize data
        /// </summary>
        /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
        /// <param name="value">Value being stored, encoded, or assigned.</param>
        /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
        void SerializeData(NdrCodec ndr, object value, CodecContext context);

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
        /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
        /// <returns>The decoded COM values produced from the serialized byte stream.</returns>
        object DeserializeData(NdrCodec ndr, CodecContext context);

        /// <summary>
        /// Get length in bytes
        /// </summary>
        /// <param name="value">Value being stored, encoded, or assigned.</param>
        /// <param name="flag">Flag value that controls the requested operation.</param>
        /// <returns>The requested data length in bytes value.</returns>
        int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL);
    }

    /// <inheritdoc/>
    private sealed class PointerImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => ((ComPointer)value).Length;
    }

    /// <inheritdoc/>
    private sealed class UIntImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            Serialize(ndr, typeof(uint), (uint)value, context);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            (uint)Deserialize(ndr, typeof(uint), context);

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 4;
    }

    /// <inheritdoc/>
    private sealed class DualStringArrayImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) => DualStringArray.Decode(ndr);

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => ((DualStringArray)value).Length;
    }

    /// <inheritdoc/>
    private sealed class ByteImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            Serialize(ndr, typeof(byte), (byte)value, context);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            (byte)Deserialize(ndr, typeof(byte), context);

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 1;
    }

    /// <inheritdoc/>
    private sealed class UShortImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            Serialize(ndr, typeof(ushort), (ushort)value, context);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            (ushort)Deserialize(ndr, typeof(ushort), context);

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 2;
    }

    /// <inheritdoc/>
    private sealed class StructImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => ((Struct)value).Length;
    }

    /// <inheritdoc/>
    private sealed class UnionImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => ((Union)value).Length;
    }

    /// <inheritdoc/>
    private sealed class ComObjectSerDer : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            var ptr = ((IComObjectInternal)value).GetInterfacePointer();
            Serialize(ndr, typeof(InterfacePointer), ptr, context);
            if (ptr.IsCustomObjRef)
            {
                // ask the session now for its marshaller unmarshaller and that should
                // write the object down into the <see cref="InterfacePointer"/>.
                // Where we are right now is where our object needs to be written.

                // TODO we have just written a "reserved" member (before we write the body),
                // it has been observed in WMIO that this reserved member
                // is the total length of the block, if this is so then the Custom Marshaller
                // for WMIO should overwrite this with the full length.

                // First write the custom marshaller unmarshaller CLSID. Then the object definition.
                var index = ndr.Buffer.Index;
                ((IComObject)value).CustomObject.Encode(ndr, context);
                var currentIndex = ndr.Buffer.Index;
                var totalLength = currentIndex - index + 48;
                ndr.Buffer.Index = ndr.Buffer.Index - totalLength - 8;
                ndr.WriteUnsignedLong(totalLength + 4);
                ndr.WriteUnsignedLong(totalLength + 4);
                ndr.Buffer.Index = currentIndex;
            }
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            var session = context.CurrentSession;
            var ptr = (InterfacePointer)Deserialize(ndr, typeof(InterfacePointer), context);
            IComObject comObject = new ComObjectImpl(session, ptr);
            if (ptr != null &&
                ((InteropFlags.FLAG_REPRESENTATION_ARRAY & context.Flag) != InteropFlags.FLAG_REPRESENTATION_ARRAY) &&
                ptr.IsCustomObjRef)
            {
                // now we need to ask the session for its marshaller unmarshaller based on the CLSID
                var customNdr = ndr;
                if (ptr.GetObjectReference(InterfacePointer.OBJREF_CUSTOM) is CustomInterfacePointerBody customBody &&
                    customBody.ObjectData.Length > 0)
                {
                    customNdr = new NdrCodec { Buffer = new NdrBuffer(customBody.ObjectData, 0), Format = ndr.Format };
                    customNdr.Buffer.Length = customBody.ObjectData.Length;
                }
                ((ComObjectImpl)comObject).CustomObject = session.GetCustomMarshallerUnMarshallerTemplate(
                    ptr.CustomCLSID).Decode(comObject, customNdr, context);
            }
            context.ComObjects.Add(comObject);
            return comObject;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL)
        {
            var interfacePointer = ((IComObjectInternal)value).GetInterfacePointer();
            return interfacePointer.Length;
        }
    }

    /// <inheritdoc/>
    private sealed class VariantBodyImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => ((VariantBody)value).LengthInBytes;
    }

    /// <inheritdoc/>
    private sealed class VariantImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL)
        {
            // 4 for pointer and rest for variant2
            try
            {
                return ((Variant)value).GetLengthInBytes(flag);
            }
            catch (InteropException e)
            {
                throw new InteropRuntimeException(e.ErrorCode);
            }
        }
    }

    /// <inheritdoc/>
    private sealed class CharacterImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            ndr.WriteUnsignedSmall((char)value);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) => (char)ndr.ReadUnsignedSmall();

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 1;
    }

    /// <inheritdoc/>
    private sealed class SByteImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            ndr.WriteUnsignedSmall((sbyte)value);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) => (sbyte)ndr.ReadUnsignedSmall();

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 1;
    }

    /// <inheritdoc/>
    private sealed class ShortImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = short.MinValue;
            }
            ndr.WriteUnsignedShort((short)value);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            var s = (short)ndr.ReadUnsignedShort();
            return s;
        }

        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) =>
            2 + 2; // ????
    }

    /// <inheritdoc/>
    private sealed class BooleanImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = false;
            }
            if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL)
            {
                ndr.WriteUnsignedShort((bool)value == true ? 0xFFFF : 0x0000);
            }
            else
            {
                ndr.WriteBoolean((bool)value);
            }
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            bool b;
            if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL)
            {
                var s = ndr.ReadUnsignedShort();
                b = s != 0 ? true : false;
            }
            else
            {
                b = Convert.ToBoolean(ndr.ReadBoolean());
            }
            return b;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL)
        {
            if ((flag & InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL) == InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL)
            {
                return 2;
            }
            return 1;
        }
    }

    /// <inheritdoc/>
    private sealed class IntegerImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = int.MinValue;
            }
            ndr.WriteUnsignedLong((int)value);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) => ndr.ReadUnsignedLong();

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 4;
    }

    /// <inheritdoc/>
    private sealed class LongImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = long.MinValue;
            }
            ndr.Buffer.Align(8); // needed?
            BinaryPrimitives.WriteInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(long)), (long)value);
            ndr.Buffer.Advance(8);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            ndr.Buffer.Align(8);// needed?
            var b = BinaryPrimitives.ReadInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(long)));
            ndr.Buffer.Advance(8);
            return b;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 8;
    }

    /// <inheritdoc/>
    private sealed class DoubleImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = double.NaN;
            }
            ndr.Buffer.Align(8);
            BinaryPrimitives.WriteInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(double)), BitConverter.DoubleToInt64Bits((double)value));
            ndr.Buffer.Advance(8);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            ndr.Buffer.Align(8);
            var b = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(double))));
            ndr.Buffer.Advance(8);
            return b;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 8;
    }

    /// <inheritdoc/>
    private sealed class CurrencyImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value,
            CodecContext context)
        {
            var currency = (Currency)value;

            var units = currency.Units;
            var fractionalUnits = currency.FractionalUnits;

            double p = units + (fractionalUnits / 10000);

            // scale the units by 10000 to remove the decimal and take two's compliment.
            var toSend = ~(int)(p * 10000.00) + 1;
            var toSend2 = toSend.ToString("x");
            var hibytes = 0;

            int lowbytes;
            if (toSend2.Length > 8)
            {
                lowbytes = Convert.ToInt32(toSend2.Substring(8), 16);
                hibytes = Convert.ToInt32(toSend2.Substring(0, 8), 16);
            }
            else
            {
                lowbytes = toSend;
                if (toSend < 0)
                {
                    hibytes = -1;
                }
            }

            // now align by 8 bytes, since this is struct has a hyper, which I don't support yet
            ndr.FillAligned(8);

            var strukt = new Struct();
            try
            {
                strukt.AddMember(lowbytes);
                strukt.AddMember(hibytes);
            }
            catch (InteropException)
            {
            }
            Serialize(ndr, typeof(Struct), strukt, context);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            // first align
            ndr.SkipAligned(8);

            // now read the low byte
            var lowbyte = ndr.ReadUnsignedLong();
            // hibyte
            var hibyte = ndr.ReadUnsignedLong();
            if (hibyte < 0)
            {
                lowbyte = -1 * Math.Abs(lowbyte);
            }

            // String newValue = Integer.toHexString(hibyte) + Integer.toHexString(lowbyte);
            // long value = Long.parseLong(newValue,16);
            return new Currency((lowbyte - (lowbyte % 10000)) / 10000, lowbyte % 10000);
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 4 + 4;
    }

    /// <inheritdoc/>
    private sealed class DateImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            ndr.Buffer.Align(8);
            BinaryPrimitives.WriteInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(double)), BitConverter.DoubleToInt64Bits(((DateTime)value).ToOADate()));
            ndr.Buffer.Advance(8);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            ndr.Buffer.Align(8);
            var b = DateTime.FromOADate(BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(double)))));
            ndr.Buffer.Advance(8);
            return b;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 8;
    }

    /// <inheritdoc/>
    private sealed class FloatImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if (value == null)
            {
                value = float.NaN;
            }
            ndr.Buffer.Align(4);
            BinaryPrimitives.WriteInt32LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(float)), BitConverter.SingleToInt32Bits((float)value));
            ndr.Buffer.Advance(4);
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            ndr.Buffer.Align(4);
            var b = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(ndr.Buffer.Buf.AsSpan(ndr.Buffer.Index, sizeof(float))));
            ndr.Buffer.Advance(4);
            return b;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 4;
    }

    /// <inheritdoc/>
    private sealed class StringImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_VALID_STRING) != InteropFlags.FLAG_REPRESENTATION_VALID_STRING)
            {
                throw new InteropRuntimeException((int)ErrorCode.INTEROP_UTIL_STRING_INVALID);
            }
            var str = (string)value;
            if (str == null)
            {
                str = "";
            }
            // BSTR encoding
            if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_BSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
            {
                byte[] strBytes;
                try
                {
                    strBytes = str.GetBytes("UTF-16LE");
                }
                catch (ArgumentException)
                {
                    throw new InteropRuntimeException((int)ErrorCode.INTEROP_UTIL_STRING_DECODE_CHARSET);
                }
                // NDR representation Max count, then offset, then, actual count
                // length of String (Maximum count)
                ndr.WriteUnsignedLong(strBytes.Length / 2);
                // last index of String (length in bytes)
                ndr.WriteUnsignedLong(strBytes.Length);
                // length of String Again !! (Actual count)
                ndr.WriteUnsignedLong(strBytes.Length / 2);
                // write an array of unsigned shorts
                var i = 0;
                while (i < strBytes.Length)
                {
                    // ndr.writeUnsignedShort(str.charAt(i));
                    ndr.WriteUnsignedSmall(strBytes[i]);
                    i++;
                }
            }
            else // Normal String
            {
                if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
                {
                    // the String is written as "short" so length is strlen/2+1
                    var strlen = (int)Math.Round(str.Length / 2.0);

                    ndr.WriteUnsignedLong(strlen + 1);
                    ndr.WriteUnsignedLong(0);
                    ndr.WriteUnsignedLong(strlen + 1);
                    if (str.Length != 0)
                    {
                        ndr.WriteCharacterArray(str.ToCharArray(), 0, str.Length);
                        // odd length
                        if (str.Length % 2 != 0)
                        {
                            // add a 0
                            ndr.WriteUnsignedSmall(0);
                        }
                    }

                    // null termination
                    ndr.WriteUnsignedShort(0);
                }
                else
                {
                    if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
                    {
                        byte[] strBytes;
                        try
                        {
                            strBytes = str.GetBytes("UTF-16LE");
                        }
                        catch (ArgumentException)
                        {
                            throw new InteropRuntimeException((int)ErrorCode.INTEROP_UTIL_STRING_DECODE_CHARSET);
                        }

                        // bytes + 1
                        ndr.WriteUnsignedLong((strBytes.Length / 2) + 1);
                        ndr.WriteUnsignedLong(0);
                        ndr.WriteUnsignedLong((strBytes.Length / 2) + 1);
                        // write an array of unsigned shorts
                        var i = 0;
                        while (i < strBytes.Length)
                        {
                            // ndr.writeUnsignedShort(str.charAt(i));
                            ndr.WriteUnsignedSmall(strBytes[i]);
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

                        // null termination
                        ndr.WriteUnsignedShort(0);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_VALID_STRING) != InteropFlags.FLAG_REPRESENTATION_VALID_STRING)
            {
                throw new InteropRuntimeException((int)ErrorCode.INTEROP_UTIL_STRING_INVALID);
            }
            // StringBuffer buffer = new StringBuffer();
            string retString = null;
            try
            {
                int retVal;
                // BSTR Decoding
                if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_BSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_BSTR)
                {
                    // Read for user
                    ndr.ReadUnsignedLong(); // eating max length
                    ndr.ReadUnsignedLong(); // eating length in bytes
                    var actuallength = ndr.ReadUnsignedLong() * 2;
                    var buffer = new byte[actuallength];
                    var i = 0;
                    while (i < actuallength)
                    {
                        retVal = ndr.ReadUnsignedSmall();
                        buffer[i] = (byte)retVal;
                        i++;
                    }

                    retString = StringHelperClass.NewString(buffer, "UTF-16LE");
                }
                else // Normal String
                {
                    if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
                    {
                        var actuallength = ndr.ReadUnsignedLong(); // max length
                        if (actuallength == 0)
                        {
                            return null;
                        }

                        ndr.ReadUnsignedLong(); // eating offset
                        ndr.ReadUnsignedLong(); // eating actuallength again
                                                // now read array.
                        var ret = new char[(actuallength * 2) - 2];
                        // read including the unsigned short (null chars)
                        ndr.ReadCharacterArray(ret, 0, (actuallength * 2) - 2);
                        if (ret[ret.Length - 1] == '0')
                        {
                            retString = new string(ret, 0, ret.Length - 1);
                        }
                        else
                        {
                            retString = new string(ret);
                        }

                        ndr.ReadUnsignedShort();
                    }
                    else
                    {
                        if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)
                        {
                            var maxlength = ndr.ReadUnsignedLong();
                            if (maxlength == 0)
                            {
                                return null;
                            }
                            ndr.ReadUnsignedLong(); // eating offset
                            var actuallength = ndr.ReadUnsignedLong() * 2;
                            var buffer = new byte[actuallength - 2];
                            var i = 0;
                            // last 2 bytes, null termination will be eaten outside the loop
                            while (i < actuallength - 2)
                            {
                                retVal = ndr.ReadUnsignedSmall();
                                buffer[i] = (byte)retVal;
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
            catch (ArgumentException)
            {
                throw new InteropRuntimeException((int)ErrorCode.INTEROP_UTIL_STRING_DECODE_CHARSET);
            }
            return retString;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL)
        {
            // rough estimate, this will vary from string to string

            var length = 4 + 4 + 4; // max len, offset,actual length
            if (!((flag & InteropFlags.FLAG_REPRESENTATION_STRING_BSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_BSTR))
            {
                length += 2; // adding null termination
            }

            if ((flag & InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR) == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR)
            {
                length += ((string)value).Length; // this is only a character array, no unicode, each char is writen in 1 byte "abcd" --> ab, cd,00 ; "abcde" --> ab,cd,e0, 00
                if (!(((string)value).Length % 2 == 0)) // odd
                {
                    length++;
                }
            }
            else
            {
                //                if (value == null)
                //                {
                //                    int i = 0;
                //                }
                length += ((string)value).Length * 2; // these are both unicode (utf-16le)
            }
            return length;
        }
    }

    /// <inheritdoc/>
    private sealed class ComStringImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL)
        {
            var length = 4;

            if (((ComString)value).String == null)
            {
                return length;
            }
            // for LPWSTR and BSTR adding 2 for the null character.
            length += ((ComString)value).Type == InteropFlags.FLAG_REPRESENTATION_STRING_LPCTSTR ? 0 : 2;
            // Pointer referentId --> USER
            return length + GetLengthInBytes(typeof(string), ((ComString)value).String, ((ComString)value).Type | flag);
        }
    }

    /// <inheritdoc/>
    private sealed class UUIDImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context)
        {
            try
            {
                ((UUID)value).Encode(ndr, ndr.Buffer);
            }
            catch (NdrException e)
            {
                Log.Logger.Error(e, "UUIDImpl serializeData");
            }
        }

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context)
        {
            var ret = new UUID();
            try
            {
                ret.Decode(ndr, ndr.Buffer);
            }
            catch (NdrException e)
            {
                Log.Logger.Error(e, "UUIDImpl deserializeData", e);
                ret = null;
            }
            return ret;
        }

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) => 16;
    }

    /// <inheritdoc/>
    private sealed class MInterfacePointerImpl : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            throw new InvalidOperationException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_UTIL_INCORRECT_CALL));

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) =>
            ((InterfacePointer)value).Length;
    }

    /// <inheritdoc/>
    private sealed class MInterfacePointerImpl2 : ISerializerDeserializer
    {
        /// <inheritdoc/>
        public void SerializeData(NdrCodec ndr, object value, CodecContext context) =>
            ((InterfacePointerBody)value).Encode(ndr, context.Flag);

        /// <inheritdoc/>
        public object DeserializeData(NdrCodec ndr, CodecContext context) =>
            InterfacePointerBody.Decode(ndr, context.Flag);

        /// <inheritdoc/>
        public int GetDataLengthInBytes(object value, int flag = InteropFlags.FLAG_NULL) =>
            ((InterfacePointerBody)value).Length;
    }

    private static readonly Dictionary<Type, ISerializerDeserializer> kMapOfSerializers =
        new Dictionary<Type, ISerializerDeserializer>
        {
            [typeof(DateTime)] = new DateImpl(),
            [typeof(Currency)] = new CurrencyImpl(),
            [typeof(VariantBody)] = new VariantBodyImpl(),
            [typeof(Variant)] = new VariantImpl(),
            [typeof(double)] = new DoubleImpl(),
            [typeof(bool)] = new BooleanImpl(),
            [typeof(float)] = new FloatImpl(),
            [typeof(string)] = new StringImpl(),
            [typeof(UUID)] = new UUIDImpl(),
            [typeof(byte)] = new ByteImpl(),
            [typeof(sbyte)] = new SByteImpl(),
            [typeof(ushort)] = new UShortImpl(),
            [typeof(short)] = new ShortImpl(),
            [typeof(uint)] = new UIntImpl(),
            [typeof(int)] = new IntegerImpl(),
            [typeof(long)] = new LongImpl(),
            [typeof(ulong)] = new LongImpl(),
            [typeof(char)] = new CharacterImpl(),
            [typeof(InterfacePointer)] = new MInterfacePointerImpl(),
            [typeof(InterfacePointerBody)] = new MInterfacePointerImpl2(),
            [typeof(IDispatch)] = new ComObjectSerDer(),
            [typeof(IComObject)] = new ComObjectSerDer(),
            [typeof(ComPointer)] = new PointerImpl(),
            [typeof(Struct)] = new StructImpl(),
            [typeof(Union)] = new UnionImpl(),
            [typeof(ComString)] = new ComStringImpl(),
            [typeof(DualStringArray)] = new DualStringArrayImpl()
        };
}

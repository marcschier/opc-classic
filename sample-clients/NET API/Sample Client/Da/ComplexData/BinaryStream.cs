//============================================================================
// TITLE: BinaryStream.cs
//
// CONTENTS:
// 
// A base class that reading/writing complex data item from/to a binary buffer.
//
// (c) Copyright 2002-2003 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2003/09/2 RSA   First release.

using System;
using System.Collections;

namespace Opc.Cpx
{
	/// <summary>
	/// Stores a value with an associated name and/or type.
	/// </summary>
	public class ComplexValue
	{
		public string Name  = null;
		public string Type  = null;
		public object Value = null;
	}

	/// <summary>
	/// Stores the current serialization context.
	/// </summary>
	public struct Context
	{
		public byte[]          Buffer;
		public int             Index;
		public TypeDictionary  Dictionary;
		public TypeDescription Type;
		public bool            BigEndian;
		public uint            CharWidth;
		public string          StringEncoding;
		public string          FloatFormat;

		public Context(byte[] buffer)
		{
			Buffer         = buffer;
			Index          = 0;
			Dictionary     = null;
			Type           = null;
			BigEndian      = false;
			CharWidth      = 2;
			StringEncoding = STRING_ENCODING_UCS2;
			FloatFormat    = FLOAT_FORMAT_IEEE754;
		}

		public const string STRING_ENCODING_ACSII = "ASCII";
		public const string STRING_ENCODING_UCS2  = "UCS-2";
		public const string FLOAT_FORMAT_IEEE754  = "IEEE-754";
	}

	/// <summary>
	/// A class that reads a complex data item from a binary buffer.
	/// </summary>
	public class BinaryStream
	{
		protected BinaryStream() {}
	
		/// <summary>
		/// Determines if a field contains an array of values.
		/// </summary>
		protected bool IsArrayField(FieldType field)
		{
			if (field.ElementCountSpecified)
			{
				if (field.ElementCountRef != null || field.FieldTerminator != null)
				{
					throw new InvalidSchema("Multiple array size attributes specified for field '" + field.Name + " '.");
				}

				return true;
			}

			if (field.ElementCountRef != null)
			{
				if (field.FieldTerminator != null)
				{
					throw new InvalidSchema("Multiple array size attributes specified for field '" + field.Name + " '.");
				}

				return true;
			}

			if (field.FieldTerminator != null)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the termininator for the field.
		/// </summary>
		protected byte[] GetTerminator(Context context, FieldType field)
		{
			if (field.FieldTerminator == null) throw new InvalidSchema(field.Name + " is not a terminated group.");

			string terminator = System.Convert.ToString(field.FieldTerminator).ToUpper();

			byte[] bytes = new byte[terminator.Length/2];

			for (int ii = 0; ii < bytes.Length; ii++)
			{
				bytes[ii] = System.Convert.ToByte(terminator.Substring(ii*2, 2), 16);
			}

			return bytes;
		}

		/// <summary>
		/// Looks up the type name in the dictionary and initializes the context.
		/// </summary>
		protected Context InitializeContext(byte[] buffer, TypeDictionary dictionary, string typeName)
		{
			Context context = new Context(buffer);
			
			context.Dictionary     = dictionary;
			context.Type           = null;
			context.BigEndian      = dictionary.DefaultBigEndian;
			context.CharWidth      = dictionary.DefaultCharWidth;
			context.StringEncoding = dictionary.DefaultStringEncoding;
			context.FloatFormat    = dictionary.DefaultFloatFormat;

			foreach (TypeDescription type in dictionary.TypeDescription)
			{
				if (type.TypeID == typeName)
				{
					context.Type = type;

					if (type.DefaultBigEndianSpecified)     context.BigEndian      = type.DefaultBigEndian;
					if (type.DefaultCharWidthSpecified)     context.CharWidth      = type.DefaultCharWidth;
					if (type.DefaultStringEncoding != null) context.StringEncoding = type.DefaultStringEncoding;
					if (type.DefaultFloatFormat != null)    context.FloatFormat    = type.DefaultFloatFormat;

					break;
				}
			}

			if (context.Type == null)
			{
				throw new InvalidSchema("Type '" + typeName + "' not found in dictionary.");
			}

			return context;
		}	

		/// <summary>
		/// Swaps the order of bytes in the buffer.
		/// </summary>
		public void SwapBytes(byte[] bytes, int index, int length)
		{
			for (int ii = 0; ii < length/2; ii++)
			{
				byte temp                = bytes[index+length-1-ii];
				bytes[index+length-1-ii] = bytes[index+ii];
				bytes[index+ii]          = temp;
			}
		}
	}
	
	/// <summary>
	/// Raised if the data in buffer is not consistent with the schema.
	/// </summary>
	public class InvalidDataInBuffer : ApplicationException
	{
		private const string Default = "The data in the buffer cannot be read because it is not consistent with the schema.";
		/// <remarks/>
		public InvalidDataInBuffer() : base(Default) {} 
		/// <remarks/>
		public InvalidDataInBuffer(string message) : base(Default + "\r\n" + message) {}
		/// <remarks/>
		public InvalidDataInBuffer(Exception e) : base(Default, e) {}
	}

	/// <summary>
	/// Raised if the schema contains errors or inconsistencies.
	/// </summary>
	public class InvalidSchema : ApplicationException
	{
		private const string Default = "The schema cannot be used because it contains errors or inconsitencies.";
		/// <remarks/>
		public InvalidSchema() : base(Default) {} 
		/// <remarks/>
		public InvalidSchema(string message) : base(Default + "\r\n" + message) {}
		/// <remarks/>
		public InvalidSchema(Exception e) : base(Default, e) {}
	}

	/// <summary>
	/// Raised if the data in buffer is not consistent with the schema.
	/// </summary>
	public class InvalidDataToWrite : ApplicationException
	{
		private const string Default = "The object cannot be written because it is not consistent with the schema.";
		/// <remarks/>
		public InvalidDataToWrite() : base(Default) {} 
		/// <remarks/>
		public InvalidDataToWrite(string message) : base(Default + "\r\n" + message) {}
		/// <remarks/>
		public InvalidDataToWrite(Exception e) : base(Default, e) {}
	}
}

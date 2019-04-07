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


	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JISystem = org.jinterop.dcom.common.JISystem;

	/// <summary>
	///<para>Class representing a COM string. The Wide Char (<code>LPWSTR</code>) and the <code>BSTR</code> are
	/// both encoded by the server in "UTF-16LE". This encoding will be preserved by the library for all to
	/// and fro operations.
	/// </para>
	/// @since 1.0
	/// </summary>
	[Serializable]
	public sealed class JIString {

		/// <summary>
		/// Represents <code>JIVariant</code> for this object, it is valid only if this object is a <code>BSTR</code>
		/// (<code>JIFlags.FLAG_REPRESENTATION_STRING_BSTR</code>) type.
		/// </summary>
		public readonly JIVariant Variant;
		/// <summary>
		/// Represents <code>JIVariant(byRef = true)</code> for this object, it is valid only if this object is a <code>BSTR</code>
		/// (<code>JIFlags.FLAG_REPRESENTATION_STRING_BSTR</code>) type.
		/// </summary>
		public readonly JIVariant VariantByRef;
		private const long SerialVersionUID = -1656299949818101872L;
		private JIPointer Member = null;
		private int Type_Renamed = JIFlags.FLAG_NULL;


		/// <summary>
		///Creates an object of the specified type. Used while deserialiazing this object.
		/// </summary>
		/// <param name="type"> JIFlags string flags </param>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_BSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPCTSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPWSTR </seealso>
		/// <exception cref="IllegalArgumentException"> if <code>type</code> is not a string flag.
		///  </exception>
		public JIString(int type) {
			this.Type_Renamed = type;
			if (type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR || type == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {
				Member = new JIPointer(typeof(string),true);
			}
			else if (type == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
				Member = new JIPointer(typeof(string),false);
			}
			else {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_FLAG_ERROR));
			}
			Variant = null;
			VariantByRef = null;
			Member.Flags = type | JIFlags.FLAG_REPRESENTATION_VALID_STRING;
		}


		/// <summary>
		/// Creates a string object of a given <code>type</code>.
		/// </summary>
		/// <param name="str"> value encapsulated by this object. </param>
		/// <param name="type"> JIFlags string flags </param>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_BSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPCTSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPWSTR </seealso>
		/// <exception cref="IllegalArgumentException"> if <code>type</code> is not a string flag. </exception>
		public JIString(string str, int type) {
			str = (str == null) ? "" : str;
			this.Type_Renamed = type;
			if (type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR || type == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {
				Member = new JIPointer(str,true);
				Variant = null;
				VariantByRef = null;
			}
			else if (type == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
				Member = new JIPointer(str,false);
				Member.SetReferent(0x72657355); //"User" in LEndian.
				Variant = new JIVariant(this);
				VariantByRef = new JIVariant(this,true);
			}
			else {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UTIL_FLAG_ERROR));
			}

			Member.Flags = type | JIFlags.FLAG_REPRESENTATION_VALID_STRING;

		}



		/// <summary>
		/// Creates a object of the <code>BSTR</code> type.
		/// </summary>
		/// <param name="str"> value encapsulated by this object. </param>
		public JIString(string str) : this(str,JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
		}


		/// <summary>
		/// String encapsulated by this object. The encoding scheme for <code>LPWSTR</code> and <code>BSTR</code> strings is "UTF-16LE".
		/// 
		/// 
		/// @return
		/// </summary>
		public string String {
			get {
				return (string)Member.GetReferent();
			}
		}

		/// <summary>
		/// Type representing this object.
		/// </summary>
		/// <returns> JIFlags string flags </returns>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_BSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPCTSTR </seealso>
		/// <seealso cref= JIFlags#FLAG_REPRESENTATION_STRING_LPWSTR </seealso>
		public int Type {
			get {
				return Type_Renamed;
			}
		}


		public void Encode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG) {
			JIMarshalUnMarshalHelper.Serialize(ndr,Member.GetType(),Member,defferedPointers,Type_Renamed | FLAG);
		}


		public JIString Decode(NetworkDataRepresentation ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
			JIString newString = new JIString(Type_Renamed);
			newString.Member = (JIPointer)JIMarshalUnMarshalHelper.DeSerialize(ndr,Member,defferedPointers,Type_Renamed | FLAG,additionalData);
			return newString;
		}

		public bool Deffered {
			set {
				/*
				//this condition is required so that only BSTRs are value and also since this member could be value and
				//setting it to true would spoil the logic
				 * this is incorrect logic in the bug sent by Kevin , the ONEVENTSTRUCT consists of LPWSTRs which are value
				*/
				if (Member != null && !Member.Reference) {
					((JIPointer)Member).Deffered = true;
				}
			}
		}

		public override string ToString() {
			return Member == null ? "[null]" : "[Type: " + Type_Renamed + " , " + Member.ToString() + "]";
		}
	}

}
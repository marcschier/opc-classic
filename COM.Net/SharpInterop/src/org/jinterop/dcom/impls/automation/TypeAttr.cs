// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {

    //import java.util.UUID;

    using JIPointer = core.JIPointer;
    using JIStruct = core.JIStruct;

    /// <summary>
    ///Implements the <i>TYPEATTR</i> structure of COM Automation and
    /// contains attributes of an IJITypeInfo.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	public sealed class TypeAttr
	{

		private const long serialVersionUID = -4450777076320962915L;
		/// <summary>
		/// GUID guid;                    // The GUID of the type information.
		///   LCID lcid;                    // Locale of member names and doc
		///                               // strings.
		///   unsigned long dwReserved;
		///   MEMBERID memidConstructor;    // ID of constructor, or MEMBERID_NIL if
		///                               // none.
		///   MEMBERID memidDestructor;    // ID of destructor, or MEMBERID_NIL if
		///                               // none.
		///   OLECHAR FAR* lpstrSchema;    // Reserved for future use.
		///   unsigned long cbSizeInstance;// The size of an instance of
		///                               // this type.
		///   TypeKind typekind;            // The kind of type this information
		///                               // describes.
		///   unsigned short cFuncs;        // Number of functions.
		///   unsigned short cVars;        // Number of variables/data members.
		///   unsigned short cImplTypes;    // Number of implemented interfaces.
		///   unsigned short cbSizeVft;    // The size of this type's VTBL.
		///   unsigned short cbAlignment;    // Byte alignment for an instance
		///                               // of this type.
		///   unsigned short wTypeFlags;
		///   unsigned short wMajorVerNum;    // Major version number.
		///   unsigned short wMinorVerNum;    // Minor version number.
		///   TYPEDESC tdescAlias;            // If TypeKind == TKIND_ALIAS,
		///                               // specifies the type for which
		///                               // this type is an alias.
		///   IDLDESC idldescType;        // IDL attributes of the
		///                               // described type.
		/// </summary>

		/// <summary>
		///  The GUID of the type information.
		/// </summary>
		public readonly string guid; // The GUID of the type information.
		/// <summary>
		/// Locale of member names and doc strings.
		/// </summary>
		public readonly int lcid; // Locale of member names and doc
									// strings.
		public readonly int dwReserved;
		/// <summary>
		/// ID of constructor, or MEMBERID_NIL if none.
		/// </summary>
		public readonly int memidConstructor; // ID of constructor, or MEMBERID_NIL if
									// none.
		/// <summary>
		/// ID of destructor, or MEMBERID_NIL if none.
		/// </summary>
		public readonly int memidDestructor; // ID of destructor, or MEMBERID_NIL if
									// none.
		public readonly JIPointer lpstrSchema; // Reserved for future use.
		/// <summary>
		/// The size of an instance of this type.
		/// </summary>
		public readonly int cbSizeInstance; // The size of an instance of
									// this type.
		/// <summary>
		/// The kind of type this information describes.
		/// </summary>
		public readonly int typekind; // The kind of type this information
									// describes.
		/// <summary>
		/// Number of functions.
		/// </summary>
		public readonly short cFuncs; // Number of functions.
		/// <summary>
		/// Number of variables/data members.
		/// </summary>
		public readonly short cVars; // Number of variables/data members.
		/// <summary>
		/// Number of implemented interfaces.
		/// </summary>
		public readonly short cImplTypes; // Number of implemented interfaces.
		/// <summary>
		/// The size of this type's VTBL.
		/// </summary>
		public readonly short cbSizeVft; // The size of this type's VTBL.
		/// <summary>
		/// Byte alignment for an instance of this type.
		/// </summary>
		public readonly short cbAlignment; // Byte alignment for an instance
									// of this type.
		public readonly short wTypeFlags;
		/// <summary>
		/// Major version number.
		/// </summary>
		public readonly short wMajorVerNum; // Major version number.
		/// <summary>
		/// Minor version number.
		/// </summary>
		public readonly short wMinorVerNum; // Minor version number.
		/// <summary>
		/// if TypeKind == TKIND_ALIAS, specifies the type for which this type is an alias.
		/// </summary>
		public readonly TypeDesc tdescAlias; // If TypeKind == TKIND_ALIAS,
									// specifies the type for which
									// this type is an alias.
		/// <summary>
		/// IDL attributes of the described type.
		/// </summary>
		public readonly IdlDesc idldescType; // IDL attributes of the
									// described type.


		public const int TYPEFLAG_FAPPOBJECT = 0x01;
		public const int TYPEFLAG_FCANCREATE = 0x02;
		public const int TYPEFLAG_FLICENSED = 0x04;
		public const int TYPEFLAG_FPREDECLID = 0x08;
		public const int TYPEFLAG_FHIDDEN = 0x10;
		public const int TYPEFLAG_FCONTROL = 0x20;
		public const int TYPEFLAG_FDUAL = 0x40;
		public const int TYPEFLAG_FNONEXTENSIBLE = 0x80;
		public const int TYPEFLAG_FOLEAUTOMATION = 0x100;
		public const int TYPEFLAG_FRESTRICTED = 0x200;
		public const int TYPEFLAG_FAGGREGATABLE = 0x400;
		public const int TYPEFLAG_FREPLACEABLE = 0x800;
		public const int TYPEFLAG_FDISPATCHABLE = 0x1000;
		public const int TYPEFLAG_FREVERSEBIND = 0x2000;

		internal TypeAttr(JIPointer values) : this(values.Null ? null : (JIStruct)values.Referent)
		{
		}

		internal TypeAttr(JIStruct filledStruct)
		{
			if (filledStruct == null)
			{
				guid = null;
				lcid = -1;
				dwReserved = -1;
				memidConstructor = -1;
				memidDestructor = -1;
				lpstrSchema = null;
				cbSizeInstance = -1;
				typekind = -1;
				cFuncs = -1;
				cVars = -1;
				cImplTypes = -1;
				cbSizeVft = -1;
				cbAlignment = -1;
				wTypeFlags = -1;
				wMajorVerNum = -1;
				wMinorVerNum = -1;
				tdescAlias = null;
				idldescType = null;
				return;
			}

			guid = ((rpc.core.UUID)filledStruct.getMember(0)).ToString();
			lcid = (int)(int?)filledStruct.getMember(1);
			dwReserved = (int)(int?)filledStruct.getMember(2);
			memidConstructor = (int)(int?)filledStruct.getMember(3);
			memidDestructor = (int)(int?)filledStruct.getMember(4);
			lpstrSchema = (JIPointer)filledStruct.getMember(5);
			cbSizeInstance = (int)(int?)filledStruct.getMember(6);
			typekind = (int)(int?)filledStruct.getMember(7);
			cFuncs = (short)(short?)filledStruct.getMember(8);
			cVars = (short)(short?)filledStruct.getMember(9);
			cImplTypes = (short)(short?)filledStruct.getMember(10);
			cbSizeVft = (short)(short?)filledStruct.getMember(11);
			cbAlignment = (short)(short?)filledStruct.getMember(12);
			wTypeFlags = (short)(short?)filledStruct.getMember(13);
			wMajorVerNum = (short)(short?)filledStruct.getMember(14);
			wMinorVerNum = (short)(short?)filledStruct.getMember(15);
			tdescAlias = new TypeDesc((JIStruct)filledStruct.getMember(16));
			idldescType = new IdlDesc((JIStruct)filledStruct.getMember(17));

		}

	}

}
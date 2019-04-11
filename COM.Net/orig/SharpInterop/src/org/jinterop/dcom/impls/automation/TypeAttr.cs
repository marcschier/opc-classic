using System;

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

namespace org.jinterop.dcom.impls.automation {

    //import java.util.UUID;

    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIStruct = org.jinterop.dcom.core.JIStruct;

    /// <summary>
    ///Implements the <i>TYPEATTR</i> structure of COM Automation and
    /// contains attributes of an IJITypeInfo.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
    public sealed class TypeAttr {

        private const long SerialVersionUID = -4450777076320962915L;
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
        public readonly string Guid; // The GUID of the type information.
        /// <summary>
        /// Locale of member names and doc strings.
        /// </summary>
        public readonly int Lcid; // Locale of member names and doc
                                    // strings.
        public readonly int DwReserved;
        /// <summary>
        /// ID of constructor, or MEMBERID_NIL if none.
        /// </summary>
        public readonly int MemidConstructor; // ID of constructor, or MEMBERID_NIL if
                                    // none.
        /// <summary>
        /// ID of destructor, or MEMBERID_NIL if none.
        /// </summary>
        public readonly int MemidDestructor; // ID of destructor, or MEMBERID_NIL if
                                    // none.
        public readonly JIPointer LpstrSchema; // Reserved for future use.
        /// <summary>
        /// The size of an instance of this type.
        /// </summary>
        public readonly int CbSizeInstance; // The size of an instance of
                                    // this type.
        /// <summary>
        /// The kind of type this information describes.
        /// </summary>
        public readonly int Typekind; // The kind of type this information
                                    // describes.
        /// <summary>
        /// Number of functions.
        /// </summary>
        public readonly short CFuncs; // Number of functions.
        /// <summary>
        /// Number of variables/data members.
        /// </summary>
        public readonly short CVars; // Number of variables/data members.
        /// <summary>
        /// Number of implemented interfaces.
        /// </summary>
        public readonly short CImplTypes; // Number of implemented interfaces.
        /// <summary>
        /// The size of this type's VTBL.
        /// </summary>
        public readonly short CbSizeVft; // The size of this type's VTBL.
        /// <summary>
        /// Byte alignment for an instance of this type.
        /// </summary>
        public readonly short CbAlignment; // Byte alignment for an instance
                                    // of this type.
        public readonly short WTypeFlags;
        /// <summary>
        /// Major version number.
        /// </summary>
        public readonly short WMajorVerNum; // Major version number.
        /// <summary>
        /// Minor version number.
        /// </summary>
        public readonly short WMinorVerNum; // Minor version number.
        /// <summary>
        /// if TypeKind == TKIND_ALIAS, specifies the type for which this type is an alias.
        /// </summary>
        public readonly TypeDesc TdescAlias; // If TypeKind == TKIND_ALIAS,
                                    // specifies the type for which
                                    // this type is an alias.
        /// <summary>
        /// IDL attributes of the described type.
        /// </summary>
        public readonly IdlDesc IdldescType; // IDL attributes of the
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

        public TypeAttr(JIPointer values) : this(values.Null ? null : (JIStruct)values.GetReferent()) {
        }

        public TypeAttr(JIStruct filledStruct) {
            if (filledStruct == null) {
                Guid = null;
                Lcid = -1;
                DwReserved = -1;
                MemidConstructor = -1;
                MemidDestructor = -1;
                LpstrSchema = null;
                CbSizeInstance = -1;
                Typekind = -1;
                CFuncs = -1;
                CVars = -1;
                CImplTypes = -1;
                CbSizeVft = -1;
                CbAlignment = -1;
                WTypeFlags = -1;
                WMajorVerNum = -1;
                WMinorVerNum = -1;
                TdescAlias = null;
                IdldescType = null;
                return;
            }

            Guid = (((rpc.core.UUID)filledStruct.GetMember(0)).ToString());
            Lcid = (int)((int?)filledStruct.GetMember(1));
            DwReserved = (int)((int?)filledStruct.GetMember(2));
            MemidConstructor = (int)((int?)filledStruct.GetMember(3));
            MemidDestructor = (int)((int?)filledStruct.GetMember(4));
            LpstrSchema = ((JIPointer)filledStruct.GetMember(5));
            CbSizeInstance = (int)((int?)filledStruct.GetMember(6));
            Typekind = (int)((int?)filledStruct.GetMember(7));
            CFuncs = (short)((short?)filledStruct.GetMember(8));
            CVars = (short)((short?)filledStruct.GetMember(9));
            CImplTypes = (short)((short?)filledStruct.GetMember(10));
            CbSizeVft = (short)((short?)filledStruct.GetMember(11));
            CbAlignment = (short)((short?)filledStruct.GetMember(12));
            WTypeFlags = (short)((short?)filledStruct.GetMember(13));
            WMajorVerNum = (short)((short?)filledStruct.GetMember(14));
            WMinorVerNum = (short)((short?)filledStruct.GetMember(15));
            TdescAlias = new TypeDesc((JIStruct)filledStruct.GetMember(16));
            IdldescType = new IdlDesc((JIStruct)filledStruct.GetMember(17));

        }

    }

}
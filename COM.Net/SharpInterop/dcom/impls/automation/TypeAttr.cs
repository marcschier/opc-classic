// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.core;
    using System;

    /// <summary>
    /// Implements the <i>TYPEATTR</i> structure of COM Automation and
    /// contains attributes of an IJITypeInfo.
    /// </summary>
    /// <remarks>
    ///   GUID guid;                    // The GUID of the type information.
    ///   LCID lcid;                    // Locale of member names and doc
    ///                                 // strings.
    ///   unsigned long dwReserved;     // reserved
    ///   MEMBERID memidConstructor;    // ID of constructor, or MEMBERID_NIL if
    ///                                 // none.
    ///   MEMBERID memidDestructor;     // ID of destructor, or MEMBERID_NIL if
    ///                                 // none.
    ///   OLECHAR FAR* lpstrSchema;     // Reserved for future use.
    ///   unsigned long cbSizeInstance; // The size of an instance of
    ///                                 // this type.
    ///   TypeKind typekind;            // The kind of type this information
    ///                                 // describes.
    ///   unsigned short cFuncs;        // Number of functions.
    ///   unsigned short cVars;         // Number of variables/data members.
    ///   unsigned short cImplTypes;    // Number of implemented interfaces.
    ///   unsigned short cbSizeVft;     // The size of this type's VTBL.
    ///   unsigned short cbAlignment;   // Byte alignment for an instance
    ///                                 // of this type.
    ///   unsigned short wTypeFlags;
    ///   unsigned short wMajorVerNum;  // Major version number.
    ///   unsigned short wMinorVerNum;  // Minor version number.
    ///   TYPEDESC tdescAlias;          // If TypeKind == TKIND_ALIAS,
    ///                                 // specifies the type for which
    ///                                 // this type is an alias.
    ///   IDLDESC idldescType;          // IDL attributes of the
    ///                                 // described type.
    /// </remarks>
    [Serializable]
    public sealed class TypeAttr {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly string guid;
        public readonly int lcid;
        public readonly int dwReserved;
        public readonly int memidConstructor; 
        public readonly int memidDestructor;
        public readonly JIPointer lpstrSchema; 
        public readonly int cbSizeInstance;
        public readonly int typekind; 
        public readonly short cFuncs; 
        public readonly short cVars; 
        public readonly short cImplTypes;
        public readonly short cbSizeVft;
        public readonly short cbAlignment;
        public readonly short wTypeFlags;
        public readonly short wMajorVerNum; 
        public readonly short wMinorVerNum; 
        public readonly TypeDesc tdescAlias; 
        public readonly IdlDesc idldescType;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


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


        /// <summary>
        /// Create type attribute
        /// </summary>
        /// <param name="values"></param>
        internal TypeAttr(JIPointer values) : 
            this(values.IsNull ? null : (JIStruct)values.GetReferent()) {
        }

        /// <summary>
        /// Create type attribute
        /// </summary>
        /// <param name="filledStruct"></param>
        internal TypeAttr(JIStruct filledStruct) {
            if (filledStruct == null) {
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

            guid = ((rpc.core.UUID)filledStruct.GetMember(0)).ToString();
            lcid = (int)filledStruct.GetMember(1);
            dwReserved = (int)filledStruct.GetMember(2);
            memidConstructor = (int)filledStruct.GetMember(3);
            memidDestructor = (int)filledStruct.GetMember(4);
            lpstrSchema = (JIPointer)filledStruct.GetMember(5);
            cbSizeInstance = (int)filledStruct.GetMember(6);
            typekind = (int)filledStruct.GetMember(7);
            cFuncs = (short)filledStruct.GetMember(8);
            cVars = (short)filledStruct.GetMember(9);
            cImplTypes = (short)filledStruct.GetMember(10);
            cbSizeVft = (short)filledStruct.GetMember(11);
            cbAlignment = (short)filledStruct.GetMember(12);
            wTypeFlags = (short)filledStruct.GetMember(13);
            wMajorVerNum = (short)filledStruct.GetMember(14);
            wMinorVerNum = (short)filledStruct.GetMember(15);
            tdescAlias = new TypeDesc((JIStruct)filledStruct.GetMember(16));
            idldescType = new IdlDesc((JIStruct)filledStruct.GetMember(17));
        }
    }
}
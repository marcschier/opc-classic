// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>TYPEATTR</i> structure of COM Automation and
/// contains attributes of an ITypeInfo.
/// </summary>
/// <remarks>
///  GUID guid;                    // The GUID of the type information.
///  LCID lcid;                    // Locale of member names and doc
///                                // strings.
///  unsigned long dwReserved;     // reserved
///  MEMBERID memidConstructor;    // ID of constructor, or MEMBERID_NIL if
///                                // none.
///  MEMBERID memidDestructor;     // ID of destructor, or MEMBERID_NIL if
///                                // none.
///  OLECHAR FAR* lpstrSchema;     // Reserved for future use.
///  unsigned long cbSizeInstance; // The size of an instance of
///                                // this type.
///  TypeKind typekind;            // The kind of type this information
///                                // describes.
///  unsigned short cFuncs;        // Number of functions.
///  unsigned short cVars;         // Number of variables/data members.
///  unsigned short cImplTypes;    // Number of implemented interfaces.
///  unsigned short cbSizeVft;     // The size of this type's VTBL.
///  unsigned short cbAlignment;   // Byte alignment for an instance
///                                // of this type.
///  unsigned short wTypeFlags;
///  unsigned short wMajorVerNum;  // Major version number.
///  unsigned short wMinorVerNum;  // Minor version number.
///  TYPEDESC tdescAlias;          // If TypeKind == TKIND_ALIAS,
///                                // specifies the type for which
///                                // this type is an alias.
///  IDLDESC idldescType;          // IDL attributes of the
///                                // described type.
/// </remarks>
[Serializable]
public sealed class TypeAttr
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public readonly string guid;
    public readonly int lcid;
    public readonly int dwReserved;
    public readonly int memidConstructor;
    public readonly int memidDestructor;
    public readonly ComPointer lpstrSchema;
    public readonly int cbSizeInstance;
    public readonly TypeKind typekind;
    public readonly short cFuncs;
    public readonly short cVars;
    public readonly short cImplTypes;
    public readonly short cbSizeVft;
    public readonly short cbAlignment;
    public readonly TypeFlag wTypeFlags;
    public readonly short wMajorVerNum;
    public readonly short wMinorVerNum;
    public readonly TypeDesc tdescAlias;
    public readonly IdlDesc idldescType;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Create type attribute
    /// </summary>
    /// <param name="values">Values being stored, encoded, or assigned.</param>
    internal TypeAttr(ComPointer values) :
        this(values.IsNull ? null : (Struct)values.Referent)
    {
    }

    /// <summary>
    /// Create type attribute
    /// </summary>
    /// <param name="filledStruct">Structure instance populated with decoded COM field values.</param>
    internal TypeAttr(Struct filledStruct)
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
            typekind = (TypeKind)(-1);
            cFuncs = -1;
            cVars = -1;
            cImplTypes = -1;
            cbSizeVft = -1;
            cbAlignment = -1;
            wTypeFlags = (TypeFlag)(-1);
            wMajorVerNum = -1;
            wMinorVerNum = -1;
            tdescAlias = null;
            idldescType = null;
            return;
        }

        guid = ((Opc.Classic.Dcom.Rpc.Core.UUID)filledStruct.GetMember(0)).ToString();
        lcid = (int)filledStruct.GetMember(1);
        dwReserved = (int)filledStruct.GetMember(2);
        memidConstructor = (int)filledStruct.GetMember(3);
        memidDestructor = (int)filledStruct.GetMember(4);
        lpstrSchema = (ComPointer)filledStruct.GetMember(5);
        cbSizeInstance = (int)filledStruct.GetMember(6);
        typekind = (TypeKind)filledStruct.GetMember(7);
        cFuncs = (short)filledStruct.GetMember(8);
        cVars = (short)filledStruct.GetMember(9);
        cImplTypes = (short)filledStruct.GetMember(10);
        cbSizeVft = (short)filledStruct.GetMember(11);
        cbAlignment = (short)filledStruct.GetMember(12);
        wTypeFlags = (TypeFlag)filledStruct.GetMember(13);
        wMajorVerNum = (short)filledStruct.GetMember(14);
        wMinorVerNum = (short)filledStruct.GetMember(15);
        tdescAlias = new TypeDesc((Struct)filledStruct.GetMember(16));
        idldescType = new IdlDesc((Struct)filledStruct.GetMember(17));
    }
}

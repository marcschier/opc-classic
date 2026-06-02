//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Two-pass NDR deferred-pointer pile decoder for OPCBROWSEELEMENT[]
// (DCE 1.1 §14.3.12.3). Splits each struct read into an inline part
// (scalars + embedded referent IDs) and a deferred part (pointed-to
// values). Required for compatibility with real DCOM peers that emit
// the canonical wire layout for IOPCBrowse::Browse responses.
//

using System;
using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// Two-pass NDR decoder for the OPC DA <c>OPCBROWSEELEMENT</c> conformant
/// array returned by <c>IOPCBrowse::Browse</c>. Handles the deferred-pointer
/// pile layout (DCE 1.1 §14.3.12.3) that real DCOM servers emit: all N
/// inline parts first, then all N deferred parts in array order.
/// </summary>
public static class NdrOpcBrowseResponseDecoder
{
    /// <summary>
    /// Writes the IDL pair <c>[out] DWORD *pdwCount, [out, size_is(,*pdwCount)]
    /// OPCBROWSEELEMENT **ppBrowseElements</c> on the response wire for an
    /// <c>IOPCBrowse::Browse</c> reply. Matches the deferred-pile layout
    /// consumed by <see cref="ReadConformantArrayWithReferent"/>.
    /// </summary>
    public static void WriteConformantArrayWithReferent(ref NdrWriter writer, OpcBrowseElementResult[]? elements)
    {
        if (elements is null || elements.Length == 0)
        {
            writer.WriteUInt32(0u);
            writer.WriteUInt32(0u);
            return;
        }
        writer.WriteUInt32((uint)elements.Length);   // pdwCount
        writer.WriteUniquePointerReferent(true);     // ppBrowseElements referent
        writer.WriteUInt32((uint)elements.Length);   // max_count
        WriteConformantArray(ref writer, elements);
    }

    /// <summary>
    /// Writes a conformant array of <c>OPCBROWSEELEMENT</c> structs in
    /// deferred-pointer pile layout. Caller must have already emitted any
    /// outer referent + max_count.
    /// </summary>
    public static void WriteConformantArray(ref NdrWriter writer, OpcBrowseElementResult[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0) { return; }
        foreach (var element in elements)
        {
            WriteBrowseElementInline(ref writer, element);
        }
        foreach (var element in elements)
        {
            WriteBrowseElementDeferred(ref writer, element);
        }
    }

    private static void WriteBrowseElementInline(ref NdrWriter writer, OpcBrowseElementResult element)
    {
        writer.WriteUniquePointerReferent(element.Name is not null);
        writer.WriteUniquePointerReferent(element.ItemId is not null);
        writer.WriteUInt32(unchecked((uint)element.FlagValue));
        writer.WriteUInt32(0u);
        // Embedded OPCITEMPROPERTIES inline part
        writer.WriteInt32(element.Properties.ErrorId);
        writer.WriteUInt32((uint)element.Properties.Properties.Length);
        writer.WriteUniquePointerReferent(element.Properties.Properties.Length > 0);
        writer.WriteUInt32(0u);
    }

    private static void WriteBrowseElementDeferred(ref NdrWriter writer, OpcBrowseElementResult element)
    {
        if (element.Name is not null) { writer.WriteUnicodeString(element.Name); }
        if (element.ItemId is not null) { writer.WriteUnicodeString(element.ItemId); }
        if (element.Properties.Properties.Length > 0)
        {
            WriteItemPropertyConformantArray(ref writer, element.Properties.Properties);
        }
    }

    /// <summary>
    /// Writes the <c>IOPCBrowse::GetProperties</c> response array:
    /// <c>[out, size_is(,dwItemCount)] OPCITEMPROPERTIES **ppItemProperties</c>.
    /// </summary>
    public static void WriteItemPropertiesConformantArray(ref NdrWriter writer, OpcItemProperties[]? itemProperties)
    {
        if (itemProperties is null || itemProperties.Length == 0)
        {
            writer.WriteNullReferent();
            return;
        }

        writer.WriteUniquePointerReferent(true);
        writer.WriteUInt32((uint)itemProperties.Length);
        foreach (OpcItemProperties item in itemProperties)
        {
            WriteItemPropertiesInline(ref writer, item);
        }
        foreach (OpcItemProperties item in itemProperties)
        {
            WriteItemPropertiesDeferred(ref writer, item);
        }
    }

    private static void WriteItemPropertiesInline(ref NdrWriter writer, OpcItemProperties itemProperties)
    {
        writer.WriteInt32(itemProperties.ErrorId);
        writer.WriteUInt32((uint)itemProperties.Properties.Length);
        writer.WriteUniquePointerReferent(itemProperties.Properties.Length > 0);
        writer.WriteUInt32(0u);
    }

    private static void WriteItemPropertiesDeferred(ref NdrWriter writer, OpcItemProperties itemProperties)
    {
        if (itemProperties.Properties.Length > 0)
        {
            WriteItemPropertyFlatConformantArray(ref writer, itemProperties.Properties);
        }
    }

    private static void WriteItemPropertyFlatConformantArray(ref NdrWriter writer, OpcItemPropertyResult[] properties)
    {
        writer.WriteUInt32((uint)properties.Length);
        foreach (OpcItemPropertyResult property in properties)
        {
            NdrOpcItemPropertyCodec.Write(ref writer, property);
        }
    }

    /// <summary>
    /// Writes a conformant array of <c>OPCITEMPROPERTY</c> structs in
    /// deferred-pile layout (max_count + N inline + N deferred). Used both
    /// for the top-level <c>IOPCBrowse::GetProperties</c> response and as
    /// the deferred body of a non-null <c>pItemProperties</c> pointer
    /// inside <see cref="OpcBrowseElementResult"/>.
    /// </summary>
    public static void WriteItemPropertyConformantArray(ref NdrWriter writer, OpcItemPropertyResult[] properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        writer.WriteUInt32((uint)properties.Length);
        foreach (var prop in properties)
        {
            WriteItemPropertyInline(ref writer, prop);
        }
        foreach (var prop in properties)
        {
            WriteItemPropertyDeferred(ref writer, prop);
        }
    }

    private static void WriteItemPropertyInline(ref NdrWriter writer, OpcItemPropertyResult prop)
    {
        writer.WriteUInt16((ushort)prop.DataType);
        writer.WriteUInt16(0);
        writer.WriteUInt32(unchecked((uint)prop.PropertyId));
        writer.WriteUniquePointerReferent(prop.ItemId is not null);
        writer.WriteUniquePointerReferent(prop.Description is not null);
        // Mirror of ReadItemPropertyInline — embedded VARIANT uses the
        // per-element envelope (referent + pad-to-8 + wireVARIANT-with-
        // duplicated-discriminator).
        writer.WriteUniquePointerReferent(true);
        writer.AlignTo(8);
        NdrVariantExtensions.WriteVariantElement(ref writer, prop.Value);
        writer.WriteInt32(prop.ErrorId);
        writer.WriteUInt32(0u);
    }

    private static void WriteItemPropertyDeferred(ref NdrWriter writer, OpcItemPropertyResult prop)
    {
        if (prop.ItemId is not null) { writer.WriteUnicodeString(prop.ItemId); }
        if (prop.Description is not null) { writer.WriteUnicodeString(prop.Description); }
    }

    /// <summary>
    /// Reads the IDL pair <c>[out] DWORD *pdwCount, [out, size_is(,*pdwCount)]
    /// OPCBROWSEELEMENT **ppBrowseElements</c> from an
    /// <c>IOPCBrowse::Browse</c> response. Reads pdwCount (consumed, but the
    /// value is derived from the conformant array's max_count), then the
    /// inner unique-pointer referent, then (if non-null) the
    /// <c>max_count</c> DWORD plus the deferred-pile layout.
    /// </summary>
    public static OpcBrowseElementResult[] ReadConformantArrayWithReferent(ref NdrReader reader)
    {
        _ = reader.ReadUInt32();           // pdwCount sibling — array max_count is authoritative
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        uint maxCount = reader.ReadUInt32();
        return ReadConformantArray(ref reader, unchecked((int)maxCount));
    }

    /// <summary>
    /// Reads a conformant array of <c>OPCBROWSEELEMENT</c> structs. The
    /// caller must have already consumed any outer unique-pointer referent
    /// and the <c>max_count</c> DWORD before calling this method; pass the
    /// element count via <paramref name="count"/>.
    /// </summary>
    public static OpcBrowseElementResult[] ReadConformantArray(ref NdrReader reader, int count)
    {
        if (count <= 0) { return []; }
        var inlineParts = new BrowseElementInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadBrowseElementInline(ref reader);
        }
        var result = new OpcBrowseElementResult[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyBrowseElementDeferred(ref reader, inlineParts[i]);
        }
        return result;
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct BrowseElementInline
    {
        public BrowseElementInline(uint nameRef, uint itemIdRef, int flagValue, ItemPropertiesInline properties)
        {
            NameRef = nameRef;
            ItemIdRef = itemIdRef;
            FlagValue = flagValue;
            Properties = properties;
        }
        public uint NameRef { get; }
        public uint ItemIdRef { get; }
        public int FlagValue { get; }
        public ItemPropertiesInline Properties { get; }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct ItemPropertiesInline
    {
        public ItemPropertiesInline(int errorId, int numProperties, uint propertiesRef)
        {
            ErrorId = errorId;
            NumProperties = numProperties;
            PropertiesRef = propertiesRef;
        }
        public int ErrorId { get; }
        public int NumProperties { get; }
        public uint PropertiesRef { get; }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct ItemPropertyInline
    {
        public ItemPropertyInline(ushort dataType, int propertyId, uint itemIdRef, uint descriptionRef, OpcVariant value, int errorId)
        {
            DataType = dataType;
            PropertyId = propertyId;
            ItemIdRef = itemIdRef;
            DescriptionRef = descriptionRef;
            Value = value;
            ErrorId = errorId;
        }
        public ushort DataType { get; }
        public int PropertyId { get; }
        public uint ItemIdRef { get; }
        public uint DescriptionRef { get; }
        public OpcVariant Value { get; }
        public int ErrorId { get; }
    }

    private static BrowseElementInline ReadBrowseElementInline(ref NdrReader reader)
    {
        uint nameRef = reader.ReadUInt32();
        uint itemIdRef = reader.ReadUInt32();
        uint flagValue = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        int hrErrorId = reader.ReadInt32();
        uint dwNumProperties = reader.ReadUInt32();
        uint pItemPropertiesRef = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        return new BrowseElementInline(
            nameRef,
            itemIdRef,
            unchecked((int)flagValue),
            new ItemPropertiesInline(hrErrorId, unchecked((int)dwNumProperties), pItemPropertiesRef));
    }

    private static OpcBrowseElementResult ApplyBrowseElementDeferred(ref NdrReader reader, BrowseElementInline inlinePart)
    {
        string? name = inlinePart.NameRef == 0u ? null : reader.ReadUnicodeString();
        string? itemId = inlinePart.ItemIdRef == 0u ? null : reader.ReadUnicodeString();
        OpcItemPropertyResult[] properties = inlinePart.Properties.PropertiesRef == 0u
            ? []
            : ReadItemPropertyConformantArray(ref reader);
        var props = new OpcItemProperties(inlinePart.Properties.ErrorId, properties);
        return new OpcBrowseElementResult(name, itemId, inlinePart.FlagValue, props);
    }

    /// <summary>
    /// Reads the <c>IOPCBrowse::GetProperties</c> response array starting with
    /// the array <c>max_count</c> followed by N <c>OPCITEMPROPERTIES</c> inline
    /// parts and their deferred <c>pItemProperties</c> arrays.
    /// </summary>
    public static OpcItemProperties[] ReadItemPropertiesConformantArray(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }

        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }

        var inlineParts = new ItemPropertiesInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadItemPropertiesInline(ref reader);
        }

        var result = new OpcItemProperties[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyItemPropertiesDeferred(ref reader, inlineParts[i]);
        }

        return result;
    }

    private static ItemPropertiesInline ReadItemPropertiesInline(ref NdrReader reader)
    {
        int hrErrorId = reader.ReadInt32();
        uint dwNumProperties = reader.ReadUInt32();
        uint pItemPropertiesRef = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        return new ItemPropertiesInline(hrErrorId, unchecked((int)dwNumProperties), pItemPropertiesRef);
    }

    private static OpcItemProperties ApplyItemPropertiesDeferred(ref NdrReader reader, ItemPropertiesInline inlinePart)
    {
        OpcItemPropertyResult[] properties = inlinePart.PropertiesRef == 0u
            ? []
            : ReadItemPropertyFlatConformantArray(ref reader);
        return new OpcItemProperties(inlinePart.ErrorId, properties);
    }

    private static OpcItemPropertyResult[] ReadItemPropertyFlatConformantArray(ref NdrReader reader)
    {
        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }

        var result = new OpcItemPropertyResult[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = NdrOpcItemPropertyCodec.Read(ref reader);
        }

        return result;
    }

    /// <summary>
    /// Reads a conformant array of <c>OPCITEMPROPERTY</c> structs starting
    /// with its <c>max_count</c> DWORD (the deferred-pile layout for a
    /// <c>[unique, size_is(N)] OPCITEMPROPERTY*</c> pointer that has been
    /// reached after consuming its referent ID).
    /// </summary>
    public static OpcItemPropertyResult[] ReadItemPropertyConformantArray(ref NdrReader reader)
    {
        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }
        var inlineParts = new ItemPropertyInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadItemPropertyInline(ref reader);
        }
        var result = new OpcItemPropertyResult[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyItemPropertyDeferred(ref reader, inlineParts[i]);
        }
        return result;
    }

    private static ItemPropertyInline ReadItemPropertyInline(ref NdrReader reader)
    {
        ushort vt = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        uint propertyId = reader.ReadUInt32();
        uint itemIdRef = reader.ReadUInt32();
        uint descriptionRef = reader.ReadUInt32();
        // Per live-Matrikon wire capture: the embedded VARIANT inside
        // OPCITEMPROPERTY uses the per-element envelope (per-element unique-
        // pointer referent + pad-to-8 + canonical wireVARIANT including the
        // duplicated [switch_is(vt)] discriminator). This matches the layout
        // VARIANT array elements use, even though the IDL declares vValue as
        // an embedded value-type field.
        _ = reader.ReadUInt32();
        reader.AlignTo(8);
        OpcVariant value = NdrVariantExtensions.ReadVariantElement(ref reader);
        int hrErrorId = reader.ReadInt32();
        _ = reader.ReadUInt32();
        return new ItemPropertyInline(vt, unchecked((int)propertyId), itemIdRef, descriptionRef, value, hrErrorId);
    }

    private static OpcItemPropertyResult ApplyItemPropertyDeferred(ref NdrReader reader, ItemPropertyInline inlinePart)
    {
        string? itemId = inlinePart.ItemIdRef == 0u ? null : reader.ReadUnicodeString();
        string? description = inlinePart.DescriptionRef == 0u ? null : reader.ReadUnicodeString();
        return new OpcItemPropertyResult(
            DataType: (VarType)inlinePart.DataType,
            PropertyId: inlinePart.PropertyId,
            ItemId: itemId,
            Description: description,
            Value: inlinePart.Value,
            ErrorId: inlinePart.ErrorId);
    }
}

// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>
/// NDR codec for DCOM OBJREF_STANDARD interface pointers.
/// </summary>
public static class OpcInterfaceRefCodec
{
    private const uint ObjRefSignature = 0x574F454D;
    private const uint ObjRefStandard = 0x00000001;

    /// <summary>
    /// Decodes an OBJREF_STANDARD payload (MEOW + STDOBJREF + DUALSTRINGARRAY).
    /// </summary>
    public static IOpcInterfaceRef Read(ref NdrReader reader)
    {
        uint signature = reader.ReadUInt32();
        if (signature != ObjRefSignature)
        {
            throw new InvalidOperationException("DCOM OBJREF did not start with the MEOW signature.");
        }

        uint objectReferenceType = reader.ReadUInt32();
        if (objectReferenceType != ObjRefStandard)
        {
            throw new InvalidOperationException("Only OBJREF_STANDARD interface pointers are supported.");
        }

        Guid iid = reader.ReadGuid();
        uint flags = reader.ReadUInt32();
        uint publicRefs = reader.ReadUInt32();
        ulong oxid = reader.ReadUInt64();
        ulong oid = reader.ReadUInt64();
        Guid ipid = reader.ReadGuid();
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        if (entryCount > reader.RemainingBytes / 2)
        {
            throw new InvalidOperationException("DCOM DUALSTRINGARRAY entry count exceeds the remaining response payload.");
        }

        var resolverBindings = new ushort[entryCount];
        for (int i = 0; i < resolverBindings.Length; i++)
        {
            resolverBindings[i] = reader.ReadUInt16();
        }

        return new OpcInterfaceRef(iid, flags, publicRefs, oxid, oid, ipid, securityOffset, resolverBindings);
    }

    /// <summary>
    /// Encodes an OBJREF_STANDARD payload (MEOW + STDOBJREF + DUALSTRINGARRAY).
    /// </summary>
    public static void Write(ref NdrWriter writer, IOpcInterfaceRef interfaceRef)
    {
        ArgumentNullException.ThrowIfNull(interfaceRef);

        if (interfaceRef.ResolverBindings.Count > ushort.MaxValue)
        {
            throw new ArgumentException("DCOM DUALSTRINGARRAY entry count exceeds UInt16.MaxValue.", nameof(interfaceRef));
        }

        writer.WriteUInt32(ObjRefSignature);
        writer.WriteUInt32(ObjRefStandard);
        writer.WriteGuid(interfaceRef.Iid);
        writer.WriteUInt32(interfaceRef.Flags);
        writer.WriteUInt32(interfaceRef.PublicRefs);
        writer.WriteUInt64(interfaceRef.Oxid);
        writer.WriteUInt64(interfaceRef.Oid);
        writer.WriteGuid(interfaceRef.Ipid);
        writer.WriteUInt16((ushort)interfaceRef.ResolverBindings.Count);
        writer.WriteUInt16(interfaceRef.SecurityOffset);
        for (int i = 0; i < interfaceRef.ResolverBindings.Count; i++)
        {
            writer.WriteUInt16(interfaceRef.ResolverBindings[i]);
        }
    }
}

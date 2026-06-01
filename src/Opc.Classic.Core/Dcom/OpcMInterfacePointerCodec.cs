//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>
/// NDR codec for DCOM <c>MInterfacePointer</c> wrappers (MS-DCOM §2.2.1.10),
/// which appear on the wire as the payload of any IDL parameter typed
/// <c>[unique, iid_is(riid)] LPUNKNOWN</c> (or the more common
/// <c>[out, iid_is(riid)] LPUNKNOWN *ppUnk</c> form).
/// </summary>
/// <remarks>
/// <para>
/// Wire layout (per MS-DCOM §2.2.1.10 + DCE 1.1 §14.3.10 for the unique-pointer
/// prefix):
/// </para>
/// <code>
/// uint  referent_id;      // 0 if pointer is NULL, non-zero otherwise
/// {if non-NULL:}
///   uint  ulCntData;      // size of OBJREF payload in bytes
///   ulCntData bytes of OBJREF (MEOW + STDOBJREF + DUALSTRINGARRAY)
/// </code>
/// <para>
/// The OBJREF body itself is decoded by <see cref="OpcInterfaceRefCodec"/>; this
/// codec only adds the MInterfacePointer framing required when the IDL declares
/// the parameter as a remotable interface pointer (so that
/// <c>iid_is(riid)</c> can carry an arbitrary IID at runtime).
/// </para>
/// </remarks>
public static class OpcMInterfacePointerCodec
{
    /// <summary>
    /// Decodes a unique-pointer-prefixed MInterfacePointer. Returns <see langword="null"/>
    /// when the on-wire pointer is NULL (referent_id == 0).
    /// </summary>
    public static IOpcInterfaceRef? Read(ref NdrReader reader)
    {
        uint referent = reader.ReadUInt32();
        if (referent == 0u)
        {
            return null;
        }

        // MInterfacePointer is a CONFORMANT struct (MS-DCOM §2.2.1.10):
        //     ULONG ulCntData;
        //     [size_is(ulCntData), ref] BYTE abData[];
        // NDR encoding adds the max_count DWORD before the struct fields.
        // Wire: max_count + ulCntData + abData[ulCntData]. Both counters
        // carry the OBJREF byte length; only the second (ulCntData) is the
        // actual struct field — the first is the conformance header.
        _ = reader.ReadUInt32();           // max_count (= ulCntData per spec)
        uint cbData = reader.ReadUInt32(); // ulCntData
        if (cbData == 0u)
        {
            return null;
        }

        // The OBJREF is opaque bytes per MS-DCOM §2.2.1.10 — abData[] is a
        // raw byte array, NOT a sub-stream that inherits the outer NDR
        // alignment. Decode it through a fresh NdrReader so embedded
        // ReadUInt64/Guid alignments are computed relative to the OBJREF's
        // own offset 0 (matching how Write composes the OBJREF via an inner
        // NdrWriter starting at its own offset 0). Without this isolation,
        // 8-byte aligned reads (Oxid/Oid) drift when MInterfacePointer
        // happens to land at a non-8-aligned outer offset (e.g. directly
        // after the 12-byte referent+max_count+cbData header at outer
        // offset 20).
        ReadOnlySpan<byte> objrefBytes = reader.ReadRawBytes((int)cbData);
        var innerReader = new NdrReader(objrefBytes);
        return OpcInterfaceRefCodec.Read(ref innerReader);
    }

    /// <summary>
    /// Encodes a unique-pointer-prefixed MInterfacePointer. A null
    /// <paramref name="interfaceRef"/> emits a single zero referent UInt32.
    /// </summary>
    public static void Write(ref NdrWriter writer, IOpcInterfaceRef? interfaceRef)
    {
        if (interfaceRef is null)
        {
            writer.WriteUInt32(0u);
            return;
        }

        // Serialize the OBJREF to a pooled buffer so we can compute ulCntData
        // before emitting the MInterfacePointer frame. Use ArrayPool rather than
        // stackalloc because NdrWriter is a ref struct and the ref-safety rules
        // forbid passing a stackalloc-backed Span across ref struct boundaries.
        const int InitialBufferSize = 1024;
        byte[] scratch = System.Buffers.ArrayPool<byte>.Shared.Rent(InitialBufferSize);
        try
        {
            var innerWriter = new NdrWriter(scratch.AsSpan());
            OpcInterfaceRefCodec.Write(ref innerWriter, interfaceRef);
            int objrefLength = innerWriter.Position;

            writer.WriteUInt32(0x00020000u);            // referent
            writer.WriteUInt32((uint)objrefLength);     // max_count (conformant struct)
            writer.WriteUInt32((uint)objrefLength);     // ulCntData
            writer.WriteRawBytes(scratch.AsSpan(0, objrefLength));
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(scratch);
        }
    }
}

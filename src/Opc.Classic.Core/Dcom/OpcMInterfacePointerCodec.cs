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

        uint cbData = reader.ReadUInt32();
        if (cbData == 0u)
        {
            return null;
        }

        // The OBJREF starts immediately after ulCntData per MS-DCOM §2.2.1.10.
        // OpcInterfaceRefCodec consumes the OBJREF in full; cbData is informational
        // for buffer sizing on the wire and is not re-validated here because the
        // OBJREF codec itself bounds-checks against the remaining payload.
        _ = cbData;
        return OpcInterfaceRefCodec.Read(ref reader);
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
            writer.WriteUInt32((uint)objrefLength);     // ulCntData
            writer.WriteRawBytes(scratch.AsSpan(0, objrefLength));
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(scratch);
        }
    }
}

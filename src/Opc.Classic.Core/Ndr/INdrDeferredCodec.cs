//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Ndr;

/// <summary>
/// Two-pass NDR struct codec contract for types with embedded unique pointers,
/// per the DCE 1.1 §14.3.12.3 "deferred-pointer pile" rule. Implementations
/// split each struct write/read into:
/// </summary>
/// <remarks>
/// <para>
/// <b>Inline part</b> — scalar fields plus the 4-byte referent IDs of any
/// embedded unique pointers (no pointed-to values).
/// </para>
/// <para>
/// <b>Deferred part</b> — the pointed-to values for the referent IDs the
/// inline part declared, written/read after the struct's inline section.
/// </para>
/// <para>
/// Top-level struct usage (e.g. <c>[out] T*</c> where T is a struct passed by
/// itself): call inline + deferred back-to-back; the wire layout is identical
/// to "single Write call" thanks to §14.3.12.1's top-level inline rule.
/// </para>
/// <para>
/// Array-element usage (e.g. <c>[out, size_is(,N)] T**</c>): the array helper
/// writes max_count + foreach inline; then foreach deferred. This produces
/// the wire layout Windows DCOM emits for conformant arrays of structs with
/// pointer fields (the case that breaks <c>IOPCBrowse::Browse</c>'s
/// <c>OPCBROWSEELEMENT</c> elements when emitted with the flat layout).
/// </para>
/// <para>
/// Migration strategy: existing static-class codecs (e.g.
/// <c>NdrOpcBrowseElementCodec</c>) can ship the <see cref="INdrDeferredCodec{T}"/>
/// implementation as a sibling instance type, keeping the original
/// <c>Write</c>/<c>Read</c> static methods callable for top-level usage and
/// gradually moving array-of-struct callers to the two-pass pattern.
/// </para>
/// </remarks>
/// <typeparam name="T">The struct type this codec serializes.</typeparam>
public interface INdrDeferredCodec<T>
{
    /// <summary>
    /// Writes the inline portion of <paramref name="value"/> — scalar fields
    /// and 4-byte referent IDs for any embedded unique-pointer fields. Does
    /// not emit the pointed-to values.
    /// </summary>
    void WriteInlinePart(ref NdrWriter writer, T value);

    /// <summary>
    /// Writes the deferred portion of <paramref name="value"/> — pointed-to
    /// values for the referent IDs <see cref="WriteInlinePart"/> declared.
    /// Called after the entire array of inline parts has been written.
    /// </summary>
    void WriteDeferredPart(ref NdrWriter writer, T value);

    /// <summary>
    /// Reads the inline portion of a value, returning the partially populated
    /// struct. Embedded unique-pointer fields are left at their default
    /// (null/empty); the referent IDs are stashed on the returned value (via
    /// implementation-specific fields) and consumed by
    /// <see cref="ApplyDeferredPart"/>.
    /// </summary>
    T ReadInlinePart(ref NdrReader reader);

    /// <summary>
    /// Reads the deferred portion of a previously-inline-decoded value,
    /// filling in pointer fields when their referent IDs were non-zero.
    /// </summary>
    void ApplyDeferredPart(ref NdrReader reader, ref T value);
}

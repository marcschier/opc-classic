//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// Managed, AOT-friendly shadow of OLE Automation's <c>IRecordInfo</c> used by
/// the optional VT_RECORD codec.
/// </summary>
/// <remarks>
/// Applications that exchange VT_RECORD payloads opt in by registering an
/// implementation with <see cref="RecordInfoRegistry"/>. The GUID identifies
/// the record layout on the wire; fields are encoded in the listed order.
/// </remarks>
public interface IRecordInfo
{
    /// <summary>
    /// The GUID that identifies this record layout.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// A human-readable layout name for diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The fields encoded in declaration order.
    /// </summary>
    IReadOnlyList<OpcRecordField> Fields { get; }
}

// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// A single field inside a <see cref="StructType"/>. Mirrors an OPCBinary
/// <c>&lt;Field&gt;</c> element.
/// </summary>
public sealed class StructField
{
    /// <summary>
    /// Field name (for diagnostics; not part of the wire encoding).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of this field.
    /// </summary>
    public TypeKind Kind { get; init; }

    /// <summary>
    /// When <see cref="Kind"/> is <see cref="TypeKind.StructReference"/>, the
    /// name of the referenced struct in the same dictionary.
    /// </summary>
    public string? TypeReference { get; init; }

    /// <summary>
    /// Number of repetitions (a "0" means single value, otherwise it's a
    /// repeated value — an array). A negative value (e.g. -1) indicates the
    /// repeat count is determined by another field's value (terminator or
    /// length-prefix; see <see cref="CountFieldName"/>).
    /// </summary>
    public int Repeats { get; init; }

    /// <summary>
    /// When <see cref="Repeats"/> is dynamic, the name of the sibling field
    /// holding the actual repeat count.
    /// </summary>
    public string? CountFieldName { get; init; }

    /// <summary>
    /// Field byte order when explicitly overridden (per-field). <see langword="null"/>
    /// = inherit dictionary default.
    /// </summary>
    public ByteOrder? ByteOrder { get; init; }
}

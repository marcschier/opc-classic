// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// OPC Complex Data item property identifiers defined by CPX 1.00 §3.3.
/// </summary>
public static class OpcComplexDataProperty
{
    /// <summary>
    /// 600 — identifies the type system, for example <c>XMLSchema</c> or <c>OPCBinary</c>.
    /// </summary>
    public const int TypeSystemId = 600;

    /// <summary>
    /// 601 — identifies the dictionary version scoped by the type system.
    /// </summary>
    public const int DictionaryId = 601;

    /// <summary>
    /// 602 — identifies the type description inside the dictionary.
    /// </summary>
    public const int TypeId = 602;

    /// <summary>
    /// 603 — contains the complete dictionary BLOB.
    /// </summary>
    public const int Dictionary = 603;

    /// <summary>
    /// 604 — contains the type-specific description BLOB.
    /// </summary>
    public const int TypeDescription = 604;

    /// <summary>
    /// 605 — describes the time-consistency window.
    /// </summary>
    public const int ConsistencyWindow = 605;

    /// <summary>
    /// 606 — describes write semantics such as <c>All or Nothing</c> or <c>Best Effort</c>.
    /// </summary>
    public const int WriteBehavior = 606;

    /// <summary>
    /// 607 — identifies the unconverted source item for type conversions.
    /// </summary>
    public const int UnconvertedItemId = 607;

    /// <summary>
    /// 608 — identifies the unfiltered source item for data filters.
    /// </summary>
    public const int UnfilteredItemId = 608;

    /// <summary>
    /// 609 — contains the active data-filter expression.
    /// </summary>
    public const int DataFilterValue = 609;
}

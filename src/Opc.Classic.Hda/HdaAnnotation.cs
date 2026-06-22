// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hda;

/// <summary>
/// An OPC HDA annotation — a user-supplied note attached to a historical
/// timestamp. Mirrors <c>OPCHDA_ANNOTATION</c>.
/// </summary>
public sealed class HdaAnnotation
{
    /// <summary>
    /// The historical timestamp the annotation is anchored to.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// UTC time at which the annotation was created.
    /// </summary>
    public DateTimeOffset AnnotationTime { get; init; }

    /// <summary>
    /// The annotation text.
    /// </summary>
    public string AnnotationText { get; init; } = string.Empty;

    /// <summary>
    /// The user (or process) that created the annotation.
    /// </summary>
    public string User { get; init; } = string.Empty;
}

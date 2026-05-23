//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA's <c>OPCHDA_ANNOTATION</c> — a series of timestamped annotations
/// for one historical item, including annotation timestamps and user names.
/// Returned by <c>IOPCHDA_SyncAnnotations</c> and friends.
/// </summary>
public sealed record OpcHdaAnnotation
{
    /// <summary>Constructor — validates the four parallel arrays have the same length.</summary>
    /// <param name="clientHandle">Client correlation handle.</param>
    /// <param name="timestamps">UTC value timestamps; same length as all other arrays.</param>
    /// <param name="annotations">Per-sample annotation text.</param>
    /// <param name="annotationTimes">UTC annotation timestamps.</param>
    /// <param name="users">Per-sample annotation user names.</param>
    public OpcHdaAnnotation(
        int clientHandle,
        DateTimeOffset[] timestamps,
        string?[] annotations,
        DateTimeOffset[] annotationTimes,
        string?[] users)
    {
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(annotations);
        ArgumentNullException.ThrowIfNull(annotationTimes);
        ArgumentNullException.ThrowIfNull(users);
        if (timestamps.Length != annotations.Length || annotations.Length != annotationTimes.Length || annotationTimes.Length != users.Length)
        {
            throw new ArgumentException(
                $"Parallel arrays must have equal length: timestamps={timestamps.Length}, annotations={annotations.Length}, annotationTimes={annotationTimes.Length}, users={users.Length}.",
                nameof(users));
        }

        ClientHandle = clientHandle;
        Timestamps = timestamps;
        Annotations = annotations;
        AnnotationTimes = annotationTimes;
        Users = users;
    }

    /// <summary>Client correlation handle.</summary>
    public int ClientHandle { get; }

    /// <summary>UTC value timestamps; parallel with <see cref="Annotations"/>.</summary>
    public DateTimeOffset[] Timestamps { get; }

    /// <summary>Per-sample annotation text.</summary>
    public string?[] Annotations { get; }

    /// <summary>UTC annotation timestamps; parallel with <see cref="Annotations"/>.</summary>
    public DateTimeOffset[] AnnotationTimes { get; }

    /// <summary>Per-sample annotation user names.</summary>
    public string?[] Users { get; }
}

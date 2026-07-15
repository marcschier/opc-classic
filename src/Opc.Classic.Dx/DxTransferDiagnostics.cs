// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // Transfer state and diagnostic contracts are kept together.

namespace Opc.Classic.Dx;

/// <summary>
/// Lifecycle state of one bounded DX transfer pipeline.
/// </summary>
public enum DxTransferState
{
    /// <summary>The transfer is not running.</summary>
    Stopped,
    /// <summary>The transfer is configured but disabled.</summary>
    Disabled,
    /// <summary>The transfer is establishing endpoints.</summary>
    Starting,
    /// <summary>The transfer is reading and writing values.</summary>
    Running,
    /// <summary>The transfer is waiting for a retry deadline.</summary>
    RetryDelay,
    /// <summary>The transfer is stopping.</summary>
    Stopping,
    /// <summary>The transfer stopped because of a terminal failure.</summary>
    Faulted,
}

/// <summary>
/// Severity of a transfer diagnostic event.
/// </summary>
public enum DxDiagnosticSeverity
{
    /// <summary>Detailed operational information.</summary>
    Trace,
    /// <summary>Normal lifecycle information.</summary>
    Information,
    /// <summary>A recoverable or degraded condition.</summary>
    Warning,
    /// <summary>A failed operation.</summary>
    Error,
    /// <summary>A terminal transfer failure.</summary>
    Critical,
}

/// <summary>
/// Operation that emitted a transfer diagnostic.
/// </summary>
public enum DxTransferOperation
{
    /// <summary>Pipeline lifecycle.</summary>
    Lifecycle,
    /// <summary>Source read.</summary>
    Read,
    /// <summary>Target write.</summary>
    Write,
    /// <summary>Endpoint health check.</summary>
    HealthCheck,
    /// <summary>Endpoint reconnect.</summary>
    Reconnect,
    /// <summary>Bounded queue management.</summary>
    Queue,
    /// <summary>Retry scheduling.</summary>
    Retry,
}

/// <summary>
/// Structured diagnostic emitted by a DX transfer engine.
/// </summary>
public sealed record DxTransferDiagnostic
{
    /// <summary>
    /// Creates a validated diagnostic.
    /// </summary>
    public DxTransferDiagnostic(
        DateTimeOffset timestamp,
        DxDiagnosticSeverity severity,
        DxTransferOperation operation,
        string code,
        string message,
        string? connectionName = null,
        OpcResultId? errorId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Timestamp = timestamp;
        Severity = severity;
        Operation = operation;
        Code = code;
        Message = message;
        ConnectionName = connectionName;
        ErrorId = errorId;
    }

    /// <summary>Time the event was observed.</summary>
    public DateTimeOffset Timestamp { get; }
    /// <summary>Diagnostic severity.</summary>
    public DxDiagnosticSeverity Severity { get; }
    /// <summary>Operation that emitted the event.</summary>
    public DxTransferOperation Operation { get; }
    /// <summary>Stable machine-readable diagnostic code.</summary>
    public string Code { get; }
    /// <summary>Human-readable diagnostic message.</summary>
    public string Message { get; }
    /// <summary>Optional connection name.</summary>
    public string? ConnectionName { get; }
    /// <summary>Optional OPC error identifier.</summary>
    public OpcResultId? ErrorId { get; }
}

/// <summary>
/// Immutable state snapshot for one bounded source-to-target transfer.
/// </summary>
public sealed record DxTransferSnapshot
{
    /// <summary>
    /// Creates a transfer state snapshot.
    /// </summary>
    public DxTransferSnapshot(
        string connectionName,
        DxTransferState state,
        int queueDepth,
        int queueCapacity,
        long readCount = 0,
        long writeCount = 0,
        long droppedCount = 0,
        int consecutiveFailures = 0,
        DateTimeOffset? lastReadTimestamp = null,
        DateTimeOffset? lastWriteTimestamp = null,
        DateTimeOffset? nextRetryTimestamp = null,
        DxDataValue? lastSourceValue = null,
        DxWriteResult? lastWriteResult = null,
        DxTransferDiagnostic? lastDiagnostic = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentOutOfRangeException.ThrowIfNegative(queueDepth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(queueCapacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(queueDepth, queueCapacity);
        ArgumentOutOfRangeException.ThrowIfNegative(readCount);
        ArgumentOutOfRangeException.ThrowIfNegative(writeCount);
        ArgumentOutOfRangeException.ThrowIfNegative(droppedCount);
        ArgumentOutOfRangeException.ThrowIfNegative(consecutiveFailures);

        ConnectionName = connectionName;
        State = state;
        QueueDepth = queueDepth;
        QueueCapacity = queueCapacity;
        ReadCount = readCount;
        WriteCount = writeCount;
        DroppedCount = droppedCount;
        ConsecutiveFailures = consecutiveFailures;
        LastReadTimestamp = lastReadTimestamp;
        LastWriteTimestamp = lastWriteTimestamp;
        NextRetryTimestamp = nextRetryTimestamp;
        LastSourceValue = lastSourceValue;
        LastWriteResult = lastWriteResult;
        LastDiagnostic = lastDiagnostic;
    }

    /// <summary>Connection represented by this snapshot.</summary>
    public string ConnectionName { get; }
    /// <summary>Current lifecycle state.</summary>
    public DxTransferState State { get; }
    /// <summary>Current bounded queue occupancy.</summary>
    public int QueueDepth { get; }
    /// <summary>Maximum bounded queue occupancy.</summary>
    public int QueueCapacity { get; }
    /// <summary>Total completed source reads.</summary>
    public long ReadCount { get; }
    /// <summary>Total completed target writes.</summary>
    public long WriteCount { get; }
    /// <summary>Total values dropped by bounded-queue policy.</summary>
    public long DroppedCount { get; }
    /// <summary>Current consecutive failure count.</summary>
    public int ConsecutiveFailures { get; }
    /// <summary>Timestamp of the last source read.</summary>
    public DateTimeOffset? LastReadTimestamp { get; }
    /// <summary>Timestamp of the last target write.</summary>
    public DateTimeOffset? LastWriteTimestamp { get; }
    /// <summary>Scheduled retry deadline, when backing off.</summary>
    public DateTimeOffset? NextRetryTimestamp { get; }
    /// <summary>Last source value observed.</summary>
    public DxDataValue? LastSourceValue { get; }
    /// <summary>Last target write result observed.</summary>
    public DxWriteResult? LastWriteResult { get; }
    /// <summary>Most recent structured diagnostic.</summary>
    public DxTransferDiagnostic? LastDiagnostic { get; }
}

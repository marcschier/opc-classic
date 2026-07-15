// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Ndr;

/// <summary>
/// Indicates that an <see cref="NdrWriter"/> needs a larger caller-provided buffer.
/// </summary>
public sealed class NdrBufferOverflowException : InvalidOperationException
{
    /// <summary>
    /// Creates an empty NDR buffer-overflow exception.
    /// </summary>
    public NdrBufferOverflowException()
    {
    }

    /// <summary>
    /// Creates an NDR buffer-overflow exception with a message.
    /// </summary>
    public NdrBufferOverflowException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an NDR buffer-overflow exception with a message and inner exception.
    /// </summary>
    public NdrBufferOverflowException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates an NDR buffer-overflow exception.
    /// </summary>
    public NdrBufferOverflowException(int position, int requiredBytes, int remainingBytes)
        : base($"NdrWriter buffer overflow: need {requiredBytes} bytes at position {position} but only {remainingBytes} remain.")
    {
        Position = position;
        RequiredBytes = requiredBytes;
        RemainingBytes = remainingBytes;
    }

    /// <summary>
    /// Writer position at which the write failed.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Number of bytes required by the failed write.
    /// </summary>
    public int RequiredBytes { get; }

    /// <summary>
    /// Number of writable bytes remaining in the supplied buffer.
    /// </summary>
    public int RemainingBytes { get; }
}

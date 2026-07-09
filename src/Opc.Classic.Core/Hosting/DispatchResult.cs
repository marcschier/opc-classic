// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Opc.Classic.Hosting;

/// <summary>
/// Result returned by a server-side OPC dispatcher.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct DispatchResult
{
    private DispatchResult(ReadOnlyMemory<byte> payload, int hresult)
    {
        Payload = payload;
        Hresult = hresult;
    }

    /// <summary>
    /// The NDR-encoded response payload, or an empty memory for failures without a body.
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; }

    /// <summary>
    /// The HRESULT returned by the dispatched server method.
    /// </summary>
    public int Hresult { get; }

    /// <summary>
    /// True when the HRESULT severity bit is clear.
    /// </summary>
    public bool IsSuccess => (Hresult & unchecked((int)0x80000000u)) == 0;

    /// <summary>
    /// True when the HRESULT severity bit is set.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful dispatch result.
    /// </summary>
    public static DispatchResult Success(byte[] payload, int hr = 0)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return new DispatchResult(payload, hr);
    }

    /// <summary>
    /// Creates a successful dispatch result from a <see cref="ReadOnlyMemory{T}"/> payload.
    /// </summary>
    public static DispatchResult Success(ReadOnlyMemory<byte> payload, int hr = 0) =>
        new(payload, hr);

    /// <summary>
    /// Creates an <c>E_NOTIMPL</c> dispatch result for an unknown or unsupported opnum.
    /// </summary>
    public static DispatchResult NotImplemented(int opnum)
    {
        _ = opnum;
        return new DispatchResult(ReadOnlyMemory<byte>.Empty, OpcResultId.NotImplemented.Code);
    }

    /// <summary>
    /// Creates a failed dispatch result with no response payload.
    /// </summary>
    public static DispatchResult Fault(int hr) => new(ReadOnlyMemory<byte>.Empty, hr);

    /// <summary>
    /// Converts this dispatch result to the call-channel result shape.
    /// </summary>
    public NdrCallResult ToNdrCallResult() => new(Hresult, Payload);
}

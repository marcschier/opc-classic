//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Hosting;

/// <summary>Result returned by a server-side OPC dispatcher.</summary>
public readonly struct DispatchResult
{
    private DispatchResult(byte[]? payload, int hresult)
    {
        Payload = payload;
        Hresult = hresult;
    }

    /// <summary>The NDR-encoded response payload, or <see langword="null" /> for failures without a body.</summary>
    public byte[]? Payload { get; }

    /// <summary>The HRESULT returned by the dispatched server method.</summary>
    public int Hresult { get; }

    /// <summary>True when the HRESULT severity bit is clear.</summary>
    public bool IsSuccess => (Hresult & unchecked((int)0x80000000u)) == 0;

    /// <summary>True when the HRESULT severity bit is set.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Creates a successful dispatch result.</summary>
    public static DispatchResult Success(byte[] payload, int hr = 0)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return new DispatchResult(payload, hr);
    }

    /// <summary>Creates an <c>E_NOTIMPL</c> dispatch result for an unknown or unsupported opnum.</summary>
    public static DispatchResult NotImplemented(int opnum)
    {
        _ = opnum;
        return new DispatchResult(null, OpcResultId.NotImplemented.Code);
    }

    /// <summary>Creates a failed dispatch result with no response payload.</summary>
    public static DispatchResult Fault(int hr) => new(null, hr);

    /// <summary>Converts this dispatch result to the call-channel result shape.</summary>
    public NdrCallResult ToNdrCallResult() =>
        new(Hresult, Payload is null ? ReadOnlyMemory<byte>.Empty : Payload);
}

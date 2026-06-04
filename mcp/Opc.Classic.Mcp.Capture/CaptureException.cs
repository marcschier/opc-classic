//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Exception surfaced by <see cref="ICaptureSource"/>,
/// <see cref="CaptureSession"/>, and the MCP capture tool surface for
/// user-actionable failure conditions (missing privileges, unknown
/// interface, exceeded session caps, ...).
/// </summary>
[Serializable]
public sealed class CaptureException : Exception
{
    public CaptureException()
    {
    }

    public CaptureException(string message)
        : base(message)
    {
    }

    public CaptureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

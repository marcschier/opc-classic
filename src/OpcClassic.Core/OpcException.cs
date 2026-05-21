//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic;

/// <summary>
/// Base exception for OPC Classic protocol failures. Carries the originating
/// <see cref="OpcResultId"/> (HRESULT + textual description).
/// </summary>
/// <remarks>
/// Subclasses partition errors by spec area (DA / AE / HDA / DX / …) to enable
/// scoped <c>catch</c> handlers. Vendor-specific HRESULTs that don't have a
/// known <c>OPC_E_*</c> constant surface as the base <see cref="OpcException"/>
/// — the <see cref="OpcResultId.Description"/> carries the server's text from
/// <c>IOPCServer::GetErrorString</c> when available.
/// </remarks>
public class OpcException : Exception
{
    /// <summary>The OPC HRESULT result-ID that caused this exception.</summary>
    public OpcResultId ResultId { get; }

    public OpcException()
    {
        ResultId = OpcResultId.Fail;
    }

    public OpcException(string message) : base(message)
    {
        ResultId = OpcResultId.Fail;
    }

    public OpcException(string message, Exception innerException)
        : base(message, innerException)
    {
        ResultId = OpcResultId.Fail;
    }

    public OpcException(OpcResultId resultId) : base(resultId.ToString())
    {
        ResultId = resultId;
    }

    public OpcException(OpcResultId resultId, string message) : base(message)
    {
        ResultId = resultId;
    }

    public OpcException(OpcResultId resultId, string message, Exception innerException)
        : base(message, innerException)
    {
        ResultId = resultId;
    }

    /// <summary>
    /// Throws an <see cref="OpcException"/> if <paramref name="resultId"/>
    /// indicates failure. Returns the resultId unchanged for chaining on success.
    /// </summary>
    public static OpcResultId ThrowIfFailed(OpcResultId resultId, string? operationDescription = null)
    {
        if (resultId.IsFailure)
        {
            var message = operationDescription is null
                ? resultId.ToString()
                : $"{operationDescription} failed: {resultId}";
            throw new OpcException(resultId, message);
        }
        return resultId;
    }
}


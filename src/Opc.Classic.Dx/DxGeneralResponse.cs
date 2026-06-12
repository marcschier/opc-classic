//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // DxIdentifiedResult is intentionally grouped with its response container.

namespace Opc.Classic.Dx;

/// <summary>Per-item HRESULT and optional diagnostic information returned by DX operations.</summary>
public sealed record DxIdentifiedResult(
    string? ItemPath,
    string? ItemName,
    string? Version,
    OpcResultId ResultId,
    string? DiagnosticInfo = null);

/// <summary>
/// OPC DX's <c>OpcDxGeneralResponse</c> — configuration version plus the
/// per-entity results returned by add/modify/delete/copy operations.
/// </summary>
public sealed record DxGeneralResponse
{
    /// <summary>Constructs a general DX response.</summary>
    public DxGeneralResponse(
        string? configurationVersion = null,
        DxIdentifiedResult[]? errors = null,
        int reserved = 0)
    {
        ConfigurationVersion = configurationVersion;
        Errors = NormalizeErrors(errors);
        Reserved = reserved;
    }

    /// <summary>Configuration version returned by the server.</summary>
    public string? ConfigurationVersion { get; init; }

    /// <summary>Per-entity operation results. Empty when the operation returned no item-level errors.</summary>
    public DxIdentifiedResult[] Errors { get; init; }

    /// <summary>Per-entity operation results using the OPC DX IDL member name.</summary>
    public DxIdentifiedResult[] IdentifiedResults => Errors;

    /// <summary>Reserved DWORD carried by the native structure.</summary>
    public int Reserved { get; init; }

    private static DxIdentifiedResult[] NormalizeErrors(DxIdentifiedResult[]? errors)
    {
        if (errors is null || errors.Length == 0)
        {
            return Array.Empty<DxIdentifiedResult>();
        }

        var copy = new DxIdentifiedResult[errors.Length];
        for (var i = 0; i < errors.Length; i++)
        {
            var error = errors[i];
            ArgumentNullException.ThrowIfNull(error);
            copy[i] = error;
        }

        return copy;
    }
}

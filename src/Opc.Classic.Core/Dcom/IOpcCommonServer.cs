//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom;

/// <summary>
/// Server implementation contract for OPC Common <c>IOPCCommon</c>.
/// </summary>
public interface IOpcCommonServer
{
    /// <summary>
    /// Sets the locale used for subsequent server-supplied strings.
    /// </summary>
    Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current locale used for server-supplied strings.
    /// </summary>
    Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists locale IDs supported by the server.
    /// </summary>
    Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an HRESULT to localized server text.
    /// </summary>
    Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the optional client name supplied by <c>IOPCCommon::SetClientName</c>.
    /// </summary>
    Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default);
}

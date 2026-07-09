// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Interface for setting user credentials.
/// </summary>
public interface IAuthInfo
{
    /// <summary>
    /// Returns username.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Returns password.
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Returns user's domain.
    /// </summary>
    string Domain { get; }
}

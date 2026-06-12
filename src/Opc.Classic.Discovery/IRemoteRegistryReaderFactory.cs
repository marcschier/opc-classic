//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;

namespace Opc.Classic.Discovery;

/// <summary>
/// Opens a remote Windows registry view used by <see cref="RemoteRegistryEnum" />.
/// </summary>
public interface IRemoteRegistryReaderFactory
{
    /// <summary>
    /// Connects to the remote registry on <paramref name="host" />.
    /// </summary>
    IRemoteRegistryReader Open(string host, NetworkCredential credentials);
}

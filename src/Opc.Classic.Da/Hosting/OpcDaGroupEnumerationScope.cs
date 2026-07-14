// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Selects groups returned by <c>IOPCServer::CreateGroupEnumerator</c>.
/// Values match the OPC DA <c>OPCENUMSCOPE</c> wire constants.
/// </summary>
public enum OpcDaGroupEnumerationScope : int
{
    /// <summary>Enumerates private group connections.</summary>
    PrivateConnections = 1,

    /// <summary>Enumerates public group connections.</summary>
    PublicConnections = 2,

    /// <summary>Enumerates private and public group connections.</summary>
    AllConnections = 3,

    /// <summary>Enumerates private group names.</summary>
    Private = 4,

    /// <summary>Enumerates public group names.</summary>
    Public = 5,

    /// <summary>Enumerates private and public group names.</summary>
    All = 6,
}

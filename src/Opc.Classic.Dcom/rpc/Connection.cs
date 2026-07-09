// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Constants
/// </summary>
public static class Connection
{
    /// <summary>
    /// Key to read max fragments
    /// </summary>
    public const string MAX_TRANSMIT_FRAGMENT = "rpc.connectionContext.maxTransmitFragment";

    /// <summary>
    /// Key to read max fragments
    /// </summary>
    public const string MAX_RECEIVE_FRAGMENT = "rpc.connectionContext.maxReceiveFragment";

    /// <summary>
    /// Default
    /// </summary>
    public const int DEFAULT_MAX_TRANSMIT_FRAGMENT = 4280;

    /// <summary>
    /// Default
    /// </summary>
    public const int DEFAULT_MAX_RECEIVE_FRAGMENT = 4280;
}

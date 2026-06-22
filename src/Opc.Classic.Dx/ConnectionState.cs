// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dx;

/// <summary>
/// Connection state per OPC DX 1.0 §4.1 (CONNECTION_STATE enum).
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Initial state — connection is configured but not yet connecting.
    /// </summary>
    Initial = 0,
    /// <summary>
    /// Connecting to the source server.
    /// </summary>
    Connecting = 1,
    /// <summary>
    /// Subscribing to the source items.
    /// </summary>
    Subscribing = 2,
    /// <summary>
    /// Data is flowing.
    /// </summary>
    Connected = 3,
    /// <summary>
    /// Disconnecting.
    /// </summary>
    Disconnecting = 4,
    /// <summary>
    /// Disconnected.
    /// </summary>
    Disconnected = 5,
}

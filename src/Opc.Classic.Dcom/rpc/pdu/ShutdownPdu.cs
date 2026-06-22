// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.pdu;

/// <summary>
/// Shutdown
/// </summary>
public class ShutdownPdu : ConnectionOrientedPdu
{
    public const int SHUTDOWN_TYPE = 0x11;

    /// <inheritdoc/>
    public override int Type => SHUTDOWN_TYPE;
}

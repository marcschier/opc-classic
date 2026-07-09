// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Decoded server-side IRemoteSCMActivator::RemoteCreateInstance request fields.
/// </summary>
public sealed record RemoteCreateInstanceRequest(
    Guid Clsid,
    Guid RequestedIid,
    IReadOnlyList<int> ProtocolSequences)
{
    /// <summary>
    /// Decoded activation properties supplied by the client.
    /// </summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>
    /// Raw activation property payload, when the caller has not decoded it yet.
    /// </summary>
    public byte[] RawActivationProperties { get; init; } = Array.Empty<byte>();
}

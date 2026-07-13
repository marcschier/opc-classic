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
    /// All interfaces requested by the activation call.
    /// </summary>
    public IReadOnlyList<Guid> RequestedIids { get; init; } = DefaultRequestedIids(RequestedIid);

    /// <summary>
    /// Decoded activation properties supplied by the client.
    /// </summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>
    /// Raw activation property payload, when the caller has not decoded it yet.
    /// </summary>
    public byte[] RawActivationProperties { get; init; } = Array.Empty<byte>();

    private static IReadOnlyList<Guid> DefaultRequestedIids(Guid requestedIid) =>
        requestedIid == Guid.Empty ? Array.Empty<Guid>() : new[] { requestedIid };
}

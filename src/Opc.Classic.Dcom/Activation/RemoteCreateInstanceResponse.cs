// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Activation;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Server-side IRemoteSCMActivator::RemoteCreateInstance response.
/// </summary>
public sealed record RemoteCreateInstanceResponse(int Hresult, Guid Oxid, Guid Ipid, byte[] ObjRef)
{
    /// <summary>
    /// IPID of the object exporter's IRemUnknown endpoint.
    /// </summary>
    public Guid IpidRemUnknown { get; init; }

    /// <summary>
    /// Object identifier allocated for the exported object.
    /// </summary>
    public Guid Oid { get; init; }

    /// <summary>
    /// Numeric OXID value used by ActivationPropertiesOut.
    /// </summary>
    public ulong OxidValue { get; init; }

    /// <summary>
    /// Activation properties returned to the client.
    /// </summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>
    /// Per-requested-IID activation results returned to modern clients.
    /// </summary>
    public IReadOnlyList<ActivationInterfaceResult> InterfaceResults { get; init; } = Array.Empty<ActivationInterfaceResult>();

    /// <summary>
    /// Encoded activation properties returned to the client.
    /// </summary>
    public byte[] EncodedActivationProperties { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Encoded DUALSTRINGARRAY of OXID resolver bindings.
    /// </summary>
    public byte[] OxidBindings { get; init; } = Array.Empty<byte>();
}

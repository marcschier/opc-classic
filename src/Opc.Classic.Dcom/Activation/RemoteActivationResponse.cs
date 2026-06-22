// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Decoded server-side <c>IActivation::RemoteActivation</c> response fields.
/// Mirrors the output-side of <c>[MS-DCOM] §3.1.2.5.2.3.1</c>.
/// </summary>
/// <param name="Hresult">Overall HRESULT for the activation (<c>phr</c>).</param>
/// <param name="Oxid">Object exporter identifier for the activated object.</param>
/// <param name="IpidRemUnknown">IPID of the <c>IRemUnknown</c> interface on the activated object.</param>
/// <param name="AuthnHint">Authentication-level hint for subsequent calls.</param>
/// <param name="ServerVersion">DCOM version of the responding server (major/minor).</param>
/// <param name="InterfaceResults">Per-requested-IID HRESULT + OBJREF payloads.</param>
public sealed record RemoteActivationResponse(
    int Hresult,
    Guid Oxid,
    Guid IpidRemUnknown,
    uint AuthnHint,
    (ushort Major, ushort Minor) ServerVersion,
    IReadOnlyList<RemoteActivationInterfaceResult> InterfaceResults)
{
    /// <summary>
    /// Encoded DUALSTRINGARRAY of OXID resolver bindings.
    /// </summary>
    public ReadOnlyMemory<byte> OxidBindings { get; init; }
}

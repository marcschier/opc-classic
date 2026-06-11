//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Source-generated proxy/dispatcher surface for the LEGACY <c>IActivation</c>
/// interface (UUID <c>4d9f4ab8-7d1c-11cf-861e-0020af6e7c57</c>) defined in
/// <c>[MS-DCOM] §3.1.2.5.2.3</c> and used by pre-XP-SP2 DCOM clients.
/// </summary>
/// <remarks>
/// <para>
/// IActivation is the original DCOM activation interface and supports a single
/// opnum (0) <c>RemoteActivation</c> that combines class-object resolution and
/// instance creation. The modern equivalent is
/// <c>IRemoteSCMActivator::RemoteGetClassObject</c> (opnum 3) +
/// <c>RemoteCreateInstance</c> (opnum 4) introduced in DCOM v5.6 (Windows XP SP2+).
/// </para>
/// <para>
/// This interface enables interop with pre-XP-SP2 DCOM servers (when running
/// over either <c>ncacn_ip_tcp</c> on port 135 OR <c>ncacn_np</c> via the
/// SMB transport added in <c>Opc.Classic.Dcom.Smb</c>). The wire-format codec
/// is emitted by the source generator from this declaration.
/// </para>
/// </remarks>
[OpcInterface(Opc.Classic.Dcom.Interfaces.IID_IActivation)]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IActivation
{
    /// <summary>
    /// <c>IActivation::RemoteActivation</c> (opnum 0). Per <c>[MS-DCOM] §3.1.2.5.2.3.1</c>,
    /// the wire signature has many additional output parameters
    /// (<c>pOxid</c>, <c>ppdsaOxidBindings</c>, <c>pipidRemUnknown</c>,
    /// <c>pAuthnHint</c>, <c>pServerVersion</c>, <c>phr</c>, <c>ppInterfaceData</c>,
    /// <c>pResults</c>). The generator-emitted proxy stub maps the per-IID activation
    /// outcomes via a server-side <see cref="IActivationServer.RemoteActivationAsync(RemoteActivationRequest, CancellationToken)" />
    /// hook; this simplified signature is the high-level entry point.
    /// </summary>
    [OpcMethod(0)]
    Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default);
}

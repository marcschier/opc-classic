//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Generators;

namespace Opc.Classic.Dcom.Core;

/// <summary>Source-generated proxy/dispatcher surface for IRemoteSCMActivator.</summary>
[OpcInterface(Opc.Classic.Dcom.Interfaces.IID_IRemoteSCMActivator)]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IRemoteSCMActivator
{
    /// <summary>IRemoteSCMActivator::RemoteGetClassObject (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> RemoteGetClassObjectAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default);

    /// <summary>IRemoteSCMActivator::RemoteCreateInstance (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> RemoteCreateInstanceAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default);
}

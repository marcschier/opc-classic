//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Dcom;

namespace Opc.Classic.Discovery;

/// <summary>
/// Creates DCOM call channels for OPCEnum activation and for interfaces returned by OBJREFs.
/// </summary>
public interface IOpcEnumCallChannelFactory
{
    /// <summary>
    /// Gets the minimum authentication level to declare in IRemoteSCMActivator activation properties.
    /// Hardened Windows DCOM requires at least packet integrity for activation.
    /// </summary>
    OpcProtectionLevel ActivationProtectionLevel => OpcProtectionLevel.Integrity;

    /// <summary>Creates a channel bound to the remote SCM activation interface.</summary>
    ValueTask<ICallChannel> CreateActivationChannelAsync(
        string host,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a channel bound to an activated OPCEnum interface pointer.</summary>
    ValueTask<ICallChannel> CreateObjectChannelAsync(
        string host,
        IOpcInterfaceRef interfaceRef,
        Guid interfaceId,
        CancellationToken cancellationToken = default);
}

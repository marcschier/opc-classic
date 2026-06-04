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

    /// <summary>
    /// Creates a channel bound to an activated OPCEnum interface pointer, with
    /// the OXID-resolver bindings returned by the activation call. The OXID
    /// bindings (<c>ppdsaOxidBindings</c> from MS-DCOM §3.1.2.5.2.3.1) carry
    /// the data-port endpoint (e.g. <c>HOST[57539]</c>) needed to reach the
    /// activated object; the OBJREF's own <c>ResolverBindings</c> point only
    /// at the OXID resolver (port 135) and lack data-port info.
    /// </summary>
    /// <remarks>
    /// Default implementation forwards to <see cref="CreateObjectChannelAsync(string, IOpcInterfaceRef, Guid, CancellationToken)"/>
    /// for backwards compatibility. Implementations that need the data-port
    /// endpoint should override this overload.
    /// </remarks>
    ValueTask<ICallChannel> CreateObjectChannelAsync(
        string host,
        IOpcInterfaceRef interfaceRef,
        Guid interfaceId,
        ReadOnlyMemory<byte> oxidBindings,
        CancellationToken cancellationToken = default) =>
        CreateObjectChannelAsync(host, interfaceRef, interfaceId, cancellationToken);
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC Security DCOM-projection interfaces. Each [OpcInterface] partial
// interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId.
//
// Cross-platform note (Phase 3D/3E):
//   IOPCSecurityNT historically depends on Windows SSPI to enumerate the
//   caller's authenticated identity. The Opc.Classic.Dcom.Kerberos stack
//   (Phase 3D scaffold) replaces SSPI with Kerberos.NET-based credential
//   acquisition. The cross-platform server-side implementation of
//   IsAvailableNT returns true iff a configured Kerberos KDC is reachable
//   from the server.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCSecurityNT)
#pragma warning disable MA0048 // Two trivial interface stubs grouped for readability

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace Opc.Classic.Security.Dcom;

/// <summary><c>IOPCSecurityNT</c> - Windows-integrated authentication (IID_IOPCSecurityNT).</summary>
[OpcInterface("7AA83A01-6C77-11D3-84F9-00008630A38B")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCSecurityNT {
    /// <summary><c>IOPCSecurityNT::IsAvailableNT</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<bool> IsAvailableNTAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCSecurityNT::QueryMinImpersonationLevel</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> QueryMinImpersonationLevelAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCSecurityNT::ChangeUser</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task ChangeUserAsync(CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCSecurityPrivate</c> - server-defined username/password (IID_IOPCSecurityPrivate).</summary>
[OpcInterface("7AA83A02-6C77-11D3-84F9-00008630A38B")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCSecurityPrivate {
    /// <summary><c>IOPCSecurityPrivate::IsAvailablePriv</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<bool> IsAvailablePrivAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCSecurityPrivate::Logon</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task LogonAsync(string userId, string password, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCSecurityPrivate::Logoff</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task LogoffAsync(CancellationToken cancellationToken = default);
}

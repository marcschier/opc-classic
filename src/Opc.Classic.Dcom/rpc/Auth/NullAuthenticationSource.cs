//
// SPDX-License-Identifier: MIT
// Copyright (c) 2013 Vikram Roopchand
// Copyright (c) 2026 Opc.Classic .NET Contributors (Phase 2H modernization)
//
// Originally Eclipse Public License v1.0 (https://www.eclipse.org/legal/epl-v10.html).
//

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;
using SharpCifs.Util.Sharpen;
using System;

namespace SharpInterop.Rpc.Auth.ntlm;

/// <summary>
/// Fallback authentication source: every method throws
/// <see cref="InvalidOperationException"/> with a clear message pointing
/// at the <see cref="AuthenticationSource.SetDefaultInstance(AuthenticationSource?)"/>
/// registration API.
/// </summary>
public sealed class NullAuthenticationSource : AuthenticationSource {

    /// <summary>Singleton instance.</summary>
    public static NullAuthenticationSource Instance { get; } = new NullAuthenticationSource();

    private NullAuthenticationSource() { }

    private const string NotRegisteredMessage =
        "No AuthenticationSource has been registered. Call " +
        "AuthenticationSource.SetDefaultInstance(...) once at host startup " +
        "to install a custom credential-validation implementation.";

    /// <inheritdoc />
    public override byte[] CreateChallenge(PropertyBag properties, Type1Message type1)
        => throw new InvalidOperationException(NotRegisteredMessage);

    /// <inheritdoc />
    public override sbyte[] Authenticate(PropertyBag properties, Type2Message type2, Type3Message type3)
        => throw new InvalidOperationException(NotRegisteredMessage);
}

// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Fallback authentication source: every method throws
/// <see cref="InvalidOperationException"/> with a clear message pointing
/// at the <see cref="AuthenticationSource.SetDefaultInstance(AuthenticationSource?)"/>
/// registration API.
/// </summary>
public sealed class NullAuthenticationSource : AuthenticationSource
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
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

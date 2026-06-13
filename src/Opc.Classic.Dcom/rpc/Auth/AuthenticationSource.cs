// SPDX-License-Identifier: MIT
//
// replaced the original Java SPI / META-INF/services service-loader
// (incompatible with NativeAOT because it uses runtime ClassLoader / Type.GetType
// reflection) with a simple register-at-startup pattern. Consumers — typically a
// Microsoft.Extensions.DependencyInjection container — register their custom
// AuthenticationSource via SetDefaultInstance(...) once during host startup.
//

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.Ntlm;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Server-side NTLM authentication source. Consumer-pluggable: an
/// implementation validates incoming NTLM credentials against the
/// consumer's credential store (e.g. an in-memory user table, LDAP, an
/// Active Directory bind, etc.).
/// </summary>
/// <remarks>
/// This contract is only exercised when the managed process is acting as
/// an inbound DCOM server (the <see cref="Opc.Classic.Dcom.Core.LocalCoClass"/>
/// + <see cref="Opc.Classic.Dcom.Core.ComOxidRuntime"/> path receiving callback
/// PDUs). Pure client scenarios never construct an
/// <see cref="AuthenticationSource"/>.
/// <para>
/// Register an implementation at host startup via
/// <see cref="SetDefaultInstance(AuthenticationSource?)"/>. If no
/// implementation is registered, <see cref="DefaultInstance"/> returns
/// <see cref="NullAuthenticationSource.Instance"/> which throws
/// <see cref="InvalidOperationException"/> on any auth attempt.
/// </para>
/// </remarks>
public abstract class AuthenticationSource
{
    private static AuthenticationSource? s_default;

    /// <summary>
    /// The current default authentication source. Never returns
    /// <see langword="null"/>; returns <see cref="NullAuthenticationSource.Instance"/>
    /// (which throws on auth attempts) until an implementation is registered
    /// via <see cref="SetDefaultInstance(AuthenticationSource?)"/>.
    /// </summary>
    public static AuthenticationSource DefaultInstance =>
        Volatile.Read(ref s_default) ?? NullAuthenticationSource.Instance;

    /// <summary>
    /// Register the global default authentication source. Intended to be
    /// called once at host startup by the DI container or test fixture.
    /// </summary>
    /// <param name="source">
    /// The implementation to use, or <see langword="null"/> to reset to
    /// the no-op default (subsequent auth attempts will throw).
    /// </param>
    public static void SetDefaultInstance(AuthenticationSource? source)
    {
        Interlocked.Exchange(ref s_default, source);
    }

    /// <summary>
    /// Produce an 8-byte NTLM challenge for the given Type-1 message and
    /// session properties.
    /// </summary>
    /// <exception cref="IOException">Underlying credential store I/O failure.</exception>
    public abstract byte[] CreateChallenge(PropertyBag properties,
        Type1Message type1);

    /// <summary>
    /// Validate the client's Type-3 response against the (Type-2 challenge,
    /// session properties) and return the session key bytes used for
    /// integrity / privacy on subsequent traffic.
    /// </summary>
    /// <exception cref="IOException">Underlying credential store I/O failure.</exception>
    public abstract sbyte[] Authenticate(PropertyBag properties,
        Type2Message type2, Type3Message type3);
}

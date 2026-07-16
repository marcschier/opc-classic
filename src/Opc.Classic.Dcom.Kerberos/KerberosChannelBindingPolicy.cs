// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Controls channel-binding enforcement for inbound Kerberos authentication.
/// </summary>
public enum KerberosChannelBindingPolicy
{
    /// <summary>
    /// Do not validate channel bindings.
    /// </summary>
    Disabled,

    /// <summary>
    /// Validate a binding supplied by the peer against the configured expected
    /// hash, but permit an absent binding.
    /// </summary>
    WhenPresent,

    /// <summary>
    /// Require a configured expected hash and reject absent or mismatched bindings.
    /// </summary>
    Required,
}

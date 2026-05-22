//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

// PHASE 3D SCAFFOLD - Kerberos / SPNEGO authentication for OpcClassic.Dcom.
//
// The current commit defines the API surface (KerberosAuthInfo, KerberosConnectionContext)
// and takes the Kerberos.NET package dependency. The AcquireApRequestAsync and
// ProcessApResponseAsync method bodies are NotImplementedException - intentional, so
// follow-up Phase 3D work fills them in with KDC + service-ticket-request integration
// without API churn.
//
// Phase 3E adds SPNEGO negotiation (wrapping AP-REQ/AP-REP in SPNEGO token frames).
// Phase 3F adds EPA (Extended Protection for Authentication / channel binding).

#pragma warning disable MA0025 // Phase 3D intentionally exposes NotImplementedException scaffold methods.

namespace OpcClassic.Dcom.Kerberos;

/// <summary>
/// Owns the per-connection Kerberos authentication handshake state.
/// </summary>
public sealed class KerberosConnectionContext
{
    private readonly KerberosAuthInfo _info;

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosConnectionContext" /> class.
    /// </summary>
    /// <param name="info">Kerberos authentication configuration.</param>
    public KerberosConnectionContext(KerberosAuthInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        _info = info;
    }

    /// <summary>
    /// Acquires an AP-REQ token for the configured SPN. Returns the GSS-API token bytes
    /// suitable for embedding in a DCOM bind PDU (after wrapping in SPNEGO if SPNEGO
    /// negotiation is enabled - see Phase 3E).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the future KDC request flow.</param>
    /// <returns>The AP-REQ token bytes.</returns>
    public Task<byte[]> AcquireApRequestAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        throw new NotImplementedException(CreateScaffoldMessage(nameof(AcquireApRequestAsync)));
    }

    /// <summary>
    /// Processes the server's AP-REP token to complete the mutual-auth handshake and
    /// derive the session key.
    /// </summary>
    /// <param name="apReply">AP-REP token bytes returned by the server.</param>
    /// <param name="cancellationToken">Cancellation token for the future AP-REP processing flow.</param>
    /// <returns>The derived session key bytes.</returns>
    public Task<byte[]> ProcessApResponseAsync(ReadOnlyMemory<byte> apReply, CancellationToken cancellationToken = default)
    {
        _ = apReply;
        _ = cancellationToken;
        throw new NotImplementedException(CreateScaffoldMessage(nameof(ProcessApResponseAsync)));
    }

    private string CreateScaffoldMessage(string methodName)
    {
        return "Phase 3D scaffold: " + methodName + " is not yet implemented for SPN '" + _info.Spn +
            "' in realm '" + _info.Realm + "'. Full implementation requires KDC integration via " +
            "Kerberos.NET's KerberosClient plus a service ticket request flow. See Phase 3D follow-up.";
    }
}

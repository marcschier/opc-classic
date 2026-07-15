// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// Applies integrity and privacy protection for an established authentication session.
/// </summary>
public interface IRpcServerProtectionContext
{
    /// <summary>
    /// Gets the RPC authentication service identifier.
    /// </summary>
    int AuthenticationService { get; }

    /// <summary>
    /// Gets the negotiated protection level.
    /// </summary>
    OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// Gets the wire verifier length.
    /// </summary>
    int VerifierLength { get; }

    /// <summary>
    /// Gets the verifier length for a specific signed and confidential region.
    /// </summary>
    int GetVerifierLength(int signedRegionLength, int confidentialLength) => VerifierLength;

    /// <summary>
    /// Protects an outgoing signed region and returns its authentication verifier.
    /// </summary>
    void Protect(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        out byte[] verifier);

    /// <summary>
    /// Verifies and unprotects an incoming signed region.
    /// </summary>
    bool Unprotect(
        Span<byte> signedRegion,
        int confidentialOffset,
        int confidentialLength,
        ReadOnlyMemory<byte> verifier);
}

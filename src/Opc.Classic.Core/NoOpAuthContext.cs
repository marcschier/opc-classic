// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Authentication context for unauthenticated in-memory and loopback test scenarios.
/// </summary>
public sealed class NoOpAuthContext : IAuthContext
{
    /// <summary>
    /// Gets the reusable no-op authentication context.
    /// </summary>
    public static NoOpAuthContext Instance { get; } = new();

    /// <inheritdoc />
    public OpcProtectionLevel ProtectionLevel => OpcProtectionLevel.None;

    /// <inheritdoc />
    public byte[] BuildInitialToken() => [];

    /// <inheritdoc />
    public byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken)
    {
        _ = serverToken;
        return [];
    }

    /// <inheritdoc />
    public void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature)
    {
        _ = signedRegion;
        _ = confidentialOffset;
        _ = confidentialLength;
        signature = [];
    }

    /// <inheritdoc />
    public bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature)
    {
        _ = signedRegion;
        _ = confidentialOffset;
        _ = confidentialLength;
        return signature.IsEmpty;
    }
}

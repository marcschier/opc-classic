// Copyright (c) 2026 marcschier. Licensed under the MIT License.

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
    public void SignAndSeal(Span<byte> pduBody, out byte[] signature)
    {
        _ = pduBody;
        signature = [];
    }

    /// <inheritdoc />
    public bool VerifyAndUnseal(Span<byte> pduBody, ReadOnlyMemory<byte> signature)
    {
        _ = pduBody;
        return signature.IsEmpty;
    }
}

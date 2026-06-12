//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom.Internal.Ntlm;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class NtlmSignatureBlockTests
{
    private static readonly byte[] SigningKey = Convert.FromHexString("4788DC861B4782F35D43FD98FE1A2D39");
    private static readonly byte[] WrongSigningKey = Convert.FromHexString("608B5D0EAE576880AE23E4C6F2E5813F");
    private static readonly byte[] PlaintextMessage = Convert.FromHexString("50006C00610069006E007400650078007400");

    /// <summary>
    /// MS-NLMP §3.4.4 defines the 16-byte SIGNATURE_BLOCK layout, and §3.4.5 defines the
    /// HMAC-MD5 checksum over SeqNum || Message used by extended session security signing.
    /// </summary>
    [Test]
    [Arguments(0u, "0100000070352851F256430900000000")]
    [Arguments(1u, "01000000126C5D58DA2144D601000000")]
    [Arguments(0xFFFFFFFEu, "01000000CCE5C9B18629556FFEFFFFFF")]
    public async Task Sign_FormsExpectedMsNlmpSignatureBlock(uint sequenceNumber, string expectedHex)
    {
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber);

        await Assert.That(Convert.ToHexString(signature)).IsEqualTo(expectedHex);
    }

    /// <summary>
    /// MS-NLMP §3.4.4 requires the receiver to compare the expected sequence number, and §3.4.5
    /// requires the receiver to verify the SIGNATURE_BLOCK checksum before accepting the message.
    /// </summary>
    [Test]
    public async Task Verify_AcceptsSignatureProducedWithMatchingKeyMessageAndSequence()
    {
        var receiver = new OrderedNtlmSignatureReceiver(SigningKey);
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);

        bool accepted = receiver.Verify(PlaintextMessage, signature);

        await Assert.That(accepted).IsTrue();
    }

    /// <summary>
    /// MS-NLMP §3.4.4 requires rejection when the SIGNATURE_BLOCK SeqNum is not the expected value,
    /// and §3.4.5 binds that same sequence number into the HMAC-MD5 checksum input.
    /// </summary>
    [Test]
    public async Task Verify_RejectsSignatureWithTamperedSequenceNumberBytes()
    {
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);
        signature[12] = 0x01;

        bool accepted = NtlmMessageSignature.Verify(SigningKey, PlaintextMessage, signature, sequenceNumber: 0);

        await Assert.That(accepted).IsFalse();
    }

    /// <summary>
    /// MS-NLMP §3.4.4 carries the eight-byte checksum in the SIGNATURE_BLOCK, and §3.4.5 requires
    /// mismatch detection when any checksum byte differs from the HMAC-MD5 result.
    /// </summary>
    [Test]
    public async Task Verify_RejectsSignatureWithTamperedChecksumBytes()
    {
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);
        signature[4] ^= 0x80;

        bool accepted = NtlmMessageSignature.Verify(SigningKey, PlaintextMessage, signature, sequenceNumber: 0);

        await Assert.That(accepted).IsFalse();
    }

    /// <summary>
    /// MS-NLMP §3.4.4 signature verification is keyed to the negotiated session security state, and
    /// §3.4.5 requires a different signing/sealing key to produce a different checksum.
    /// </summary>
    [Test]
    public async Task Verify_RejectsSignatureProducedWithDifferentNegotiatedKey()
    {
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);

        bool accepted = NtlmMessageSignature.Verify(WrongSigningKey, PlaintextMessage, signature, sequenceNumber: 0);

        await Assert.That(accepted).IsFalse();
    }

    /// <summary>
    /// MS-NLMP §3.4.4 requires connection-oriented receivers to accept only the next sequence number,
    /// and §3.4.5 binds each SIGNATURE_BLOCK checksum to that monotonically increasing sequence.
    /// </summary>
    [Test]
    public async Task Verify_RejectsReplayedSignatureBlockWithAlreadyAcceptedSequenceNumber()
    {
        var receiver = new OrderedNtlmSignatureReceiver(SigningKey);
        byte[] signature = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);

        bool firstAccepted = receiver.Verify(PlaintextMessage, signature);
        bool replayAccepted = receiver.Verify(PlaintextMessage, signature);

        await Assert.That(firstAccepted).IsTrue();
        await Assert.That(replayAccepted).IsFalse();
    }

    /// <summary>
    /// MS-NLMP §3.4.4 defines SeqNum as a 32-bit unsigned integer, and §3.4.5 requires signing to
    /// continue using the exact little-endian sequence bytes at the UInt32 wrap boundary.
    /// </summary>
    [Test]
    public async Task Verify_AcceptsExpectedSignatureBlocksAtUInt32WrapBoundary()
    {
        var receiver = new OrderedNtlmSignatureReceiver(SigningKey, initialSequenceNumber: uint.MaxValue - 1u);
        byte[] penultimate = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, uint.MaxValue - 1u);
        byte[] final = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, uint.MaxValue);
        byte[] wrapped = NtlmMessageSignature.Sign(SigningKey, PlaintextMessage, sequenceNumber: 0);

        bool penultimateAccepted = receiver.Verify(PlaintextMessage, penultimate);
        bool finalAccepted = receiver.Verify(PlaintextMessage, final);
        bool wrappedAccepted = receiver.Verify(PlaintextMessage, wrapped);

        await Assert.That(penultimateAccepted).IsTrue();
        await Assert.That(finalAccepted).IsTrue();
        await Assert.That(wrappedAccepted).IsTrue();
    }

    private sealed class OrderedNtlmSignatureReceiver
    {
        private readonly byte[] _signingKey;
        private uint _nextSequenceNumber;

        public OrderedNtlmSignatureReceiver(ReadOnlySpan<byte> signingKey, uint initialSequenceNumber = 0)
        {
            _signingKey = signingKey.ToArray();
            _nextSequenceNumber = initialSequenceNumber;
        }

        public bool Verify(ReadOnlySpan<byte> message, ReadOnlySpan<byte> signature)
        {
            if (!NtlmMessageSignature.Verify(_signingKey, message, signature, _nextSequenceNumber))
            {
                return false;
            }

            unchecked
            {
                _nextSequenceNumber++;
            }

            return true;
        }
    }
}

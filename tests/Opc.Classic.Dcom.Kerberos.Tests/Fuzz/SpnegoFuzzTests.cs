//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Formats.Asn1;
using System.IO;
using CsCheck;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Tests.Fuzz;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests.Fuzz;

public sealed class SpnegoFuzzTests {
    private static readonly Type[] AllowedSpnegoExceptions =
    [
        typeof(InvalidDataException),
        typeof(FormatException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(EndOfStreamException),
        typeof(AsnContentException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task NegToken_Parse_RandomBytes_DoesNotCrash() {
        FuzzHarness.BytesEdgeWeighted.Sample(
            static input => FuzzHarness.AssertParseDoesNotCrash(
                input,
                static object (ReadOnlyMemory<byte> bytes) => ParseEitherNegToken(bytes),
                AllowedSpnegoExceptions),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task NegToken_Parse_MutatedValid_DoesNotCrash() {
        foreach (byte[] valid in ValidTokens()) {
            FuzzHarness.MutateValid(valid).Sample(
                static input => FuzzHarness.AssertParseDoesNotCrash(
                    input,
                    static object (ReadOnlyMemory<byte> bytes) => ParseEitherNegToken(bytes),
                    AllowedSpnegoExceptions),
                iter: FuzzHarness.Iterations,
                threads: 1);

            foreach (byte[] mutated in DerLengthAndTagMutations(valid)) {
                FuzzHarness.AssertParseDoesNotCrash(
                    mutated,
                    static object (ReadOnlyMemory<byte> bytes) => ParseEitherNegToken(bytes),
                    AllowedSpnegoExceptions);
            }
        }

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    private static object ParseEitherNegToken(ReadOnlyMemory<byte> bytes) {
        try {
            return SpnegoDecoder.DecodeNegTokenInit(bytes);
        }
        catch (Exception ex) when (IsAllowed(ex)) {
            return SpnegoDecoder.DecodeNegTokenResp(bytes);
        }
    }

    private static bool IsAllowed(Exception ex) {
        foreach (Type type in AllowedSpnegoExceptions) {
            if (type.IsAssignableFrom(ex.GetType())) {
                return true;
            }
        }

        return false;
    }

    private static byte[][] ValidTokens() =>
    [
        SpnegoEncoder.EncodeNegTokenInit(new SpnegoNegTokenInit(
            [SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp],
            new byte[] { 0x01, 0x02, 0x03 },
            null)),
        SpnegoEncoder.EncodeNegTokenResp(new SpnegoNegTokenResp(
            SpnegoNegState.AcceptIncomplete,
            SpnegoOids.KerberosV5,
            new byte[] { 0x04, 0x05, 0x06 },
            new byte[] { 0x07, 0x08 })),
    ];

    private static byte[][] DerLengthAndTagMutations(byte[] input) {
        byte[] tlvLengthOverflow = (byte[])input.Clone();
        if (tlvLengthOverflow.Length > 1) {
            tlvLengthOverflow[1] = 0x82;
        }

        byte[] swappedTag = (byte[])input.Clone();
        if (swappedTag.Length > 0) {
            swappedTag[0] ^= 0x20;
        }

        byte[] indefiniteLength = (byte[])input.Clone();
        if (indefiniteLength.Length > 1) {
            indefiniteLength[1] = 0x80;
        }

        return [tlvLengthOverflow, swappedTag, indefiniteLength];
    }
}

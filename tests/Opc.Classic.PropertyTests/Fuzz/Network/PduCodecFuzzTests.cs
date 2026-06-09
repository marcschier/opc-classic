//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using CsCheck;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Tests.Fuzz;
using TUnit.Core;

namespace Opc.Classic.PropertyTests.Fuzz.Network;

public sealed class PduCodecFuzzTests {
    private static readonly Type[] AllowedPduExceptions =
    [
        typeof(InvalidDataException),
        typeof(EndOfStreamException),
        typeof(IOException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(NotSupportedException),
        typeof(InvalidOperationException),
        typeof(NdrException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task PduCodec_DecodePdu_RandomBytes_DoesNotCrash() {
        FuzzHarness.BytesEdgeWeighted.Sample(
            input => AssertPduParseDoesNotCrash(input),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task PduCodec_DecodePdu_MutatedValidBind_DoesNotCrash() {
        byte[] valid = Encode(CreatePdu("BindPdu"));
        FuzzHarness.MutateValid(valid).Sample(
            input => AssertPduParseDoesNotCrash(input),
            iter: FuzzHarness.Iterations,
            threads: 1);

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    [Arguments("BindPdu")]
    [Arguments("BindAcknowledgePdu")]
    [Arguments("BindNoAcknowledgePdu")]
    [Arguments("AlterContextPdu")]
    [Arguments("AlterContextResponsePdu")]
    [Arguments("RequestCoPdu")]
    [Arguments("ResponseCoPdu")]
    [Arguments("FaultCoPdu")]
    [Arguments("CancelCoPdu")]
    [Arguments("ShutdownPdu")]
    [Arguments("Auth3Pdu")]
    [Arguments("OrphanedPdu")]
    public async Task PduCodec_DecodePdu_StructuralMutations_DoesNotCrash(string pduType) {
        byte[] valid = Encode(CreatePdu(pduType));
        foreach (byte[] mutated in StructuralMutations(valid)) {
            AssertPduParseDoesNotCrash(mutated);
        }

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    [Arguments("BindAcknowledgePdu")]
    [Arguments("BindNoAcknowledgePdu")]
    [Arguments("AlterContextResponsePdu")]
    [Arguments("RequestCoPdu")]
    [Arguments("ResponseCoPdu")]
    [Arguments("FaultCoPdu")]
    [Arguments("CancelCoPdu")]
    [Arguments("ShutdownPdu")]
    [Arguments("Auth3Pdu")]
    [Arguments("OrphanedPdu")]
    public async Task PduCodec_EncodeDecodeEncode_ValidPdu_CanonicalRoundTrips(string pduType) {
        byte[] encoded = Encode(CreatePdu(pduType));
        ConnectionOrientedPdu decoded = PduCodec.DecodePdu(encoded);
        decoded.CallId = BinaryPrimitives.ReadInt32LittleEndian(
            encoded.AsSpan(ConnectionOrientedPdu.CALL_ID_OFFSET, sizeof(int)));
        byte[] reencoded = Encode(decoded);
        ConnectionOrientedPdu redecoded = PduCodec.DecodePdu(reencoded);
        redecoded.CallId = decoded.CallId;
        byte[] canonical = Encode(redecoded);

        await Assert.That(canonical.SequenceEqual(reencoded)).IsTrue();
    }

    [Test]
    [Category("Fuzz")]
    public async Task PduCodec_CorpusReplay_DoesNotCrash() {
        foreach (object[] row in FuzzHarness.LoadCorpus("PduCodec")) {
            AssertPduParseDoesNotCrash((byte[])row[0]);
        }

        bool completed = true;
        await Assert.That(completed).IsTrue();
    }

    private static void AssertPduParseDoesNotCrash(byte[] input) =>
        FuzzHarness.AssertParseDoesNotCrash(
            input,
            static ConnectionOrientedPdu (ReadOnlyMemory<byte> bytes) => PduCodec.DecodePdu(bytes.ToArray()),
            AllowedPduExceptions,
            static pdu => {
                if (pdu.FragmentLength < ConnectionOrientedPdu.HEADER_LENGTH) {
                    throw new InvalidDataException("Decoded PDU advertised a fragment shorter than the DCE/RPC header.");
                }
            });

    private static byte[] Encode(ConnectionOrientedPdu pdu) =>
        PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

    private static ConnectionOrientedPdu CreatePdu(string pduType) => pduType switch {
        "BindPdu" => new BindPdu {
            CallId = 1,
            ContextList = [NewPresentationContext()],
        },
        "BindAcknowledgePdu" => new BindAcknowledgePdu {
            CallId = 2,
            SecondaryAddress = new Port("135"),
            ResultList = [new PresentationResult()],
        },
        "BindNoAcknowledgePdu" => new BindNoAcknowledgePdu {
            CallId = 3,
            RejectReason = BindNoAcknowledgeReason.REASON_NOT_SPECIFIED,
        },
        "AlterContextPdu" => new AlterContextPdu {
            CallId = 4,
            ContextList = [NewPresentationContext()],
        },
        "AlterContextResponsePdu" => new AlterContextResponsePdu {
            CallId = 5,
            SecondaryAddress = new Port("135"),
            ResultList = [new PresentationResult()],
        },
        "RequestCoPdu" => new RequestCoPdu {
            CallId = 6,
            ContextId = 1,
            Opnum = 1,
            AllocationHint = 4,
            Stub = [0x01, 0x02, 0x03, 0x04],
        },
        "ResponseCoPdu" => new ResponseCoPdu {
            CallId = 7,
            ContextId = 1,
            AllocationHint = 4,
            Stub = [0x05, 0x06, 0x07, 0x08],
        },
        "FaultCoPdu" => new FaultCoPdu {
            CallId = 8,
            ContextId = 1,
            Status = FaultCode.UNSPECIFIED_REJECTION,
            Stub = [],
        },
        "CancelCoPdu" => new CancelCoPdu { CallId = 9 },
        "ShutdownPdu" => new ShutdownPdu { CallId = 10 },
        "Auth3Pdu" => new Auth3Pdu { CallId = 11 },
        "OrphanedPdu" => new OrphanedPdu { CallId = 12 },
        _ => throw new ArgumentOutOfRangeException(nameof(pduType), pduType, "Unknown PDU type."),
    };

    private static PresentationContext NewPresentationContext() =>
        new(1, new PresentationSyntax(new UUID("00000131-0000-0000-c000-000000000046"), 0, 0));

    private static byte[][] StructuralMutations(byte[] input) =>
    [
        input.AsSpan(0, Math.Max(0, Math.Min(input.Length, ConnectionOrientedPdu.HEADER_LENGTH - 1))).ToArray(),
        WithUInt16(input, ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, ushort.MaxValue),
        WithUInt16(input, ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, ushort.MaxValue),
        OverlapStubAndAuth(input),
    ];

    private static byte[] WithUInt16(byte[] input, int offset, ushort value) {
        byte[] copy = (byte[])input.Clone();
        if (copy.Length >= offset + sizeof(ushort)) {
            BinaryPrimitives.WriteUInt16LittleEndian(copy.AsSpan(offset, sizeof(ushort)), value);
        }

        return copy;
    }

    private static byte[] OverlapStubAndAuth(byte[] input) {
        byte[] copy = WithUInt16(input, ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 8);
        if (copy.Length >= ConnectionOrientedPdu.FRAG_LENGTH_OFFSET + sizeof(ushort)) {
            BinaryPrimitives.WriteUInt16LittleEndian(
                copy.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, sizeof(ushort)),
                (ushort)Math.Min(copy.Length, ConnectionOrientedPdu.HEADER_LENGTH + 1));
        }

        return copy;
    }
}

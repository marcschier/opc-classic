//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Transport;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class OrpcEnvelopeHelpersTests
{
    [Test]
    public async Task BuildRequestStub_ExtractRequestBody_round_trip_preserves_payload()
    {
        byte[] payload = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
        Guid causalityId = Guid.NewGuid();

        byte[] stub = OrpcEnvelope.BuildRequestStub(payload, causalityId);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractRequestBody(stub);

        await Assert.That(body.ToArray()).IsEquivalentTo(payload);
    }

    [Test]
    public async Task BuildResponseStub_ExtractResponseBody_round_trip_preserves_payload()
    {
        byte[] payload = [0x10, 0x20, 0x30, 0x40];

        byte[] stub = OrpcEnvelope.BuildResponseStub(payload);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractResponseBody(stub);

        await Assert.That(body.ToArray()).IsEquivalentTo(payload);
    }

    [Test]
    public async Task BuildRequestStub_supports_empty_payload()
    {
        Guid causalityId = Guid.NewGuid();

        byte[] stub = OrpcEnvelope.BuildRequestStub(Array.Empty<byte>(), causalityId);
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractRequestBody(stub);

        await Assert.That(body.Length).IsEqualTo(0);
    }

    [Test]
    public async Task BuildResponseStub_supports_empty_payload()
    {
        byte[] stub = OrpcEnvelope.BuildResponseStub(Array.Empty<byte>());
        ReadOnlyMemory<byte> body = OrpcEnvelope.ExtractResponseBody(stub);

        await Assert.That(body.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ExtractRequestBody_throws_on_empty_stub()
    {
        await Assert.That(() => { _ = OrpcEnvelope.ExtractRequestBody(Array.Empty<byte>()); })
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExtractResponseBody_throws_on_empty_stub()
    {
        await Assert.That(() => { _ = OrpcEnvelope.ExtractResponseBody(Array.Empty<byte>()); })
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExtractRequestBody_throws_on_null_stub()
    {
        await Assert.That(() => { _ = OrpcEnvelope.ExtractRequestBody(null!); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ExtractResponseBody_throws_on_null_stub()
    {
        await Assert.That(() => { _ = OrpcEnvelope.ExtractResponseBody(null!); })
            .Throws<ArgumentNullException>();
    }
}

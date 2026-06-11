//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Bidirectional helper for wrapping and unwrapping OPC ORPC envelopes around
/// raw NDR method bodies. Requests carry an
/// <see cref="OrpcThis" /> prefix; responses carry an
/// <see cref="OrpcThat" /> prefix. The source-generated dispatchers see only
/// the method body without these envelopes.
/// </summary>
public static class OrpcEnvelope
{
    /// <summary>
    /// Wraps a raw request method body in an <see cref="OrpcThis" /> envelope
    /// with the supplied causality identifier. Used by the client-side
    /// <c>DcomCallChannel</c> when constructing a <c>RequestCoPdu.Stub</c>.
    /// </summary>
    public static byte[] BuildRequestStub(ReadOnlyMemory<byte> requestPayload, Guid causalityId)
    {
        byte[] stub = new byte[OrpcThis.NullExtensionsWireSize + requestPayload.Length];
        var writer = new NdrWriter(stub);
        new OrpcThis { CausalityId = causalityId }.Write(ref writer);
        requestPayload.Span.CopyTo(stub.AsSpan(writer.Position));
        return stub;
    }

    /// <summary>
    /// Strips the <see cref="OrpcThis" /> envelope from a request stub on the
    /// server side, returning the underlying method body bytes ready for
    /// dispatch.
    /// </summary>
    public static ReadOnlyMemory<byte> ExtractRequestBody(byte[] stub)
    {
        ArgumentNullException.ThrowIfNull(stub);
        if (stub.Length == 0)
        {
            throw new InvalidOperationException("DCOM request stub is missing the ORPC_THIS envelope.");
        }

        var reader = new NdrReader(stub);
        _ = OrpcThis.Read(ref reader);
        return stub.AsMemory(reader.Position);
    }

    /// <summary>
    /// Wraps a raw response method body in an <see cref="OrpcThat" /> envelope
    /// for the server's <c>ResponseCoPdu.Stub</c>.
    /// </summary>
    public static byte[] BuildResponseStub(ReadOnlyMemory<byte> responsePayload)
    {
        byte[] stub = new byte[OrpcThat.NullExtensionsWireSize + responsePayload.Length];
        var writer = new NdrWriter(stub);
        new OrpcThat().Write(ref writer);
        responsePayload.Span.CopyTo(stub.AsSpan(writer.Position));
        return stub;
    }

    /// <summary>
    /// Strips the <see cref="OrpcThat" /> envelope from a response stub on
    /// the client side, returning the underlying method body bytes for the
    /// generated client proxy.
    /// </summary>
    public static ReadOnlyMemory<byte> ExtractResponseBody(byte[] stub)
    {
        ArgumentNullException.ThrowIfNull(stub);
        if (stub.Length == 0)
        {
            throw new InvalidOperationException("DCOM response stub is missing the ORPC_THAT envelope.");
        }

        var reader = new NdrReader(stub);
        _ = OrpcThat.Read(ref reader);
        return stub.AsMemory(reader.Position);
    }
}

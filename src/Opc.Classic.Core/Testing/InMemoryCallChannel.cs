//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Testing;

/// <summary>
/// Managed <see cref="ICallChannel" /> test double that dispatches calls to a supplied handler.
/// </summary>
/// <remarks>
/// Use this channel to unit-test generated proxy call shims without opening a real DCOM connection.
/// The call log records only interface ID, opnum, and payload length; handlers can capture request
/// bytes themselves when a test needs to assert payload contents.
/// </remarks>
public sealed class InMemoryCallChannel : ICallChannel
{
    private readonly InMemoryCallHandler _handler;
    private readonly ConcurrentQueue<InMemoryCall> _callLog = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCallChannel" /> class.
    /// </summary>
    /// <param name="handler">The handler that simulates server responses.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler" /> is <see langword="null" />.</exception>
    public InMemoryCallChannel(InMemoryCallHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handler = handler;
    }

    /// <summary>
    /// Gets a thread-safe point-in-time snapshot of calls received by this channel.
    /// </summary>
    public IReadOnlyList<InMemoryCall> CallLog => _callLog.ToArray();

    /// <inheritdoc />
    public Task<NdrCallResult> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _callLog.Enqueue(new InMemoryCall(interfaceId, opnum, requestPayload.Length));
        return _handler(interfaceId, opnum, requestPayload, cancellationToken);
    }
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Opc.Classic.Testing;

/// <summary>
/// Builds <see cref="InMemoryCallChannel" /> instances that route calls by interface ID and opnum.
/// </summary>
public sealed class InMemoryCallChannelBuilder
{
    private const int ENotImpl = unchecked((int)0x80004001u);

    private readonly Dictionary<CallKey, InMemoryCallHandler> _handlers = new();
    private InMemoryCallHandler _fallback = static (_, _, _, cancellationToken) =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new NdrCallResult(ENotImpl, ReadOnlyMemory<byte>.Empty));
    };

    /// <summary>
    /// Registers a handler for a specific interface ID and opnum pair.
    /// </summary>
    /// <param name="interfaceId">The interface IID to match.</param>
    /// <param name="opnum">The DCE/RPC operation number to match.</param>
    /// <param name="handler">The handler to invoke for the matching call.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler" /> is <see langword="null" />.</exception>
    public InMemoryCallChannelBuilder Register(Guid interfaceId, int opnum, InMemoryCallHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers[new CallKey(interfaceId, opnum)] = handler;
        return this;
    }

    /// <summary>
    /// Sets the fallback handler used when no registered interface ID and opnum pair matches.
    /// </summary>
    /// <param name="handler">The fallback handler.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handler" /> is <see langword="null" />.</exception>
    public InMemoryCallChannelBuilder WithFallback(InMemoryCallHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _fallback = handler;
        return this;
    }

    /// <summary>
    /// Sets a constant fallback result used when no registered interface ID and opnum pair matches.
    /// </summary>
    /// <param name="fallbackResult">The fallback result to return.</param>
    /// <returns>This builder instance.</returns>
    public InMemoryCallChannelBuilder WithFallback(NdrCallResult fallbackResult)
    {
        _fallback = (_, _, _, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(fallbackResult);
        };
        return this;
    }

    /// <summary>
    /// Creates an <see cref="InMemoryCallChannel" /> using the current registrations and fallback.
    /// </summary>
    /// <returns>A configured in-memory call channel.</returns>
    public InMemoryCallChannel Build()
    {
        var handlers = new Dictionary<CallKey, InMemoryCallHandler>(_handlers);
        InMemoryCallHandler fallback = _fallback;

        return new InMemoryCallChannel((interfaceId, opnum, requestPayload, cancellationToken) =>
        {
            if (handlers.TryGetValue(new CallKey(interfaceId, opnum), out InMemoryCallHandler? handler))
            {
                return handler(interfaceId, opnum, requestPayload, cancellationToken);
            }

            return fallback(interfaceId, opnum, requestPayload, cancellationToken);
        });
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CallKey(Guid InterfaceId, int Opnum);
}

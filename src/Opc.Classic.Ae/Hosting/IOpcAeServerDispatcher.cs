//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>Dispatches NDR-encoded AE DCOM calls to a managed AE server implementation.</summary>
public interface IOpcAeServerDispatcher
{
    /// <summary>Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.</summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);

    /// <summary>Creates a dispatcher for an <c>IOPCEventAreaBrowser</c> instance.</summary>
    Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        CancellationToken cancellationToken = default) =>
        throw new OpcException(OpcResultId.NotImplemented);

    /// <summary>Creates a dispatcher for an <c>IOPCEventSubscriptionMgt</c> instance.</summary>
    Task<IOPCEventSubscriptionMgt> CreateEventSubscriptionAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientSubscription,
        Guid requestedInterfaceId,
        out int revisedBufferTime,
        out int revisedMaxSize,
        CancellationToken cancellationToken = default) =>
        throw NotImplemented(out revisedBufferTime, out revisedMaxSize);

    /// <summary>Removes a subscription created by <see cref="CreateEventSubscriptionAsync" />.</summary>
    Task RemoveSubscriptionAsync(IOPCEventSubscriptionMgt subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        cancellationToken.ThrowIfCancellationRequested();
        if (subscription is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync().AsTask();
        }
        if (subscription is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return Task.CompletedTask;
    }

    private static OpcException NotImplemented<T1, T2>(out T1 value1, out T2 value2)
    {
        value1 = default!;
        value2 = default!;
        return new OpcException(OpcResultId.NotImplemented);
    }
}

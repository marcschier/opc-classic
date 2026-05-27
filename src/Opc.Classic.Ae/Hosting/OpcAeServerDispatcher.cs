//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>AE dispatcher adapter that delegates to the source-generated IOPCEventServer dispatcher.</summary>
public sealed class OpcAeServerDispatcher : IOpcAeServerDispatcher
{
    private readonly IOpcAeServer _server;
    private readonly IOPCEventServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcAeServerDispatcher" /> class.</summary>
    public OpcAeServerDispatcher(IOpcAeServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverDispatcher = new IOPCEventServerServerDispatcher(server);
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCEventServer.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }

    /// <inheritdoc />
    public async Task<IOpcAeAreaBrowserDispatcher> CreateAreaBrowserAsync(
        Guid requestedInterfaceId,
        CancellationToken cancellationToken = default)
    {
        IOPCEventServer server = _server;
        await server.CreateAreaBrowserAsync(requestedInterfaceId, out IOPCEventAreaBrowser areaBrowser, cancellationToken).ConfigureAwait(false);
        if (areaBrowser is null)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }
        return areaBrowser is IOpcAeAreaBrowserDispatcher dispatcher
            ? dispatcher
            : new EventAreaBrowserAdapter(areaBrowser);
    }

    private sealed class EventAreaBrowserAdapter : IOpcAeAreaBrowserDispatcher
    {
        private readonly IOPCEventAreaBrowser _browser;

        public EventAreaBrowserAdapter(IOPCEventAreaBrowser browser) =>
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));

        public Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default) =>
            _browser.ChangeBrowsePositionAsync(browseDirection, position, cancellationToken);

        public async Task<string[]> BrowseAreasAsync(int browseFilterType, string filterCriteria, CancellationToken cancellationToken = default)
        {
            await _browser.BrowseOPCAreasAsync(browseFilterType, filterCriteria, out IEnumString enumString, cancellationToken).ConfigureAwait(false);
            if (enumString is IOpcAeStringEnumerator stringEnumerator)
            {
                return await stringEnumerator.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            }
            throw new OpcException(OpcResultId.NotImplemented);
        }

        public Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default) =>
            _browser.GetQualifiedAreaNameAsync(areaName, cancellationToken);

        public Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default) =>
            _browser.GetQualifiedSourceNameAsync(sourceName, cancellationToken);
    }
}

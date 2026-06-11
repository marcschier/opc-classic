//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Xml;

/// <summary>
/// Transport-agnostic OPC XML-DA 1.0 client. The default implementation
/// (<see cref="HttpXmlDaClient"/>) speaks SOAP-over-HTTP; alternate impls
/// can shim in-memory transports for testing or layered architectures.
/// </summary>
public interface IXmlDaClient
{
    /// <summary>
    /// Sends a <c>GetStatus</c> request and returns the server's status.
    /// </summary>
    Task<XmlDaServerStatus> GetStatusAsync(XmlDaRequestHeader header, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a <c>Read</c> request and returns the per-item values.
    /// </summary>
    Task<XmlDaReadResponse> ReadAsync(XmlDaReadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a <c>Write</c> request and returns per-item result codes.
    /// </summary>
    Task<XmlDaWriteResponse> WriteAsync(XmlDaWriteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a <c>Browse</c> request and returns the elements at the requested branch.
    /// </summary>
    Task<XmlDaBrowseResponse> BrowseAsync(XmlDaBrowseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a server-side subscription and returns the subscription handle.
    /// </summary>
    Task<XmlDaSubscribeResponse> SubscribeAsync(XmlDaSubscribeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls one or more server-side subscriptions for accumulated value changes.
    /// </summary>
    Task<XmlDaSubscriptionPolledRefreshResponse> SubscriptionPolledRefreshAsync(XmlDaSubscriptionPolledRefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a server-side subscription previously created via Subscribe.
    /// </summary>
    Task<XmlDaSubscriptionCancelResponse> SubscriptionCancelAsync(XmlDaSubscriptionCancelRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a <c>GetProperties</c> request and returns per-item property metadata.
    /// </summary>
    Task<XmlDaGetPropertiesResponse> GetPropertiesAsync(XmlDaGetPropertiesRequest request, CancellationToken cancellationToken = default);
}

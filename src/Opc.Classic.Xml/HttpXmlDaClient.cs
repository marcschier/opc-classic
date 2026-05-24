//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// HttpClient-based OPC XML-DA 1.0 client. Constructs SOAP envelopes via
// Opc.Classic.Xml.Serialization, POSTs them as text/xml with the
// per-operation SOAPAction header, then deserializes the response.
//
// Cross-platform by construction — relies only on System.Net.Http and
// System.Xml; no DCOM, no NTLM, no Win32 dependencies.
//

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Opc.Classic.Xml.Serialization;

namespace Opc.Classic.Xml;

/// <summary>
/// SOAP-over-HTTP implementation of <see cref="IXmlDaClient"/>.
/// </summary>
public sealed class HttpXmlDaClient : IXmlDaClient
{
    private readonly HttpClient _http;
    private readonly Uri _endpoint;

    /// <summary>
    /// Creates a new client targeting the supplied XML-DA endpoint.
    /// The <see cref="HttpClient"/> lifetime is the caller's
    /// responsibility — typical usage is a process-wide singleton.
    /// </summary>
    public HttpXmlDaClient(HttpClient httpClient, Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(endpoint);
        _http = httpClient;
        _endpoint = endpoint;
    }

    /// <inheritdoc />
    public async Task<XmlDaServerStatus> GetStatusAsync(
        XmlDaRequestHeader header,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(header);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 256))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                GetStatusSerializer.WriteRequest(w, header);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionGetStatus,
            static r => GetStatusSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaReadResponse> ReadAsync(
        XmlDaReadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 512))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                ReadSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionRead,
            static r => ReadSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaWriteResponse> WriteAsync(
        XmlDaWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 512))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                WriteSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionWrite,
            static r => WriteSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaBrowseResponse> BrowseAsync(
        XmlDaBrowseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 256))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                BrowseSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionBrowse,
            static r => BrowseSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaSubscribeResponse> SubscribeAsync(
        XmlDaSubscribeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 512))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                SubscribeSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionSubscribe,
            static r => SubscribeSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaSubscriptionPolledRefreshResponse> SubscriptionPolledRefreshAsync(
        XmlDaSubscriptionPolledRefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 512))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                SubscriptionPolledRefreshSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionSubscriptionPolledRefresh,
            static r => SubscriptionPolledRefreshSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaSubscriptionCancelResponse> SubscriptionCancelAsync(
        XmlDaSubscriptionCancelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 256))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                SubscriptionCancelSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionSubscriptionCancel,
            static r => SubscriptionCancelSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<XmlDaGetPropertiesResponse> GetPropertiesAsync(
        XmlDaGetPropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] requestBytes;
        using (var ms = new MemoryStream(capacity: 512))
        {
            using (var w = new SoapEnvelopeWriter(ms))
            {
                GetPropertiesSerializer.WriteRequest(w, request);
            }
            requestBytes = ms.ToArray();
        }

        return await PostAsync(requestBytes,
            XmlDaConstants.SoapActionGetProperties,
            static r => GetPropertiesSerializer.ReadResponse(r),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> PostAsync<T>(
        byte[] requestBytes,
        string soapAction,
        Func<SoapEnvelopeReader, T> deserialize,
        CancellationToken cancellationToken)
    {
        using var content = new ByteArrayContent(requestBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
        content.Headers.Add("SOAPAction", "\"" + soapAction + "\"");

        using var response = await _http.PostAsync(_endpoint, content, cancellationToken).ConfigureAwait(false);

        Stream responseStream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (responseStream.ConfigureAwait(false))
        {
            using var reader = new SoapEnvelopeReader(responseStream);
            try
            {
                T result = deserialize(reader);
                response.EnsureSuccessStatusCode();
                return result;
            }
            catch (XmlDaSoapFaultException)
            {
                throw;
            }
            catch (InvalidDataException) when (!response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                throw;
            }
            catch (XmlException) when (!response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                throw;
            }
        }
    }
}

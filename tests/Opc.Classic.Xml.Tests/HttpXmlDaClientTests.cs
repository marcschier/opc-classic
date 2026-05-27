//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// End-to-end test for HttpXmlDaClient using an in-process
// HttpMessageHandler. Exercises the full pipeline:
//   serialize -> POST -> response body -> deserialize.
//

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Xml;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests;

public sealed class HttpXmlDaClientTests
{
    /// <summary>
    /// Captures the outbound HTTP request and replays a canned response.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }
        public string ResponseBody { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastRequest = request;
            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            var response = new HttpResponseMessage(StatusCode);
            response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(ResponseBody));
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
            return response;
        }
    }

    private static HttpXmlDaClient BuildClient(CapturingHandler handler)
    {
        var http = new HttpClient(handler, disposeHandler: false);
        return new HttpXmlDaClient(http, new Uri("http://example.test/xmlda"));
    }

    [Test]
    public async Task GetStatusAsync_PostsToEndpoint()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                      <GetStatusResult ServerState="running" />
                      <Status StartTime="2026-05-22T03:00:00Z" ProductVersion="1.0" VendorInfo="Acme" />
                    </GetStatusResponse>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        var status = await client.GetStatusAsync(new XmlDaRequestHeader("en-US", "h1"));

        await Assert.That(status.ServerState).IsEqualTo(XmlDaServerState.Running);
        await Assert.That(status.ProductVersion).IsEqualTo("1.0");
        await Assert.That(handler.LastRequest!.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(handler.LastRequest.RequestUri!.ToString())
            .IsEqualTo("http://example.test/xmlda");
    }

    [Test]
    public async Task GetStatusAsync_SendsSoapActionHeader()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                      <GetStatusResult ServerState="running" />
                      <Status />
                    </GetStatusResponse>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        await client.GetStatusAsync(new XmlDaRequestHeader(null, null));

        var soapActionValues = handler.LastRequest!.Content!.Headers
            .GetValues("SOAPAction")
            .ToArray();
        await Assert.That(soapActionValues.Length).IsEqualTo(1);
        await Assert.That(soapActionValues[0])
            .IsEqualTo("\"http://opcfoundation.org/webservices/XMLDA/1.0/GetStatus\"");
    }

    [Test]
    public async Task GetStatusAsync_SendsTextXmlContentType()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                      <GetStatusResult ServerState="running" />
                      <Status />
                    </GetStatusResponse>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        await client.GetStatusAsync(new XmlDaRequestHeader(null, null));

        var contentType = handler.LastRequest!.Content!.Headers.ContentType!;
        await Assert.That(contentType.MediaType).IsEqualTo("text/xml");
        await Assert.That(contentType.CharSet).IsEqualTo("utf-8");
    }

    [Test]
    public async Task GetStatusAsync_RequestBodyCarriesGetStatusElement()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <GetStatusResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/">
                      <GetStatusResult ServerState="running" />
                      <Status />
                    </GetStatusResponse>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        await client.GetStatusAsync(new XmlDaRequestHeader("de-DE", "req-99"));

        await Assert.That(handler.LastRequestBody).IsNotNull();
        await Assert.That(handler.LastRequestBody!).Contains("<GetStatus");
        await Assert.That(handler.LastRequestBody!).Contains("LocaleID=\"de-DE\"");
        await Assert.That(handler.LastRequestBody!).Contains("ClientRequestHandle=\"req-99\"");
    }

    [Test]
    public async Task GetStatusAsync_ThrowsOnHttpError()
    {
        var handler = new CapturingHandler
        {
            StatusCode = HttpStatusCode.InternalServerError,
            ResponseBody = "internal server error body",
        };
        var client = BuildClient(handler);

        bool threw = false;
        try
        {
            await client.GetStatusAsync(new XmlDaRequestHeader(null, null));
        }
        catch (HttpRequestException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task GetStatusAsync_MapsSoapFaultOnHttpError_ToTypedEnum()
    {
        var handler = new CapturingHandler
        {
            StatusCode = HttpStatusCode.InternalServerError,
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                               xmlns:xmlDa="http://opcfoundation.org/webservices/XMLDA/1.0/">
                  <soap:Body>
                    <soap:Fault>
                      <faultcode>xmlDa:E_SERVERSTATE</faultcode>
                      <faultstring>Server suspended</faultstring>
                    </soap:Fault>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        XmlDaSoapFaultException? faultException = null;
        try
        {
            await client.GetStatusAsync(new XmlDaRequestHeader(null, null));
        }
        catch (XmlDaSoapFaultException ex)
        {
            faultException = ex;
        }

        await Assert.That(faultException).IsNotNull();
        await Assert.That(faultException!.ErrorCode).IsEqualTo(XmlDaErrorCode.ServerState);
    }

    [Test]
    public async Task ReadAsync_TreatsClampResultIdAsSuccess()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = """
                <?xml version="1.0"?>
                <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                  <soap:Body>
                    <ReadResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/"
                                  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                  xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                      <ReadResult ServerState="running" />
                      <RItemList>
                        <Items ItemName="ClampedTag" ClientItemHandle="h-clamp" ResultID="S_CLAMP">
                          <Value xsi:type="xsd:double">100</Value>
                          <Quality QualityField="good" />
                        </Items>
                      </RItemList>
                    </ReadResponse>
                  </soap:Body>
                </soap:Envelope>
                """,
        };
        var client = BuildClient(handler);

        var response = await client.ReadAsync(new XmlDaReadRequest(
            new XmlDaRequestHeader(null, null),
            new[] { new XmlDaReadItem("ClampedTag", "h-clamp") }));

        await Assert.That(response.Items.Count).IsEqualTo(1);
        var item = response.Items[0];
        await Assert.That(item.ResultId).IsEqualTo("S_CLAMP");
        await Assert.That(item.ResultCode).IsEqualTo(XmlDaErrorCode.Clamp);
        await Assert.That(item.ResultCode.IsSuccess()).IsTrue();
        await Assert.That(item.Value!.AsDouble()).IsEqualTo(100d);
    }

    [Test]
    public async Task GetStatusAsync_HonorsCancellation()
    {
        var handler = new CapturingHandler
        {
            ResponseBody = "ignored",
        };
        var client = BuildClient(handler);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        bool threw = false;
        try
        {
            await client.GetStatusAsync(new XmlDaRequestHeader(null, null), cts.Token);
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }
}

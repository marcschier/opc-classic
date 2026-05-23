# XML-DA modernization status

Phase 9F is complete for `src\Opc.Classic.Xml`. The XML-DA stack no longer contains a generated `OpcXml.Da10.Proxy.cs` or any dependency on `System.Web.Services`, `WebRequest`, `WebClient`, WCF SOAP proxies, or `System.ServiceModel.Http`. The project file references only `Opc.Classic.Core`; strict NativeAOT/trimming settings are inherited from `src\Directory.Build.props` without local relaxation.

The active transport is `HttpXmlDaClient`, an `IXmlDaClient` implementation that accepts caller-owned `HttpClient` and endpoint instances. Each request is encoded into a hand-written SOAP 1.1 envelope, sent as `text/xml; charset=utf-8`, and tagged with the per-operation `SOAPAction` header. Responses are streamed back through `SoapEnvelopeReader` and decoded by operation-specific serializers. This keeps XML-DA decoupled from the DCOM stack and avoids reflection-heavy XML serializer or legacy SOAP proxy generation.

The eight XML-DA operations are implemented and routed through the same HttpClient plumbing:

| Operation | Serializer |
|---|---|
| `GetStatus` | `GetStatusSerializer` |
| `Read` | `ReadSerializer` |
| `Write` | `WriteSerializer` |
| `Browse` | `BrowseSerializer` |
| `GetProperties` | `GetPropertiesSerializer` |
| `Subscribe` | `SubscribeSerializer` |
| `SubscriptionPolledRefresh` | `SubscriptionPolledRefreshSerializer` |
| `SubscriptionCancel` | `SubscriptionCancelSerializer` |

Envelope handling is AOT-safe and based on `System.Xml.XmlWriter` and `System.Xml.XmlReader`. `SoapEnvelopeReader` disables DTD processing and external XML resolution for XXE resistance. Serializer and HTTP behavior is covered by `tests\Opc.Classic.Xml.Tests`, including `HttpXmlDaClientTests` and per-operation serializer tests.

Audit result: Outcome A. No Phase 9F modernization work remains in `src\Opc.Classic.Xml`; the permanent deliverable from this audit is this status note.

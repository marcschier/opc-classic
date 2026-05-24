# XML-DA spec coverage

`Opc.Classic.Xml` provides an AOT- and trim-compatible OPC XML-DA 1.0 client surface. The package contains request/response DTOs, SOAP 1.1 serializers, response readers, and the `HttpXmlDaClient` transport that uses a caller-owned `HttpClient`.

## Supported transport model

- SOAP 1.1 envelopes over HTTP with `text/xml; charset=utf-8` content.
- Per-operation `SOAPAction` headers from the XML-DA namespace.
- Caller-controlled endpoint URI, authentication, proxy, TLS, timeout, and retry policy through `HttpClient`.
- Streaming XML read/write through `System.Xml.XmlReader` and `System.Xml.XmlWriter`.
- DTD processing and external XML resolution disabled in the SOAP reader.

## Supported XML-DA operations

| Operation | Client API | Coverage |
|---|---|---|
| `GetStatus` | `GetStatusAsync` | Server state, vendor info, product/version info, supported locale IDs, status info, start time, current time, and last update time. |
| `Read` | `ReadAsync` | Item names, client item handles, max age, per-item value, quality, timestamp, and result ID. |
| `Write` | `WriteAsync` | Item values, client item handles, per-item result IDs, and optional error text. |
| `Browse` | `BrowseAsync` | Root/branch browsing, branch/item/all filters, max elements, continuation point, element name filter, item path, item name, leaf flag, and child flag. |
| `GetProperties` | `GetPropertiesAsync` | Item properties, selected or all property names, optional property values, descriptions, and per-item/per-property result IDs. |
| `Subscribe` | `SubscribeAsync` | Server subscription handle, requested and revised sampling rate, ping rate, buffering flag, deadband, initial values, and per-item results. |
| `SubscriptionPolledRefresh` | `SubscriptionPolledRefreshAsync` | One or more subscription handles, hold time, wait time, changed/all item mode, data-buffer overflow flag, invalid handles, and per-subscription item lists. |
| `SubscriptionCancel` | `SubscriptionCancelAsync` | Cancellation by server subscription handle and echoed client request handle. |

## Value and quality coverage

`XmlDaValue` supports the scalar XML Schema types used by common XML-DA servers: `string`, signed and unsigned integer widths, `float`, `double`, `boolean`, and `dateTime`. Unknown value types preserve raw text for diagnostics. `OpcQuality` maps the DA packed quality bits exposed by XML-DA item values.

## Not supported

- Hosting an XML-DA server endpoint.
- SOAP 1.2 bindings.
- XML-DA array values, BSTR variants, and vendor-specific value carriers beyond raw-text preservation.
- Generated SOAP proxy types or reflection-based XML serialization.
- Built-in WS-Security policy; use the supplied `HttpClient` for authentication and transport security.

## Verification

`tests\Opc.Classic.Xml.Tests` covers the HTTP client path and per-operation serializers. The project participates in the repository build and test gates, which currently complete with 0 build warnings and a green test suite.

## Roadmap

For `1.0.0-rc.1`:

- complete XML-DA interop runs against representative third-party servers;
- expand error-code and quality-code coverage examples;
- add a compact client cookbook for read/write/subscribe flows.

For `2.0.0`:

- evaluate XML-DA server hosting;
- add array and vendor-specific value carriers where interop demand justifies them;
- consider optional SOAP security helpers layered on top of `HttpClient`.

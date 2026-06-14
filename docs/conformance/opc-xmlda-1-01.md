# OPC XML-DA 1.01 conformance review

**Spec:** `opc-classic-docs/OPC-XMLDA-1.01.md` (OPC XML-DA Specification, Version 1.01, October 2003).

**Scope:** SOAP 1.1 / HTTP client transport, the 8 XML-DA service operations (`GetStatus`, `Read`, `Write`, `Subscribe`, `SubscriptionPolledRefresh`, `SubscriptionCancel`, `Browse`, `GetProperties`), request / response DTOs, XML-DA scalar and array value carriers, OPC quality-byte mapping, SOAP faults, per-item `ResultID` values, and polled subscription client flow. XML-DA server hosting is intentionally out of scope for the current package and is tracked as a waiver, not as a hard client conformance gap.

**Implementing assemblies:** `Opc.Classic.Xml`, `Opc.Classic.Core`.

**Status overview:**

| Surface | Spec § | Implementation | Tests | Outcome |
|---|---|---|---|---|
| SOAP 1.1 HTTP client transport | §2.1, §4 | ✅ `HttpXmlDaClient` + hand-written SOAP envelope reader/writer | ✅ | conformant |
| XML-DA service operations (8/8) | §3.2 - §3.9 | ✅ `IXmlDaClient` + `HttpXmlDaClient` methods | ✅ | conformant |
| Request / response XML serializers | §3.1 - §3.9 | ✅ per-operation serializers under `src/Opc.Classic.Xml/Serialization/` | ✅ | conformant |
| Scalar values | §2.7.1 | ✅ `XmlDaValueType` + `XmlDaValue` | ✅ | conformant |
| Array and binary values | §2.7.3 | ✅ `ArrayOf*` + `base64Binary` carriers | ✅ | conformant |
| SOAP faults + `ResultID` mapping | §2.6, §3.1.9 | ✅ `XmlDaSoapFaultException`, `XmlDaErrorCode`, `XmlDaErrorCodes` | ✅ | conformant |
| Quality fields | §3.1.5 / Table 3.1.8-1 | ✅ `XmlDaQualityCompat` bridges XML-DA low byte to `OpcQuality` | ✅ | conformant |
| Polled subscriptions | §2.5, §3.5 - §3.7 | ✅ subscribe / poll / cancel client DTOs and serializers | ✅ | conformant |
| XML-DA server hosting | §2.9, §3, §4 | ❌ no inbound SOAP endpoint implementation | n/a | **WAIVED** — see §3.1 |
| SOAP 1.2 binding | not required by §2.1 | ❌ no SOAP 1.2 alternate envelope/content type | n/a | **WAIVED** — see §3.1 |

---

## 1 Surface-by-surface coverage matrix

### 1.1 XML-DA SOAP operations (spec §3.2 - §3.9)

8 SOAP operations. The user-facing client contract is `src/Opc.Classic.Xml/IXmlDaClient.cs`; the default HTTP/SOAP implementation is `src/Opc.Classic.Xml/HttpXmlDaClient.cs`.

| Operation | Spec § | Source client method | Tests |
|---|---|---|---|
| `GetStatus` | §3.2.1 / §3.2.2 | `IXmlDaClient.GetStatusAsync` (`IXmlDaClient.cs` lines 15-18); `HttpXmlDaClient.GetStatusAsync` (`HttpXmlDaClient.cs` lines 41-60) | `tests/Opc.Classic.Xml.Tests/GetStatusSerializerTests.cs`, `tests/Opc.Classic.Xml.Tests/HttpXmlDaClientTests.cs` |
| `Read` | §3.3.1 / §3.3.2 | `IXmlDaClient.ReadAsync` (`IXmlDaClient.cs` lines 20-23); `HttpXmlDaClient.ReadAsync` (`HttpXmlDaClient.cs` lines 64-83) | `tests/Opc.Classic.Xml.Tests/ReadSerializerTests.cs`, `tests/Opc.Classic.Xml.Tests/HttpXmlDaClientTests.cs` |
| `Write` | §3.4.1 / §3.4.2 | `IXmlDaClient.WriteAsync` (`IXmlDaClient.cs` lines 25-28); `HttpXmlDaClient.WriteAsync` (`HttpXmlDaClient.cs` lines 87-106) | `tests/Opc.Classic.Xml.Tests/WriteSerializerTests.cs` |
| `Subscribe` | §3.5.1 / §3.5.2 | `IXmlDaClient.SubscribeAsync` (`IXmlDaClient.cs` lines 35-38); `HttpXmlDaClient.SubscribeAsync` (`HttpXmlDaClient.cs` lines 133-152) | `tests/Opc.Classic.Xml.Tests/SubscribeSerializerTests.cs` |
| `SubscriptionPolledRefresh` | §3.6.1 / §3.6.2 | `IXmlDaClient.SubscriptionPolledRefreshAsync` (`IXmlDaClient.cs` lines 40-43); `HttpXmlDaClient.SubscriptionPolledRefreshAsync` (`HttpXmlDaClient.cs` lines 156-175) | `tests/Opc.Classic.Xml.Tests/SubscriptionPolledRefreshSerializerTests.cs` |
| `SubscriptionCancel` | §3.7.1 / §3.7.2 | `IXmlDaClient.SubscriptionCancelAsync` (`IXmlDaClient.cs` lines 45-48); `HttpXmlDaClient.SubscriptionCancelAsync` (`HttpXmlDaClient.cs` lines 179-198) | `tests/Opc.Classic.Xml.Tests/SubscriptionCancelSerializerTests.cs` |
| `Browse` | §3.8.1 / §3.8.2 | `IXmlDaClient.BrowseAsync` (`IXmlDaClient.cs` lines 30-33); `HttpXmlDaClient.BrowseAsync` (`HttpXmlDaClient.cs` lines 110-129) | `tests/Opc.Classic.Xml.Tests/BrowseSerializerTests.cs` |
| `GetProperties` | §3.9.1 / §3.9.2 | `IXmlDaClient.GetPropertiesAsync` (`IXmlDaClient.cs` lines 50-53); `HttpXmlDaClient.GetPropertiesAsync` (`HttpXmlDaClient.cs` lines 202-221) | `tests/Opc.Classic.Xml.Tests/GetPropertiesSerializerTests.cs` |

Operation notes:

- `GetStatus` covers locale and client request handle attributes plus server state, product version, vendor info, supported locale IDs, supported interface versions, start time, and status text (serializer tests lines 30-240).
- `Read` covers item names, client item handles, max age, per-item values, quality, timestamp, per-item result IDs, and wrong-operation rejection (serializer tests lines 30-277).
- `Write` covers `ReturnValuesOnReply`, item values with `xsi:type`, client item handles, per-item result IDs, empty response lists, and wrong-operation rejection (serializer tests lines 30-146).
- `Subscribe` covers per-item and list sampling rates, ping rate, return-values-on-reply, buffering, server subscription handle, revised sampling rates, initial values, and wrong-operation rejection (serializer tests lines 30-135).
- `SubscriptionPolledRefresh` covers multiple subscription handles, hold time, wait time, `ReturnAllItems`, per-subscription item lists, invalid handles, data-buffer overflow, and wrong-operation rejection (serializer tests lines 30-132).
- `SubscriptionCancel` covers required `ServerSubHandle`, optional echoed client request handle, missing-handle validation, and wrong-operation rejection (serializer tests lines 30-99).
- `Browse` covers all / branch / item filters, max elements, continuation point, returned element flags, empty elements, paging, and wrong-operation rejection (serializer tests lines 30-149).
- `GetProperties` covers item IDs, property names, request flags, property values, multiple property lists, per-item / per-property result IDs, omitted values, and wrong-operation rejection (serializer tests lines 30-169).

### 1.2 SOAP 1.1 HTTP transport (spec §2.1 and §4)

| Requirement | Source | Tests |
|---|---|---|
| SOAP 1.1 envelope namespace `http://schemas.xmlsoap.org/soap/envelope/` | `src/Opc.Classic.Xml/XmlDaConstants.cs` lines 21-24; `Serialization/SoapEnvelopeWriter.cs` lines 71-80; `Serialization/SoapEnvelopeReader.cs` lines 61-101 | `tests/Opc.Classic.Xml.Tests/SoapEnvelopeTests.cs`, `tests/Opc.Classic.Xml.Tests/XmlDaConstantsTests.cs` |
| XML-DA namespace `http://opcfoundation.org/webservices/XMLDA/1.0/` | `XmlDaConstants.cs` lines 16-19; `SoapEnvelopeWriter.cs` lines 107-117 | `XmlDaConstantsTests.cs`, per-operation serializer tests |
| HTTP POST body uses `text/xml; charset=utf-8` | `HttpXmlDaClient.cs` lines 224-234 | `HttpXmlDaClientTests.cs` lines 111-138 |
| Per-operation `SOAPAction` header | `XmlDaConstants.cs` lines 41-81; `HttpXmlDaClient.cs` lines 224-234 | `HttpXmlDaClientTests.cs` lines 82-111; `XmlDaConstantsTests.cs` |
| Safe XML parsing | `SoapEnvelopeReader.cs` lines 41-50 disables DTD processing and external resolution | `SoapEnvelopeTests.cs`, `tests/Opc.Classic.Xml.Tests/Fuzz/SoapEnvelopeReaderFuzzTests.cs` |
| Caller-owned endpoint / auth / timeout policy | `HttpXmlDaClient.cs` lines 24-38 | `HttpXmlDaClientTests.cs` lines 54-260 |

### 1.3 Data values, arrays, quality, and errors

| Surface | Spec § | Source | Tests |
|---|---|---|---|
| Simple scalar values | §2.7.1 | `src/Opc.Classic.Xml/XmlDaValueType.cs` lines 21-109; `XmlDaValue.cs` lines 25-148 | `tests/Opc.Classic.Xml.Tests/XmlDaValueSerializerTests.cs`, `ReadSerializerTests.cs`, `WriteSerializerTests.cs` |
| XML-DA arrays | §2.7.3 | `XmlDaValueType.cs` lines 111-159; `XmlDaValue.cs` lines 150-239 | `XmlDaValueSerializerTests.cs` |
| `base64Binary` | §2.7.1 | `XmlDaValueType.cs` lines 161-164; `XmlDaValue.cs` lines 241-248 | `XmlDaValueSerializerTests.cs` |
| Unknown / vendor value carriers | §2.7.1 notes | `XmlDaValueType.Unknown` (`XmlDaValueType.cs` lines 16-19); `XmlDaValue.Unknown` (`XmlDaValue.cs` lines 19-23) | `ReadSerializerTests.cs` lines 171-199 |
| Quality byte bridge | §3.1.5 / Table 3.1.8-1 | `src/Opc.Classic.Xml/XmlDaQualityCompat.cs` lines 15-29 | `tests/Opc.Classic.Xml.Tests/XmlDaQualityCompatTests.cs`, `ReadSerializerTests.cs` lines 198-224 |
| SOAP fault mapping | §2.6 | `Serialization/SoapEnvelopeReader.cs` lines 94-148; `XmlDaSoapFaultException.cs` | `SoapEnvelopeTests.cs`, `HttpXmlDaClientTests.cs` lines 186-221 |
| Per-item `ResultID` mapping | §2.6, §3.1.9 | `src/Opc.Classic.Xml/XmlDaErrorCode.cs`; `XmlDaErrorCodes.cs` lines 11-113 | `tests/Opc.Classic.Xml.Tests/XmlDaErrorCodesTests.cs`, `XmlDaErrorCodesMappingTests.cs` |

---

## 2 Normative-clause checklist

The Phase 0 inventory CSV `opc-xmlda-1-01-clauses.csv` contains 1 `SHALL` entry, but it is a legal-disclaimer false positive from the specification front matter, not a protocol requirement. The protocol checklist below is therefore derived from the sampled XML-DA §2 / §3 / §4 sections and validated against source/test citations.

| § | Clause | Status | Evidence |
|---|---|---|---|
| §2.1 | XML-DA messages are SOAP 1.1 body payloads. | ✅ honored | `SoapEnvelopeWriter.cs` emits SOAP 1.1 envelope/body wrappers; `SoapEnvelopeReader.cs` consumes SOAP 1.1 responses. |
| §4 | Clients and servers use HTTP for compliance / interoperability transport. | ✅ honored | `HttpXmlDaClient.PostAsync` posts to the configured endpoint with `HttpClient` (`HttpXmlDaClient.cs` lines 224-234). |
| §3.2 | `GetStatus` carries optional `LocaleID` / `ClientRequestHandle` and returns server status metadata. | ✅ honored | `IXmlDaClient.GetStatusAsync`, `GetStatusSerializerTests.cs`. |
| §3.3 | `Read` submits one or more items and receives ordered value / quality / timestamp or error results. | ✅ honored | `XmlDaReadDtos.cs` lines 10-59; `ReadSerializerTests.cs`. |
| §3.4 | `Write` submits one or more item values and may request returned accepted values. | ✅ honored | `XmlDaWriteDtos.cs` lines 10-57; `WriteSerializerTests.cs`. |
| §2.5, §3.5 - §3.7 | Subscription flow is polled-pull: subscribe, poll one or more handles, then cancel. | ✅ honored | `XmlDaSubscribeDtos.cs` lines 10-55; `XmlDaSubscriptionPolledRefreshDtos.cs` lines 10-47; `SubscriptionCancelSerializerTests.cs`. |
| §3.8 | `Browse` supports single-level browsing with filters, continuation points, property names, and returned element flags. | ✅ honored | `BrowseSerializerTests.cs` lines 30-149. |
| §3.9 | `GetProperties` supports item IDs, selected or all property names, property values, and verbose error-text requests. | ✅ honored | `GetPropertiesSerializerTests.cs` lines 30-169. |
| §2.6 | Operation-wide failures are SOAP faults; item-level outcomes use `ResultID` QNames and optional `Errors` text. | ✅ honored | `SoapEnvelopeReader.cs` lines 94-148; `XmlDaErrorCodes.cs` lines 11-113; error-code tests. |
| §2.7.1 | XML-DA item values use XML Schema scalar types including strings, signed/unsigned integer widths, floats, decimal, date/time, QName, and `base64Binary`. | ✅ honored | `XmlDaValueType.cs` lines 21-109 and 161-164; `XmlDaValue.cs`; value serializer tests. |
| §2.7.3 | XML-DA defines array carriers for common simple types. | ✅ honored | `XmlDaValueType.cs` lines 111-159; `XmlDaValue.cs` lines 150-239; `XmlDaValueSerializerTests.cs`. |
| §2.9 | OPC Foundation compliance tooling is server-oriented; client compliance is established through operation and wire-format behavior. | ✅ documented | This review classifies `Opc.Classic.Xml` as a client implementation and waives server hosting in §3.1. |

---

## 3 Gap register

### 3.1 Soft gaps (waivers)

#### 3.1.1 XML-DA server hosting is not implemented

The specification defines request and response messages for XML-DA services and discusses server compliance (§2.9). The current `Opc.Classic.Xml` package is a managed SOAP-over-HTTP **client**: `IXmlDaClient` exposes outbound operation methods, and `HttpXmlDaClient` posts serialized SOAP requests to a caller-provided endpoint. There is no ASP.NET Core middleware, inbound SOAP dispatcher, operation handler set, or XML-DA-to-DA bridge under `src/Opc.Classic.Xml/`.

Status: **WAIVED**. This is a deliberate client-only scope decision for the current package, and the existing aggregate conformance doc records XML-DA server hosting as deferred-by-design.

#### 3.1.2 SOAP 1.2 binding is not implemented

The XML-DA 1.01 text explicitly models the protocol on SOAP 1.1 (§2.1) and uses the SOAP 1.1 envelope namespace. `Opc.Classic.Xml` implements SOAP 1.1 only (`XmlDaConstants.SoapEnvelopeNamespace`, `SoapEnvelopeWriter`, `SoapEnvelopeReader`) and sends `text/xml; charset=utf-8` with a separate `SOAPAction` header. There is no alternate SOAP 1.2 envelope namespace or `application/soap+xml` content-type path.

Status: **WAIVED**. SOAP 1.2 is not required by OPC XML-DA 1.01 and remains an optional future interoperability extension.

#### 3.1.3 Third-party XML-DA server interop runs are not present

Unit tests cover all operation serializers, SOAP envelope parsing, HTTP request construction, quality mapping, value carriers, and result-code mapping. No checked-in test currently exercises a live third-party XML-DA server such as Kepware, Matrikon, or Softing.

Status: **WAIVED** as validation hardening, not a protocol-surface gap. Add operator-gated interop tests when a redistributable XML-DA endpoint is available.

### 3.2 Hard gaps

None at present. No rows were appended to `files/conformance/gap-master.csv` for OPC-XMLDA-1.01 because the remaining XML-DA items are waived scope decisions or validation hardening, not hard implementation gaps in the client surface.

---

## 4 Cross-references

- Existing aggregate doc: [`docs/CONFORMANCE.md` § OPC XML-DA 1.01](../CONFORMANCE.md#opc-xml-da-101)
- XML-DA client cookbook: [`docs/cookbook/06-xmlda-client-flows.md`](../cookbook/06-xmlda-client-flows.md)
- XML-DA package source: `src/Opc.Classic.Xml/`
- XML-DA test project: `tests/Opc.Classic.Xml.Tests/`
- Root solution entries: `Opc.Classic.slnx` includes `src/Opc.Classic.Xml/Opc.Classic.Xml.csproj` and `tests/Opc.Classic.Xml.Tests/Opc.Classic.Xml.Tests.csproj`.

---

## 5 Citation footer

Source: vendored `opc-classic-docs/OPC-XMLDA-1.01.md` (OPC XML-DA Specification Version 1.01, October 2003). Key sampled sections: §2.1 SOAP, §2.5 subscription architecture, §2.6 faults and result codes, §2.7 data types, §2.9 compliance, §3.2 - §3.9 service schemas, and §4 transports.

Phase 0 inventory:

- `files/conformance/inventory/opc-xmlda-1-01-headings.csv` (68 entries)
- `files/conformance/inventory/opc-xmlda-1-01-clauses.csv` (1 false-positive front-matter `SHALL` entry)
- `files/conformance/inventory/opc-xmlda-1-01-interfaces.csv` (incidental COM references from property-description text; XML-DA itself is a SOAP API, not a COM/DCOM interface)

Verified implementation and tests:

- `src/Opc.Classic.Xml/HttpXmlDaClient.cs`
- `src/Opc.Classic.Xml/IXmlDaClient.cs`
- `src/Opc.Classic.Xml/XmlDa*.cs`
- `src/Opc.Classic.Xml/Serialization/*.cs`
- `tests/Opc.Classic.Xml.Tests/*.cs`

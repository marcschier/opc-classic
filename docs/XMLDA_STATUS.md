# XML-DA spec coverage

`Opc.Classic.Xml` provides an AOT- and trim-compatible OPC XML-DA 1.0 client surface. The package contains request/response DTOs, SOAP 1.1 serializers, response readers, and the `HttpXmlDaClient` transport that uses a caller-owned `HttpClient`. It does not expose XML-DA server hosting APIs.

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

`XmlDaValue` supports the XML Schema scalar types used by common XML-DA servers: `string`, signed and unsigned integer widths, `float`, `double`, `decimal`, `boolean`, `dateTime`, `time`, `date`, `duration`, `QName`, and `base64Binary`. It also supports XML-DA array carriers for byte, short, int, long, float, double, string, boolean, and dateTime values. Unknown value types preserve raw text for diagnostics. `OpcQuality` maps the DA packed quality bits exposed by XML-DA item values.

## Error and quality codes

`XmlDaErrorCode` and `XmlDaErrorCodes` map XML-DA SOAP fault codes and per-item `ResultID` values to typed results. Missing per-item `ResultID` values parse as `XmlDaErrorCode.Ok`; unknown, vendor-specific, or malformed strings parse as `XmlDaErrorCode.Unknown`. `XmlDaErrorCode.IsSuccess()` returns `true` only for the four XML-DA success results.

| Enum name | OPC result ID string | Spec section | Success vs fault | Description |
|---|---|---|---|---|
| `Unknown` | Vendor-specific, malformed, or empty `Parse(...)` input | Implementation sentinel | Unknown (`IsSuccess()` false) | Unknown result code; `ToResultId` returns an empty string. |
| `Ok` | `S_OK` | 3.1.9 | Success (`IsSuccess()` true) | Operation succeeded; also used for omitted per-item `ResultID`. |
| `Clamp` | `S_CLAMP` | 3.1.9 | Success (`IsSuccess()` true) | Value was clamped to a valid range. |
| `DataQueueOverflow` | `S_DATAQUEUEOVERFLOW` | 3.1.9 | Success (`IsSuccess()` true) | Subscription data queue overflowed. |
| `UnsupportedRate` | `S_UNSUPPORTEDRATE` | 3.1.9 | Success (`IsSuccess()` true) | Requested subscription sampling rate was not supported; use the revised rate. |
| `AccessDenied` | `E_ACCESS_DENIED` | 3.1.9 | Fault (`IsSuccess()` false) | Caller lacks permission for the operation or item. |
| `Busy` | `E_BUSY` | 3.1.9 | Fault (`IsSuccess()` false) | Server is busy. |
| `Fail` | `E_FAIL` | 3.1.9 | Fault (`IsSuccess()` false) | Unspecified server failure. |
| `InvalidContinuationPoint` | `E_INVALIDCONTINUATIONPOINT` | 3.1.9 | Fault (`IsSuccess()` false) | Browse continuation point is invalid. |
| `InvalidFilter` | `E_INVALIDFILTER` | 3.1.9 | Fault (`IsSuccess()` false) | Browse filter is invalid. |
| `InvalidHoldTime` | `E_INVALIDHOLDTIME` | 3.1.9 | Fault (`IsSuccess()` false) | Subscription hold time is invalid. |
| `InvalidItemId` | `E_INVALIDITEMID` | 3.1.9 | Fault (`IsSuccess()` false) | Item identifier is syntactically invalid. |
| `InvalidItemName` | `E_INVALIDITEMNAME` | 3.1.9 | Fault (`IsSuccess()` false) | Item name is syntactically invalid. |
| `InvalidItemPath` | `E_INVALIDITEMPATH` | 3.1.9 | Fault (`IsSuccess()` false) | Item path is syntactically invalid. |
| `InvalidPid` | `E_INVALIDPID` | 3.1.9 | Fault (`IsSuccess()` false) | Property ID is invalid. |
| `NoSubscription` | `E_NOSUBSCRIPTION` | 3.1.9 | Fault (`IsSuccess()` false) | Subscription handle is unknown or no longer active. |
| `NotSupported` | `E_NOTSUPPORTED` | 3.1.9 | Fault (`IsSuccess()` false) | Operation or requested feature is not supported. |
| `OutOfMemory` | `E_OUTOFMEMORY` | 3.1.9 | Fault (`IsSuccess()` false) | Server could not allocate memory. |
| `Range` | `E_RANGE` | 3.1.9 | Fault (`IsSuccess()` false) | Value is outside the accepted range. |
| `BadType` | `E_BADTYPE` | 3.1.9 | Fault (`IsSuccess()` false) | Value type conversion or requested type is unsupported. |
| `ReadOnly` | `E_READONLY` | 3.1.9 | Fault (`IsSuccess()` false) | Item cannot be written. |
| `ServerState` | `E_SERVERSTATE` | 3.1.9 | Fault (`IsSuccess()` false) | Server is not in an operational state for the request. |
| `TimedOut` | `E_TIMEDOUT` | 3.1.9 | Fault (`IsSuccess()` false) | Operation timed out. |
| `UnknownItemId` | `E_UNKNOWNITEMID` | 3.1.9 | Fault (`IsSuccess()` false) | Item identifier is not known to the server. |
| `UnknownItemName` | `E_UNKNOWNITEMNAME` | 3.1.9 | Fault (`IsSuccess()` false) | Item name is not known to the server. |
| `UnknownItemPath` | `E_UNKNOWNITEMPATH` | 3.1.9 | Fault (`IsSuccess()` false) | Item path is not known to the server. |
| `WriteOnly` | `E_WRITEONLY` | 3.1.9 | Fault (`IsSuccess()` false) | Item cannot be read. |
| `BadRights` | `E_BADRIGHTS` | Legacy compatibility | Fault (`IsSuccess()` false) | Legacy server result for insufficient item rights. |

`OpcQuality` packs the DA quality word as quality kind, substatus, limit, and vendor-extension fields. XML-DA readers map `QualityField` to the top-level quality kind and `LimitField` to the limit field; `XmlDaQualityCompat` preserves the low XML-DA wire byte and drops the high vendor-extension byte when writing XML-DA quality bytes.

| Bit name | Value | Meaning | Related XML-DA quality string |
|---|---|---|---|
| `OpcQuality.QualityMask` | `0x0003` (bits 0-1) | Selects the top-level `OpcQualityKind`. | `QualityField` values below. |
| `OpcQualityKind.Bad` / `OpcQuality.Bad` | `0` (`0x0000`) | Value is not useful. | `bad` |
| `OpcQualityKind.Uncertain` / `OpcQuality.Uncertain` | `1` (`0x0001`) | Value is not known to be correct. | `uncertain` |
| `OpcQualityKind.Reserved` | `2` (`0x0002`) | Reserved by OPC DA; should not appear. | None; unrecognized `QualityField` strings decode as `Bad`. |
| `OpcQualityKind.Good` / `OpcQuality.Good` | `3` (`0x0003`) | Value is current and reliable. | `good`, `goodNonSpecific` |
| `OpcQuality.SubstatusMask` | `0x003C` (bits 2-5) | Four-bit `Substatus` value, 0 through 15. | Not separately decoded by the XML-DA readers. |
| Substatus bit 0 | `0x0004` | Adds 1 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 1 | `0x0008` | Adds 2 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 2 | `0x0010` | Adds 4 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| Substatus bit 3 | `0x0020` | Adds 8 to `Substatus`. | Not separately decoded by the XML-DA readers. |
| `OpcQuality.LimitMask` | `0x00C0` (bits 6-7) | Selects the `OpcQualityLimit`. | `LimitField` values below. |
| `OpcQualityLimit.NotLimited` | `0` (`0x0000`) | Value is not limited. | `none` |
| `OpcQualityLimit.Low` | `1` (`0x0040`) | Value has been pegged to the low limit. | `low` |
| `OpcQualityLimit.High` | `2` (`0x0080`) | Value has been pegged to the high limit. | `high` |
| `OpcQualityLimit.Constant` | `3` (`0x00C0`) | Value is constant and cannot move. | `constant` |
| `OpcQuality.VendorMask` | `0xFF00` (bits 8-15) | Vendor-specific extension byte, exposed as `VendorExtension`. | No XML-DA quality string; `XmlDaQualityCompat.ToWireByte` drops this high byte. |

Inspect quality bits directly from any XML-DA item result:

```csharp
using Opc.Classic;
using Opc.Classic.Xml;

static void PrintQuality(XmlDaItemValueResult item)
{
    OpcQuality quality = item.Quality;
    byte xmlDaWireByte = XmlDaQualityCompat.ToWireByte(quality);

    Console.WriteLine(
        $"raw=0x{quality.RawValue:X4} wire=0x{xmlDaWireByte:X2} " +
        $"kind={quality.Quality} substatus={quality.Substatus} " +
        $"limit={quality.Limit} vendor=0x{quality.VendorExtension:X2}");

    if (quality.Quality == OpcQualityKind.Bad)
    {
        Console.WriteLine($"{item.ItemName}: bad quality");
    }

    if (quality.Limit != OpcQualityLimit.NotLimited)
    {
        Console.WriteLine($"{item.ItemName}: limited at {quality.Limit}");
    }
}
```

## Not supported

- Hosting an XML-DA server endpoint.
- SOAP 1.2 bindings.
- BSTR variants and vendor-specific value carriers beyond raw-text preservation.
- Generated SOAP proxy types or reflection-based XML serialization.
- Built-in WS-Security policy; use the supplied `HttpClient` for authentication and transport security.

## Verification

`tests\Opc.Classic.Xml.Tests` covers the HTTP client path and per-operation serializers. The project participates in the repository build and test gates; the rc.7 validation sweep has 0 build warnings and all 17 .NET test projects green.

## Roadmap

Before `1.0.0` FINAL:

- complete XML-DA interop runs against representative third-party servers when access is available.

For `2.0.0`:

- evaluate XML-DA server hosting;
- add vendor-specific value carriers where interop demand justifies them;
- consider optional SOAP security helpers layered on top of `HttpClient`.

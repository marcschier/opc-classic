# NDR pointer marshaling

> Reference: [MS-RPCE] §2.2.5 (NDR Transfer Syntax), DCE 1.1 §14.3.10
> (pointer kinds), §14.3.12 (pointer placement).

Real Windows DCOM follows DCE 1.1 NDR strictly. Opc.Classic's generated
client proxies and server dispatchers used to emit "flat NDR" — every
parameter written as a flat sequence of scalar/array writes — which
worked for managed loopback (both ends used identical layout) but
mis-aligned every method whose IDL declares one or more pointer-typed
parameters when talking to Matrikon, Kepware, OPC Foundation reference
servers, or any other native DCOM peer.

This document describes how the proxy/dispatch source generators map
IDL pointer shapes onto C# parameter signatures and per-shape wire
emission.

## Pointer kinds and where they show up

| IDL declaration | NDR pointer kind | Wire shape | Typical OPC use |
|---|---|---|---|
| `[in] T*` under `pointer_default(unique)` | unique | 4-byte referent + value | `[in, unique] LONG *pTimeBias` |
| `[in, unique] T*` (explicit) | unique | same as above | optional scalar input |
| `[in] T` scalar / `[in] REFIID` | inline | value bytes (no prefix) | `Guid riid` |
| `[in, string] LPWSTR` / `LPCWSTR` | unique-pointer string | referent + conformant-varying chars | `[in, string] LPWSTR szName` |
| `[in, size_is(n)] T*` | conformant array (unique-ptr in IDL with `pointer_default(unique)`) | referent + max\_count + elements | `[in, size_is(dwCount)] OPCITEMDEF *pItemArray` |
| `[out] T*` (scalar) | reference pointer | value inline (no prefix) | `[out] OPCHANDLE *phServerGroup` |
| `[out] T**` (pointer-to-pointer) | unique pointer to struct/array | referent + struct/array | `[out] OPCSERVERSTATUS **ppServerStatus` |
| `[out, iid_is(riid)] LPUNKNOWN*` | unique pointer to MInterfacePointer | referent + `cbData` + OBJREF | `[out, iid_is(riid)] LPUNKNOWN *ppUnk` |

The non-obvious cases are the ones that bit us:

- `[in, unique] T*` does emit the referent ID inline alongside the value
  even at the top-level parameter list (per C706 §14.3.12.1 — top-level
  unique pointers are *not* deferred; only struct-embedded pointers are
  pushed to the deferred-pointer pile per §14.3.12.3).
- `[out] T**` requires a leading 4-byte referent that the proxy must
  consume before reading the inner value, even though the inner value
  is the entire response body.
- `[out] T*` (one star) is a *reference* pointer with the value inline
  with no referent at all — this is the default scalar output shape and
  is by far the most common use of `[out]` in OPC IDL.

## Generator attribute model

Two attributes plus C# nullability drive the per-parameter shape:

### `Nullable<T>` value types

`int?`, `float?`, `Guid?` (etc.) on a parameter automatically become
NDR unique pointers. The generator emits:

```csharp
// request encode (proxy)
writer.WriteUInt32(value.HasValue ? 0x00020000u : 0u);
if (value.HasValue) writer.WriteInt32(value.Value);

// response decode (proxy) / request decode (dispatcher)
uint referent = reader.ReadUInt32();
int? value = referent != 0u ? reader.ReadInt32() : null;
```

The referent ID `0x00020000` matches what Windows DCOM emits by default
for non-null unique pointers; any non-zero value is spec-compliant, but
matching the Windows convention reduces interop friction for any peer
that compares-by-value.

### `[OpcUniquePointer]` on a parameter

Place `[OpcUniquePointer]` on a non-nullable parameter when the IDL
declares it as `[in, unique] T*` but the value can never legitimately be
NULL in C# (because there's no null state for a non-nullable scalar).
The generator emits:

```csharp
writer.WriteUInt32(0x00020000u);
writer.WriteInt32(value);
```

Used for OPC's `[in, unique] LONG *pTimeBias` and
`[in, unique] FLOAT *pPercentDeadband` on `IOPCServer::AddGroup`.

### `[OpcUniquePointer]` on an `IOpcInterfaceRef` (interface-pointer wrapping)

For IDL parameters declared `[in, unique, iid_is(riid)] LPUNKNOWN` or
`[out, iid_is(riid)] LPUNKNOWN *ppUnk` (e.g.
`IOPCServer::AddGroup`'s `ppUnk` and
`IOPCServer::GetGroupByName`'s return value), the wire shape is a
**MInterfacePointer** (MS-DCOM §2.2.1.10) behind a unique-pointer
referent:

```
uint  referent_id;          // 0 if NULL, non-zero otherwise
{if non-null:}
  uint  ulCntData;          // size of the OBJREF body in bytes
  ulCntData bytes of OBJREF (MEOW + STDOBJREF + DUALSTRINGARRAY)
```

Tag the relevant `IOpcInterfaceRef` parameter or return value with
`[OpcUniquePointer]` (or `[return: OpcUniquePointer]`) and the
generator routes the read/write through `OpcMInterfacePointerCodec`
instead of the bare `OpcInterfaceRefCodec`. Without the wrapper, the
decoder reads `ulCntData` as the start of the MEOW signature and
throws.

### `[return: OpcUniquePointer]` on a method

Place `[return: OpcUniquePointer]` on a `Task<T>` method whose IDL
output is `[out] T**` (unique pointer to T). The proxy decoder reads a
4-byte referent before invoking the struct codec; the dispatcher writes
the referent before invoking the struct codec.

Used for `IOPCServer::GetStatus` and `IOPCEventServer::GetStatus`, both
of which declare `[out] OPCSERVERSTATUS **ppServerStatus`.

## What the generators DO NOT yet handle

Scope-limited to keep this commit reviewable; tracked as follow-up
work in `plan.md` under Track Y:

- **Deferred unique pointers inside conformant-array struct elements.**
  Spec structs like `OPCBROWSEELEMENT` carry embedded `[unique] LPWSTR`
  fields that, per C706 §14.3.12.3, must be marshaled as referents
  inline with the struct, then pushed to a deferred-pointer pile
  emitted after the whole struct. The generator currently emits flat
  fields. Affects `IOPCBrowse::Browse`'s `[out, size_is(,*pdwCount)]
  OPCBROWSEELEMENT** ppBrowseElements`.

- **MS-OAUT `_wireVARIANT` array element envelope.** Z2 + Z4 ship the
  per-element envelope (wireVARIANT + pad-to-8) and supporting
  `[OpcVariantElements]` attribute, plus the new
  `WriteVariantElement`/`ReadVariantElement` helpers. Confirmed
  end-to-end on the request side. On the response side, real-DCOM
  servers emit an additional 8-byte per-element block beyond the
  standard wireVARIANT layout (observed in wire dump). Tracked for
  follow-up — needs a MIDL `/server`-generated reference trace to
  byte-exact-validate. The diagnostic helper
  `OPC_CLASSIC_DCOM_WIRE_DUMP=1` env var on `DcomCallChannel` enables
  hex dump of request/response bytes for further reverse engineering.

- **Explicit NDR alignment in the emitter.** Today, every scalar codec
  call relies on the underlying `NdrWriter`/`NdrReader` to maintain
  alignment internally. The generator does not yet emit
  `writer.AlignTo(8)` before `WriteInt64`/`WriteDouble`/`WriteGuid`
  sequences spanning natural alignment boundaries. The codec layer is
  4-byte-correct today; 8-byte-aware once unique-pointer shapes are
  layered onto 8-byte payloads.

- **Conformant-array `[size_is(N)]` sibling-parameter encoding.**
  Today an array parameter carries its own 4-byte max\_count prefix.
  When the IDL pairs the array with a separate `[in] DWORD N`
  parameter, the wire carries two copies of `N` (the standalone scalar
  and the array's max\_count). The generator already emits both; this
  note is just a reminder that this is correct per spec, not redundant.

## Available runtime-navigation primitives

- **`IRemUnknown::RemQueryInterface`** (Track Y7a) — `IID 00000131-…`,
  opnum 3. Use to obtain new IPIDs on an existing OXID (e.g. QI from
  `IOPCGroupStateMgt` to `IOPCSyncIO`/`IOPCItemMgt` after `AddGroup`).
  Generated proxy lives in `src/Opc.Classic.Dcom/Remoting/IRemUnknown.cs`;
  returned `OpcRemQIResult[]` carries the per-IID HRESULT + STDOBJREF
  (flags + cPublicRefs + OXID + OID + IPID) per MS-DCOM §2.2.19. The
  `ipidRemUnknown` value to use for the call is returned by
  `IActivation::RemoteActivation` in the activation response.

## Wire-format regression net (Track Y6, Y7a, Y10)

`tests/Opc.Classic.Da.Tests/Wire/` pins byte-shape fixtures for the
methods we've already shipped:

- `NdrOpcServerStatusWireFixtures` — OPCSERVERSTATUS layout and the
  unique-pointer LPWSTR VendorInfo (referent + max\_count + offset +
  actual\_count + WCHAR[]).
- `OpcMInterfacePointerCodecWireFixtures` — referent + cbData + OBJREF
  wrapping; null-pointer path (single zero referent).
- `IOPCServerAddGroupWireFixtures` — full AddGroup request encoding
  (including `[OpcUniquePointer]` referent prefix on `pTimeBias` and
  `pPercentDeadband`) and response decode through the MInterfacePointer
  codec.
- `NdrOpcVariantWireFixtures` — _wireVARIANT layout per MS-OAUT §2.2.29
  for VT_I4, VT_R4, VT_R8, VT_BOOL, VT_BSTR, VT_UI1.

`tests/Opc.Classic.Dcom.Tests/Remoting/IRemUnknownProxyTests.cs` does
the same for IRemUnknown::RemQueryInterface request body + REMQIRESULT
array response.

These fixtures must fail loudly if any future generator/codec refactor
silently changes the wire shape.

## Loopback-vs-real-wire reconciliation

Before Track Y, loopback round-trip tests were the source of truth: if
a codec round-trip worked through `InMemoryCallChannel`, the code was
"correct". That assumption was wrong because both sides emitted the
same flat layout.

After Track Y, the loopback wire format matches what real DCOM emits.
Test helpers that emulate a server response payload (e.g.
`F1DaRoundTrip.EncodeStatus`, `ErrorPathTests.EncodeStatus`,
`OpcDaServerDispatcherTests.ReadStatus`) prepend the
`0x00020000` referent before invoking the spec codec. This keeps
loopback tests honest about the wire shape the proxy expects.

## Future-proofing

When adding new `[OpcInterface]` methods, audit each pointer parameter
against the IDL signature:

1. Open the vendored `opcda.idl` (or the relevant vendored spec file).
2. For every `[in, unique]` scalar: use `int?` / `float?` / `Guid?` or
   tag with `[OpcUniquePointer]`.
3. For every `[out] T**`: tag the return value with
   `[return: OpcUniquePointer]` (single-result methods) or the out
   parameter with `[OpcUniquePointer]` (multi-out methods).
4. For every `[out, iid_is(riid)] LPUNKNOWN*` or
   `[in, unique, iid_is(riid)] LPUNKNOWN`: tag the
   `IOpcInterfaceRef`-typed parameter/return with
   `[OpcUniquePointer]` so the generator routes through
   `OpcMInterfacePointerCodec`.
5. For every `[size_is(N)] T*` array: ensure `N` is a sibling
   parameter declared earlier in the method signature.

When in doubt, capture a Wireshark trace of a Windows OPC client
calling the method against the same server and compare byte-by-byte
against the generated request/response. The fixtures under
`tests/Opc.Classic.Da.Tests/Wire/` (planned for Track Y6) will codify
known-good wire bytes for the most common methods so regressions are
caught at unit-test time rather than at Matrikon-integration time.

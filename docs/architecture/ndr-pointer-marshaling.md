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

- **MInterfacePointer wrapping for `[out, iid_is] LPUNKNOWN*`.**
  The current `OpcInterfaceRefCodec.Read` decodes a raw OBJREF that
  starts with the MEOW signature. Real DCOM wraps the OBJREF in a
  `MInterfacePointer` (4-byte `ulCntData` + OBJREF bytes) behind a
  unique-pointer referent. `IOPCServer::AddGroup`'s `ppUnk` requires
  this wrapping to decode against Matrikon.

- **Deferred unique pointers inside conformant-array struct elements.**
  Spec structs like `OPCBROWSEELEMENT` carry embedded `[unique] LPWSTR`
  fields that, per C706 §14.3.12.3, must be marshaled as referents
  inline with the struct, then pushed to a deferred-pointer pile
  emitted after the whole struct. The generator currently emits flat
  fields. Affects `IOPCBrowse::Browse`'s `[out, size_is(,*pdwCount)]
  OPCBROWSEELEMENT** ppBrowseElements`.

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

1. Open `ext/inc/opcda.idl` (or the relevant spec file).
2. For every `[in, unique]` scalar: use `int?` / `float?` / `Guid?` or
   tag with `[OpcUniquePointer]`.
3. For every `[out] T**`: tag the return value with
   `[return: OpcUniquePointer]`.
4. For every `[out, iid_is] LPUNKNOWN*`: NOT yet supported — track
   separately until MInterfacePointer wrapping lands.
5. For every `[size_is(N)] T*` array: ensure `N` is a sibling
   parameter declared earlier in the method signature.

When in doubt, capture a Wireshark trace of a Windows OPC client
calling the method against the same server and compare byte-by-byte
against the generated request/response. The fixtures under
`tests/Opc.Classic.Da.Tests/Wire/` (planned for Track Y6) will codify
known-good wire bytes for the most common methods so regressions are
caught at unit-test time rather than at Matrikon-integration time.

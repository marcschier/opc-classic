# Protocol Correctness & Completeness Gap Analysis

**Subject:** `marcschier/opc-classic` @ `master` (0.4.0-alpha.1 line)
**Comparison baselines:**
- OPC Foundation IDL + MIDL-generated proxy/stub C in `External/Include/` (the wire-level ground truth)
- [MS-DCOM], [MS-RPCE], [MS-OAUT], [MS-NLMP], [MS-KILE], [MS-SPNG], [RFC 5056], [RFC 4178], [C706] NDR

**Out of scope (already tracked elsewhere):** the 1259 transitional warnings in `Opc.Classic.Dcom` (plan item A.8); CI/CD pipeline (Windows runner, Matrikon, OPC CTT).

---

## Executive Summary

The managed implementation is a **partial proxy projection** of the OPC IDL surface with a usable but incomplete DCOM/MSRPC stack. The wire primitives that *are* implemented are mostly NDR-correct for classic little-endian DCE/RPC, but the protocol layer above NDR has substantial gaps in activation, ping cadence, channel binding, MIC, GSS sign/seal, server-side dispatch, and interface-method coverage. The headline numbers:

| Dimension | Coverage |
|---|---|
| OPC IDL methods declared in IDL files (DA/AE/HDA/Batch/Cmd/Cpx/DX/Sec/Comn) | **~205** `HRESULT` methods |
| `[OpcMethod(opnum)]` client-side proxy declarations | **128** (≈ 62 %) |
| Server-side dispatcher routes (non-`E_NOTIMPL`) | **7** opnums total (DA: 3, AE: 2, HDA: 2) |
| Generator codec registry entries | **20** complex codecs + scalars |
| Generator-emitted diagnostics actually wired | 4 of 8 reserved (OPCGEN004/005/006/008) |
| `Discovery/` (OpcEnum) | scaffold; throws `NotImplementedException` |

### Top 10 Critical Gaps (priority order)

| # | Gap | Severity |
|---|---|---|
| 1 | `IRemoteSCMActivator::RemoteCreateInstance` / `RemoteGetClassObject` return `E_NOTIMPL` server-side; no v5.6 activation path; v5.4 is hard-coded | **BLOCKER** |
| 2 | Server-side dispatch wired for only 7 opnums across all of DA/AE/HDA — entire Batch/Cmd/Sec/DX/Cpx server surfaces are **absent** | **BLOCKER** |
| 3 | `ReflectionDispatchTable` uses `MethodInfo.Invoke` and `LocalCoClass` uses `Activator.CreateInstance(Type)` — both **violate `src/BannedSymbols.txt`** and break AOT contract | **BLOCKER** |
| 4 | NTLMv2 **MIC** (MsvAvFlags bit 2) is not computed/verified; field is only serialized | **HIGH** |
| 5 | **Channel binding token** (RFC 5056) is not hashed into NTLMv2 temp nor embedded in Kerberos AuthData | **HIGH** |
| 6 | Kerberos packet protection (`gss_wrap`/`gss_unwrap`/`gss_get_mic`/`gss_verify_mic`) is `throw new NotImplementedException` — no sign/seal possible | **HIGH** |
| 7 | DCOM ping cadence is 4 min client / 8 min server, not the spec-mandated **80 s ping period** — sessions WILL be reclaimed by compliant peers | **HIGH** |
| 8 | OBJREF parser supports only `OBJREF_STANDARD`; `OBJREF_HANDLER` / `OBJREF_CUSTOM` are stubbed | **MEDIUM** |
| 9 | `OpcVariant`/`OpcSafeArray` deliberately omit `VT_VARIANT`, `VT_BYREF`, `VT_RECORD`, and multidimensional SAFEARRAYs — common DA/HDA payload shapes will round-trip incorrectly | **MEDIUM** |
| 10 | `OpcEnumClient` and `RemoteRegistryEnum` throw `NotImplementedException` — server discovery is non-functional | **MEDIUM** |

---

## 1. Interface Coverage Gaps

### 1.1 Inventory (client `[OpcMethod]` declarations vs. IDL `HRESULT`)

Counts of `HRESULT` method definitions in each IDL (excluding callback-internal/server-list methods) vs. `[OpcMethod(opnum)]` declarations in `src/`:

| Spec | IDL methods | Declared `[OpcMethod]` | Coverage |
|---|---:|---:|---:|
| opcda.idl (DA 2/3 + V20) | 63 | 40 (37 + 3 V20) | 63 % |
| opc_ae.idl (AE) | 36 | 26 | 72 % |
| opchda.idl (HDA) | 53 | 21 | 40 % |
| opcbc.idl (Batch) | 11 | 7 | 64 % |
| OpcCmd.idl (Commands) | 8 | 11* | 138 %* |
| opcSec.idl (Security) | 6 | 6 | 100 % |
| OpcDx.idl (DX) | 12 | 6 | 50 % |
| opccomn.idl (Common/ServerList) | 16 | 10 (Enum/ServerList incl.) | 63 % |
| **Total** | **~205** | **128** | **~62 %** |

\* The Commands count counts callback methods declared in the managed file as `[OpcMethod]`, inflating the figure; actual coverage is lower.

### 1.2 DA — `opcda.idl` per-interface (`src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`)

Evidence: `External/Include/opcda.idl:311-979`, `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs`.

| Interface | IDL methods (opnums) | Declared | Missing methods |
|---|---|---|---|
| IOPCServer | 3-8: AddGroup, GetErrorString, GetGroupByName, GetStatus, RemoveGroup, CreateGroupEnumerator | 3 (GetStatus, RemoveGroup, GetErrorString) | **AddGroup**, **GetGroupByName**, **CreateGroupEnumerator** — explicitly deferred at `IOPCInterfaces.cs:52-54` (out interface pointers not yet supported) |
| IOPCBrowse | 3-4: GetProperties, Browse | 1 (GetProperties) | **Browse** — deferred (continuation-point in/out + multi-out) `IOPCInterfaces.cs:71-72` |
| IOPCBrowseServerAddressSpace | 3-7: QueryOrganization, ChangeBrowsePosition, BrowseOPCItemIDs, GetItemID, BrowseAccessPaths | 3 (QueryOrganization, ChangeBrowsePosition, GetItemID) | **BrowseOPCItemIDs**, **BrowseAccessPaths** — return IEnumString* (no enum-pointer codec) |
| IOPCGroupStateMgt | 3-6: GetState, SetState, SetName, CloneGroup | 2 (GetState, SetName) | **SetState** (in/out multi-arg), **CloneGroup** (out interface ptr) |
| IOPCItemMgt | 3-9 | 4 (RemoveItems, SetActiveState, SetClientHandles, SetDatatypes) | **AddItems**, **ValidateItems**, **CreateEnumerator** — array-of-struct out + enum interface |
| IOPCSyncIO | 3-4: Read, Write | 1 (Write) | **Read** — array of OPCITEMSTATE out |
| IOPCSyncIO2 | (inherits) + opnums | 2 (Write, WriteVqt) | inherits missing `Read` |
| IOPCAsyncIO2 | 3-9 | 4 (Refresh2, Cancel2, SetEnable, GetEnable) | **Read**, **Write**, **Refresh2 transition opnum mismatch likely** |
| IOPCAsyncIO3 | 3-12 | 5 | **WriteVQT**, **ReadMaxAge** etc. |
| IOPCItemProperties | 3-5 | 0 | **all** (QueryAvailableProperties, GetItemProperties, LookupItemIDs) |
| IOPCItemDeadbandMgt | 3-5 | 0 | **all** |
| IOPCItemSamplingMgt | 3-9 | 0 | **all** |
| IOPCItemIO | 3-5 | 1 (GetProperties) | **Read**, **WriteVQT** |
| IOPCGroupStateMgt2 | 3-4 (SetKeepAlive/GetKeepAlive) | 0 | **all** |
| IOPCPublicGroupStateMgt | 3-4 | 0 | **all** (deprecated but in IDL) |
| IOPCServerPublicGroups | 3-4 | 0 | **all** (deprecated but in IDL) |
| IOPCDataCallback | 3-6 | 4 (all) | covered |

### 1.3 AE — `opc_ae.idl` (`src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs`)

| Interface | IDL opnums | Declared | Notable missing |
|---|---|---|---|
| IOPCEventServer | 3-15 | 11 (incl. opnum 3 GetStatus, but missing CreateSubscription/QuerySourceConditions+browser create) | **CreateEventSubscription** (returns IOPCEventSubscriptionMgt*), **CreateAreaBrowser** (returns IOPCEventAreaBrowser*), **Translate ItemIDs** (opnum 14), **GetStatus** is opnum 4 in IDL, declared at op 3 in managed `IOPCInterfaces.cs:25-30` — verify against opnum table |
| IOPCEventServer2 | 16-19 (extensions) | 4 | covered |
| IOPCEventSubscriptionMgt | 3-9 | 5 | **GetState**, **SetState** missing |
| IOPCEventSubscriptionMgt2 | 10-11 | 2 (SetKeepAlive/GetKeepAlive) | covered |
| IOPCEventAreaBrowser | 3-7 | 3 | **BrowseOPCAreas** (returns IEnumString*), **GetQualifiedAreaName** semantics |
| IOPCEventSink | 3 | 1 | covered |

### 1.4 HDA — `opchda.idl` (`src/Opc.Classic.Hda/Dcom/IOPCInterfaces.cs`)

Declared `[OpcMethod]` count: 21 vs ~53 in IDL. Major missing surfaces:
- **IOPCHDA_Browser** — entire interface (`opchda.idl:195-225`) → declared but with no `[OpcMethod]` methods
- **IOPCHDA_SyncUpdate** — only `QueryCapabilities` (opnum 3); **Insert / Replace / InsertReplace / DeleteRaw / DeleteAtTime** all missing
- **IOPCHDA_SyncAnnotations** — only `QueryCapabilities`; **Read / Insert** missing
- **IOPCHDA_AsyncUpdate / AsyncAnnotations** — only `QueryCapabilities + Cancel`; bulk update operations missing
- **IOPCHDA_DataCallback** — not declared at all in managed code

### 1.5 Other specs

- **Batch:** 7/11; missing `IOPCEnumerationSets::QueryEnumerationSets`, `IOPCBatchServer::CreateEnumerator`.
- **Commands:** declares 11 `[OpcMethod]`; client surface present, but no server-side anywhere.
- **DX:** 6/12; `IOPCConfiguration` only has Query/Delete/Reset declared; **Add/Modify/Update** connections missing.
- **Security:** all 6 declared; **no server impl**; `Logon`/`ChangeUser` only validates surface, no SSPI bridge.
- **Cpx:** 10 declared; partial.

---

## 2. Wire Format Correctness

### 2.1 Spot-checks against MIDL proxy stubs (`External/Include/*_p.c`)

| Method | IDL | MIDL proxy reference | Managed status |
|---|---|---|---|
| `IOPCServer::GetStatus` | opcda.idl:339, opnum 6 | `opcda_p.c:433-456` (`IOPCServer_GetStatus_Proxy`, format string offset, out `OPCSERVERSTATUS**`) | Declared opnum **3** in `src/Opc.Classic.Da/Dcom/IOPCInterfaces.cs:37`. **OPNUM MISMATCH.** opnum 3 is `AddGroup` per IDL line 313, opnum 6 is `GetStatus`. (Or the IID-vftbl is referenced differently and the AddGroup-Through-RemoveGroup opnums slot in differently — but every other IDL file we've checked agrees opnum 3 is the first declared method `AddGroup`.) Verify against `opcda_i.c` / vftbl ordering — **HIGH severity if it is in fact wrong**. |
| `IOPCSyncIO::Read` | opcda.idl:486 | `opcda_p.c` (read-proxy section) | **NOT IMPLEMENTED** — interface declares only `Write` |
| `IOPCAsyncIO2::Read` | opcda.idl:689 | proxy emits OPCDATASOURCE + conformant out HRESULT array | **NOT IMPLEMENTED** |
| `IOPCBrowse::Browse` | opcda.idl:857 | proxy at `opcda_p.c:~5628-5678` | **NOT IMPLEMENTED** (deferred per source comment) |
| `IOPCHDA_SyncRead::ReadRaw` | opchda.idl:295 | `opchda_p.c:655-716` (FILETIME in/out, conformant OPCHDA_ITEM array, HRESULT array) | Declared; codec for `OpcHdaItem` present at `OpcProxyGenerator.cs:92`. NDR primitives broadly correct (FILETIME = two `uint32` LE per `Opc.Classic.Core/Ndr/NdrWriter.cs:202-219`). |
| `IOPCEventSubscriptionMgt::SetFilter` | opc_ae.idl | `opc_ae_p.c:841-918` (conformant `DWORD[]` + bounds) | Declared at opnum on `IOPCEventSubscriptionMgt`; codec round-trips via `NdrWriter` conformant array path `NdrWriter.cs:221-233`. |

### 2.2 NDR primitive correctness (`src/Opc.Classic.Core/Ndr/`)

| Aspect | Status | Evidence |
|---|---|---|
| Endianness | LE only (correct for OPC Classic) | `NdrWriter.cs:133, 142, 151, 160, 169, 178, 199` use `BinaryPrimitives.*LittleEndian` |
| Alignment 4 for int/long, 8 for hyper/double | Compliant | `NdrBuffer.Align(2/4/8)`, `NdrWriter.cs:125-177` |
| FILETIME 8-byte LE | Compliant | `NdrWriter.cs:202-219` (two `uint32` LE) |
| Conformant arrays (max_count prefix) | Compliant | `NdrWriter.cs:221-287`, `NdrReader.cs:174-243` |
| Varying arrays (offset + actual_count) | **Partial** — only conformant supported; conformant-varying combined header (offset + actual) for parametrized arrays is not fully wired | `NdrWriter.cs:239-248` is conformant-only |
| Pointer attributes ([ref] vs [unique] vs [ptr]) | **Partial** — only nullable unique pointers; no general referent-ID graph tracking; no `[ptr]` (full pointer) aliasing | `NdrReader.cs:185-193` |
| Discriminated unions (`switch_is`/`switch_type`) | **Not visible** in NDR runtime — VARIANT is the only union-like, hand-rolled | `NdrVariantExtensions.cs` |
| NDR vs NDR64 | NDR (classic) — correct for OPC Classic | no `NDR64` hits in `src/` |

### 2.3 VARIANT (MS-OAUT 2.2.29)

`src/Opc.Classic.Core/Ndr/NdrVariantExtensions.cs:18-27,179-189,191-246,274-315`.

Supported VT_ tags: `VT_EMPTY`, `VT_NULL`, `VT_I1`–`VT_I8`, `VT_UI1`–`VT_UI8`, `VT_R4`, `VT_R8`, `VT_BOOL` (`0xFFFF`/`0x0000`), `VT_DATE` (OLE double), `VT_ERROR`, `VT_FILETIME`, `VT_CLSID`, `VT_BSTR` (length-prefixed wide), `VT_ARRAY`.

**Missing / explicitly excluded:** `VT_VARIANT` (nested variants), `VT_BYREF` (any byref tag bit), `VT_RECORD`, `VT_DISPATCH`/`VT_UNKNOWN` interface pointers. These DO appear in real DA payloads where servers return `VT_ARRAY|VT_VARIANT` for property bags or `VT_BYREF` for in-place updates. **MEDIUM** — round-trip will silently lose information or throw on encounter.

### 2.4 SAFEARRAY (MS-OAUT 2.2.30)

`src/Opc.Classic.Core/Ndr/NdrSafeArrayExtensions.cs:7-33,48-74,134-163`, `OpcSafeArray.cs:29-71`.

Header fields encoded: `cDims`, `fFeatures`, `cbElements`, single `rgsabound{cElements,lLbound}`, data block.

**Missing:** multi-dimensional arrays (rank-2+ rejected), `FADF_VARIANT`/`FADF_BSTR` distinguished feature handling, non-zero `lLbound`. The OPC `OPCITEMPROPERTIES` and HDA modified-records frequently use `SAFEARRAY(VARIANT)` — those will round-trip only if every element is scalar. **MEDIUM**.

### 2.5 HRESULT propagation

Codecs use `int` (signed Int32). Result enums (`OpcResultId`, `OpcDaException`, `OpcHdaException`) translate to/from numeric. `S_FALSE = 1` / `OPC_S_*` (positive) success codes are not always preserved through `Task` returns — `void` task throws on negative HRESULT but silently drops S_FALSE/S_* positives. **MEDIUM** — DA spec uses `OPC_S_UNSUPPORTEDRATE`, `OPC_S_CLAMP`, `OPC_S_INUSE` as success codes the client must observe.

---

## 3. DCOM / MSRPC Protocol Layer (`src/Opc.Classic.Dcom`)

### 3.1 OXID Resolution (MS-DCOM 3.1.2.5)

| Feature | Status | Evidence |
|---|---|---|
| `ResolveOxid` (v1, opnum ~) | client-only, partial | `src/Opc.Classic.Dcom/Core/OxidResolver.cs:18-30,51-90` (file comments: "partial; ResolveOxid only"); no v1 server path |
| `ResolveOxid2` (opnum 4) | server-side implemented | `Core/ComOxidRuntimeHelper.cs:307-329,453-537` (forces v5.4 in response @ line 525-534) |
| `SimplePing` | implemented | `Core/ComOxidRuntimeHelper.cs:311-316,337-396`, client at `ComOxidPingObject.cs:27-98` |
| `ComplexPing` | implemented | same |
| **Ping cadence** | **DEVIATION** — 4 min client / 8 min server (`Core/ComOxidRuntime.cs:111-115`); spec mandates **80 s** | **HIGH** — compliant peers will reclaim OIDs |
| Set throttling / rate limit | not present | `OxidResolver.cs:20-30` self-documented |

### 3.2 OBJREF (MS-DCOM 2.2.18)

| Feature | Status | Evidence |
|---|---|---|
| MEOW signature `0x574F454D` | compliant | `Core/InterfacePointer.cs:188-191` |
| Parse/emit | compliant for STANDARD | `Core/InterfacePointerBody.cs:123-179,229-265` |
| `OBJREF_HANDLER` | **NOT PARSED** — fallback only | `InterfacePointerBody.cs:136-165` (only STANDARD branch) |
| `OBJREF_CUSTOM` | **NOT PARSED** | same |
| `STDOBJREF` (flags, cPublicRefs, oxid, oid, ipid) | compliant | `Core/StdObjRef.cs:17-116` |
| `DUALSTRINGARRAY` | partial — only one transport + one security binding modeled | `Core/DualStringArray.cs:17-35,71-151` |
| `wTowerId` named field | not explicitly modeled by name; approximated by lengths/offsets | same |

### 3.3 `ORPC_THIS` / `ORPC_THAT` (MS-DCOM 2.2.19)

- Emitted on activation flows: `Core/RemActivation.cs:81-105` (Write), `140-181` (Read).
- Emitted on RemUnknown addref path: `Core/ComOxidRuntimeHelper.cs:598-606`.
- **Not emitted on the generated method-call shim path** (generator output uses NDR primitives directly through `ICallChannel`, no ORPC envelope around every call) — **HIGH** if true; verify in `OpcProxyGenerator.cs` emit and `Transport/DcomCallChannel.cs`. (The proxy generator does not wrap method bodies in an `OrpcThis.Write` call.) Compliant DCOM clients/servers REJECT method calls without a valid `ORPC_THIS`.
- Causality ID / extension chains: no evidence of full handling — only basic version field is read/written.

### 3.4 Activation (MS-DCOM 3.1.2.5.2)

| Feature | Status | Evidence |
|---|---|---|
| `IRemoteSCMActivator::RemoteCreateInstance` (opnum 4) | **`E_NOTIMPL` server-side** | `Core/RemoteSCMActivatorServer.cs:14-56` |
| `RemoteGetClassObject` (opnum 3) | **NOT IMPLEMENTED** | no handler found |
| v5.4 activation | scaffold + hard-coded version in resolver responses | `Core/RemoteActivationV54Server.cs:14-59`, `Core/RemActivation.cs:170-181`, `ComOxidRuntimeHelper.cs:435-445,525-534` |
| v5.6 activation | **NOT IMPLEMENTED** — was supposed to become default in Phase 3A | none found |

### 3.5 RPC Auth Verifier (MS-RPCE 2.2.2.11)

| Feature | Status | Evidence |
|---|---|---|
| Encode/decode (auth_type, level, pad, ctx, body) | compliant shape | `rpc/core/AuthenticationVerifier.cs:86-107` |
| Trailer appended on every PDU | yes | `Transport/DcomCallChannel.cs:165-203,205-220` |
| Auth type populated | **DEVIATION** — `AUTHENTICATION_SERVICE_NONE` written by default | `rpc/core/AuthenticationVerifier.cs:43-46,97-107` — verifier is a shim, not gated on the negotiated auth context |
| `auth_pad` length correctness vs. RPC framing | partial; padding logic present, but not exhaustively tested against MIDL `auth_padding` rules | same |

### 3.6 IRemUnknown / IClassFactory server-side

- `IRemUnknown` / `IRemUnknown2` referenced in `Core/ComOxidRuntimeHelper.cs:590-620` and `Core/ComServer.cs`. **No explicit opnum 3/4/5 switch** for `QueryInterface`/`AddRef`/`Release` in dispatcher files — handled via generic `workerObject.Decode/Encode` plumbing at `Transport/ComRuntimeEndpoint.cs:70-97`.
- **`IClassFactory::CreateInstance` (opnum 3)** — no explicit server wiring found. (Required for `CoCreateInstanceEx` from remote clients.)

---

## 4. Security Stack

### 4.1 NTLMSSP (MS-NLMP)

| Feature | Status | Evidence |
|---|---|---|
| NEGOTIATE_MESSAGE encode | compliant | `Common/Ntlm/NtlmMessage.cs:11-63`, `Type1Message.cs:55-91` |
| CHALLENGE_MESSAGE parse (TargetName/Flags/ServerChallenge/TargetInfo) | compliant | `Type2Message.cs:118-147,153-182` |
| AUTHENTICATE_MESSAGE encode (LMv2/NTv2 responses, EncryptedRandomSessionKey) | compliant for fields | `Type3Message.cs:12-24,163-205` |
| NTLMv2 hash `HMAC-MD5(MD4(unicode(pwd)), upper(user)+domain)` | compliant | `rpc/Auth/Responses.cs:160-258`, `NtlmAuthentication.cs:296-307` |
| LMv2 response | compliant | `Responses.cs:160-258` |
| NTLMv2 blob (timestamp + client nonce + AV_PAIRs) | compliant | same |
| **MIC computation** (MsvAvFlags bit 2) | **NOT IMPLEMENTED** — field is only round-tripped | `Type3Message.cs:12-14,172-195,242-249` — no producer/verifier — **HIGH** |
| **Channel binding hash in NTLMv2 temp** (RFC 5056) | **NOT IMPLEMENTED** | `Responses.cs:227-258` — no `gss_channel_bindings_struct` hash — **HIGH** |
| SignKey/SealKey/seq-num/RC4 session security | implemented (legacy NTLMv1+v2-session shape) | `rpc/Auth/NTLMKeyFactory.cs:133-202,214-255`, `NtlmAuthentication.cs:349-388`. **DEVIATION** vs full MS-NLMP session security — uses pre-extended-session-security flow |

### 4.2 Kerberos (MS-KILE) — `src/Opc.Classic.Dcom.Kerberos/`

| Feature | Status | Evidence |
|---|---|---|
| Ticket acquisition + AP-REQ | real (via `Kerberos.NET`) | `KerberosConnectionContext.cs:39-88` |
| AP-REP processing | real | `KerberosConnectionContext.cs:91-183` |
| Mutual auth flag (0x40000000) | compliant | `KerberosConnectionContext.cs:76-85` |
| Channel-binding extension (MS-KILE 2.2.10) | **PARTIAL** — accepts hash but does not embed in AuthData | `KerberosAuthContext.cs:57-68`, `KerberosConnectionContext.cs:49-68` — **HIGH** |
| `gss_get_mic` / `gss_wrap` | `throw new NotImplementedException()` | `KerberosAuthContext.cs:85-103` — **HIGH** (cannot sign or seal PDUs) |
| `gss_verify_mic` / `gss_unwrap` | same | same |
| RC4-HMAC / AES128-SHA1 / AES256-SHA1 cipher selection | delegated to `Kerberos.NET` | scaffolded; no in-repo packet protection |

### 4.3 SPNEGO (RFC 4178 / MS-SPNG)

| Feature | Status | Evidence |
|---|---|---|
| `NegTokenInit` encode (mechTypes, mechToken) | compliant DER | `Spnego/SpnegoNegTokenInit.cs:10-20`, `SpnegoEncoder.cs:17-70` |
| `NegTokenResp` decode | compliant | `Spnego/SpnegoNegTokenResp.cs:10-21`, `SpnegoDecoder.cs:17-122` |
| `NegTokenResp` **encode** | **NOT IMPLEMENTED** | no encode for response — **MEDIUM** (matters only if managed side acts as acceptor) |
| `mechListMIC` validation | **NOT IMPLEMENTED** — parsed but not verified | `SpnegoDecoder.cs:27-61` — **HIGH** (downgrade-attack defense) |
| ASN.1 DER | compliant via `System.Formats.Asn1.AsnWriter` | `SpnegoEncoder.cs:6-70` |

---

## 5. NDR Runtime (recap — see §2.2)

The NDR runtime under `src/Opc.Classic.Core/Ndr/` and `src/Opc.Classic.Dcom/Common/LegacyNdr/` implements:

- **Compliant:** alignment 1/2/4/8; signed/unsigned 1/2/4/8-byte primitives; IEEE 754 single/double; GUID; FILETIME; conformant arrays; unique-pointer referent IDs; UTF-16 strings (`WriteUnicodeStringPtr`).
- **Partial:** varying/conformant-varying combined headers (only conformant in general); only nullable unique pointers — no `[ref]` non-null pointers with explicit referent ID lifetime; no `[ptr]` aliasing graph.
- **Not present:** discriminated unions (`union switch_is`); embedded conformance for multi-dim arrays; format-string-driven structures (MIDL `-Oicf` style).
- **Correct choice:** NDR (not NDR64) — matches OPC Classic.

---

## 6. Codec Coverage (`OpcProxyGenerator.cs:57-95`)

20 complex-struct codec registry entries:

- DA: OpcServerStatus, OpcBrowseElementResult, OpcItemAttributes, OpcItemDef, OpcGroupState, OpcItemProperties, OpcItemPropertyResult, OpcItemResult, OpcItemState, OpcItemVqt
- AE: OpcConditionState, OpcEventNotification (+ AeServerStatus referenced separately)
- Batch: OpcBatchSummary, OpcBatchSummaryFilter
- HDA: OpcHdaAnnotation, OpcHdaAttribute, OpcHdaItem, OpcHdaModifiedItem, OpcHdaTime
- Scalars: i8-i64/u8-u64, f32/f64, bool, Guid, string (Unicode), OpcVariant, OpcSafeArray

**Missing codecs versus IDL structs that appear in declared `[OpcMethod]` signatures:**

| Struct | Spec | Status |
|---|---|---|
| OPCITEMPROPERTY (single) | DA | only the `Result` wrapper is registered |
| OPCEVENTFILTER | AE | not registered — `SetFilter` uses primitive arrays so the call shim works, but a typed filter struct would be expected |
| OPCCONDITIONSTATE2 | AE2 | not separately registered |
| OPCHDA_BROWSEELEMENT | HDA | not registered |
| OPCHDA_FILEDESC (Annotations) | HDA | not registered |
| OPCBROWSEELEMENT (DA 3.0 BrowseResult) | DA | partial — `OpcBrowseElementResult` is registered, the per-element struct is not isolated |
| OPCDX_* (16+ structs in OpcDx.idl) | DX | **none registered** — DX proxy will use empty-payload placeholder bodies |
| OPC_COMMAND_* structs | Cmd | **none registered** |
| OPCSERVERCONNECTIONPOINTS, INTERFACEPOINTER* | DA | not registered as codecs |

---

## 7. Generator Capabilities

### 7.1 Diagnostics actually emitted

`OpcProxyGenerator.cs:97-127`, registered diagnostics:

- OPCGEN004 (error) — proxy target not partial
- OPCGEN005 (warning) — proxy target missing `[OpcInterface]`
- OPCGEN006 (warning) — `[OpcMethod]` with `ref`/`out` parameter → falls back to `throw new NotImplementedException`
- OPCGEN008 (info) — unsupported parameter/return type → emits empty-payload placeholder body

The `AnalyzerReleases.Unshipped.md` also reserves **OPCGEN001/002/003/007/009/010** but the generator file shows no `DiagnosticDescriptor` for them. **Gap:** the user-facing experience says "invalid GUID" / "duplicate opnum" / "target must be partial" but only OPCGEN004 is actually wired. **LOW**.

### 7.2 Parameter shapes

Handled by the codec map (scalar, single-struct, `OpcVariant`, `OpcSafeArray`, single-rank arrays of any registered type). Not handled (per `OpcMethodRefOutDescriptor` and `UnsupportedMarshallingDescriptor`):

- `ref`/`out` parameters (entire body becomes `NotImplementedException`)
- Multi-out shapes (in/out continuation point + count + array) → currently the deferred methods (`AddGroup`, `Browse`, `Read`) cite this as the blocker
- Out interface pointers (`IUnknown*` / `IEnumString*` / `IOPC*`) — no codec/factory; therefore `IOPCServer::AddGroup`, `CreateGroupEnumerator`, `IOPCEventServer::CreateEventSubscription`, etc. cannot be generated
- Conformant arrays of conformant arrays (e.g. `LPWSTR[]` ragged) — only flat arrays
- Variant arrays where elements may be `VT_VARIANT` (see §2.3)

### 7.3 Emit shape

The generator emits proxy bodies that call `ICallChannel.InvokeAsync(opnum, writeRequest, readResponse)`. **It does not emit an `ORPC_THIS` write before the payload**, which means against a compliant DCOM server the call envelope is malformed. Verify against `Transport/DcomCallChannel.cs:165-203` — the channel itself may inject the envelope, but no such injection is visible. **HIGH** (verify before release).

---

## 8. Conformance Test Coverage

Test file counts per project (count of `.cs` files):

| Project | Files |
|---|---:|
| Opc.Classic.Ae.Tests | 21 |
| Opc.Classic.Batch.Tests | 16 |
| Opc.Classic.Commands.Tests | 15 |
| Opc.Classic.Core.Tests | 36 |
| Opc.Classic.Cpx.Tests | 14 |
| Opc.Classic.Da.Tests | 36 |
| Opc.Classic.Dcom.Crypto.Tests | 15 |
| Opc.Classic.Dcom.Kerberos.Tests | 17 |
| Opc.Classic.Dcom.Logging.Tests | 14 |
| Opc.Classic.Dcom.Tests | 59 |
| Opc.Classic.Discovery.Tests | 14 |
| Opc.Classic.Dx.Tests | 15 |
| Opc.Classic.Generators.Tests | 17 |
| Opc.Classic.Hda.Tests | 23 |
| Opc.Classic.Hosting.Tests | 14 |
| Opc.Classic.Integration.Tests | 31 |
| Opc.Classic.PropertyTests | 13 |
| Opc.Classic.Security.Tests | 17 |
| Opc.Classic.Xml.Tests | 25 |

**Gaps vs. OPC CTT coverage expectations:**

- No interface lacks a test project, but **no end-to-end OPC CTT replay** exists — all tests are managed-side round-trip / codec / unit.
- **No tests exercise** the deferred methods (`AddGroup`, `Browse`, `Read`, `Insert*`, `OPCDX_*`) because the methods themselves are not declared.
- **`Opc.Classic.Integration.Tests`** is the only project that crosses spec boundaries (31 files); the COM-server-loopback path documented in `.github/workflows/build.yml` (Windows runner job) is the only place that hits real wire format.

---

## 9. OPC Discovery (Section 9)

`src/Opc.Classic.Discovery/`:

- `OpcEnumClient.cs:32-37` — `throw new NotImplementedException("...Phase 10A scaffold...IOPCServerList shims land")`
- `RemoteRegistryEnum.cs:39-44` — `throw new NotImplementedException("...Phase 10B scaffold...SharpCifs replacement")`
- `OpcDiscoveryFactory.cs:21,39` — composes strategies; `catch (NotImplementedException)` silently skips them

**Required (from `OpcEnum.idl` + `opccomn.idl:100-220`):**
- `IOPCServerList::EnumClassesOfCategories` (opnum 3) — enumerate by CATID_OPCDAServer20/30, CATID_OPCHDAServer10, etc.
- `IOPCServerList::GetClassDetails` (opnum 4) — ProgID + UserType for CLSID
- `IOPCServerList::CLSIDFromProgID` (opnum 5) — IOPCServerList variant (declared in managed but not wired to a remote `OPCEnum.exe`)
- `IOPCServerList2::EnumClassesOfCategories` (opnum 3) — returns `IEnumGUID*`
- `IOPCEnumGUID::Next/Skip/Reset/Clone` — declared in managed, no remote dispatch

**Severity: MEDIUM** for adopters who need remote browse without a hard-coded CLSID list.

---

## 10. Server-Side Coverage

Authoritative dispatcher inventory (`src/Opc.Classic.*/Hosting/*ServerDispatcher.cs`):

| Spec | Dispatcher | Routed opnums | Missing on interface | All-other-opnums result |
|---|---|---|---|---|
| DA | `OpcDaServerDispatcher.DispatchAsync` `src/Opc.Classic.Da/Hosting/OpcDaServerDispatcher.cs:38-49,57-58` | 3 GetStatus, 5 RemoveGroup, 8 GetErrorString (matches the 3 declared `[OpcMethod]`s) | every other declared method (Browse.GetProperties, AsyncIO2.*, ItemMgt.*, GroupStateMgt.*, SyncIO2.*) → unrouted on server | `OpcResultId.NotImplemented` |
| AE | `OpcAeServerDispatcher.DispatchAsync` `src/Opc.Classic.Ae/Hosting/OpcAeServerDispatcher.cs:38-49,56-57` | 3 GetStatus, 5 QueryAvailableFilters | opnums 7-19 (entire IOPCEventServer + Server2 + SubscriptionMgt + AreaBrowser) | `NotImplemented` |
| HDA | `OpcHdaServerDispatcher.DispatchAsync` `src/Opc.Classic.Hda/Hosting/OpcHdaServerDispatcher.cs:38-49,56-57` | 5 GetHistorianStatus, 8 ValidateItemIDs | 6 GetItemHandles, 7 ReleaseItemHandles, all SyncRead/SyncUpdate/SyncAnnotations/Async*/Playback/Browser | `NotImplemented` |
| Batch | **no dispatcher** | — | entire surface | — |
| Commands | **no dispatcher** | — | entire surface | — |
| Security | **no dispatcher** | — | entire surface | — |
| DX | **no dispatcher** | — | entire surface | — |
| Cpx | **no dispatcher** | — | entire surface | — |

**Reflection / AOT compliance:**
- `Core/ReflectionDispatchTable.cs:34-41` calls `MethodInfo.Invoke` — listed in `src/BannedSymbols.txt`.
- `Core/LocalCoClass.cs:364-389,401-406,430-445` uses `Activator.CreateInstance(...)` — banned.
- `Core/ComOxidRuntime.cs:100-117`, `ComOxidRuntimeHelper.cs:52-88,105-111`, `Transport/ComRuntimeEndpoint.cs:46-89` are legacy runtime glue, not source-generated dispatch.

**Phase 4A intent** (per `.github/copilot-instructions.md`): replace reflection-based dispatch with source-generated dispatch. The proxy generator exists but a **server-dispatch generator** is not implemented — only client proxies are emitted. **BLOCKER** for AOT cleanliness on the server side.

---

## Conclusion — Ordered Priority for 1.0

| # | Item | Effort | Dependency |
|---|---|---|---|
| 1 | **Implement `IRemoteSCMActivator` v5.6 + `RemoteCreateInstance` + `RemoteGetClassObject` server paths** | M (2-3 wks) | DCOM wire format primitives (done) |
| 2 | **Generate server-side dispatch table** (mirror `OpcProxyGenerator`) — emit per-interface `OpcXxxServerDispatcher.DispatchAsync` from `[OpcMethod]` declarations | L (3-4 wks) | banned-symbol-clean replacement for `ReflectionDispatchTable` |
| 3 | **Fix DCOM ping cadence to 80 s** + add throttling | S (days) | none |
| 4 | **Emit `ORPC_THIS` envelope around every generated proxy call** (and parse `ORPC_THAT` on response) | S-M | generator change |
| 5 | **NTLMv2 MIC + RFC 5056 channel binding** in NTLMv2 temp | M | `NtlmAuthentication.cs`, `Responses.cs` |
| 6 | **Kerberos `gss_wrap`/`gss_unwrap`/`gss_get_mic`/`gss_verify_mic`** implementation (RC4-HMAC + AES128/256) | L | `KerberosAuthContext.cs` |
| 7 | **SPNEGO `mechListMIC` verification + NegTokenResp encode** | S | `SpnegoDecoder.cs` |
| 8 | **VARIANT/SAFEARRAY full surface** — `VT_VARIANT`, `VT_BYREF`, multi-dim, `FADF_*` features | M | `NdrVariantExtensions.cs`, `NdrSafeArrayExtensions.cs` |
| 9 | **Declare + implement missing DA methods**: `IOPCServer::AddGroup`/`GetGroupByName`/`CreateGroupEnumerator`, `IOPCSyncIO::Read`, `IOPCAsyncIO2::Read/Write`, `IOPCBrowse::Browse`, all `IOPCItemProperties/Deadband/Sampling`, all `IOPCAsyncIO3` | L | generator multi-out + interface-pointer codec |
| 10 | **Declare + implement missing HDA methods**: `IOPCHDA_Browser::*`, all `Insert/Replace/InsertReplace/DeleteRaw/DeleteAtTime` on `Sync/AsyncUpdate`, `IOPCHDA_DataCallback` | M | codec extensions |
| 11 | **`OBJREF_HANDLER` / `OBJREF_CUSTOM` parsing** | S | `InterfacePointerBody.cs` |
| 12 | **`OpcEnumClient` real implementation** against remote `OPCEnum.exe` (CLSID `13486D51-4821-11D2-A494-3CB306C10000`) | M | DCOM activation (#1) |
| 13 | **OPCGEN001/002/003/007/009/010 diagnostics wired** | S | generator pass |
| 14 | **OPC CTT replay test project** | L | requires running CTT against managed loopback |
| 15 | **Verify `IOPCServer::GetStatus` opnum (3 vs 6)** — likely incorrect in `Da/Dcom/IOPCInterfaces.cs:37` | XS — but **HIGH** correctness impact |

**Strict ordering for a 1.0 DCOM-compatible client:** 15 → 4 → 3 → 5 → 9 → 8 → 11 → 7.
**Strict ordering for a 1.0 DCOM-compatible server:** 1 → 2 → 6 → 5 → 8 → 10 → 12.

The DA + HDA + AE surfaces together account for 80 % of OPC Classic deployments; Batch / Commands / DX / Cpx server-side can ship later.

---
*Authored programmatically by review pass on commit `3a85307` (master). Numbers reflect `[OpcMethod]` attribute counts and dispatcher routing tables as of that commit.*

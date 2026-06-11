# Architecture diagrams

This page collects the architectural diagrams for `Opc.Classic.*`: source-generated client proxies and server dispatchers, `ICallChannel` with in-memory and DCOM implementations, channel-level NTLM/Kerberos/SPNEGO/CBT, NativeAOT-compatible libraries, and coverage across DA, AE, HDA, Batch, Commands, Security, DX, Cpx, and Discovery.

## High-level architecture

This diagram shows the portable OPC Classic path at the level most adopters encounter first. A client application talks to managed `Opc.Classic.*` facades and projection packages for DA, AE, HDA, Batch, Commands, Security, DX, Cpx, and Discovery instead of creating Windows COM runtime callable wrappers.

The facade delegates DCOM-shaped calls to generator-emitted proxies such as `IOPCServerClientProxy`. Those proxies use compile-time interface IDs, opnums, and codec emitters to produce NDR request payloads and decode NDR response payloads. Server hosting uses generated dispatchers for the matching interface/opnum tables.

`ICallChannel` is the seam between generated call shims and transport. Its implementations include `DcomCallChannel` for DCE/RPC over a pipelines-backed `IAsyncTransport` and `InMemoryCallChannel` for managed loopback. The DCOM channel binds to an interface, wraps payloads in DCE/RPC PDUs, applies channel-level authentication protection, and exchanges frames over the selected RPC sequence: direct TCP (`ncacn_ip_tcp`) through `TcpClientTransport` or named-pipe RPC (`ncacn_np`) through `NcacnNpTransport`. Direct TCP callers use `DcomCallChannelFactory.ConnectTcpAsync`; managed servers accept peers with `OpcServerListener` and route object IPIDs through `OpcObjectRegistry`.

```mermaid
flowchart TB
    App["Client app"]
    Facade["Opc.Classic.* facade<br/>DA AE HDA Batch Cmd Sec DX Cpx Discovery"]
    Proxy["Generator emitted proxy<br/>for example IOPCServerClientProxy"]
    Channel["ICallChannel.InvokeAsync<br/>Dcom or InMemory"]
    Dcom["DcomCallChannel"]
    Ndr["NDR encoder and decoder<br/>NdrWriter and NdrReader"]
    TcpClient["IAsyncTransport<br/>TcpClientTransport or NcacnNpTransport"]
    Tcp["RPC bytes<br/>ncacn_ip_tcp or ncacn_np"]
    Listener["OpcServerListener<br/>RpcServerConnectionProcessor"]
    Server["OPC Classic server<br/>generated dispatchers"]

    App --> Facade
    Facade --> Proxy
    Proxy --> Channel
    Channel --> Dcom
    Dcom --> Ndr
    Ndr --> TcpClient
    TcpClient --> Tcp
    Tcp --> Listener
    Listener --> Server
    Server --> Listener
    Listener --> Tcp
    Tcp --> TcpClient
    TcpClient --> Ndr
    Ndr --> Dcom
    Dcom --> Channel
    Channel --> Proxy
    Proxy --> Facade
    Facade --> App
```

### Where to read more

- `IDaServer`, `IAeServer`, and `IHdaServer` define the managed facade shapes.
- `ICallChannel` defines the transport-agnostic generated-proxy contract.
- `DcomCallChannel` implements `ICallChannel` over DCE/RPC PDUs.
- `InMemoryCallChannel` implements the managed loopback channel.
- `NdrWriter` and `NdrReader` are the span-based NDR primitives.
- `IAsyncTransport` describes the pipelines-backed transport contract.
- `TcpClientTransport` and `DcomCallChannelFactory` implement the direct TCP client path.
- `OpcServerListener` and `RpcServerConnectionProcessor` implement the managed listener path.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L35-L63) and [`docs\ADOPTION.md`](../ADOPTION.md#L37-L79).

## Call shim flow

This sequence follows one outbound generated-proxy call, using `IOPCServer::GetStatus` as the example. The app calls the generated `GetStatusAsync` method and remains unaware of interface IDs, DCE/RPC opnums, PDU framing, or authentication verifiers.

The generated proxy body is emitted at compile time. It rents a buffer, writes request parameters with the generator codec table, calls `ICallChannel.InvokeAsync`, checks the returned HRESULT, and decodes response bytes through `NdrReader` before returning a managed `OpcServerStatus`.

`DcomCallChannel` owns the transport side of the call. It ensures a presentation context for the interface, builds a `RequestCoPdu`, writes it to the connected `IAsyncTransport` (for example `TcpClientTransport` from `DcomCallChannelFactory.ConnectTcpAsync` or `NcacnNpTransport` for SMB named pipes), reads one or more response fragments, and returns an `NdrCallResult` to the generated shim.

### ORPC envelope

Generated proxies pass only the method payload to `ICallChannel`. The DCOM channel adds the required [MS-DCOM] ORPC envelope centrally:

```text
RequestCoPdu.Stub  = ORPC_THIS  + method request payload
ResponseCoPdu.Stub = ORPC_THAT  + method response payload
```

`ORPC_THIS` carries COMVERSION 5.7, flags `0`, a causality GUID shared through `CausalityContext`, and a normally-null extension pointer. `ORPC_THAT` is read and stripped before the generated proxy decodes the response payload.

```mermaid
sequenceDiagram
    autonumber
    participant App as Client app
    participant Proxy as IOPCServerClientProxy
    participant Codecs as Codec table
    participant Ndr as NDR writer reader
    participant Channel as ICallChannel
    participant Dcom as DcomCallChannel
    participant Transport as IAsyncTransport
    participant Rpc as RPC PDU stream
    participant Server as OPC DA server

    App->>Proxy: GetStatusAsync()
    Proxy->>Codecs: Resolve method opnum and response codec
    Proxy->>Ndr: Encode request payload
    Proxy->>Channel: InvokeAsync(interfaceId, opnum 6, payload)
    Channel->>Dcom: Dispatch call to DCOM channel
    Dcom->>Dcom: Ensure presentation context
    Dcom->>Transport: Write RequestCoPdu with NDR stub
    Transport->>Rpc: Send ncacn_ip_tcp bytes
    Rpc->>Server: Deliver IOPCServer GetStatus request
    Server-->>Rpc: ResponseCoPdu with status stub
    Rpc-->>Transport: Return response fragments
    Transport-->>Dcom: Read response fragments
    Dcom-->>Channel: NdrCallResult HRESULT plus payload
    Channel-->>Proxy: Return result
    Proxy->>Ndr: Decode OpcServerStatus
    Proxy-->>App: Return managed status
```

### Where to read more

- `IOPCInterfaces` defines `IOPCServer::GetStatus` with `[OpcMethod(6)]`.
- `OpcProxyGenerator` emits marshalled `InvokeAsync` bodies.
- `OpcProxyGenerator` emits codec writes and response reads from the generator codec table.
- `ICallChannel` is the generated-shim call seam.
- `DcomCallChannel` sends the DCE/RPC request and maps response or fault PDUs into `NdrCallResult`.
- `TcpClientTransport` and `DcomCallChannelFactory` are the direct TCP transport entry points.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L157-L168) and [`docs\ADOPTION.md`](../ADOPTION.md#L39-L79).

## Server dispatch flow

This diagram shows the server-side mirror of a generated client proxy. A TCP connection reaches `OpcServerListener`, the per-connection `RpcServerConnectionProcessor` reads DCE/RPC PDUs, and the request's interface ID, opnum, and optional object IPID route to the right generated dispatcher.

`OpcDaServerDispatcher.DispatchAsync` is the DA hosting adapter. It recognizes `IOPCServer.InterfaceId` and delegates to the source-generated `IOPCServerServerDispatcher`. For group, item, and enumerator calls that carry `PFC_OBJECT_UUID`, `OpcObjectRegistry` resolves the IPID to the per-object dispatcher map before the adapter/generator handoff.

The same adapter plus generated-dispatcher pattern exists for AE and HDA hosting, and generated dispatchers cover annotated `Opc.Classic.*` DCOM projections across the sub-spec packages. Keeping dispatch generated and explicit preserves OPC IDL opnums while avoiding reflection-based invocation in the portable server path.

```mermaid
sequenceDiagram
    autonumber
    participant Client as Remote client
    participant Tcp as TCP transport
    participant Listener as OpcServerListener
    participant Processor as RpcServerConnectionProcessor
    participant Registry as OpcObjectRegistry
    participant Adapter as OpcDaServerDispatcher
    participant Dispatcher as IOPCServerServerDispatcher
    participant Impl as IOpcDaServer implementation
    participant Ndr as NDR reader writer

    Client->>Tcp: RequestCoPdu
    Tcp->>Listener: Accepted IAsyncTransport
    Listener->>Processor: ProcessConnectionAsync
    Processor->>Processor: Read bind or request PDU
    alt Request has object IPID
        Processor->>Registry: TryGetDispatcher(ipid, interfaceId)
        Registry-->>Processor: Per-object dispatcher
    else Root server request
        Processor->>Processor: Use root dispatcher map
    end
    Processor->>Adapter: DispatchAsync(interfaceId, opnum, payload)
    Adapter->>Dispatcher: DispatchAsync(opnum, payload)
    Dispatcher->>Dispatcher: Switch on generated opnums
    alt GetStatus opnum 6
        Dispatcher->>Impl: GetStatusAsync()
        Impl-->>Dispatcher: OpcServerStatus
        Dispatcher->>Ndr: NdrOpcServerStatusCodec.Write
    else RemoveGroup opnum 7
        Dispatcher->>Ndr: Read serverHandle and force
        Dispatcher->>Impl: RemoveGroupAsync(serverHandle, force)
        Impl-->>Dispatcher: Completed
    end
    Ndr-->>Dispatcher: DispatchResult payload
    Dispatcher-->>Adapter: DispatchResult
    Adapter-->>Processor: NdrCallResult
    Processor-->>Tcp: ResponseCoPdu or fault PDU
    Tcp-->>Client: Reply
```

### Where to read more

- `OpcServerListener` owns the TCP accept loop for managed servers.
- `RpcServerConnectionProcessor` reads PDUs and routes requests to dispatchers.
- `OpcObjectRegistry` maps IPIDs to per-object dispatcher sets.
- `OpcDaServerDispatcher` defines the DA adapter that delegates to the generated dispatcher.
- `OpcServerDispatchGenerator` emits the generated opnum switch.
- `OpcServerDispatchGenerator` emits request decoding, implementation calls, and response encoding.
- `IOpcDaServer` is the managed implementation contract the dispatcher calls.
- `OpcAeServerDispatcher` and `OpcHdaServerDispatcher` follow the same adapter shape for AE and HDA.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L170-L200).

## NTLM handshake

This sequence shows the NTLMSSP handshake used by the DCOM authentication context. The client starts with a NEGOTIATE message, the server returns a CHALLENGE, and the client completes the exchange with an AUTHENTICATE message carrying NTLMv2 responses and negotiated flags.

`IAuthContext` gives `DcomCallChannel` a mechanism-neutral API: build the first bind token, process the server token, and then sign or seal later PDU bodies. The NTLM implementation adapts those calls to `Type1Message`, `Type2Message`, `Type3Message`, and the session-security object that signs and verifies DCE/RPC verifiers.

The diagram also includes the channel binding token path required by Extended Protection for Authentication. The shared `ChannelBindingsFactory` builds `tls-server-end-point` application data, and `ChannelBindingsHash` computes the RFC 2744 MD5 hash used by [MS-NLMP] for the `MsvAvChannelBindings` AV pair.

```mermaid
sequenceDiagram
    autonumber
    participant Client as DCOM client
    participant Cbt as Channel binding helper
    participant Auth as NtlmAuthContext
    participant Channel as DcomCallChannel
    participant Server as DCOM server

    Client->>Cbt: Read TLS server certificate
    Cbt->>Cbt: Build tls-server-end-point data
    Cbt->>Cbt: Compute RFC 2744 MD5 CBT hash
    Client->>Auth: BuildInitialToken()
    Auth-->>Client: NTLMSSP NEGOTIATE Type1
    Client->>Channel: Bind PDU with NEGOTIATE
    Channel->>Server: RPC bind plus auth verifier
    Server-->>Channel: bind_ack with NTLMSSP CHALLENGE Type2
    Channel-->>Auth: ProcessChallengeToken(challenge)
    Auth->>Auth: Create NTLMv2 proof using challenge and optional CBT
    Auth-->>Channel: NTLMSSP AUTHENTICATE Type3
    Channel->>Server: auth3 PDU with AUTHENTICATE
    Server-->>Channel: Authentication complete
    Channel->>Auth: SignAndSeal later request PDUs
    Server-->>Auth: VerifyAndUnseal response PDUs
```

### Where to read more

- `IAuthContext` defines the authentication seam used by DCOM channels.
- `NtlmAuthentication` adapts NTLM to `IAuthContext`, including `BuildInitialToken`, `ProcessChallengeToken`, `SignAndSeal`, and `VerifyAndUnseal`.
- `Type1Message`, `Type2Message`, and `Type3Message` model the three NTLMSSP messages.
- `ChannelBindingsFactory` and `ChannelBindingsHash` implement CBT construction and hashing.
- Protocol references: [MS-NLMP](https://learn.microsoft.com/openspecs/windows_protocols/ms-nlmp/) and [`docs\cookbook\05-dcom-hardening-pkt-integrity-explainer.md`](../cookbook/05-dcom-hardening-pkt-integrity-explainer.md#L49-L53).

## Kerberos handshake

This diagram shows the Kerberos path used when DCOM authentication is backed by Kerberos and SPNEGO. The client acquires a service ticket for the configured SPN, emits an AP-REQ in a GSS-API token, and requests mutual authentication so the server must answer with AP-REP.

`KerberosConnectionContext` owns the ticket acquisition and AP-REP processing state. `KerberosAuthContext` wraps that state behind `IAuthContext`, computes optional channel-binding hashes, wraps the AP-REQ in a SPNEGO initial token, and unwraps the server's response token from `NegTokenResp`.

The final packet-protection stage uses RFC 4121 GSS-API wrap and MIC tokens through `KerberosSession`. `KerberosAuthContext.SignAndSeal` and `VerifyAndUnseal` call that session for DCE/RPC packet integrity or privacy after AP-REQ/AP-REP establishes the key.

```mermaid
sequenceDiagram
    autonumber
    participant App as Client app
    participant Krb as KerberosConnectionContext
    participant Kdc as KDC
    participant Spnego as SPNEGO wrapper
    participant Channel as DcomCallChannel
    participant Server as DCOM server

    App->>Krb: AcquireApRequestAsync(CBT hash)
    Krb->>Kdc: Authenticate client credential
    Kdc-->>Krb: TGT or credential context
    Krb->>Kdc: Request service ticket for SPN
    Kdc-->>Krb: Service ticket and AP-REQ state
    Krb-->>Spnego: GSS-API AP-REQ with mutual flag
    Spnego-->>Channel: NegTokenInit carrying AP-REQ
    Channel->>Server: RPC bind with AP-REQ token
    Server-->>Channel: bind_ack with NegTokenResp AP-REP
    Channel->>Spnego: Decode NegTokenResp
    Spnego->>Krb: Process AP-REP response token
    Krb-->>App: Derived session key
    App->>Channel: Invoke protected DCOM calls
    Channel->>Server: RFC 4121 MIC or Wrap token protected PDU
    Server-->>Channel: RFC 4121 protected response
```

### Where to read more

- `KerberosAuthContext` adapts Kerberos/SPNEGO tokens to `IAuthContext`, and `KerberosAuthContext` applies packet protection.
- `KerberosConnectionContext` acquires AP-REQ tokens, requests mutual authentication, and processes AP-REP tokens.
- `IKerberosConnectionContext` defines the AP-REQ and AP-REP abstraction.
- `SpnegoTokenBuilder` wraps Kerberos AP-REQ tokens in SPNEGO.
- `KerberosSession` implements RFC 4121 wrap and unwrap tokens.
- Protocol references: [MS-KILE](https://learn.microsoft.com/openspecs/windows_protocols/ms-kile/) and [`docs\cookbook\03-kerberos-in-active-directory.md`](../cookbook/03-kerberos-in-active-directory.md#L36-L52).

## SPNEGO negotiation

This sequence focuses on the SPNEGO wrapper rather than the underlying Kerberos or NTLM mechanism. The initiator sends `NegTokenInit` with an ordered mechanism list and, when Kerberos is preferred, an optimistic AP-REQ mechanism token.

The acceptor selects one mechanism and returns `NegTokenResp`. That response can carry `accept-incomplete` plus a mechanism response token, `accept-completed`, `request-mic`, or `reject`, matching the RFC 4178 negotiation state model.

In Opc.Classic, `SpnegoTokenBuilder` currently builds the common Kerberos-first, NTLMSSP-fallback initial token. `SpnegoEncoder` and `SpnegoDecoder` handle the DER shapes, while `KerberosAuthContext` feeds decoded response tokens back into the Kerberos AP-REP processing path.

```mermaid
sequenceDiagram
    autonumber
    participant Client as Initiator
    participant Builder as SpnegoTokenBuilder
    participant Encoder as SpnegoEncoder
    participant Server as Acceptor
    participant Decoder as SpnegoDecoder
    participant Mech as Selected mechanism

    Client->>Builder: Build init token with AP-REQ
    Builder->>Encoder: NegTokenInit(mechTypes, mechToken, mechListMic)
    Encoder-->>Client: InitialContextToken for SPNEGO
    Client->>Server: NegTokenInit with Kerberos and NTLMSSP OIDs
    Server->>Server: Select supported mechanism
    alt Kerberos selected
        Server-->>Client: NegTokenResp supportedMech Kerberos plus AP-REP
        Client->>Decoder: DecodeNegTokenResp
        Decoder->>Mech: Pass responseToken to Kerberos
    else NTLMSSP selected
        Server-->>Client: NegTokenResp supportedMech NTLMSSP plus challenge
        Client->>Decoder: DecodeNegTokenResp
        Decoder->>Mech: Pass responseToken to NTLM
    else MIC required
        Server-->>Client: NegTokenResp request-mic
        Client->>Server: NegTokenResp with mechListMic
    end
    Server-->>Client: accept-completed or reject
```

### Where to read more

- `SpnegoNegTokenInit` models RFC 4178 `NegTokenInit`.
- `SpnegoNegTokenResp` models `NegTokenResp`.
- `SpnegoOids` defines the SPNEGO, Kerberos, and NTLMSSP OIDs.
- `SpnegoEncoder` and `SpnegoDecoder` encode and decode the DER tokens.
- Protocol references: [RFC 4178](https://www.rfc-editor.org/rfc/rfc4178) and [MS-SPNG](https://learn.microsoft.com/openspecs/windows_protocols/ms-spng/).

## Discovery flow

This diagram shows current OPC Classic server discovery. `IOpcDiscovery` is the shared async contract, and `OpcDiscoveryFactory` composes multiple discovery strategies, isolates transport or authorization failures per strategy, and de-duplicates results by CLSID.

The OPCEnum path activates the standard `OPC.ServerList.1` DCOM server, prefers `IOPCServerList2` when available, uses `ICallChannel` shims for `EnumClassesOfCategories`, `GetClassDetails`, and `IOPCEnumGUID::Next`, and maps descriptors to `OpcServerEntry` values.

The remote-registry path enumerates OPC category registrations from a target machine's registry through the managed WINREG reader over SMB/ncacn_np. It returns `OpcServerEntry` values from CLSID, ProgID, friendly-name, and category metadata, while `OpcDiscoveryFactory` isolates per-strategy failures.

```mermaid
sequenceDiagram
    autonumber
    participant App as Client app
    participant Factory as OpcDiscoveryFactory
    participant OpcEnum as OpcEnumClient
    participant ServerList as OPC.ServerList.1
    participant Registry as RemoteRegistryEnum
    participant WinReg as Remote registry

    App->>Factory: DiscoverAsync(host)
    Factory->>OpcEnum: DiscoverAsync(host)
    OpcEnum->>ServerList: Activate OPC.ServerList.1
    OpcEnum->>ServerList: IOPCServerList2 or IOPCServerList EnumClassesOfCategories
    ServerList-->>OpcEnum: CLSIDs for OPC categories
    OpcEnum->>ServerList: GetClassDetails for each CLSID
    ServerList-->>OpcEnum: ProgID friendly name and CLSID
    OpcEnum-->>Factory: OpcServerEntry stream

    Factory->>Registry: DiscoverAsync(host)
    Registry->>WinReg: Open HKLM classes hive
    Registry->>WinReg: Enumerate Component Categories implementations
    WinReg-->>Registry: CLSID ProgID friendly name category IDs
    Registry-->>Factory: OpcServerEntry stream

    Factory->>Factory: De-duplicate by CLSID
    Factory-->>App: Discovered server entries
```

### Where to read more

- `IOpcDiscovery` defines the shared async discovery contract.
- `OpcDiscoveryFactory` composes strategies and de-duplicates by CLSID; `OpcDiscoveryFactory` isolates per-strategy failures.
- `OpcEnumClient` activates OPCEnum and selects the server-list interface; `OpcEnumClient` maps server-list results into descriptors.
- `OpcEnumDcomInterfaces` contains the OPCEnum `ICallChannel` shims for `IOPCServerList` and `IOPCServerList2`.
- `IOPCInterfaces` defines `IOPCEnumGUID`, `IOPCServerList`, and `IOPCServerList2` projections.
- `RemoteRegistryEnum` enumerates registry entries, and `RemoteRegistryEnum` reads category and CLSID metadata.
- `WinRegClient`, `RemoteRegistryEnum`, and `NcacnNpTransport` show the `ncacn_np` / `\\PIPE\\winreg` path.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L216-L236) and [`docs\ADOPTION.md`](../ADOPTION.md#L242-L280).

## Source generator pipeline

This flowchart shows the compile-time path for source-generated OPC projections. Source interfaces declare their DCOM identity and method opnums with `[OpcInterface]`, `[GenerateOpcProxy]`, `[OpcGenerateServerDispatch]`, and `[OpcMethod]` attributes.

Roslyn runs the generator package during compilation. `OpcInterfaceGenerator` emits interface metadata such as `InterfaceId` and nested `Opnums`; `OpcProxyGenerator` inspects annotated interfaces, validates supported method shapes, and maps parameter and return types through its codec table; and `OpcServerDispatchGenerator` emits server-side dispatchers for the same opnum tables. The current projection set covers 49 generated dispatchers and 228 annotated OPC opnums.

The outputs are generated partial client proxy classes and server dispatcher classes. Proxy method bodies allocate NDR buffers, emit direct codec calls, call `ICallChannel.InvokeAsync`, check HRESULTs, decode response payloads, and return managed values. Dispatcher bodies switch on generated opnum constants, decode request payloads, call managed implementations, and encode response payloads without runtime reflection or expression-tree compilation.

```mermaid
flowchart LR
    Source["Source interface<br/>partial IOPCServer"]
    Attributes["Attributes<br/>OpcInterface<br/>GenerateOpcProxy<br/>OpcGenerateServerDispatch<br/>OpcMethod"]
    Roslyn["Roslyn analyzer pipeline<br/>incremental generator"]
    InterfaceGen["OpcInterfaceGenerator<br/>InterfaceId and Opnums"]
    ProxyGen["OpcProxyGenerator"]
    DispatchGen["OpcServerDispatchGenerator"]
    Registry["CodecRegistry lookup<br/>Codecs table"]
    ProxyBody["Generated partial proxy<br/>InvokeAsync method bodies"]
    DispatchBody["Generated server dispatcher<br/>opnum switch bodies"]
    Compile["Consumer compilation"]

    Source --> Attributes
    Attributes --> Roslyn
    Roslyn --> InterfaceGen
    Roslyn --> ProxyGen
    Roslyn --> DispatchGen
    ProxyGen --> Registry
    DispatchGen --> Registry
    InterfaceGen --> ProxyBody
    InterfaceGen --> DispatchBody
    Registry --> ProxyBody
    Registry --> DispatchBody
    ProxyBody --> Compile
    DispatchBody --> Compile
```

### Where to read more

- `IOPCInterfaces` shows an annotated `IOPCServer` projection with proxy and server-dispatch generation enabled.
- `OpcInterfaceGenerator` defines the generated attributes and `OpcInterfaceGenerator` entry point.
- `OpcProxyGenerator` defines the proxy generator and its generated `[GenerateOpcProxy]` attribute.
- `OpcProxyGenerator` is the generator codec table.
- `OpcProxyGenerator` emits generated `InvokeAsync` bodies.
- `OpcServerDispatchGenerator` emits generated dispatcher classes, and `OpcServerDispatchGenerator` emits their opnum switches.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L135-L168).

## Subscription data flow

This sequence shows the OPC DA subscription lifecycle. A client creates a group with `AddGroup`, adds items, activates them, and then receives batched `IOPCDataCallback::OnDataChange` notifications as values change or keep-alive heartbeats are emitted.

The managed public model names the server-side group as an `IDaSubscription`. Its `AddItemsAsync`, `SetActiveStateAsync`, `RefreshAsync`, and `DataChanges` stream map the COM subscription pattern into async .NET shapes.

On the hosting side, user code produces `OpcDaDataChange` batches and `OpcDaDataChangePublisher` fans those batches out to advised callback subscribers. `IOpcDataCallbackSink` is the unified server-side callback abstraction: cross-platform DCOM sinks marshal the payload back over the managed transport, while the Windows SCM CCW path uses `OpcDataCallbackProxy` to invoke the client-supplied COM vtable. The DCOM projection for `IOPCDataCallback` defines the wire-facing `OnDataChangeAsync` callback with transaction ID, group handle, values, qualities, timestamps, and per-item HRESULTs.

```mermaid
sequenceDiagram
    autonumber
    participant App as Client app
    participant Server as DA server facade
    participant Sub as IDaSubscription group
    participant Items as IOPCItemMgt
    participant Sampler as Server sampling loop
    participant Publisher as OpcDaDataChangePublisher
    participant Sink as IOpcDataCallbackSink
    participant Callback as IOPCDataCallback

    App->>Server: AddGroup or CreateSubscriptionAsync
    Server-->>App: Server group handle
    App->>Sub: AddItemsAsync(items)
    Sub->>Items: IOPCItemMgt AddItems
    Items-->>Sub: Per item handles and results
    App->>Sub: SetActiveStateAsync(handles, true)
    Sub->>Items: IOPCItemMgt SetActiveState
    Items-->>Sub: Per item HRESULTs
    App->>Sub: Advise callback sink
    Sub->>Sink: Register IOpcDataCallbackSink
    Sampler->>Sampler: Poll or sample active items
    Sampler->>Publisher: PublishAsync(OpcDaDataChange)
    Publisher->>Sink: OnDataChange(payload)
    alt Cross-platform DCOM sink
        Sink->>Callback: OnDataChangeAsync over managed transport
    else Windows SCM CCW sink
        Sink->>Callback: OpcDataCallbackProxy invokes COM vtable
    end
    Callback-->>App: Deliver DataChange stream item
    App->>Sub: RefreshAsync(optional)
    Sub->>Publisher: Force OnDataChange for active items
```

### Where to read more

- `SubscriptionState` describes DA group and subscription state, including active state and keep-alive.
- `IDaServer` creates managed DA subscriptions.
- `IDaSubscription` maps DA groups to async subscription operations and a `DataChanges` stream.
- `IOPCInterfaces` defines item management methods including `SetActiveState`.
- `IOPCInterfaces` defines `IOPCDataCallback::OnDataChange`.
- `IOpcDaDataChangePublisher` and `OpcDaDataChangePublisher` implement callback fan-out.
- `IOpcDataCallbackSink` is the unified callback sink abstraction.
- `OpcDataCallbackProxy` implements the Windows CCW callback sink.

## AOT and trimming shape

This diagram shows what the trimmer and NativeAOT compiler see in the portable * libraries. The design goal is static, analyzable code: generated proxies and dispatchers call known methods and codecs directly rather than reflecting over interface metadata at runtime.

`Directory.Build` enables AOT and trimming analyzers for source projects, and `BannedSymbols` blocks the dynamic patterns that would hide code from the trimmer. The Roslyn generator assembly itself is build-time only, so it opts out of AOT properties while keeping its emitted output AOT-safe.

`Opc.Classic.Dcom` participates in the current AOT shape. Its channel-level DCOM transport, packet protection, source-generated shims and dispatchers, and explicit codecs keep the runtime path statically visible to analyzers and NativeAOT.

```mermaid
flowchart TD
    App["Consumer app<br/>PublishAot true"]
    Props["src Directory.Build.props<br/>IsAotCompatible true<br/>IsTrimmable true"]
    Analyzer["Trim and AOT analyzers"]
    Banned["BannedSymbols.txt<br/>no Reflection.Emit<br/>no MethodInfo.Invoke<br/>no ComImport"]
    SourceGen["Roslyn source generators<br/>build time only"]
    Proxies["Generated proxies<br/>static InvokeAsync bodies"]
    Codecs["Explicit codec calls<br/>NdrWriter and NdrReader"]
    Dispatch["Generated dispatchers<br/>opnums are constants"]
    Dcom["Opc.Classic.Dcom<br/>AOT-compatible DCOM channel"]
    Canary["AOT canary sample<br/>publish smoke test"]

    App --> Props
    Props --> Analyzer
    Analyzer --> Banned
    SourceGen --> Proxies
    SourceGen --> Dispatch
    Proxies --> Codecs
    Dispatch --> Codecs
    Codecs --> Analyzer
    Dcom --> Analyzer
    Props --> Canary
    Canary --> App
```

### Where to read more

- `Directory.Build` sets `IsAotCompatible`, `IsTrimmable`, and analyzer properties for source assemblies.
- `BannedSymbols` lists banned reflection, expression compilation, COM RCW, and native marshal patterns.
- `Opc.Classic.Generators` explains why generators are build-time only and why their output must be AOT-safe.
- `Opc.Classic.Dcom` defines the pure-managed DCOM assembly identity, and `DcomCallChannel` shows channel-level packet protection.
- AotCanary sample and `Program` show the AOT smoke sample.
- See also [`docs\ARCHITECTURE.md`](../ARCHITECTURE.md#L281-L292) and [`docs\ADOPTION.md`](../ADOPTION.md#L302-L312).

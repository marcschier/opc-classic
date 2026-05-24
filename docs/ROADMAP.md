# Opc.Classic Roadmap

This document tracks what's planned beyond the current release. For implemented features, see [CHANGELOG](../CHANGELOG.md).

Current release: **0.6.0-alpha.1**. The next release tag is **1.0.0-rc.1**.

## 1.0.0 (release candidate)

Items required to ship 1.0.0 GA:

- Phase 14D bidirectional compat matrix GREEN (managed↔native interop on Windows runner)
- OPC Compliance Test Tool (CTT) pass
- NTLMv2 wire verification against real Windows Server
- nuget.org publish workflow

Release-candidate administration:

- Finish the public XML documentation audit.
- Tag and publish `1.0.0-rc.1` after the documentation IA refresh and release notes are complete.
- Tag `1.0.0` after the GA gates above are green.

## 2.0.0 and beyond

- Third-party crypto/security audit of NTLMSSP implementation
- Full `<Nullable>enable</Nullable>` migration on Opc.Classic.Dcom
- Additional spec extensions (Web-DA, Compliance 2.0 if/when published)
- Broader generated coverage for COM interface-pointer returns and enumerator-producing methods
- More native-server interoperability fixtures for vendor-specific DA/AE/HDA behavior
- Expanded XML-DA serializer coverage for uncommon SOAP payload shapes

## Known coverage gaps

Generated client and server DCOM projections cover the main DA/AE/HDA, Batch, Commands, Cpx, DX, Security, and Discovery paths. The current gaps are concentrated in methods that return COM interface pointers, legacy/deprecated OPC surfaces, and payload shapes that still need explicit codecs.

### IDL methods not yet declared in generated interfaces

- **DA:** `IOPCServer.AddGroup`, `GetGroupByName`, `CreateGroupEnumerator`; `IOPCBrowseServerAddressSpace.BrowseOPCItemIDs`, `BrowseAccessPaths`; `IOPCItemMgt.CreateEnumerator`; `IOPCGroupStateMgt.CloneGroup`; `IOPCGroupStateMgt2.SetKeepAlive`, `GetKeepAlive`; `IOPCSyncIO2.ReadMaxAge`; `IConnectionPointContainer` and `IConnectionPoint` interface-pointer operations; `IOPCEnumGUID.Clone`.
- **DA legacy/deprecated:** `IOPCAsyncIO.Read`, `Write`; `IEnumOPCItemAttributes.Next`, `Skip`, `Reset`, `Clone`; `IOPCPublicGroupStateMgt.GetState`, `MoveToPublic`; `IOPCServerPublicGroups.GetPublicGroupByName`, `RemovePublicGroup`; `IOPCCommon` locale/client-name methods; `IOPCShutdown.ShutdownRequest`.
- **AE:** `IOPCEventServer.CreateEventSubscription`, `QueryEventCategories`, `QueryEventAttributes`, `TranslateToItemIDs`, `CreateAreaBrowser`; `IOPCEventServer2.GetEnableStateByArea`, `GetEnableStateBySource`; `IOPCEventSubscriptionMgt.GetFilter`, `GetState`, `SetState`; `IOPCEventAreaBrowser.BrowseOPCAreas`.
- **HDA:** `IOPCHDA_Server.CreateBrowse`.
- **Batch:** `IOPCBatchServer.CreateEnumerator`; `IOPCBatchServer2.CreateFilteredEnumerator`; `IEnumOPCBatchSummary.Clone`; `IOPCEnumerationSets.QueryEnumerationSets`.
- **Commands:** the IDL spells the callback interface as `IOPCComandCallback`; the managed projection uses `IOPCCommandCallback`. Keep this spelling difference visible in generated metadata and compatibility tests.
- **DX:** `IOPCConfiguration.GetServers`, `AddServers`, `ModifyServers`, `DeleteServers`, `CopyDefaultServerAttributes`, `AddDXConnections`, `UpdateDXConnections`, `ModifyDXConnections`, `CopyDXConnectionDefaultAttributes`.
- **Discovery:** generated `IOPCServerList`/`IOPCServerList2` omit `EnumClassesOfCategories` and `GetClassDetails`; `OpcEnumClient` currently covers those calls with hand-written codecs.

### Codec and runtime gaps

- COM interface-pointer return codecs are needed for enumerators, browse objects, event subscriptions, class factories, and connection points.
- Additional multi-out record generation is needed for AE catalog/state calls, Batch enumeration-set discovery, and DX configuration record arrays.
- Complex Data dictionary, type-description, binary, and XML payload codecs remain limited to the current metadata projections.
- `IOPCItemMgt.AddItems`, `IOPCItemMgt.ValidateItems`, `IOPCGroupStateMgt.SetState`, and `IOPCSyncIO.Read` have generated declarations but still need concrete call-shim/server adapter completion for all payload shapes.

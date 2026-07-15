# Opc.Classic.Mcp tool reference

This reference is maintained from the MCP tool metadata in Tools compatibility helpers: `[McpServerTool]` names, `[Description]` text, and public method signatures.

| Area | Coverage |
| --- | --- |
| Session | Session lifecycle tools |
| Discovery | OPCEnum / OPC.ServerList discovery |
| DA | Data Access tools |
| AE | Alarms & Events tools |
| HDA | Historical Data Access tools |
| Batch | Batch model tools |
| Commands | Commands tools |
| Cpx | Complex Data metadata tools |
| Dx | Data eXchange configuration tools |
| Security | OPC Security tools |
| XmlDa | XML-DA HTTP/SOAP tools |
| Capture | Packet capture, DCE/RPC decode, and replay tools |

## Session

### `opcclassic.session.create`

Creates an OPC Classic MCP session and returns the sessionId used by discovery and DA tools.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `idleExpirySeconds` | `int?` | `null` | Optional idle timeout in seconds. If omitted, the session expires after 30 minutes of inactivity. |

**Returns:** `OpcSessionDto`

### `opcclassic.session.close`

Closes an OPC Classic MCP session, releasing all DA groups, subscriptions, clients, and channels.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |

**Returns:** `OpcResultDto`

### `opcclassic.session.list`

Lists active OPC Classic MCP sessions, including expiry and DA connection state.

**Parameters**

None.

**Returns:** `IReadOnlyList<OpcSessionDto>`

## Discovery

### `opcclassic.discovery.enumerate_servers`

Enumerates OPC Classic server registrations on a host through OPCEnum / OPC.ServerList.1.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `host` | `string` | `"localhost"` | Host name or IP address to query. Use localhost for the local machine. |
| `categoryIds` | `string[]?` | `null` | Optional OPC category GUID strings to filter, such as CATID_OPCDAServer20 or CATID_OPCDAServer30. Omit for the default OPCEnum categories. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous discovery. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `useSso` | `bool` | `false` | True to authenticate using the current Windows logon via NegotiateAuthentication (no username/password needed). Windows-only. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<IReadOnlyList<OpcServerDescriptorDto>>`

## DA

### `opcclassic.da.connect`

Connects an existing MCP session to an OPC DA server using DCOM or an in-memory test channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC DA server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC DA server ProgID, for example Matrikon.OPC.Simulation.1. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC DA server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or dcom://host/ProgID for DCOM. |
| `useSso` | `bool` | `false` | True to authenticate using the current Windows logon via NegotiateAuthentication (no username/password needed). Windows-only. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcSessionDto>`

### `opcclassic.da.get_status`

Gets the OPC DA server status, including runtime state, version, vendor information, and group count.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect. |

**Returns:** `Task<OpcServerStatusDto>`

### `opcclassic.da.browse`

Browses the OPC DA address space below an item ID using DA 3.0 browse semantics.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemId` | `string` | `""` | The DA item ID to browse below. Use an empty string for the root. |
| `browseFilter` | `string` | `"all"` | Browse filter: all, branch, or leaf. |
| `maxElements` | `int` | `0` | Maximum elements per server browse call. Use 0 for the server default. |
| `elementNameFilter` | `string` | `""` | Optional element name filter, such as *Temp*. |
| `vendorFilter` | `string` | `""` | Optional vendor-specific browse filter. |
| `propertyIds` | `int[]?` | `null` | Optional property IDs to include in each browse element. |
| `returnPropertyValues` | `bool` | `false` | True to include property values when propertyIds are requested. |

**Returns:** `Task<IReadOnlyList<OpcBrowseElementDto>>`

### `opcclassic.da.get_properties`

Gets OPC DA item properties for one or more item IDs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemIds` | `string[]` | `required` | One or more OPC DA item IDs whose properties should be queried. |
| `propertyIds` | `int[]?` | `null` | Optional property IDs to retrieve. Omit to let the server return its default property set. |
| `returnValues` | `bool` | `true` | True to include property values; false to return only property metadata. |

**Returns:** `Task<IReadOnlyList<OpcBrowseElementDto>>`

### `opcclassic.da.add_group`

Creates an OPC DA server-side group used for item add, synchronous I/O, and subscriptions.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `name` | `string` | `"mcp-da-group"` | Unique group name. If omitted or empty, the server may assign a name. |
| `active` | `bool` | `true` | True to make the group active immediately. |
| `updateRateMs` | `int` | `1000` | Requested group update rate in milliseconds. |
| `clientHandle` | `int` | `1` | Client-supplied group handle echoed by callbacks. |
| `timeBiasMinutes` | `int` | `0` | Time bias in minutes from UTC. |
| `deadbandPercent` | `float` | `0` | Deadband percentage, 0 to 100. |
| `localeId` | `int` | `0` | Locale ID for server messages, such as 1033 for en-US. Use 0 for server default. |
| `keepAliveMs` | `int` | `0` | DA 3.0 keep-alive interval in milliseconds. Use 0 to leave disabled. |

**Returns:** `Task<OpcGroupStateDto>`

### `opcclassic.da.add_items`

Adds item IDs to an OPC DA group and returns per-item server handles and HRESULTs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `groupHandle` | `int` | `required` | Server group handle returned by opcclassic.da.add_group. |
| `itemIds` | `string[]` | `required` | OPC DA item IDs to add to the group. |
| `clientHandles` | `int[]?` | `null` | Optional client handles aligned with itemIds. Defaults to 1-based handles. |
| `active` | `bool` | `true` | True to make the items active immediately. |
| `requestedVarType` | `ushort` | `0` | Requested VARTYPE numeric code. Use 0 (VT_EMPTY) for the server canonical type. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.da.read_items_by_id`

Reads OPC DA item values by item ID using the DA 3.0 stateless IOPCItemIO interface; no group is required.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemIds` | `string[]` | `required` | OPC DA item IDs to read, such as Random.Int1 or Random.Real8. |
| `maxAges` | `int[]?` | `null` | Optional per-item max-age values in milliseconds. Omit or pass an empty array for no cache constraint. |

**Returns:** `Task<IReadOnlyList<OpcItemValueDto>>`

### `opcclassic.da.read_sync`

Synchronously reads item values from an OPC DA group by server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `groupHandle` | `int` | `required` | Server group handle returned by opcclassic.da.add_group. |
| `serverHandles` | `int[]?` | `null` | Optional item server handles returned by opcclassic.da.add_items. Omit or pass an empty array to read all known group items. |
| `fromCache` | `bool` | `true` | True to read from the server cache; false to read from the underlying device. |

**Returns:** `Task<IReadOnlyList<OpcItemValueDto>>`

### `opcclassic.da.write_sync`

Synchronously writes values to OPC DA group items by server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `groupHandle` | `int` | `required` | Server group handle returned by opcclassic.da.add_group. |
| `serverHandles` | `int[]` | `required` | Item server handles returned by opcclassic.da.add_items. |
| `values` | `JsonElement[]` | `required` | JSON values to write, aligned with serverHandles. Supported values: null, bool, number, string, DateTime string, or GUID string. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.da.subscribe`

Starts a poll-based OPC DA subscription for a group. MCP cannot push callbacks, so use opcclassic.da.poll_subscription to retrieve values.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `groupHandle` | `int` | `required` | Server group handle returned by opcclassic.da.add_group. |
| `fromCache` | `bool` | `true` | True to refresh/read from the server cache; false to use device reads where supported. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.da.poll_subscription`

Polls a DA subscription for values. The initial implementation uses a pull model and returns current values for known group items.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `subscriptionId` | `string` | `required` | Subscription identifier returned by opcclassic.da.subscribe. |
| `maxNotifications` | `int` | `0` | Maximum item values to return. Use 0 for all currently known group items. |

**Returns:** `Task<IReadOnlyList<OpcItemValueDto>>`

### `opcclassic.da.remove_group`

Removes an OPC DA server-side group and forgets its item handles and poll subscriptions.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `groupHandle` | `int` | `required` | Server group handle returned by opcclassic.da.add_group. |
| `force` | `bool` | `true` | True to force removal even if callbacks or operations are active. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.da.get_error_string`

Translates an OPC HRESULT to a server-localized message using IOPCServer::GetErrorString.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `hresult` | `int` | `required` | HRESULT as a signed 32-bit integer, for example -1073479674 for 0xC0040006. |
| `localeId` | `int` | `0` | Locale ID for the returned message, such as 1033 for en-US. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.da.disconnect`

Disconnects the session from its OPC DA server and releases the DA channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## AE

### `opcclassic.ae.connect`

Connects an existing MCP session to an OPC AE server using DCOM or an in-memory test channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC AE server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC AE server ProgID. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC AE server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or opcae://host/ProgID for DCOM. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.ae.get_status`

Gets the OPC AE event server status, including runtime state, version, vendor information, and operational state.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.ae.connect. |

**Returns:** `Task<OpcServerStatusDto>`

### `opcclassic.ae.browse_areas`

Browses the OPC AE area/source tree below a qualified area name. Use an empty area for the root.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `areaQualifiedName` | `string` | `""` | Qualified area name to browse below. Use an empty string for the root. |

**Returns:** `Task<IReadOnlyList<OpcAreaBrowseElementDto>>`

### `opcclassic.ae.query_event_categories`

Queries event categories supported by the AE server for simple, tracking, condition, or all event types.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `eventTypes` | `string` | `"all"` | Event type filter: all, simple, tracking, condition, or a comma-separated combination. |

**Returns:** `Task<IReadOnlyList<OpcEventCategoryDto>>`

### `opcclassic.ae.query_event_attributes`

Queries server-defined attribute metadata for an OPC AE event category.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `eventCategory` | `int` | `required` | Server-defined event category ID returned by opcclassic.ae.query_event_categories. |

**Returns:** `Task<IReadOnlyList<OpcEventAttributeDto>>`

### `opcclassic.ae.create_subscription`

Creates a poll-based AE subscription. MCP cannot push callbacks, so use opcclassic.ae.poll_events to retrieve queued events.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `active` | `bool` | `true` | True to make the subscription active immediately. |
| `bufferTimeMs` | `int` | `1000` | Requested event buffer time in milliseconds. |
| `maxBufferSize` | `int` | `0` | Requested maximum event buffer size. Use 0 for the server default. |
| `clientSubscription` | `int` | `0` | Client subscription handle echoed by callbacks. Defaults to a generated positive handle when 0. |

**Returns:** `Task<OpcAeSubscriptionDto>`

### `opcclassic.ae.set_filter`

Sets an AE subscription filter using event type, category, severity, area, and source criteria.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `subscriptionId` | `string` | `required` | Subscription identifier returned by opcclassic.ae.create_subscription. |
| `eventTypes` | `string` | `"all"` | Event type filter: all, simple, tracking, condition, or a comma-separated combination. |
| `eventCategories` | `int[]?` | `null` | Optional event category IDs to include. Empty means all categories. |
| `minSeverity` | `int` | `0` | Minimum severity to include, from 0 to 1000. |
| `maxSeverity` | `int` | `1000` | Maximum severity to include, from 0 to 1000. |
| `areas` | `string[]?` | `null` | Optional qualified areas to include. Empty means all areas. |
| `sources` | `string[]?` | `null` | Optional source names to include. Empty means all sources. |

**Returns:** `Task<OpcAeSubscriptionDto>`

### `opcclassic.ae.poll_events`

Polls a subscription queue for AE notifications. MCP cannot receive pushed callbacks directly.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `subscriptionId` | `string` | `required` | Subscription identifier returned by opcclassic.ae.create_subscription. |
| `maxNotifications` | `int` | `0` | Maximum notifications to return. Use 0 for all queued notifications. |
| `waitMilliseconds` | `int` | `100` | Milliseconds to wait for at least one event when the queue is empty. |

**Returns:** `Task<IReadOnlyList<OpcEventNotificationDto>>`

### `opcclassic.ae.refresh_subscription`

Triggers an AE condition refresh so active conditions are re-emitted to the subscription queue.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `subscriptionId` | `string` | `required` | Subscription identifier returned by opcclassic.ae.create_subscription. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.ae.ack_condition`

Acknowledges an AE condition by source and condition name. For DCOM servers, activeTime and cookie identify the event instance.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `source` | `string` | `required` | Event source name that owns the condition. |
| `conditionName` | `string` | `required` | Condition name to acknowledge. |
| `actor` | `string` | `"mcp"` | Acknowledging actor or operator ID. |
| `comment` | `string?` | `null` | Optional acknowledgement comment. |
| `activeTime` | `DateTimeOffset?` | `null` | Optional active time for DCOM acknowledgements. Use the event ActiveTime returned by poll_events. |
| `cookie` | `int` | `0` | Optional AE cookie for DCOM acknowledgements. Use the event Cookie returned by poll_events. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.ae.get_condition_state`

Gets the current server state for a named AE condition and optional attribute IDs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `source` | `string` | `required` | Event source name that owns the condition. |
| `conditionName` | `string` | `required` | Condition name to inspect. |
| `attributeIds` | `int[]?` | `null` | Optional event attribute IDs whose current values should be returned. |

**Returns:** `Task<OpcConditionStateDto>`

### `opcclassic.ae.cancel_subscription`

Cancels and removes an AE subscription from the MCP session.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `subscriptionId` | `string` | `required` | Subscription identifier returned by opcclassic.ae.create_subscription. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.ae.disconnect`

Disconnects the session from its OPC AE server and releases AE subscriptions and channels.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## HDA

### `opcclassic.hda.connect`

Connects an existing MCP session to an OPC HDA server using DCOM or an in-memory test channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC HDA server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC HDA server ProgID. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC HDA server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or opchda://host/ProgID for DCOM. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.hda.get_status`

Gets the OPC HDA historian status, including runtime state, version, vendor information, and max return values.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.hda.connect. |

**Returns:** `Task<OpcServerStatusDto>`

### `opcclassic.hda.browse`

Browses the OPC HDA address space below an item ID prefix. In-memory loopback connections return branch and leaf metadata directly.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemIdPrefix` | `string` | `""` | Item ID prefix or branch to browse. Use an empty string for the root. |
| `browseType` | `string` | `"leaf"` | Browse type: branch, leaf, or flat. |

**Returns:** `Task<IReadOnlyList<OpcHdaBrowseElementDto>>`

### `opcclassic.hda.validate_items`

Validates OPC HDA item IDs and returns per-item HRESULTs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemIds` | `string[]` | `required` | OPC HDA item IDs to validate. |

**Returns:** `Task<IReadOnlyList<OpcHdaItemHandleDto>>`

### `opcclassic.hda.get_item_handles`

Gets server handles for OPC HDA item IDs and stores them in the MCP session for subsequent reads and updates.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemIds` | `string[]` | `required` | OPC HDA item IDs to bind. |
| `clientHandles` | `int[]?` | `null` | Optional client handles aligned with itemIds. Defaults to 1-based handles. |

**Returns:** `Task<IReadOnlyList<OpcHdaItemHandleDto>>`

### `opcclassic.hda.release_item_handles`

Releases OPC HDA server handles and removes them from the MCP session.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles returned by opcclassic.hda.get_item_handles. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.read_raw`

Synchronously reads raw historical values over a time range using HDA server handles or item IDs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |
| `serverHandles` | `int[]?` | `null` | Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used. |
| `itemIds` | `string[]?` | `null` | Optional item IDs to bind automatically when serverHandles are omitted. |
| `maxValuesPerItem` | `int` | `0` | Maximum values per item. Use 0 for server default or unlimited subject to server limits. |
| `includeBounds` | `bool` | `false` | True to include bounding values at the start and end times when supported. |

**Returns:** `Task<IReadOnlyList<OpcHdaReadResultDto>>`

### `opcclassic.hda.read_processed`

Synchronously reads processed/aggregated historical values over fixed resample intervals.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |
| `resampleIntervalSeconds` | `double` | `required` | Resample interval in seconds for aggregate buckets. |
| `aggregate` | `string` | `"Average"` | Aggregate name or numeric ID, such as Average, Minimum, Maximum, StandardDeviation, or 3. |
| `serverHandles` | `int[]?` | `null` | Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used. |
| `itemIds` | `string[]?` | `null` | Optional item IDs to bind automatically when serverHandles are omitted. |

**Returns:** `Task<IReadOnlyList<OpcHdaReadResultDto>>`

### `opcclassic.hda.read_at_time`

Synchronously reads interpolated or nearest historical values at specific timestamps.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `timestamps` | `DateTimeOffset[]` | `required` | Timestamps to read at, as ISO-8601 UTC strings. |
| `serverHandles` | `int[]?` | `null` | Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used. |
| `itemIds` | `string[]?` | `null` | Optional item IDs to bind automatically when serverHandles are omitted. |

**Returns:** `Task<IReadOnlyList<OpcHdaReadResultDto>>`

### `opcclassic.hda.read_modified`

Synchronously reads modified historical data, including modification time, edit type, and user metadata.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |
| `maxValuesPerItem` | `int` | `0` | Maximum modified values per item. Use 0 for server default. |
| `serverHandles` | `int[]?` | `null` | Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used. |
| `itemIds` | `string[]?` | `null` | Optional item IDs to bind automatically when serverHandles are omitted. |

**Returns:** `Task<IReadOnlyList<OpcHdaModifiedReadResultDto>>`

### `opcclassic.hda.read_attribute`

Synchronously reads one or more HDA attributes for a server handle over a time range.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandle` | `int` | `required` | HDA server handle returned by opcclassic.hda.get_item_handles. |
| `attributeIds` | `int[]` | `required` | HDA attribute IDs to read, such as 1 for DataType or 2 for Description. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |

**Returns:** `Task<IReadOnlyList<OpcHdaAttributeResultDto>>`

### `opcclassic.hda.read_annotations`

Synchronously reads annotations for HDA items over a time range.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |
| `serverHandles` | `int[]?` | `null` | Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used. |
| `itemIds` | `string[]?` | `null` | Optional item IDs to bind automatically when serverHandles are omitted. |

**Returns:** `Task<IReadOnlyList<OpcHdaAnnotationResultDto>>`

### `opcclassic.hda.insert_data`

Inserts historical values for HDA server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles aligned with timestamps and values. |
| `timestamps` | `DateTimeOffset[]` | `required` | UTC timestamps aligned with serverHandles and values. |
| `values` | `JsonElement[]` | `required` | JSON values aligned with serverHandles and timestamps. |
| `qualities` | `int[]?` | `null` | Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.replace_data`

Replaces existing historical values for HDA server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles aligned with timestamps and values. |
| `timestamps` | `DateTimeOffset[]` | `required` | UTC timestamps aligned with serverHandles and values. |
| `values` | `JsonElement[]` | `required` | JSON values aligned with serverHandles and timestamps. |
| `qualities` | `int[]?` | `null` | Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.insert_replace_data`

Inserts historical values or replaces existing values for HDA server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles aligned with timestamps and values. |
| `timestamps` | `DateTimeOffset[]` | `required` | UTC timestamps aligned with serverHandles and values. |
| `values` | `JsonElement[]` | `required` | JSON values aligned with serverHandles and timestamps. |
| `qualities` | `int[]?` | `null` | Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.delete_raw`

Deletes raw historical values over a time range for one or more HDA server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `startTime` | `string` | `required` | Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H. |
| `endTime` | `string` | `required` | End time as ISO-8601 UTC or HDA relative expression such as NOW. |
| `serverHandles` | `int[]` | `required` | HDA server handles returned by opcclassic.hda.get_item_handles. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.delete_at_time`

Deletes historical values at exact timestamps for HDA server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles aligned with timestamps. |
| `timestamps` | `DateTimeOffset[]` | `required` | UTC timestamps to delete, aligned with serverHandles. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.insert_annotations`

Inserts annotations attached to exact HDA timestamps for server handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverHandles` | `int[]` | `required` | HDA server handles aligned with timestamps and annotation texts. |
| `timestamps` | `DateTimeOffset[]` | `required` | Historical timestamps to annotate, aligned with serverHandles. |
| `annotationTexts` | `string[]` | `required` | Annotation texts aligned with serverHandles. |
| `users` | `string[]?` | `null` | Annotation users aligned with serverHandles. Defaults to mcp. |
| `annotationTimes` | `DateTimeOffset[]?` | `null` | Optional annotation creation times aligned with serverHandles. Defaults to now. |

**Returns:** `Task<IReadOnlyList<OpcResultDto>>`

### `opcclassic.hda.get_aggregates`

Enumerates aggregate functions supported by the HDA server.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<IReadOnlyList<OpcHdaAggregateDto>>`

### `opcclassic.hda.disconnect`

Disconnects the session from its OPC HDA server and releases HDA state.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## Batch

### `opcclassic.batch.connect`

Connects an existing MCP session to an OPC Batch server using DCOM or an in-memory test channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC Batch server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC Batch server ProgID. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC Batch server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered InMemoryBatchConnectionRegistry channel, or dcom://host/ProgID for DCOM. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.batch.get_status`

Gets Batch connection status and verifies the server delimiter when supported.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.batch.connect. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.batch.disconnect`

Disconnects the session from its OPC Batch server and releases the Batch channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.batch.query_batch_summaries`

Queries OPC Batch summaries with optional Batch 2.0 filter fields and returns JSON-friendly summary DTOs.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `id` | `string?` | `null` | Optional batch identifier substring filter. |
| `description` | `string?` | `null` | Optional batch description substring filter. |
| `opcItemId` | `string?` | `null` | Optional OPC item identifier substring filter. |
| `masterRecipeId` | `string?` | `null` | Optional master recipe identifier substring filter. |
| `minBatchSize` | `float?` | `null` | Optional minimum batch size. Omit to leave unbounded. |
| `maxBatchSize` | `float?` | `null` | Optional maximum batch size. Omit to leave unbounded. |
| `engineeringUnits` | `string?` | `null` | Optional engineering-units substring filter. |
| `executionState` | `string?` | `null` | Optional execution-state substring filter, such as RUNNING or COMPLETE. |
| `executionMode` | `string?` | `null` | Optional execution-mode substring filter, such as AUTOMATIC or MANUAL. |
| `minStartTime` | `DateTimeOffset?` | `null` | Optional minimum actual start time, as an ISO-8601 timestamp. |
| `maxStartTime` | `DateTimeOffset?` | `null` | Optional maximum actual start time, as an ISO-8601 timestamp. |
| `minEndTime` | `DateTimeOffset?` | `null` | Optional minimum actual end time, as an ISO-8601 timestamp. |
| `maxEndTime` | `DateTimeOffset?` | `null` | Optional maximum actual end time, as an ISO-8601 timestamp. |
| `model` | `string` | `"OPCBBatchModel"` | Batch model string passed to Batch 2.0 filtered enumeration. Defaults to OPCBBatchModel. |
| `maxResults` | `int` | `100` | Maximum summaries to return. Use 0 to request up to 1000 summaries. |

**Returns:** `Task<IReadOnlyList<OpcBatchSummaryDto>>`

### `opcclassic.batch.query_enumeration_sets`

Queries the OPC Batch enumeration-set IDs and names exposed by IOPCEnumerationSets.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<IReadOnlyList<OpcBatchEnumerationSetDto>>`

### `opcclassic.batch.query_enumeration`

Queries the display name for a single OPC Batch enumeration value.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `enumerationSetId` | `int` | `required` | Enumeration set ID returned by opcclassic.batch.query_enumeration_sets. |
| `enumerationValue` | `int` | `required` | Numeric enumeration value to resolve. |

**Returns:** `Task<OpcBatchEnumerationDto>`

### `opcclassic.batch.query_enumeration_list`

Queries the complete OPC Batch enumeration-value list for an enumeration set.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `enumerationSetId` | `int` | `required` | Enumeration set ID returned by opcclassic.batch.query_enumeration_sets. |

**Returns:** `Task<IReadOnlyList<OpcBatchEnumerationDto>>`

## Commands

### `opcclassic.commands.connect`

Connects an existing MCP session to an OPC Commands server using DCOM or an in-memory test channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC Commands server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC Commands server ProgID. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC Commands server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\user when a Windows domain is required. |
| `password` | `string?` | `null` | Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered InMemoryCommandsConnectionRegistry channel, or dcom://host/ProgID for DCOM. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.commands.get_status`

Gets OPC Commands connection status, maximum storage time, and command count.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.commands.connect. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.commands.disconnect`

Disconnects the session from its OPC Commands server and releases the Commands channel.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.commands.get_command_descriptions`

Lists command names and retrieves the server description text for each command.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `commandNamespace` | `string` | `""` | Optional command namespace. Use an empty string for the server default namespace. |
| `commandNames` | `string[]?` | `null` | Optional command names to describe. Omit or pass an empty array to describe all commands returned by the server. |

**Returns:** `Task<IReadOnlyList<OpcCommandDescriptionDto>>`

### `opcclassic.commands.invoke_command`

Invokes an OPC Commands command. Asynchronous invocations return an invocationId for polling and cancellation.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `commandName` | `string` | `required` | Command name returned by opcclassic.commands.get_command_descriptions. |
| `commandNamespace` | `string` | `""` | Optional command namespace. Use an empty string for the server default namespace. |
| `targetId` | `string` | `""` | Command target identifier. Use an empty string for server-level commands. |
| `arguments` | `string[]?` | `null` | Command arguments, in server-defined order. |
| `filters` | `string[]?` | `null` | Optional command result filters, in server-defined order. |
| `asynchronous` | `bool` | `true` | True to use async invocation and return an invocationId; false to block for synchronous results. |
| `updateFrequencyMs` | `int` | `1000` | Requested async state-update frequency in milliseconds. |
| `keepAliveTimeMs` | `int` | `30000` | Requested async keep-alive time in milliseconds. |

**Returns:** `Task<OpcCommandInvocationDto>`

### `opcclassic.commands.poll_command_state`

Polls a command invocation for state-change notifications using IOPCCommandExecution::QueryState.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `invocationId` | `string` | `required` | Invocation identifier returned by opcclassic.commands.invoke_command. |
| `waitTimeMs` | `int` | `0` | Server wait time in milliseconds for QueryState. Use 0 for a non-blocking poll. |

**Returns:** `Task<OpcCommandStateDto>`

### `opcclassic.commands.cancel_command`

Sends the Cancel control to an OPC Commands invocation and disconnects the poll connection.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `invocationId` | `string` | `required` | Invocation identifier returned by opcclassic.commands.invoke_command. |

**Returns:** `Task<OpcResultDto>`

## Cpx

### `opcclassic.cpx.get_complex_type`

Gets OPC Complex Data metadata for a DA item, including type ID, dictionary ID, type item ID, unconverted item ID, and available filters.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect. |
| `itemId` | `string` | `required` | The DA item ID whose complex-data metadata should be queried. |

**Returns:** `Task<OpcComplexTypeDto>`

### `opcclassic.cpx.get_type_system`

Gets the OPC Complex Data namespace descriptor for a supported type system: OPCBinary or XMLSchema.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect. |
| `typeSystemId` | `string` | `TypeDictionary.OpcBinaryTypeSystemId` | Type system identifier. Accepted values include OPCBinary, binary, XMLSchema, xml, and schema. |

**Returns:** `OpcTypeSystemDto`

### `opcclassic.cpx.get_dictionary`

Gets a Complex Data type dictionary by dictionary ID and parses OPCBinary or XMLSchema dictionaries when possible.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect. |
| `dictionaryId` | `string` | `required` | Dictionary identifier returned by opcclassic.cpx.get_complex_type. |

**Returns:** `Task<OpcTypeDictionaryDto>`

## Dx

### `opcclassic.dx.connect`

Connects an existing MCP session to an OPC DX server. Use connectionString=inmemory://name for registered test clients.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `host` | `string` | `"localhost"` | OPC DX server host name or IP address. Ignored when connectionString uses inmemory://. |
| `progId` | `string?` | `null` | OPC DX server ProgID. Optional when clsid or connectionString is supplied. |
| `clsid` | `string?` | `null` | OPC DX server CLSID as a GUID string. Optional when progId or connectionString is supplied. |
| `username` | `string?` | `null` | Optional user name for DCOM authentication. |
| `password` | `string?` | `null` | Optional password for DCOM authentication. |
| `useKerberos` | `bool` | `false` | True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied. |
| `connectionString` | `string?` | `null` | Optional connection string. Use inmemory://name for a registered in-memory DX client. |
| `authLevel` | `string?` | `null` | Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM. |

**Returns:** `Task<OpcSessionDto>`

### `opcclassic.dx.get_status`

Gets the OPC DX server status exposed by the connected server.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.dx.connect. |

**Returns:** `Task<OpcServerStatusDto>`

### `opcclassic.dx.query_connections`

Queries OPC DX connection names using QueryDXConnectionNames semantics.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `browsePath` | `string` | `""` | DX browse path to query. Use an empty string for the root. |
| `connectionMasks` | `string[]?` | `null` | Optional connection-name masks. Omit or pass empty to return all names. |
| `recursive` | `bool` | `false` | True to include descendant browse paths. |

**Returns:** `Task<IReadOnlyList<string>>`

### `opcclassic.dx.query_source_servers`

Lists the source servers configured in the connected OPC DX server.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<IReadOnlyList<OpcDxSourceServerDto>>`

### `opcclassic.dx.add_connection`

Adds an OPC DX connection definition.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `connection` | `OpcDxConnectionDto` | `required` | DX connection definition to add. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.modify_connection`

Modifies an existing OPC DX connection definition.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `connection` | `OpcDxConnectionDto` | `required` | DX connection definition to modify. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.update_connection`

Updates OPC DX connections matching a connection name and browse path.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `connectionName` | `string` | `required` | Connection name or mask to update. |
| `connection` | `OpcDxConnectionDto` | `required` | Updated DX connection fields. |
| `browsePath` | `string` | `""` | DX browse path to search. Use an empty string for the root. |
| `recursive` | `bool` | `false` | True to include descendant browse paths. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.delete_connection`

Deletes an OPC DX connection by name.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `connectionName` | `string` | `required` | DX connection name to delete. |
| `browsePath` | `string` | `""` | DX browse path to search. Use an empty string for the root. |
| `recursive` | `bool` | `false` | True to include descendant browse paths. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.add_source_server`

Adds an OPC DX source-server definition.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `sourceServer` | `OpcDxSourceServerDto` | `required` | DX source-server definition to add. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.modify_source_server`

Modifies an existing OPC DX source-server definition.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `sourceServer` | `OpcDxSourceServerDto` | `required` | DX source-server definition to modify. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.reset_configuration`

Resets all configured OPC DX connections and source servers.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `configurationVersion` | `string` | `""` | Optional current configuration version supplied to the server. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.dx.disconnect`

Disconnects the session from its OPC DX server and releases DX client state.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## Security

### `opcclassic.security.is_available_nt`

Checks whether the connected OPC server supports IOPCSecurityNT Windows-integrated authentication.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create, typically connected to a DCOM OPC server. |

**Returns:** `Task<OpcSecurityInfoDto>`

### `opcclassic.security.is_available_private`

Checks whether the connected OPC server supports IOPCSecurityPrivate username/password authentication.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create, typically connected to a DCOM OPC server. |

**Returns:** `Task<OpcSecurityInfoDto>`

### `opcclassic.security.logon`

Logs on to IOPCSecurityPrivate with server-managed username/password credentials.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `username` | `string` | `required` | Server-private username. |
| `password` | `string` | `required` | Server-private password. This is sent to the OPC server according to the server's configured DCOM security. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.security.logoff`

Logs off IOPCSecurityPrivate and returns to the connection's default identity.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## XmlDa

### `opcclassic.xmlda.connect`

Connects an existing MCP session to an OPC XML-DA HTTP/SOAP endpoint URL.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create. |
| `endpointUrl` | `string` | `required` | XML-DA HTTP or HTTPS endpoint URL. Use inmemory://name for a registered test client. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.xmlda.get_status`

Calls XML-DA GetStatus on the connected HTTP endpoint.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The sessionId returned by opcclassic.session.create and connected with opcclassic.xmlda.connect. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcXmlDaServerStatusDto>`

### `opcclassic.xmlda.browse`

Calls XML-DA Browse on the connected HTTP endpoint.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemName` | `string` | `""` | The XML-DA item name/branch to browse. Use an empty string for the root. |
| `itemPath` | `string` | `""` | Optional vendor-defined item path. |
| `continuationPoint` | `string` | `""` | Optional continuation point from a previous browse response. |
| `maxElementsReturned` | `int` | `0` | Maximum elements to return. Use 0 for server default/no limit. |
| `browseFilter` | `string` | `"all"` | Browse filter: all, branch, or item. |
| `elementNameFilter` | `string` | `""` | Optional element name filter. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcXmlDaBrowseResponseDto>`

### `opcclassic.xmlda.get_properties`

Calls XML-DA GetProperties for one or more item names.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `itemNames` | `string[]` | `required` | Item names whose properties should be retrieved. |
| `itemPath` | `string` | `""` | Optional vendor-defined item path applied to all items. |
| `propertyNames` | `string[]?` | `null` | Optional property names to retrieve. Omit or pass empty with returnAllProperties=true for all properties. |
| `returnAllProperties` | `bool` | `true` | True to return all properties. |
| `returnPropertyValues` | `bool` | `false` | True to include property values. |
| `returnErrorText` | `bool` | `true` | True to include localized error text. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcXmlDaGetPropertiesResponseDto>`

### `opcclassic.xmlda.read`

Calls XML-DA Read for one or more items.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `items` | `OpcXmlDaReadItemDto[]` | `required` | Items to read, including optional client handles and MaxAge values. |
| `returnErrorText` | `bool` | `true` | True to include localized error text. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<IReadOnlyList<OpcXmlDaItemValueDto>>`

### `opcclassic.xmlda.write`

Calls XML-DA Write for one or more items.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `items` | `OpcXmlDaWriteItemDto[]` | `required` | Items and JSON values to write. |
| `returnValuesOnReply` | `bool` | `false` | True to have the server echo values on reply where supported. |
| `returnErrorText` | `bool` | `true` | True to include localized error text. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<IReadOnlyList<OpcXmlDaWriteResultDto>>`

### `opcclassic.xmlda.subscribe`

Calls XML-DA Subscribe. Use opcclassic.xmlda.poll_subscription to retrieve changes.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `items` | `OpcXmlDaSubscribeItemDto[]` | `required` | Items to subscribe to. |
| `itemPath` | `string` | `""` | Optional vendor-defined item path applied to all items. |
| `requestedSamplingRate` | `int` | `0` | Default requested sampling rate in milliseconds. |
| `subscriptionPingRate` | `int` | `0` | Subscription ping/keep-alive rate in milliseconds. |
| `returnValuesOnReply` | `bool` | `false` | True to return initial values in the subscribe response. |
| `returnErrorText` | `bool` | `true` | True to include localized error text. |
| `enableBuffering` | `bool` | `false` | True to enable server-side buffering for changes. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcXmlDaSubscriptionDto>`

### `opcclassic.xmlda.poll_subscription`

Calls XML-DA SubscriptionPolledRefresh for one or more server subscription handles.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverSubHandles` | `string[]` | `required` | Server subscription handles returned by opcclassic.xmlda.subscribe. |
| `holdTime` | `DateTimeOffset?` | `null` | Optional earliest hold time. Omit to poll immediately. |
| `waitTime` | `int` | `0` | Maximum server wait time in milliseconds. |
| `returnAllItems` | `bool` | `false` | True to return all subscribed item values, not only changes. |
| `returnErrorText` | `bool` | `true` | True to include localized error text. |
| `localeId` | `string?` | `null` | Optional requested locale ID, such as en-US. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcXmlDaSubscriptionPollDto>`

### `opcclassic.xmlda.cancel_subscription`

Calls XML-DA SubscriptionCancel for a server subscription handle.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |
| `serverSubHandle` | `string` | `required` | Server subscription handle returned by opcclassic.xmlda.subscribe. |
| `clientRequestHandle` | `string?` | `null` | Optional client request handle echoed by the server. |

**Returns:** `Task<OpcResultDto>`

### `opcclassic.xmlda.disconnect`

Disconnects the session from its OPC XML-DA endpoint and releases HTTP client state.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | The connected OPC Classic sessionId. |

**Returns:** `Task<OpcResultDto>`

## Capture

### `opcclassic.capture.list_interfaces`

Enumerates NICs that can be used as `interfaceName` for live capture.

**Parameters**

None.

**Returns:** `IReadOnlyList<CaptureInterfaceDto>`

### `opcclassic.capture.start`

Begins a network packet capture session. Defaults the BPF filter to TCP DCOM traffic and returns a capture session id.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `interfaceName` | `string` | `required` | Network interface name from `opcclassic.capture.list_interfaces`. |
| `bpfFilter` | `string?` | `null` | Optional BPF filter override. Takes precedence over `serverPorts`. |
| `promiscuous` | `bool` | `true` | True to open the interface in promiscuous mode. |
| `maxBytes` | `long?` | `null` | Optional cap on captured bytes; the capture source applies its default when omitted. |
| `maxPackets` | `long?` | `null` | Optional cap on captured frame count. |
| `maxDurationSeconds` | `int?` | `null` | Optional cap on wall-clock duration; the capture source applies its default when omitted. |
| `serverPorts` | `int[]?` | `null` | Optional explicit OPC server data ports used to narrow the default DCOM capture filter. |
| `ntlmSessionKeyHex` | `string?` | `null` | Developer-only 32-character hex NTLMv2 session key for inline auth-trailer unwrap in `capture.tail`, `capture.get`, and `capture.summarize`. Capture must begin before the NTLM Type3 handshake so direction counters remain synchronized. The key is cloned into session-owned storage, redacted from session metadata, and zeroed on failure/disposal. |

**Returns:** `Task<CaptureSessionDto>`

NTLM unwrap is implemented for live-session tail/get/summarize decoding. Remaining gaps are ad-hoc decode/replay key input and compatibility variants for externally produced protected traffic.

### `opcclassic.capture.stop`

Stops an in-progress capture and finalizes the trace.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id returned by `opcclassic.capture.start`. |

**Returns:** `Task<CaptureSessionDto>`

### `opcclassic.capture.list`

Lists known capture sessions.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `state` | `string?` | `null` | Optional state filter: active, running, completed, failed, or all. |

**Returns:** `IReadOnlyList<CaptureSessionDto>`

### `opcclassic.capture.get`

Returns captured trace data as a decoded summary, JSON records, or a pcap file path. When the session was started with `ntlmSessionKeyHex`, protected request/response/fault records are unwrapped inline and include `authUnwrapStatus` plus an operator-safe failure reason when applicable.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id from `opcclassic.capture.start`. |
| `format` | `string` | `"dcom"` | Output format: dcom, json, or pcap-path. |
| `maxPdus` | `int` | `200` | Maximum decoded PDUs to return. |

**Returns:** `Task<string>`

### `opcclassic.capture.tail`

Returns the next decoded-PDU window for a live or completed capture using a polling cursor. Sessions started with `ntlmSessionKeyHex` unwrap protected request/response/fault records inline.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id from `opcclassic.capture.start`. |
| `max` | `int` | `200` | Maximum PDUs to return in this call. |
| `sinceIndex` | `long` | `0` | Cursor returned by the previous tail call as `nextIndex`; pass 0 for the first call. |
| `subscriberId` | `string?` | `null` | Optional stable bounded replay cursor id. |
| `subscriberCapacity` | `int?` | `null` | Retained decoded-PDU capacity, 1–5000. |

**Returns:** `Task<CaptureTailResultDto>`

Each capture session owns one incremental decoder and bounded shared cache. Multiple
tail and notification subscribers read indexes from that cache; they do not each
rescan the pcap file or retain frame copies. Repeating the same `sinceIndex`
replays the same retained window. `overflowed` and `droppedRanges` identify records
displaced before acknowledgement.

### `opcclassic.capture.close_cursor`

Closes a named tail cursor without stopping the capture.

### `opcclassic.capture.subscribe_notifications`

Reserves a named cursor synchronously, starts the session's single incremental
producer if necessary, and sends lightweight `notifications/opcclassic/capture`
index/state/drop metadata. The call fails without returning a subscription id when
cursor capacity or producer initialization fails. Tail remains authoritative.

### `opcclassic.capture.unsubscribe_notifications`

Stops a notification subscription and releases its reserved cursor.

### `opcclassic.capture.summarize`

Returns top-N roll-ups for a completed capture, including talkers, ports, IIDs, opnums, IPIDs, fault codes, and bind-reject reasons. Protected traffic is unwrapped inline when the session has an NTLM key.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id from `opcclassic.capture.start`. |
| `top` | `int` | `10` | Top-N entries per category. |

**Returns:** `Task<CaptureSummary>`

### `opcclassic.capture.remove`

Stops a capture if needed, disposes it, and removes its scratch folder.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id from `opcclassic.capture.start`. |

**Returns:** `Task<bool>`

### `opcclassic.capture.decode_pdu`

Decodes exactly one raw DCE/RPC PDU frame from hex bytes through the same `PduCodec` projection used by capture sessions. Empty, truncated, fragment-length-mismatched, and undecodable inputs return a structured MCP error with a bounded hex context.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `hex` | `string` | `required` | Hex string of the raw frame bytes, with or without whitespace or a 0x prefix. |

**Returns:** `string`

### `opcclassic.capture.replay`

Re-decodes captured request/response/fault frames through `PduCodec`, validates request `ORPC_THIS` and response `ORPC_THAT` envelopes, and reports per-(IID,opnum,direction) succeeded/failed/skipped counts. Each failing bucket includes the first failure message and a bounded hex context; records without retained raw frame bytes are skipped rather than reported as validated.

**Parameters**

| Name | Type | Default | Description |
| --- | --- | --- | --- |
| `sessionId` | `string` | `required` | Capture session id from `opcclassic.capture.start`. |

**Returns:** `Task<ReplayReport>`

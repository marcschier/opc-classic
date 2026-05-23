# Troubleshooting and diagnostics

Updated for Opc.Classic 0.4.0-alpha.1.

OPC Classic failures often look vague at the top of the stack: "cannot connect", "access denied", "bad quality", "decode failed", or `E_FAIL`. The root cause may be DNS, firewall, endpoint mapper, authentication level, SPN, channel binding, NDR shape, per-item HRESULT, or a server-specific behavior. This tutorial gives you a structured diagnostic workflow for Opc.Classic clients and managed servers.

Use this article with the architecture guide [../ARCHITECTURE.md](../ARCHITECTURE.md), the DCOM hardening cookbook [../cookbook/05-dcom-hardening-pkt-integrity-explainer.md](../cookbook/05-dcom-hardening-pkt-integrity-explainer.md), and the Kerberos tutorial [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md).

## Prerequisites

- A failing or suspicious OPC Classic client/server workflow.
- Access to application logs and, for Windows servers, Event Viewer.
- Ability to run network and Kerberos checks from the client host.
- Familiarity with `OpcException`, `OpcResultId`, and `ILogger`.

## What you'll learn

- How to separate connection, authentication, NDR, and application failures.
- How to decode common HRESULTs.
- How to enable library and application logging.
- How to add OpenTelemetry-friendly spans around OPC operations.
- How to diagnose channel-binding token mismatches.

## Start with a failure envelope

Before changing code, capture the envelope:

- target URL (`opcda://host/ProgId`, `opcae://...`, or `opchda://...`);
- client OS, container image, and runtime identifier;
- server vendor/version from a successful or previous `GetStatus`;
- authentication mode and protection level;
- exact HRESULT and text;
- whether the failure is connect-time, first call, per-item, callback, or shutdown;
- timestamps in UTC.

Most long investigations become short when you can say, "Kerberos ticket acquisition succeeds, DCOM bind succeeds, `IOPCServer::GetStatus` fails with `OPC_E_INVALIDHANDLE` after group removal." Without the envelope, every layer is suspect.

## Connection failures

Connection failures usually fall into these buckets:

1. **Name resolution.** The client cannot resolve the host or resolves to the wrong interface.
2. **Endpoint mapping.** TCP/135 or the configured managed endpoint is blocked.
3. **Activation.** The ProgID/CLSID is not registered or the server process cannot start.
4. **Callback reachability.** The server cannot call back to the client for DA/AE subscriptions.
5. **Timeouts.** Firewalls drop packets instead of rejecting them.

For managed servers, avoid ephemeral listen addresses in production. `127.0.0.1:0` is a sample setting, not a deployment plan. For Windows DCOM servers, coordinate endpoint mapper and dynamic RPC port range with the Windows administrators.

## Authentication errors

Authentication errors are often policy mismatches. Hardened Windows DCOM servers require packet integrity. Opc.Classic defaults to `OpcProtectionLevel.Integrity`; keep it that way unless you are testing an isolated legacy endpoint.

```csharp
using System.Net;
using Opc.Classic;

OpcConnectData connectData = OpcConnectData.WithNtlmV2(
    OpcUrl.Parse("opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1"),
    new NetworkCredential("opc-reader", password, "PLANT"),
    OpcProtectionLevel.Integrity,
    TimeSpan.FromSeconds(30));
```

If Kerberos is enabled, validate outside the app:

```bash
kinit -kt /etc/opc/opc-client.keytab opc-client@PLANT.EXAMPLE.COM
kvno RPCSS/opc01.plant.example.com
klist -e
```

If external validation fails, fix Kerberos first. Application retries cannot repair a duplicate SPN or expired keytab.

## HRESULT decoding

`OpcResultId` wraps HRESULTs and exposes `IsFailure`, `Facility`, and `CodePart`. Use it in logs:

```csharp
using Opc.Classic;

public static void LogResult(ILogger logger, string operation, OpcResultId result)
{
    logger.LogInformation(
        "{Operation}: result={Result}, failure={Failure}, facility={Facility}, codePart=0x{CodePart:X4}",
        operation,
        result,
        result.IsFailure,
        result.Facility,
        result.CodePart);
}
```

Common values:

| HRESULT | Meaning | Diagnostic action |
| --- | --- | --- |
| `S_OK` | success | Continue. |
| `S_FALSE` | success with a method-specific caveat | Read the method semantics; do not throw automatically. |
| `E_FAIL` | generic failure | Ask server for error text; inspect lower layers. |
| `E_INVALIDARG` | bad argument shape | Check nulls, counts, LCID, time range, or invalid enum values. |
| `OPC_E_INVALIDHANDLE` | bad group/item handle | Check lifecycle and stale handles. |
| `OPC_E_UNKNOWNITEMID` | item does not exist | Browse and validate item IDs. |
| `OPC_E_BADRIGHTS` | access denied for read/write | Check access rights and write policy. |
| `OPC_S_UNSUPPORTEDRATE` | server revised update rate | Treat as warning and use revised state. |

Per-item HRESULTs matter. A DA read can return two good values and one `OPC_E_UNKNOWNITEMID`. Do not fail the entire batch unless your business operation requires all items.

## NDR decode mismatches

NDR mismatches usually mean the client and server disagree about the IDL shape, array counts, pointer nullability, or alignment. Symptoms include unexpected end-of-buffer, impossible string lengths, invalid referent IDs, or decoded values shifted by a few bytes.

Checklist:

- Verify the interface IID and opnum.
- Compare the method signature with `External\Include\*.idl` and the `Dcom\IOPCInterfaces.cs` projection.
- Confirm conformant array counts match the number of elements decoded.
- Check whether a string is `LPWSTR`, `BSTR`, or an array of string pointers.
- Confirm `FILETIME` is two 32-bit halves, not an aligned 64-bit integer.
- Preserve `S_FALSE` response bodies; some methods return partial data with success warnings.

The span-based `NdrReader` exposes `Position` and `RemainingBytes`, which are useful in focused tests. Do not log entire payloads from production if they may contain process data or credentials.

## Enable library logging

The modern hosting libraries use `Microsoft.Extensions.Logging`. Configure logging in your host:

```csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(static options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

The transitional DCOM layer has a process-wide logging bridge:

```csharp
using Microsoft.Extensions.Logging;
using Opc.Classic.Dcom.Internal;

using ILoggerFactory loggerFactory = LoggerFactory.Create(static builder =>
{
    builder.AddSimpleConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
});

LogHost.ConfigureFactory(loggerFactory);
```

Use trace logging during diagnosis, then return to information/warning in production. Trace logs around NDR and authentication can be high volume.

## OpenTelemetry-friendly tracing

Opc.Classic does not require OpenTelemetry, but application code can create spans around logical operations:

```csharp
using System.Diagnostics;
using Opc.Classic.Da;

public sealed class TracedDaReader
{
    private static readonly ActivitySource ActivitySource = new("Contoso.OpcDa");
    private readonly IDaServer _server;

    public TracedDaReader(IDaServer server) => _server = server;

    public async Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken)
    {
        using Activity? activity = ActivitySource.StartActivity("opc.da.read");
        activity?.SetTag("opc.system", "classic");
        activity?.SetTag("opc.spec", "da");
        activity?.SetTag("opc.item_count", items.Count);

        try
        {
            IReadOnlyList<ItemValueResult> results = await _server.ReadAsync(items, cancellationToken).ConfigureAwait(false);
            int failures = results.Count(static r => r.ResultId.IsFailure);
            activity?.SetTag("opc.failure_count", failures);
            return results;
        }
        catch (OpcException ex)
        {
            activity?.SetTag("opc.hresult", $"0x{ex.ResultId.Code:X8}");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

If you add OpenTelemetry packages, export these spans to your normal collector. Keep tag values low-cardinality: item count is fine, thousands of item IDs are not.

## Channel binding mismatches

Channel binding token (CBT) mismatches appear during authentication, not during DA read logic. They happen when the client and server hash different channel data. For TLS server endpoint binding, both sides must agree on the DER certificate bytes behind `tls-server-end-point:`.

Diagnostic steps:

1. Identify where TLS terminates.
2. Export the certificate seen by the client.
3. Export the certificate expected by the server.
4. Compute `ChannelBindingsHash.ForTlsServerCert` for both.
5. Compare hashes.

```csharp
using Opc.Classic.Security;

byte[] certificate = await File.ReadAllBytesAsync("server.cer", cancellationToken);
byte[] hash = ChannelBindingsHash.ForTlsServerCert(certificate);
Console.WriteLine(Convert.ToHexString(hash));
```

If a proxy terminates TLS, the proxy certificate is the channel from the client's perspective. Either configure the server to expect that binding or avoid CBT on that path after security review.

## Common scenarios

### Connect works, first read fails

Likely an item, group, or rights issue. Browse the namespace, validate items, check per-item results, and call `GetErrorTextAsync`.

### Reads work, subscriptions do not

Check callback reachability, group active state, item active state, update rate, deadband, and server-to-client firewall rules. Containers are especially prone to advertising unreachable callback addresses.

### HDA raw read returns no values

Check UTC range, relative time parsing, `includeBounds`, item ID, max value cap, and whether the historian stores data at the requested resolution.

### AE acknowledges fail

Check that the event is a condition event, `ConditionName` is not null, the condition still exists, actor/comment policy is satisfied, and the event has not already been acknowledged.

## Pitfalls

- Treating `S_FALSE` as failure can break browse and partial-result flows.
- Logging credentials or keytab bytes is a security incident.
- Retrying authentication failures can lock accounts.
- Capturing every tag name as a tracing tag creates cardinality problems.
- Fixing NDR by adding padding randomly will create a different bug. Compare IDL.

## Structured log fields

Logs are most useful when every OPC operation uses the same field names. Standardize on fields such as `opc.url`, `opc.host`, `opc.server_id`, `opc.spec`, `opc.operation`, `opc.hresult`, `opc.result`, `opc.item_count`, `opc.failure_count`, `opc.group_handle`, `opc.transaction_id`, `opc.auth_mode`, and `opc.protection_level`. Avoid embedding all of that in a free-form message only. Structured fields let you build dashboards and alerts without brittle parsing.

For sensitive environments, classify fields. URLs and item IDs may reveal process structure; credentials and tokens must never be logged. If you export logs outside the plant network, review whether tag names are considered sensitive operational data.

## Diagnostic runbook template

Create a runbook that an on-call engineer can follow without knowing OPC internals:

1. Check process health and recent deployment version.
2. Confirm DNS and route to the OPC host.
3. Confirm Kerberos ticket acquisition or NTLM credential validity.
4. Confirm packet-integrity setting.
5. Run or trigger `GetStatus` and capture server vendor/version.
6. Browse one known branch.
7. Validate one known good item and one known bad item.
8. Read one known item.
9. Create a small subscription and force refresh.
10. Collect HRESULTs, logs, and timestamps before restarting.

A restart should be an action with a reason, not the first diagnostic step. Restarting can hide evidence such as stale handles, callback connection failures, or authentication expiry.

## Testing diagnostics in CI

Add tests for error formatting and result handling. For example, construct `OpcResultId.UnknownItemId`, `OpcResultId.False`, and a vendor-specific failure and assert your logging or UI layer displays them correctly. Simulate partial batch failure and verify successful rows are still processed. For NDR changes, keep fixture payloads that intentionally fail with clear messages when an array count or alignment rule changes.

CI cannot reproduce every DCOM or Kerberos environment, but it can prevent your application from collapsing all failures to `E_FAIL` or dropping per-item details. That alone dramatically improves production troubleshooting.

## When to escalate

Escalate to network or domain administrators with evidence: target host, ports, timestamps, SPN, realm, client IP, server IP, Event Viewer IDs, and packet capture references if allowed. Escalate to server vendors with OPC method, HRESULT, item IDs, server version, and a minimal reproduction. Escalating with only "the OPC client failed" wastes everyone’s time.

## Packet capture guidance

Packet captures can answer questions that logs cannot, but they are sensitive. Capture only with approval, scope the capture to client/server/KDC hosts, and store files securely. For DCOM, captures can confirm TCP connection attempts, endpoint mapper traffic, bind/auth exchanges, fragmentation, and resets. They usually cannot show decrypted application values when packet privacy is enabled, and they may contain authentication tokens even with integrity only.

Annotate captures with UTC timestamps and application log correlation IDs. A capture without a matching log window is hard to interpret. If you share captures with a vendor, strip unrelated traffic and confirm your organization's data-handling requirements.

## Creating minimal reproductions

A minimal reproduction should remove business logic. Build a tiny console app that parses the same URL, uses the same auth mode, calls `GetStatus`, then performs one failing operation with one item. Include appsettings, command line, exact package versions, and expected/actual HRESULT. For server issues, include the smallest managed server or sample configuration that reproduces the problem.

Minimal reproductions are also useful internally. They distinguish a protocol failure from a scheduling, database, UI, or message-bus problem. Keep a `diagnostics` project outside production if your team frequently supports plant integrations.

## Knowledge base entries

After resolving an incident, write a small knowledge-base entry. Include symptoms, exact HRESULTs, logs, root cause, fix, and prevention. Tag entries by layer: DNS, firewall, Kerberos, DCOM hardening, item configuration, NDR, callback, HDA range, AE acknowledgement. Over time, this becomes faster than searching vendor forums during an outage.

Keep examples sanitized but concrete. "KRB_AP_ERR_MODIFIED due to duplicate RPCSS SPN on old service account" is useful. "Authentication failed" is not. Link to the commands that proved the diagnosis so the next engineer can repeat them.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Baseline success logs

Capture logs from a healthy run, not only from failures. A known-good status call, browse, read, subscription refresh, HDA query, and AE acknowledgement provide comparison points during incidents. Store these examples in the runbook with timestamps and expected fields.

## Next steps

- Review Kerberos troubleshooting in [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md).
- Review deployment health checks in [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Review performance counters in [08-performance-tuning.md](08-performance-tuning.md).

## References

- [MS-DCOM], [MS-RPCE], [MS-KILE], and [MS-CSSP].
- OPC DA 3.00, HDA 1.20, and AE 1.10 HRESULT semantics.
- Repository files: `src\Opc.Classic.Core\OpcResultId.cs`, `src\Opc.Classic.Core\Ndr\`, and `src\Opc.Classic.Dcom\Internal\LogHost.cs`.






//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.Text;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Ae.Tests.Wire.Dr3233;

/// <summary>
/// DR32/DR33 wire-byte capture: capture the EXACT bytes the managed proxy/dispatcher
/// pair emits for <c>GetConditionState</c> and <c>AckCondition</c>, then
/// regression-lock them so any encoder change is visible as a wire-byte
/// delta.
/// </summary>
/// <remarks>
/// <para>The captured bytes are diffed against the MIDL spec captured in
/// <c>docs/conformance/ae-wire-format.md</c> to identify the
/// exact byte-level discrepancies that cause <c>opcae_ps.dll</c> to crash
/// (<c>GetConditionState</c> response) and reject (<c>AckCondition</c>
/// request) on the <c>samples-ae</c> native-CCW path. This file is the
/// "actual" side of that diff; the spec doc is the "expected" side.</para>
///
/// <para><strong>Fixture regeneration:</strong> the test asserts captured
/// bytes match committed hex fixtures at
/// <c>tests/Opc.Classic.Ae.Tests/Wire/Dr3233/Fixtures/</c>. To regenerate
/// the fixtures (after an intentional encoder change), set
/// <c>OPCAE_DR3233_REGENERATE=1</c> in the environment; the test rewrites
/// the fixture files instead of asserting, then passes. Commit the
/// regenerated fixtures + the encoder change together.</para>
///
/// <para><strong>Determinism:</strong> the stub server returns fully
/// fixed (non-timestamp-dependent) data so the captured bytes are
/// reproducible across runs and machines.</para>
/// </remarks>
public sealed class Dr3233WireCaptureTests
{
    // 2026-01-01T00:00:00Z = 0x01D7461036000000 in FILETIME ticks (100-ns since 1601).
    private const long FixedFileTime = 132521184000000000L;
    private static readonly DateTimeOffset FixedTimestamp =
        DateTimeOffset.FromFileTime(FixedFileTime);

    private const string FixtureDir = "Wire/Dr3233/Fixtures";
    private const string RegenerateEnvVar = "OPCAE_DR3233_REGENERATE";

    [Test]
    public async Task GetConditionState_capture_request_and_response_match_fixture()
    {
        var stub = new DeterministicEventServer();
        var dispatcher = new IOPCEventServerServerDispatcher(stub);
        byte[] requestBytes = Array.Empty<byte>();
        byte[] responseBytes = Array.Empty<byte>();
        var captureChannel = new ByteCapturingChannel(
            dispatcher,
            (req, resp) =>
            {
                requestBytes = req.ToArray();
                responseBytes = resp.ToArray();
            });
        var proxy = new IOPCEventServerClientProxy(captureChannel);

        OpcConditionState state = await proxy.GetConditionStateAsync(
            source: "Random.Int4",
            conditionName: "LimitAlarm",
            attributeIds: new[] { 1, 2, 3 },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(state).IsNotNull();
        await Assert.That(state.ActiveSubCondition).IsEqualTo("HiHi");

        await AssertOrRegenerateAsync("get_condition_state.hex", requestBytes, responseBytes,
            iid: IOPCEventServer.InterfaceId, opnum: IOPCEventServer.Opnums.GetConditionStateAsync);
    }

    [Test]
    public async Task AckCondition_capture_request_and_response_match_fixture()
    {
        var stub = new DeterministicEventServer();
        var dispatcher = new IOPCEventServerServerDispatcher(stub);
        byte[] requestBytes = Array.Empty<byte>();
        byte[] responseBytes = Array.Empty<byte>();
        var captureChannel = new ByteCapturingChannel(
            dispatcher,
            (req, resp) =>
            {
                requestBytes = req.ToArray();
                responseBytes = resp.ToArray();
            });
        var proxy = new IOPCEventServerClientProxy(captureChannel);

        int[] errors = await proxy.AckConditionAsync(
            dwCount: 2,
            acknowledgerId: "operator1",
            comment: "scheduled ack",
            sources: new[] { "Random.Int4", "Random.Real8" },
            conditionNames: new[] { "LimitAlarm", "DeviationAlarm" },
            activeTimes: new[] { FixedFileTime, FixedFileTime + 10000 },
            cookies: new[] { 42, 43 },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(errors).IsNotNull();
        await Assert.That(errors.Length).IsEqualTo(2);

        await AssertOrRegenerateAsync("ack_condition.hex", requestBytes, responseBytes,
            iid: IOPCEventServer.InterfaceId, opnum: IOPCEventServer.Opnums.AckConditionAsync);
    }

    [Test]
    public async Task GetConditionState_capture_with_null_optional_strings_matches_fixture()
    {
        // Variant: ascDefinition + acknowledgerId + comment are all null —
        // this is the "happy-path quiescent condition" the matrix probes
        // typically hit, and exercises null-referent handling which is one
        // of the hypothesized response-crash root causes.
        var stub = new DeterministicEventServer
        {
            ReturnNullOptionalStrings = true,
        };
        var dispatcher = new IOPCEventServerServerDispatcher(stub);
        byte[] requestBytes = Array.Empty<byte>();
        byte[] responseBytes = Array.Empty<byte>();
        var captureChannel = new ByteCapturingChannel(
            dispatcher,
            (req, resp) =>
            {
                requestBytes = req.ToArray();
                responseBytes = resp.ToArray();
            });
        var proxy = new IOPCEventServerClientProxy(captureChannel);

        OpcConditionState state = await proxy.GetConditionStateAsync(
            source: "Random.Int4",
            conditionName: "LimitAlarm",
            attributeIds: Array.Empty<int>(),
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(state).IsNotNull();
        await Assert.That(state.AcknowledgerId).IsNull();

        await AssertOrRegenerateAsync("get_condition_state_nulls.hex", requestBytes, responseBytes,
            iid: IOPCEventServer.InterfaceId, opnum: IOPCEventServer.Opnums.GetConditionStateAsync);
    }

    /// <summary>
    /// Loads the committed fixture and asserts the captured bytes match.
    /// When the <c>OPCAE_DR3233_REGENERATE</c> environment variable is set
    /// to <c>1</c>, instead overwrites the fixture file with the new bytes
    /// and passes the test (operator commits the regenerated fixture +
    /// the encoder change together).
    /// </summary>
    private static async Task AssertOrRegenerateAsync(
        string fixtureFileName, byte[] requestBytes, byte[] responseBytes,
        Guid iid, int opnum)
    {
        string fixturePath = ResolveFixturePath(fixtureFileName);
        string serialized = FormatCapture(iid, opnum, requestBytes, responseBytes);

        bool regenerate = string.Equals(
            Environment.GetEnvironmentVariable(RegenerateEnvVar),
            "1", StringComparison.Ordinal);

        if (regenerate || !File.Exists(fixturePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fixturePath)!);
            await File.WriteAllTextAsync(fixturePath, serialized);
            // First-run / regeneration: pass the test but warn loudly so the
            // operator notices and reviews + commits the new fixture.
            await Assert.That(File.Exists(fixturePath)).IsTrue();
            return;
        }

        string expected = await File.ReadAllTextAsync(fixturePath);
        // Strip the volatile timestamp line from both sides before comparing.
        string normalizedExpected = StripTimestampLine(expected);
        string normalizedActual = StripTimestampLine(serialized);
        await Assert.That(normalizedActual).IsEqualTo(normalizedExpected);
    }

    private static string ResolveFixturePath(string fileName)
    {
        // Test binary lives under tests/Opc.Classic.Ae.Tests/bin/Debug/net10.0/;
        // walk up to the repo root + Fixtures dir.
        string candidate = Path.Combine(AppContext.BaseDirectory, FixtureDir, fileName);
        if (File.Exists(candidate))
        {
            return candidate;
        }
        // Fallback: walk up to find the source-tree fixtures dir.
        string? current = AppContext.BaseDirectory;
        while (current is not null)
        {
            string sourceTreeCandidate = Path.Combine(current, "tests", "Opc.Classic.Ae.Tests", FixtureDir, fileName);
            if (Directory.Exists(Path.GetDirectoryName(sourceTreeCandidate)))
            {
                return sourceTreeCandidate;
            }
            current = Path.GetDirectoryName(current);
        }
        // Last resort: dump alongside the test binary.
        return candidate;
    }

    private static string FormatCapture(Guid iid, int opnum, byte[] request, byte[] response)
    {
        var sb = new StringBuilder();
        sb.Append("# Opc.Classic AE wire capture (DR32/DR33 wire fixture)\n");
        sb.Append("# iid:     ").Append(iid.ToString("D", CultureInfo.InvariantCulture)).Append('\n');
        sb.Append("# opnum:   ").Append(opnum.ToString(CultureInfo.InvariantCulture)).Append('\n');
        sb.Append("# timestamp_utc: 2026-01-01T00:00:00.000\n");
        sb.Append("# stub_seed: deterministic-fixed-ftime-").Append(FixedFileTime).Append('\n');
        sb.Append('\n');
        sb.Append("## request (").Append(request.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes)\n");
        AppendHexDump(sb, request);
        sb.Append('\n');
        sb.Append("## response (").Append(response.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes)\n");
        AppendHexDump(sb, response);
        return sb.ToString();
    }

    private static void AppendHexDump(StringBuilder sb, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            sb.Append("  (empty)\n");
            return;
        }

        const int rowBytes = 16;
        for (int offset = 0; offset < bytes.Length; offset += rowBytes)
        {
            sb.Append(offset.ToString("X4", CultureInfo.InvariantCulture)).Append(": ");
            int end = Math.Min(offset + rowBytes, bytes.Length);
            for (int i = offset; i < end; i++)
            {
                sb.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture)).Append(' ');
            }
            // Pad short rows so the ASCII column lines up
            int pad = (offset + rowBytes) - end;
            for (int j = 0; j < pad; j++)
            {
                sb.Append("   ");
            }
            sb.Append(' ');
            for (int i = offset; i < end; i++)
            {
                byte b = bytes[i];
                sb.Append(b >= 0x20 && b < 0x7F ? (char)b : '.');
            }
            sb.Append('\n');
        }
    }

    private static string StripTimestampLine(string capture)
    {
        // Normalize CRLF -> LF up-front so the line-by-line filtering produces
        // a consistent canonical form regardless of whether the fixture file
        // was checked out with CRLF (Windows default for text=auto) or LF
        // (e.g. on Linux/macOS CI runners).
        string normalized = capture.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var sb = new StringBuilder(normalized.Length);
        foreach (string line in normalized.Split('\n'))
        {
            if (line.StartsWith("# timestamp_utc:", StringComparison.Ordinal))
            {
                continue;
            }
            sb.Append(line).Append('\n');
        }
        return sb.ToString();
    }

    /// <summary>In-memory <see cref="ICallChannel"/> that captures the request
    /// + response bytes for offline byte-diff analysis.</summary>
    private sealed class ByteCapturingChannel : ICallChannel
    {
        private readonly IOPCEventServerServerDispatcher _dispatcher;
        private readonly Action<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> _onCapture;

        public ByteCapturingChannel(
            IOPCEventServerServerDispatcher dispatcher,
            Action<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> onCapture)
        {
            _dispatcher = dispatcher;
            _onCapture = onCapture;
        }

        public async Task<NdrCallResult> InvokeAsync(
            Guid interfaceId, int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken = default)
        {
            _ = interfaceId;
            DispatchResult result = await _dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false);
            NdrCallResult ndr = result.ToNdrCallResult();
            _onCapture(requestPayload, ndr.ResponsePayload);
            return ndr;
        }
    }

    private sealed class DeterministicEventServer : IOPCEventServer
    {
        public bool ReturnNullOptionalStrings { get; set; }

        public Task<OpcConditionState> GetConditionStateAsync(
            string source, string conditionName, int[] attributeIds,
            CancellationToken cancellationToken = default)
        {
            _ = source;
            _ = conditionName;
            _ = attributeIds;
            _ = cancellationToken;
            return Task.FromResult(new OpcConditionState(
                state: 0x0007, // active + acked + enabled
                activeSubCondition: "HiHi",
                activeSubConditionDefinition: ReturnNullOptionalStrings ? null! : "value > 90",
                activeSubConditionSeverity: 700,
                activeSubConditionDescription: ReturnNullOptionalStrings ? null! : "High-high limit exceeded",
                quality: OpcQuality.Good,
                lastAckTime: FixedTimestamp,
                subConditionLastActive: FixedTimestamp,
                conditionLastActive: FixedTimestamp,
                conditionLastInactive: FixedTimestamp,
                acknowledgerId: ReturnNullOptionalStrings ? null : "operator1",
                comment: ReturnNullOptionalStrings ? null : "scheduled ack",
                subConditionNames: new[] { "Hi", "HiHi" },
                subConditionDefinitions: new[] { "value > 80", "value > 90" },
                subConditionSeverities: new uint[] { 500, 700 },
                subConditionDescriptions: new[] { "High limit", "High-high limit" },
                eventAttributes: Array.Empty<OpcVariant>(),
                errors: Array.Empty<int>()));
        }

        public Task<int[]> AckConditionAsync(
            int dwCount, string acknowledgerId, string comment,
            string[] sources, string[] conditionNames,
            long[] activeTimes, int[] cookies,
            CancellationToken cancellationToken = default)
        {
            _ = acknowledgerId;
            _ = comment;
            _ = sources;
            _ = conditionNames;
            _ = activeTimes;
            _ = cookies;
            _ = cancellationToken;
            return Task.FromResult(new int[dwCount]);
        }

        // The remaining IOPCEventServer surface is irrelevant to DR32/DR33;
        // throw to flag any accidental dispatch.
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task CreateEventSubscriptionAsync(bool active, int bufferTime, int maxSize, int clientSubscription, Guid requestedInterfaceId, out IOPCEventSubscriptionMgt subscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken ct = default) =>
            throw NotImpl(out subscription, out revisedBufferTime, out revisedMaxSize);
        public Task<int> QueryAvailableFiltersAsync(CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task QueryEventCategoriesAsync(int eventType, out int[] eventCategories, out string[] eventCategoryDescriptions, CancellationToken ct = default) =>
            throw NotImpl(out eventCategories, out eventCategoryDescriptions);
        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task QueryEventAttributesAsync(int eventCategory, out int[] attributeIds, out string[] attributeDescriptions, out ushort[] attributeTypes, CancellationToken ct = default) =>
            throw NotImpl(out attributeIds, out attributeDescriptions, out attributeTypes);
        public Task TranslateToItemIDsAsync(string source, int eventCategory, string conditionName, string subConditionName, int[] attributeIds, out string[] attributeItemIds, out string[] nodeNames, out Guid[] classIds, CancellationToken ct = default) =>
            throw NotImpl(out attributeItemIds, out nodeNames, out classIds);
        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken ct = default) =>
            throw new NotImplementedException();
        public Task CreateAreaBrowserAsync(Guid requestedInterfaceId, out IOPCEventAreaBrowser areaBrowser, CancellationToken ct = default) =>
            throw NotImpl(out areaBrowser);

        private static NotImplementedException NotImpl<T>(out T v) { v = default!; return new NotImplementedException(); }
        private static NotImplementedException NotImpl<T1, T2>(out T1 v1, out T2 v2) { v1 = default!; v2 = default!; return new NotImplementedException(); }
        private static NotImplementedException NotImpl<T1, T2, T3>(out T1 v1, out T2 v2, out T3 v3) { v1 = default!; v2 = default!; v3 = default!; return new NotImplementedException(); }
    }
}

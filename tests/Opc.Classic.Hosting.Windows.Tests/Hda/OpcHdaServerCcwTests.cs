// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Dcom;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

/// <summary>
/// Windows-only smoke tests for <see cref="OpcHdaServerCcw"/> — the HDA
/// parity to OpcDaServerCcw, providing IUnknown identity for SCM activation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcHdaServerCcwTests
{
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_zero_for_unsupported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), Guid.NewGuid());

        await Assert.That(ccw).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IID_IUnknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IID_IUnknown);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcHdaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IOPCHDA_Server_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new StubHdaServer(), IOPCHDA_Server.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task SupportsInterface_returns_true_for_known_iids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcHdaServerCcw.SupportsInterface(IID_IUnknown)).IsTrue();
        await Assert.That(OpcHdaServerCcw.SupportsInterface(IOPCHDA_Server.InterfaceId)).IsTrue();
        await Assert.That(OpcHdaServerCcw.SupportsInterface(OpcCommonClientProxy.InterfaceId)).IsTrue();
        await Assert.That(OpcHdaServerCcw.SupportsInterface(Guid.NewGuid())).IsFalse();
    }

    [Test]
    public async Task IOPCCommon_tearoff_round_trips_locale_error_text_and_client_name()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new StubHdaServer { SupportedLocales = [0x0409, 0x0411] };
        IntPtr ccw = OpcHdaServerCcw.Create(server, OpcCommonClientProxy.InterfaceId);
        CommonCcwResult result = ExerciseCommonCcw(ccw, 0x0411, "hda-client");

        await Assert.That(result.SetLocaleHresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.GetLocaleHresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.QueryLocalesHresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.GetErrorStringHresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.SetClientNameHresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(result.Locale).IsEqualTo(0x0411u);
        await Assert.That(result.Count).IsEqualTo(2u);
        await Assert.That(result.SecondLocale).IsEqualTo(0x0411);
        await Assert.That(result.ErrorText).IsEqualTo("HDA text 0x80004005");
        await Assert.That(server.ClientName).IsEqualTo("hda-client");
    }

    private static unsafe CommonCcwResult ExerciseCommonCcw(IntPtr ccw, uint localeToSet, string clientName)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var setLocale = (delegate* unmanaged<IntPtr, uint, int>)vtable[3];
        var getLocale = (delegate* unmanaged<IntPtr, uint*, int>)vtable[4];
        var queryLocales = (delegate* unmanaged<IntPtr, uint*, IntPtr*, int>)vtable[5];
        var getErrorString = (delegate* unmanaged<IntPtr, int, IntPtr*, int>)vtable[6];
        var setClientName = (delegate* unmanaged<IntPtr, IntPtr, int>)vtable[7];

        uint locale = 0;
        uint count = 0;
        IntPtr localePtr = IntPtr.Zero;
        IntPtr errorPtr = IntPtr.Zero;
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(clientName);
        try
        {
            int setLocaleHr = setLocale(ccw, localeToSet);
            int getLocaleHr = getLocale(ccw, &locale);
            int queryLocalesHr = queryLocales(ccw, &count, &localePtr);
            int getErrorStringHr = getErrorString(ccw, OpcResultId.Fail.Code, &errorPtr);
            int setClientNameHr = setClientName(ccw, namePtr);
            int secondLocale = localePtr == IntPtr.Zero ? 0 : Marshal.ReadInt32(localePtr, sizeof(int));
            string errorText = errorPtr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(errorPtr) ?? string.Empty;
            return new CommonCcwResult(setLocaleHr, getLocaleHr, queryLocalesHr, getErrorStringHr, setClientNameHr, locale, count, secondLocale, errorText);
        }
        finally
        {
            Marshal.FreeCoTaskMem(localePtr);
            Marshal.FreeCoTaskMem(errorPtr);
            Marshal.FreeCoTaskMem(namePtr);
        }
    }

    private sealed record CommonCcwResult(
        int SetLocaleHresult,
        int GetLocaleHresult,
        int QueryLocalesHresult,
        int GetErrorStringHresult,
        int SetClientNameHresult,
        uint Locale,
        uint Count,
        int SecondLocale,
        string ErrorText);
    private sealed class StubHdaServer : IOpcHdaServer
    {
        public IReadOnlyList<int> SupportedLocales { get; init; } = [0];
        public int LocaleId { get; private set; }
        public string ClientName { get; private set; } = string.Empty;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds?.Length ?? 0]);

        public Task<Opc.Classic.Hda.OpcHdaItem[]> ReadRawAsync(
            int[] serverHandles,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int numValues,
            bool returnBounds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<Opc.Classic.Hda.OpcHdaItem>());

        public Task<Opc.Classic.Hda.OpcHdaItem[]> ReadProcessedAsync(
            int[] serverHandles,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            long resampleInterval,
            int[] aggregates,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<Opc.Classic.Hda.OpcHdaItem>());

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
        {
            LocaleId = localeId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(SupportedLocales);

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult($"HDA text 0x{resultId.Code:X8}");

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ClientName = clientName;
            return Task.CompletedTask;
        }
    }
}

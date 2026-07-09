// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Raw CCW tests assert HRESULT constants.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Hosting.Windows;

namespace Opc.Classic.Hda.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcHdaSyncUpdateCcwTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task QueryCapabilities_returns_update_capability_mask()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        using Native.CoTaskMemBlock capabilities = Native.CoTaskMemBlock.Allocate(sizeof(int));

        int hr = Native.GetMethod<Native.QueryCapabilitiesDelegate>(syncUpdate, 3)(syncUpdate, capabilities.Pointer);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(Marshal.ReadInt32(capabilities.Pointer)).IsEqualTo(0x1F);
    }

    [Test]
    [Arguments(4, OpcHdaErrors.OPCHDA_S_INSERTED)]
    [Arguments(5, OpcHdaErrors.OPCHDA_S_REPLACED)]
    [Arguments(6, S_OK)]
    public async Task Insert_replace_and_insert_replace_dispatch_values(int slot, int expectedFirstError)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new UpdateServer();
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([10, 11]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1)]);
        using Native.DoubleVariantArray values = Native.DoubleVariantArray.From([1.5, 2.5]);
        using Native.IntArray qualities = Native.IntArray.From([192, 192]);

        Native.UpdateDelegate method = Native.GetMethod<Native.UpdateDelegate>(syncUpdate, slot);
        int hr = method(syncUpdate, 2, handles.Pointer, timestamps.Pointer, values.Pointer, qualities.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(errors[0]).IsEqualTo(expectedFirstError);
        await Assert.That(server.LastValues![1].AsDouble()).IsEqualTo(2.5);
        await Assert.That(server.LastQualities![0]).IsEqualTo(192);
    }

    [Test]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    public async Task Insert_replace_and_insert_replace_return_per_item_invalid_handle(int slot)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([10, 404]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1)]);
        using Native.DoubleVariantArray values = Native.DoubleVariantArray.From([1.5, 2.5]);
        using Native.IntArray qualities = Native.IntArray.From([192, 192]);

        Native.UpdateDelegate method = Native.GetMethod<Native.UpdateDelegate>(syncUpdate, slot);
        int hr = method(syncUpdate, 2, handles.Pointer, timestamps.Pointer, values.Pointer, qualities.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);

        await Assert.That(hr).IsEqualTo(S_FALSE);
        await Assert.That(errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
    }

    [Test]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    public async Task Insert_replace_and_insert_replace_reject_count_zero(int slot)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        Native.UpdateDelegate method = Native.GetMethod<Native.UpdateDelegate>(syncUpdate, slot);

        int hr = method(syncUpdate, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out IntPtr errorsPtr);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(errorsPtr).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task DeleteRaw_dispatches_range_and_returns_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new UpdateServer();
        IntPtr ccw = OpcHdaServerCcw.Create(server, IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        using Native.TimeBlock start = Native.TimeBlock.From(DateTimeOffset.UtcNow.AddHours(-1));
        using Native.TimeBlock end = Native.TimeBlock.From(DateTimeOffset.UtcNow);
        using Native.IntArray handles = Native.IntArray.From([10, 404]);

        Native.DeleteRawDelegate method = Native.GetMethod<Native.DeleteRawDelegate>(syncUpdate, 7);
        int hr = method(syncUpdate, start.Pointer, end.Pointer, 2, handles.Pointer, out IntPtr errorsPtr);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 2);

        await Assert.That(hr).IsEqualTo(S_FALSE);
        await Assert.That(errors[0]).IsEqualTo(S_OK);
        await Assert.That(errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
        await Assert.That(server.DeleteRawCalls).IsEqualTo(1);
    }

    [Test]
    public async Task DeleteAtTime_dispatches_timestamps_and_rejects_count_zero()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcHdaServerCcw.Create(new UpdateServer(), IID_IUnknown);
        IntPtr syncUpdate = Native.InvokeQI(ccw, IOPCHDA_SyncUpdate.InterfaceId);
        using Native.IntArray handles = Native.IntArray.From([10]);
        using Native.FileTimeArray timestamps = Native.FileTimeArray.From([DateTimeOffset.UtcNow]);
        Native.DeleteAtTimeDelegate method = Native.GetMethod<Native.DeleteAtTimeDelegate>(syncUpdate, 8);

        int hr = method(syncUpdate, 1, handles.Pointer, timestamps.Pointer, out IntPtr errorsPtr);
        int zeroHr = method(syncUpdate, 0, IntPtr.Zero, IntPtr.Zero, out IntPtr zeroErrors);
        int[] errors = Native.ReadAndFreeErrors(errorsPtr, 1);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(errors[0]).IsEqualTo(S_OK);
        await Assert.That(zeroHr).IsEqualTo(E_INVALIDARG);
        await Assert.That(zeroErrors).IsEqualTo(IntPtr.Zero);
    }

    private sealed class UpdateServer : IOpcHdaServer, IOPCHDA_SyncUpdate
    {
        public OpcVariant[]? LastValues { get; private set; }
        public int[]? LastQualities { get; private set; }
        public int DeleteRawCalls { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Hda });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(new int[itemIds.Length]);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0x1F);

        public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            LastValues = dataValues;
            LastQualities = qualities;
            _ = timestampFileTimes;
            return Task.FromResult(Errors(serverHandles, OpcHdaErrors.OPCHDA_S_INSERTED));
        }

        public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            LastValues = dataValues;
            LastQualities = qualities;
            _ = timestampFileTimes;
            return Task.FromResult(Errors(serverHandles, OpcHdaErrors.OPCHDA_S_REPLACED));
        }

        public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            LastValues = dataValues;
            LastQualities = qualities;
            _ = timestampFileTimes;
            return Task.FromResult(Errors(serverHandles, S_OK));
        }

        public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            _ = startTime;
            _ = endTime;
            DeleteRawCalls++;
            return Task.FromResult(Errors(serverHandles, S_OK));
        }

        public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
        {
            _ = timestampFileTimes;
            return Task.FromResult(Errors(serverHandles, S_OK));
        }

        private static int[] Errors(int[] handles, int success)
        {
            var errors = new int[handles.Length];
            for (int i = 0; i < handles.Length; i++)
            {
                errors[i] = handles[i] == 404 ? OpcResultId.InvalidHandle.Code : success;
            }

            return errors;
        }
    }

    private static class Native
    {
        private static int HdaTimeSize => IntPtr.Size == 8 ? 24 : 16;
        private static int HdaTimeFileTimeOffset => IntPtr.Size == 8 ? 16 : 8;
        private static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        internal static int[] ReadAndFreeErrors(IntPtr ptr, int count)
        {
            var values = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, values, 0, count);
                Marshal.FreeCoTaskMem(ptr);
            }

            return values;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int QueryCapabilitiesDelegate(IntPtr pThis, IntPtr capabilities);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int UpdateDelegate(IntPtr pThis, uint count, IntPtr handles, IntPtr timestamps, IntPtr values, IntPtr qualities, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int DeleteRawDelegate(IntPtr pThis, IntPtr start, IntPtr end, uint count, IntPtr handles, out IntPtr errors);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int DeleteAtTimeDelegate(IntPtr pThis, uint count, IntPtr handles, IntPtr timestamps, out IntPtr errors);

        internal sealed class IntArray : IDisposable
        {
            private IntArray(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static IntArray From(int[] values)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
                Marshal.Copy(values, 0, ptr, values.Length);
                return new IntArray(ptr);
            }
            public void Dispose() => Marshal.FreeCoTaskMem(Pointer);
        }

        internal sealed class FileTimeArray : IDisposable
        {
            private FileTimeArray(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static FileTimeArray From(DateTimeOffset[] values)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(long));
                for (int i = 0; i < values.Length; i++)
                {
                    Marshal.WriteInt64(ptr, i * sizeof(long), values[i].ToFileTime());
                }
                return new FileTimeArray(ptr);
            }
            public void Dispose() => Marshal.FreeCoTaskMem(Pointer);
        }

        internal sealed class DoubleVariantArray : IDisposable
        {
            private DoubleVariantArray(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static DoubleVariantArray From(double[] values)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * VariantSize);
                for (int i = 0; i < values.Length; i++)
                {
                    IntPtr slot = IntPtr.Add(ptr, i * VariantSize);
                    Marshal.WriteInt16(slot, 0, 5);
                    Marshal.WriteInt64(slot, 8, BitConverter.DoubleToInt64Bits(values[i]));
                }
                return new DoubleVariantArray(ptr);
            }
            public void Dispose() => Marshal.FreeCoTaskMem(Pointer);
        }

        internal sealed class TimeBlock : IDisposable
        {
            private TimeBlock(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static TimeBlock From(DateTimeOffset value)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(HdaTimeSize);
                Marshal.WriteInt32(ptr, 0);
                Marshal.WriteIntPtr(ptr, IntPtr.Size == 8 ? 8 : 4, IntPtr.Zero);
                Marshal.WriteInt64(ptr, HdaTimeFileTimeOffset, value.ToFileTime());
                return new TimeBlock(ptr);
            }
            public void Dispose() => Marshal.FreeCoTaskMem(Pointer);
        }

        internal sealed class CoTaskMemBlock : IDisposable
        {
            private CoTaskMemBlock(IntPtr pointer) => Pointer = pointer;
            public IntPtr Pointer { get; }
            public static CoTaskMemBlock Allocate(int byteCount)
            {
                IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
                Span<byte> zeros = stackalloc byte[byteCount];
                Marshal.Copy(zeros.ToArray(), 0, ptr, byteCount);
                return new CoTaskMemBlock(ptr);
            }
            public void Dispose() => Marshal.FreeCoTaskMem(Pointer);
        }
    }
}

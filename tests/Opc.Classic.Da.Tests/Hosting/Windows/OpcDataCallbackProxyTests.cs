//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>Windows-only unit tests for <see cref="OpcDataCallbackProxy"/>.</summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDataCallbackProxyTests
{
    [Test]
    public async Task Construction_succeeds_when_client_supports_IOPCDataCallback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);

            await Assert.That(proxy).IsNotNull();
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(2L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task Construction_throws_when_client_lacks_IOPCDataCallback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: false);
        try
        {
            await Assert.That(() =>
            {
                using var proxy = new OpcDataCallbackProxy(stub);
            }).Throws<COMException>();
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task OnCancelComplete_invokes_opnum_6_with_payload_values()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            using var proxy = new OpcDataCallbackProxy(stub);
            proxy.OnCancelComplete(new OpcDaGroup.CancelCompletePayload(123, 456));

            Helpers.Invocation invocation = Helpers.GetLastInvocation(stub);
            await Assert.That(invocation.Opnum).IsEqualTo(6);
            await Assert.That(invocation.TransactionId).IsEqualTo(123U);
            await Assert.That(invocation.GroupHandle).IsEqualTo(456U);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    [Test]
    public async Task Dispose_releases_held_IOPCDataCallback_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateStub(supportsDataCallback: true);
        try
        {
            var proxy = new OpcDataCallbackProxy(stub);
            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(2L);

            proxy.Dispose();

            await Assert.That(Helpers.GetReferenceCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyStub(stub);
        }
    }

    private static unsafe class Helpers
    {
        private const int S_OK = 0;
        private const int E_NOINTERFACE = unchecked((int)0x80004002);
        private const int E_POINTER = unchecked((int)0x80004003);
        private const int VtableSlotCount = 7;

        private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
        private static readonly Guid s_iidDataCallback = IOPCDataCallback.InterfaceId;
        private static readonly ConcurrentDictionary<IntPtr, StubSession> s_sessions = new();

        internal readonly record struct Invocation(int Opnum, uint TransactionId, uint GroupHandle);

        internal static IntPtr CreateStub(bool supportsDataCallback)
        {
            IntPtr* vtable = AllocateVtable();
            IntPtr instance = AllocateInstance(vtable);
            s_sessions[instance] = new StubSession(vtable, supportsDataCallback);
            return instance;
        }

        internal static void DestroyStub(IntPtr stub)
        {
            if (!s_sessions.TryRemove(stub, out StubSession? session))
            {
                return;
            }

            NativeMemory.Free((void*)stub);
            NativeMemory.Free(session.Vtable);
        }

        internal static long GetReferenceCount(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? Interlocked.Read(ref session.RefCount)
                : -1L;

        internal static Invocation GetLastInvocation(IntPtr stub) =>
            s_sessions.TryGetValue(stub, out StubSession? session)
                ? session.LastInvocation
                : default;

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr* AllocateVtable()
        {
            IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(VtableSlotCount * sizeof(IntPtr)));
            vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
            vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
            vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
            vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OnDataChange;
            vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OnReadComplete;
            vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, uint, IntPtr, IntPtr, int>)&OnWriteComplete;
            vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int>)&OnCancelComplete;
            return vtable;
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr AllocateInstance(IntPtr* vtable)
        {
            IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
            instance[0] = (IntPtr)vtable;
            return (IntPtr)instance;
        }

        [UnmanagedCallersOnly]
        private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
        {
            if (ppv == null)
            {
                return E_POINTER;
            }
            if (!s_sessions.TryGetValue(pThis, out StubSession? session) || riid == null)
            {
                *ppv = IntPtr.Zero;
                return E_NOINTERFACE;
            }

            if (*riid == s_iidUnknown || (session.SupportsDataCallback && *riid == s_iidDataCallback))
            {
                *ppv = pThis;
                Interlocked.Increment(ref session.RefCount);
                return S_OK;
            }

            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        [UnmanagedCallersOnly]
        private static uint AddRef(IntPtr pThis)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return 1;
            }
            return (uint)Interlocked.Increment(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static uint Release(IntPtr pThis)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return 0;
            }
            return (uint)Interlocked.Decrement(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static int OnDataChange(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors) => Record(pThis, 3, transactionId, groupHandle);

        [UnmanagedCallersOnly]
        private static int OnReadComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors) => Record(pThis, 4, transactionId, groupHandle);

        [UnmanagedCallersOnly]
        private static int OnWriteComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr errors) => Record(pThis, 5, transactionId, groupHandle);

        [UnmanagedCallersOnly]
        private static int OnCancelComplete(IntPtr pThis, uint transactionId, uint groupHandle) =>
            Record(pThis, 6, transactionId, groupHandle);

        private static int Record(IntPtr pThis, int opnum, uint transactionId, uint groupHandle)
        {
            if (!s_sessions.TryGetValue(pThis, out StubSession? session))
            {
                return E_NOINTERFACE;
            }
            session.LastInvocation = new Invocation(opnum, transactionId, groupHandle);
            return S_OK;
        }

        private sealed class StubSession(IntPtr* vtable, bool supportsDataCallback)
        {
            internal readonly IntPtr* Vtable = vtable;
            internal readonly bool SupportsDataCallback = supportsDataCallback;
            internal long RefCount = 1;
            internal Invocation LastInvocation;
        }
    }
}

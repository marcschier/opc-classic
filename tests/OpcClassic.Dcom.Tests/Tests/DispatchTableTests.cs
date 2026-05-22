//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Reflection;
using SharpInterop.Core;
using TUnit.Core;

namespace OpcClassic.Dcom.Tests;

public sealed class DispatchTableTests
{
    [Test]
    public async Task ReflectionDispatchTable_returns_dispatcher_for_registered_method()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var method = GetDispatchMethod(nameof(DispatchTarget.Echo));
        var table = CreateDispatchTable(target, iid, 7, method);

        var found = TryGetDispatcher(table, iid, 7, out var dispatcher);

        await Assert.That(found).IsTrue();
        await Assert.That(dispatcher).IsNotNull();
    }

    [Test]
    public async Task ReflectionDispatchTable_returns_false_for_unregistered_iid_opnum()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var method = GetDispatchMethod(nameof(DispatchTarget.Echo));
        var table = CreateDispatchTable(target, iid, 7, method);

        var found = TryGetDispatcher(table, Guid.NewGuid(), 8, out var dispatcher);

        await Assert.That(found).IsFalse();
        await Assert.That(dispatcher).IsNull();
    }

    [Test]
    public async Task ReflectionDispatchTable_dispatcher_calls_target_method()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var method = GetDispatchMethod(nameof(DispatchTarget.Echo));
        var table = CreateDispatchTable(target, iid, 7, method);

        TryGetDispatcher(table, iid, 7, out var dispatcher);
        var result = dispatcher!(new object[] { "payload" });

        await Assert.That(result).IsEqualTo("echo:payload");
    }

    private static object CreateDispatchTable(object target, Guid iid, int opnum, MethodInfo method)
    {
        var tableType = typeof(LocalCoClass).Assembly.GetType(
            "SharpInterop.Core.ReflectionDispatchTable", throwOnError: true)!;
        var registrationType = typeof(ValueTuple<Guid, int, MethodInfo>);
        var registrations = Array.CreateInstance(registrationType, 1);
        registrations.SetValue((iid, opnum, method), 0);

        return Activator.CreateInstance(tableType, target, registrations)!;
    }

    private static bool TryGetDispatcher(
        object table,
        Guid iid,
        int opnum,
        out Func<object[], object?>? dispatcher)
    {
        var method = table.GetType().GetMethod(nameof(TryGetDispatcher))!;
        object?[] args = new object?[] { iid, opnum, null };
        var found = (bool)method.Invoke(table, args)!;
        dispatcher = (Func<object[], object?>?)args[2];
        return found;
    }

    private static MethodInfo GetDispatchMethod(string methodName) =>
        typeof(DispatchTarget).GetMethod(methodName)!;

    private sealed class DispatchTarget
    {
        public string Echo(string value) => $"echo:{value}";
    }
}

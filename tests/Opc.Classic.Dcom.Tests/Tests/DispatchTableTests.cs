// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class DispatchTableTests
{
    [Test]
    public async Task DelegateDispatchTable_returns_dispatcher_for_registered_method()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var table = CreateDispatchTable(iid, 7, args => target.Echo((string)args[0]));

        var found = TryGetDispatcher(table, iid, 7, out var dispatcher);

        await Assert.That(found).IsTrue();
        await Assert.That(dispatcher).IsNotNull();
    }

    [Test]
    public async Task DelegateDispatchTable_returns_false_for_unregistered_iid_opnum()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var table = CreateDispatchTable(iid, 7, args => target.Echo((string)args[0]));

        var found = TryGetDispatcher(table, Guid.NewGuid(), 8, out var dispatcher);

        await Assert.That(found).IsFalse();
        await Assert.That(dispatcher).IsNull();
    }

    [Test]
    public async Task DelegateDispatchTable_dispatcher_calls_target_method()
    {
        var target = new DispatchTarget();
        var iid = Guid.NewGuid();
        var table = CreateDispatchTable(iid, 7, args => target.Echo((string)args[0]));

        TryGetDispatcher(table, iid, 7, out var dispatcher);
        var result = dispatcher!(new object[] { "payload" });

        await Assert.That(result).IsEqualTo("echo:payload");
    }

    private static object CreateDispatchTable(Guid iid, int opnum, Func<object[], object?> dispatcher)
    {
        var tableType = typeof(LocalCoClass).Assembly.GetType(
            "Opc.Classic.Dcom.Core.DelegateDispatchTable", throwOnError: true)!;
        var registrationType = typeof(ValueTuple<Guid, int, Func<object[], object?>>);
        var registrations = Array.CreateInstance(registrationType, 1);
        registrations.SetValue((iid, opnum, dispatcher), 0);

        return Activator.CreateInstance(tableType, registrations)!;
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

    private sealed class DispatchTarget
    {
        public string Echo(string value) => $"echo:{value}";
    }
}

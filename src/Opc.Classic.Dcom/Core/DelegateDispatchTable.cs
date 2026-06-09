//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

internal sealed class DelegateDispatchTable : IDispatchTable {
    private readonly Dictionary<(Guid Iid, int Opnum), Func<object[], object?>> _dispatchers = new();

    public DelegateDispatchTable(
        IEnumerable<(Guid Iid, int Opnum, Func<object[], object?> Dispatcher)> registrations) {
        ArgumentNullException.ThrowIfNull(registrations);

        foreach (var (iid, opnum, dispatcher) in registrations) {
            _dispatchers[(iid, opnum)] = dispatcher ?? throw new ArgumentNullException(nameof(registrations));
        }
    }

    public bool TryGetDispatcher(Guid iid, int opnum, out Func<object[], object?> dispatcher) {
        if (_dispatchers.TryGetValue((iid, opnum), out dispatcher!)) {
            return true;
        }

        dispatcher = null!;
        return false;
    }
}

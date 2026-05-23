//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpInterop.Core;

internal sealed class ReflectionDispatchTable : IDispatchTable
{
    private readonly Dictionary<(Guid Iid, int Opnum), MethodInfo> _methods = new();
    private readonly object _target;

    public ReflectionDispatchTable(
        object target,
        IEnumerable<(Guid Iid, int Opnum, MethodInfo Method)> registrations)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

        if (registrations == null)
        {
            throw new ArgumentNullException(nameof(registrations));
        }

        foreach (var (iid, opnum, method) in registrations)
        {
            _methods[(iid, opnum)] = method ?? throw new ArgumentNullException(nameof(registrations));
        }
    }

    public bool TryGetDispatcher(Guid iid, int opnum, out Func<object[], object?> dispatcher)
    {
        if (_methods.TryGetValue((iid, opnum), out var method))
        {
            // TODO N1.2-followup: remove this MethodInfo.Invoke fallback once all LocalCoClass
            // registrations are source-generated IDispatchTable delegates.
            dispatcher = args => method.Invoke(_target, args);
            return true;
        }

        dispatcher = null!;
        return false;
    }
}

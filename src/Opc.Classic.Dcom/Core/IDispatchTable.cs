// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

internal interface IDispatchTable
{
    bool TryGetDispatcher(Guid iid, int opnum, out Func<object[], object?> dispatcher);
}

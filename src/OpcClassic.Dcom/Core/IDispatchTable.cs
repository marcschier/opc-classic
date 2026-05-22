//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace SharpInterop.Core;

internal interface IDispatchTable
{
    bool TryGetDispatcher(Guid iid, int opnum, out Func<object[], object?> dispatcher);
}

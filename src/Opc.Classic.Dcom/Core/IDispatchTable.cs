//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Core;

internal interface IDispatchTable
{
    bool TryGetDispatcher(Guid iid, int opnum, out Func<object[], object?> dispatcher);
}

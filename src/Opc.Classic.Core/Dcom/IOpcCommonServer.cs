//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Dcom;

/// <summary>Server implementation contract for OPC Common <c>IOPCCommon</c> debug metadata.</summary>
public interface IOpcCommonServer
{
    /// <summary>Stores the optional client name supplied by <c>IOPCCommon::SetClientName</c>.</summary>
    Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default);
}

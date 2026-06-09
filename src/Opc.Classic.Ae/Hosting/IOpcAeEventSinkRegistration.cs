//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>In-process registration surface for AE event-sink callbacks.</summary>
public interface IOpcAeEventSinkRegistration {
    /// <summary>Registers a client event sink and returns the connection cookie.</summary>
    Task<int> AdviseEventSinkAsync(IOPCEventSink sink, CancellationToken cancellationToken = default);

    /// <summary>Unregisters a client event sink by connection cookie.</summary>
    Task UnadviseEventSinkAsync(int connection, CancellationToken cancellationToken = default);
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Hosting;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Marker for DCE/RPC interfaces whose stubs are plain NDR, not DCOM ORPC envelopes.
/// </summary>
public interface IRpcRawNdrDispatcher : IOpcServerDispatcher
{
}

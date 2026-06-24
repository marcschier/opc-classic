// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Creates transport-backed DA data-callback sinks from client-supplied OBJREFs.
/// </summary>
public interface IOpcDataCallbackSinkFactory
{
    /// <summary>
    /// Creates a callback sink for an advised <c>IOPCDataCallback</c> interface reference.
    /// </summary>
    IOpcDataCallbackSink Create(IOpcInterfaceRef sink);
}

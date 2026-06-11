// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Endpoint interface
/// </summary>
public interface IEndpoint
{

    /// <summary>
    /// Transport
    /// </summary>
    ITransport Transport { get; }

    /// <summary>
    /// Syntax
    /// </summary>
    PresentationSyntax Syntax { get; }

    /// <summary>
    /// Call
    /// </summary>
    /// <param name="semantics">Parameter semantics flags that describe direction and marshaling behavior.</param>
    /// <param name="object">NDR operation object that encodes the request and decodes the response.</param>
    /// <param name="opnum">RPC operation number to invoke on the remote interface.</param>
    /// <param name="ndrobj">NDR object whose wire fields are being encoded or decoded.</param>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Call(Semantics semantics, UUID @object, int opnum, NdrOp ndrobj);

    /// <summary>
    /// Detach
    /// </summary>
    /// <exception cref="IOException">Thrown when the underlying stream, socket, or named pipe read/write operation fails.</exception>
    void Detach();
}

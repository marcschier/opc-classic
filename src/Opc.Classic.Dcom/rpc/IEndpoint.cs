// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Endpoint interface
/// </summary>
public interface IEndpoint {

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
    /// <param name="semantics"></param>
    /// <param name="object"></param>
    /// <param name="opnum"></param>
    /// <param name="ndrobj"></param>
    /// <exception cref="IOException"></exception>
    void Call(Semantics semantics, UUID @object, int opnum, NdrOp ndrobj);

    /// <summary>
    /// Detach
    /// </summary>
    /// <exception cref="IOException"></exception>
    void Detach();
}

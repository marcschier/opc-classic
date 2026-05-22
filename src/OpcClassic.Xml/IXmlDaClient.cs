//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace OpcClassic.Xml;

/// <summary>
/// Transport-agnostic OPC XML-DA 1.0 client. The default implementation
/// (<see cref="HttpXmlDaClient"/>) speaks SOAP-over-HTTP; alternate impls
/// can shim in-memory transports for testing or layered architectures.
/// </summary>
public interface IXmlDaClient
{
    /// <summary>
    /// Sends a <c>GetStatus</c> request and returns the server's status.
    /// </summary>
    Task<XmlDaServerStatus> GetStatusAsync(XmlDaRequestHeader header, CancellationToken cancellationToken = default);
}

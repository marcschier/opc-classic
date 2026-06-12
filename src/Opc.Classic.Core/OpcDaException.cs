//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>OPC Data Access (DA) specification errors.</summary>
public sealed class OpcDaException : OpcException
{
    public OpcDaException() { }
    public OpcDaException(string message) : base(message) { }
    public OpcDaException(string message, Exception innerException) : base(message, innerException) { }
    public OpcDaException(OpcResultId resultId) : base(resultId) { }
    public OpcDaException(OpcResultId resultId, string message) : base(resultId, message) { }
    public OpcDaException(OpcResultId resultId, string message, Exception innerException) : base(resultId, message, innerException) { }
}

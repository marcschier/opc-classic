//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>OPC Historical Data Access (HDA) specification errors.</summary>
public sealed class OpcHdaException : OpcException
{
    public OpcHdaException() { }
    public OpcHdaException(string message) : base(message) { }
    public OpcHdaException(string message, Exception innerException) : base(message, innerException) { }
    public OpcHdaException(OpcResultId resultId) : base(resultId) { }
    public OpcHdaException(OpcResultId resultId, string message) : base(resultId, message) { }
    public OpcHdaException(OpcResultId resultId, string message, Exception innerException) : base(resultId, message, innerException) { }
}

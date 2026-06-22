// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// OPC Alarms &amp; Events (AE) specification errors.
/// </summary>
public sealed class OpcAeException : OpcException
{
    public OpcAeException() { }
    public OpcAeException(string message) : base(message) { }
    public OpcAeException(string message, Exception innerException) : base(message, innerException) { }
    public OpcAeException(OpcResultId resultId) : base(resultId) { }
    public OpcAeException(OpcResultId resultId, string message) : base(resultId, message) { }
    public OpcAeException(OpcResultId resultId, string message, Exception innerException) : base(resultId, message, innerException) { }
}

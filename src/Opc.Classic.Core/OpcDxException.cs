// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// OPC Data eXchange (DX) specification errors.
/// </summary>
public sealed class OpcDxException : OpcException
{
    public OpcDxException() { }
    public OpcDxException(string message) : base(message) { }
    public OpcDxException(string message, Exception innerException) : base(message, innerException) { }
    public OpcDxException(OpcResultId resultId) : base(resultId) { }
    public OpcDxException(OpcResultId resultId, string message) : base(resultId, message) { }
    public OpcDxException(OpcResultId resultId, string message, Exception innerException) : base(resultId, message, innerException) { }
}

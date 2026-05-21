//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic;

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

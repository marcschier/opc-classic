// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Raised when the client/server tried to use a protocol feature that is
/// unavailable on the current platform (e.g. legacy NTLM SSO on net10).
/// </summary>
public sealed class OpcPlatformNotSupportedException : OpcException
{
    public OpcPlatformNotSupportedException() : base("OPC feature is not supported on this platform.") { }
    public OpcPlatformNotSupportedException(string message) : base(message) { }
    public OpcPlatformNotSupportedException(string message, Exception innerException) : base(message, innerException) { }

    public OpcPlatformNotSupportedException(OpcResultId resultId) : base(resultId)
    {
    }

    public OpcPlatformNotSupportedException(OpcResultId resultId, string message) : base(resultId, message)
    {
    }

    public OpcPlatformNotSupportedException(OpcResultId resultId, string message, Exception innerException) : base(resultId, message, innerException)
    {
    }
}

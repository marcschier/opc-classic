//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic;

/// <summary>
/// Raised when the client/server tried to use a protocol feature that is
/// unavailable on the current platform (e.g. legacy NTLM SSO on net10).
/// </summary>
public sealed class OpcPlatformNotSupportedException : OpcException
{
    public OpcPlatformNotSupportedException() : base("OPC feature is not supported on this platform.") { }
    public OpcPlatformNotSupportedException(string message) : base(message) { }
    public OpcPlatformNotSupportedException(string message, Exception innerException) : base(message, innerException) { }
}

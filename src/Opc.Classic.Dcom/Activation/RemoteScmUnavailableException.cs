// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// The remote SCM does not expose the modern activation interface.
/// </summary>
public sealed class RemoteScmUnavailableException : InvalidOperationException
{
    public RemoteScmUnavailableException()
    {
    }

    public RemoteScmUnavailableException(string message)
        : base(message)
    {
    }

    public RemoteScmUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

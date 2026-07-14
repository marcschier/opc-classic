// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// A modern activation response could not be decoded.
/// </summary>
public sealed class ActivationPropertiesFormatException : InvalidOperationException
{
    public ActivationPropertiesFormatException()
    {
    }

    public ActivationPropertiesFormatException(string message)
        : base(message)
    {
    }

    public ActivationPropertiesFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

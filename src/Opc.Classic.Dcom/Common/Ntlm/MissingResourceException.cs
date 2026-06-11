// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public sealed class MissingResourceException : Exception
{
    public MissingResourceException()
    {
    }

    public MissingResourceException(string? message) : base(message)
    {
    }

    public MissingResourceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

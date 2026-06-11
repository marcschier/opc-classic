// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public sealed class NoSuchElementException : Exception
{
    public NoSuchElementException()
    {
    }

    public NoSuchElementException(string? message) : base(message)
    {
    }

    public NoSuchElementException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public sealed class UnknownHostException : Exception
{
    public UnknownHostException()
    {
    }

    public UnknownHostException(string message)
        : base(message)
    {
    }

    public UnknownHostException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

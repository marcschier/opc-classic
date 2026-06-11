// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Util.Sharpen;

public sealed class InstantiationException : Exception
{
    public InstantiationException()
    {
    }

    public InstantiationException(string? message) : base(message)
    {
    }

    public InstantiationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

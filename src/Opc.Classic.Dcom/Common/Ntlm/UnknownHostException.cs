// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Util.Sharpen;

public sealed class UnknownHostException : Exception
{
    public UnknownHostException()
    {
    }

    public UnknownHostException(string message)
        : base(message)
    {
    }
}

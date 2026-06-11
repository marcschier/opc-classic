// SPDX-License-Identifier: MIT

using System;

namespace SharpCifs.Util.Sharpen;

public sealed class UnsupportedEncodingException : Exception
{
    public UnsupportedEncodingException()
    {
    }

    public UnsupportedEncodingException(string? message) : base(message)
    {
    }

    public UnsupportedEncodingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

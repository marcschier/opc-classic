// SPDX-License-Identifier: EPL-1.0

using System;
using System.IO;

namespace OpcClassic.Dcom.Internal.LegacyNdr;

public class NdrException : IOException
{
    public const int Subprotocol = 0;
    public const int NoNullRefReason = 1;
    public const int InvalidArrayConformance = 2;

    public const string NoNullRef = "ref pointer cannot be null";
    public const string InvalidConformance = "invalid array conformance";

    public NdrException()
    {
    }

    public NdrException(string message)
        : base(message)
    {
    }

    public NdrException(string message, int reason)
        : base(message)
    {
        Reason = reason;
    }

    public NdrException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public int Reason { get; }

    public int GetReason() => Reason;
}

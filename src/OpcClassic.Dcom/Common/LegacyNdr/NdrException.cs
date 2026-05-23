// SPDX-License-Identifier: EPL-1.0

using System.IO;

namespace OpcClassic.Dcom.Internal.LegacyNdr;

public class NdrException : IOException
{
    public const string NoNullRef = "ref pointer cannot be null";
    public const string InvalidConformance = "invalid array conformance";

    public NdrException(string message)
        : base(message)
    {
    }
}

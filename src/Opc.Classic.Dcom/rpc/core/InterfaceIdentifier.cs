// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common.Ntlm;
using System.Globalization;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Interface id
/// </summary>
public class InterfaceIdentifier : NdrOp
{

    /// <summary>
    /// Id
    /// </summary>
    public UUID Uuid { get; set; }

    /// <summary>
    /// Major
    /// </summary>
    public int MajorVersion { get; set; }

    /// <summary>
    /// Minor
    /// </summary>
    public int MinorVersion { get; set; }

    /// <summary>
    /// Create id
    /// </summary>
    /// <param name="syntax"></param>
    public InterfaceIdentifier(string syntax)
    {
        var tokens = syntax.Split(new[] { ':', '.' });
        if (tokens.Length < 3)
        {
            throw new ArgumentException(
                "Interface identifier must have the form <uuid>:<major>.<minor>.", nameof(syntax));
        }
        Uuid.Parse(tokens[0]);
        MajorVersion = int.Parse(tokens[1], CultureInfo.InvariantCulture);
        MinorVersion = int.Parse(tokens[2], CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Create id
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="majorVersion"></param>
    /// <param name="minorVersion"></param>
    public InterfaceIdentifier(UUID uuid, int majorVersion,
        int minorVersion)
    {
        Uuid = uuid;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <override/>
    public override string ToString() => Uuid + ":" + MajorVersion + "." + MinorVersion;
}

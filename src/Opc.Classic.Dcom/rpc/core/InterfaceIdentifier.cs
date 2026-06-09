// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System.Globalization;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Interface id
/// </summary>
public class InterfaceIdentifier : NdrOp {

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
    public InterfaceIdentifier(string syntax) {
        var tokenizer = new StringTokenizer(syntax, ":.");
        Uuid.Parse(tokenizer.NextToken());
        MajorVersion = int.Parse(tokenizer.NextToken(), CultureInfo.InvariantCulture);
        MinorVersion = int.Parse(tokenizer.NextToken(), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Create id
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="majorVersion"></param>
    /// <param name="minorVersion"></param>
    public InterfaceIdentifier(UUID uuid, int majorVersion,
        int minorVersion) {
        Uuid = uuid;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <override/>
    public override string ToString() => Uuid + ":" + MajorVersion + "." + MinorVersion;
}

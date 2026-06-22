// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;
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
    /// <param name="syntax">Presentation syntax negotiated for the RPC context.</param>
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
    /// <param name="uuid">UUID value encoded in the RPC or COM descriptor.</param>
    /// <param name="majorVersion">Major version component of the protocol or COM descriptor.</param>
    /// <param name="minorVersion">Minor version component of the protocol or COM descriptor.</param>
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

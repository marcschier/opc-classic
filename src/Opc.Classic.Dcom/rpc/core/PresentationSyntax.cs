// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Globalization;

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Presentation syntax
/// </summary>
public class PresentationSyntax : NdrOp
{
    /// <summary>
    /// Uuuid
    /// </summary>
    public UUID Uuid { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Major
    /// </summary>
    public int MajorVersion => Version & 0xffff;

    /// <summary>
    /// Minor
    /// </summary>
    public int MinorVersion => (Version >> 16) & 0xffff;

    /// <summary>
    /// Create syntax
    /// </summary>
    public PresentationSyntax()
    {
    }

    /// <summary>
    /// Create presentation syntax
    /// </summary>
    /// <param name="syntax">Presentation syntax negotiated for the RPC context.</param>
    public PresentationSyntax(string syntax) : this()
    {
        ArgumentNullException.ThrowIfNull(syntax);

        var versionSeparator = syntax.LastIndexOf(':');
        if (versionSeparator < 0 || versionSeparator == syntax.Length - 1)
        {
            throw new FormatException("Presentation syntax must include a version suffix.");
        }

        var version = syntax[(versionSeparator + 1)..];
        var minorSeparator = version.IndexOf('.');
        if (minorSeparator < 0 || minorSeparator == version.Length - 1)
        {
            throw new FormatException("Presentation syntax version must be major.minor.");
        }

        Uuid = new UUID(syntax[..versionSeparator]);
        Version = (int.Parse(version[..minorSeparator], CultureInfo.InvariantCulture) & 0xffff) |
            (int.Parse(version[(minorSeparator + 1)..], CultureInfo.InvariantCulture) << 16);
    }

    /// <summary>
    /// Create syntax
    /// </summary>
    /// <param name="uuid">UUID value encoded in the RPC or COM descriptor.</param>
    /// <param name="majorVersion">Major version component of the protocol or COM descriptor.</param>
    /// <param name="minorVersion">Minor version component of the protocol or COM descriptor.</param>
    public PresentationSyntax(UUID uuid, int majorVersion, int minorVersion) : this()
    {
        Uuid = uuid;
        Version = (majorVersion & 0xffff) | (minorVersion << 16);
    }

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst)
    {
        Uuid.Encode(ndr, dst);
        dst.Enc_ndr_long(Version);
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src)
    {
        Uuid = new UUID();
        Uuid.Decode(ndr, src);
        Version = src.Dec_ndr_long();
    }

    /// <inheritdoc/>
    public override string ToString() => Uuid + ":" + MajorVersion + "." + MinorVersion;
}

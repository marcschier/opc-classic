// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Framework Internal.
/// This class represents the <code>COM</code> version of the currently
/// supported COM protocol. Default version is 5.4.
/// </summary>
[Serializable]
public sealed class ComVersion
{
    /// <summary>
    /// Create version
    /// </summary>
    public ComVersion()
    {
    }

    /// <summary>
    /// Create version
    /// </summary>
    /// <param name="majorVersion">Major version component of the protocol or COM descriptor.</param>
    /// <param name="minorVersion">Minor version component of the protocol or COM descriptor.</param>
    public ComVersion(int majorVersion, int minorVersion)
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
    }

    /// <summary>
    /// Major
    /// </summary>
    public int MajorVersion { set; get; } = 5;

    /// <summary>
    /// Minor
    /// </summary>
    public int MinorVersion { set; get; } = 4;
}

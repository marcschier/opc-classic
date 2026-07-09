// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Identifies one member of the DCOM activation property array carried by
/// <c>IActivationProperties</c> OBJREFs.
/// </summary>
public enum ActivationPropertyId : uint
{
    /// <summary>
    /// Unrecognized or opaque activation property payload.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// MS-DCOM SPECIAL_PROPERTIES_DATA.
    /// </summary>
    SpecialProperties = 1,

    /// <summary>
    /// Requested class and interface activation data.
    /// </summary>
    InstanceInfo = 2,

    /// <summary>
    /// Client/server location and protocol-sequence data.
    /// </summary>
    LocationInfo = 3,

    /// <summary>
    /// SCM reply data containing returned interface references.
    /// </summary>
    ScmReplyInfo = 4,

    /// <summary>
    /// Authentication, impersonation, and capability data.
    /// </summary>
    SecurityInfo = 5,
}

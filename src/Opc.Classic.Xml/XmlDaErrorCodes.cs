// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Xml;

/// <summary>
/// Helpers for converting OPC XML-DA QName result IDs to <see cref="XmlDaErrorCode"/>.
/// </summary>
public static class XmlDaErrorCodes
{
    /// <summary>
    /// Parses an optional per-item <c>ResultID</c>; missing values map to <see cref="XmlDaErrorCode.Ok"/>.
    /// </summary>
    public static XmlDaErrorCode ParseResultId(string? resultId) =>
        string.IsNullOrWhiteSpace(resultId) ? XmlDaErrorCode.Ok : Parse(resultId);

    /// <summary>
    /// Returns true when <paramref name="code"/> is an XML-DA success result.
    /// </summary>
    public static bool IsSuccess(this XmlDaErrorCode code) => code switch
    {
        XmlDaErrorCode.Ok or XmlDaErrorCode.Clamp or XmlDaErrorCode.DataQueueOverflow or
            XmlDaErrorCode.UnsupportedRate => true,
        _ => false,
    };

    /// <summary>
    /// Parses a SOAP fault code or non-empty <c>ResultID</c>.
    /// </summary>
    public static XmlDaErrorCode Parse(string? qualifiedName)
    {
        if (string.IsNullOrWhiteSpace(qualifiedName))
        {
            return XmlDaErrorCode.Unknown;
        }

        string localName = GetLocalName(qualifiedName.Trim());
        return localName switch
        {
            "S_OK" => XmlDaErrorCode.Ok,
            "S_CLAMP" => XmlDaErrorCode.Clamp,
            "S_DATAQUEUEOVERFLOW" => XmlDaErrorCode.DataQueueOverflow,
            "S_UNSUPPORTEDRATE" => XmlDaErrorCode.UnsupportedRate,
            "E_ACCESS_DENIED" => XmlDaErrorCode.AccessDenied,
            "E_BUSY" => XmlDaErrorCode.Busy,
            "E_FAIL" => XmlDaErrorCode.Fail,
            "E_INVALIDCONTINUATIONPOINT" => XmlDaErrorCode.InvalidContinuationPoint,
            "E_INVALIDFILTER" => XmlDaErrorCode.InvalidFilter,
            "E_INVALIDHOLDTIME" => XmlDaErrorCode.InvalidHoldTime,
            "E_INVALIDITEMID" => XmlDaErrorCode.InvalidItemId,
            "E_INVALIDITEMNAME" => XmlDaErrorCode.InvalidItemName,
            "E_INVALIDITEMPATH" => XmlDaErrorCode.InvalidItemPath,
            "E_INVALIDPID" => XmlDaErrorCode.InvalidPid,
            "E_NOSUBSCRIPTION" => XmlDaErrorCode.NoSubscription,
            "E_NOTSUPPORTED" => XmlDaErrorCode.NotSupported,
            "E_OUTOFMEMORY" => XmlDaErrorCode.OutOfMemory,
            "E_RANGE" => XmlDaErrorCode.Range,
            "E_BADTYPE" => XmlDaErrorCode.BadType,
            "E_READONLY" => XmlDaErrorCode.ReadOnly,
            "E_SERVERSTATE" => XmlDaErrorCode.ServerState,
            "E_TIMEDOUT" => XmlDaErrorCode.TimedOut,
            "E_UNKNOWNITEMID" => XmlDaErrorCode.UnknownItemId,
            "E_UNKNOWNITEMNAME" => XmlDaErrorCode.UnknownItemName,
            "E_UNKNOWNITEMPATH" => XmlDaErrorCode.UnknownItemPath,
            "E_WRITEONLY" => XmlDaErrorCode.WriteOnly,
            "E_BADRIGHTS" => XmlDaErrorCode.BadRights,
            _ => XmlDaErrorCode.Unknown,
        };
    }

    /// <summary>
    /// Returns the XML-DA result ID text for a known code.
    /// </summary>
    public static string ToResultId(XmlDaErrorCode code) => code switch
    {
        XmlDaErrorCode.Ok => "S_OK",
        XmlDaErrorCode.Clamp => "S_CLAMP",
        XmlDaErrorCode.DataQueueOverflow => "S_DATAQUEUEOVERFLOW",
        XmlDaErrorCode.UnsupportedRate => "S_UNSUPPORTEDRATE",
        XmlDaErrorCode.AccessDenied => "E_ACCESS_DENIED",
        XmlDaErrorCode.Busy => "E_BUSY",
        XmlDaErrorCode.Fail => "E_FAIL",
        XmlDaErrorCode.InvalidContinuationPoint => "E_INVALIDCONTINUATIONPOINT",
        XmlDaErrorCode.InvalidFilter => "E_INVALIDFILTER",
        XmlDaErrorCode.InvalidHoldTime => "E_INVALIDHOLDTIME",
        XmlDaErrorCode.InvalidItemId => "E_INVALIDITEMID",
        XmlDaErrorCode.InvalidItemName => "E_INVALIDITEMNAME",
        XmlDaErrorCode.InvalidItemPath => "E_INVALIDITEMPATH",
        XmlDaErrorCode.InvalidPid => "E_INVALIDPID",
        XmlDaErrorCode.NoSubscription => "E_NOSUBSCRIPTION",
        XmlDaErrorCode.NotSupported => "E_NOTSUPPORTED",
        XmlDaErrorCode.OutOfMemory => "E_OUTOFMEMORY",
        XmlDaErrorCode.Range => "E_RANGE",
        XmlDaErrorCode.BadType => "E_BADTYPE",
        XmlDaErrorCode.ReadOnly => "E_READONLY",
        XmlDaErrorCode.ServerState => "E_SERVERSTATE",
        XmlDaErrorCode.TimedOut => "E_TIMEDOUT",
        XmlDaErrorCode.UnknownItemId => "E_UNKNOWNITEMID",
        XmlDaErrorCode.UnknownItemName => "E_UNKNOWNITEMNAME",
        XmlDaErrorCode.UnknownItemPath => "E_UNKNOWNITEMPATH",
        XmlDaErrorCode.WriteOnly => "E_WRITEONLY",
        XmlDaErrorCode.BadRights => "E_BADRIGHTS",
        _ => string.Empty,
    };

    private static string GetLocalName(string qualifiedName)
    {
        int colon = qualifiedName.LastIndexOf(':');
        return colon >= 0 ? qualifiedName[(colon + 1)..] : qualifiedName;
    }
}

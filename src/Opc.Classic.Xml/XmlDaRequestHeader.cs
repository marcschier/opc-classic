// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Xml;

/// <summary>
/// XML-DA request header common to every operation. Mirrors the
/// <c>RequestOptions</c>-adjacent attributes on each request element:
/// <c>LocaleID</c> and <c>ClientRequestHandle</c>.
/// </summary>
/// <param name="LocaleId">
/// IETF language tag identifying the locale the client wants the response
/// messages localized into (e.g. <c>en-US</c>). Servers may revise this
/// to their closest supported match.
/// </param>
/// <param name="ClientRequestHandle">
/// Free-form client-supplied correlation ID. The server echoes this back in
/// the response so the client can pair requests with responses.
/// </param>
public sealed record XmlDaRequestHeader(string? LocaleId, string? ClientRequestHandle);

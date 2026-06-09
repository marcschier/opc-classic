//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA's <c>OPCHDA_TIME</c> — a time value carried either as a
/// server-evaluated string expression (e.g. <c>"NOW-1H"</c>) or as
/// an absolute FILETIME timestamp.
/// </summary>
/// <param name="IsStringExpression">True if <see cref="StringExpression"/> is set; false if <see cref="Timestamp"/> is set.</param>
/// <param name="StringExpression">Server-evaluated time expression when <see cref="IsStringExpression"/> is true.</param>
/// <param name="Timestamp">Absolute UTC timestamp when <see cref="IsStringExpression"/> is false.</param>
public sealed record OpcHdaTime(
    bool IsStringExpression,
    string? StringExpression,
    DateTimeOffset Timestamp) {
    /// <summary>Creates an OPCHDA_TIME carrying a server-evaluated expression.</summary>
    public static OpcHdaTime FromString(string expression) =>
        new(IsStringExpression: true, StringExpression: expression, Timestamp: default);

    /// <summary>Creates an OPCHDA_TIME carrying an absolute timestamp.</summary>
    public static OpcHdaTime FromTimestamp(DateTimeOffset timestamp) =>
        new(IsStringExpression: false, StringExpression: null, Timestamp: timestamp);
}

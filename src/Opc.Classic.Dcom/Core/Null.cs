// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Null type
/// </summary>
[Serializable]
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
public sealed class Null {
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.

    /// <summary>
    /// Null value
    /// </summary>
    public static Null Value { get; } = new Null();
}

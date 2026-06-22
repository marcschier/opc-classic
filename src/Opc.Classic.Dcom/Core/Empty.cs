// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Empty type
/// </summary>
[Serializable]
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
public sealed class Empty
{
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.

    /// <summary>
    /// Empty value
    /// </summary>
    public static Empty Value { get; } = new Empty();
}

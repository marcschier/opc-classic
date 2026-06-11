//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic;

/// <summary>A named field in an optional VT_RECORD type description.</summary>
public readonly record struct OpcRecordField
{
    /// <summary>Creates a record field descriptor.</summary>
    public OpcRecordField(string name, VarType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Type = type;
    }

    /// <summary>The field name from the record layout.</summary>
    public string Name { get; }

    /// <summary>The field VARTYPE used by the record codec.</summary>
    public VarType Type { get; }
}

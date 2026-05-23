//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMPROPERTIES</c> — item-level error plus the set of
/// properties returned for that item by browse/property APIs.
/// </summary>
/// <param name="ErrorId">HRESULT — 0 on success; nonzero when the item-level property lookup failed.</param>
/// <param name="Properties">Per-property results; empty when no properties were returned.</param>
public sealed record OpcItemProperties(int ErrorId, OpcItemPropertyResult[] Properties)
{
    private OpcItemPropertyResult[] _properties = Properties ?? throw new ArgumentNullException(nameof(Properties));

    /// <summary>Per-property results; empty when no properties were returned.</summary>
    public OpcItemPropertyResult[] Properties
    {
        get => _properties;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _properties = value;
        }
    }
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>The OPC quality limit field (bits 6-7 of the DA quality WORD).</summary>
public enum OpcQualityLimit
{
    /// <summary>The value is not limited.</summary>
    NotLimited = 0,
    /// <summary>The value has been pegged to the low limit.</summary>
    Low = 1,
    /// <summary>The value has been pegged to the high limit.</summary>
    High = 2,
    /// <summary>The value is constant and cannot move.</summary>
    Constant = 3,
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dx;

/// <summary>Override state per OPC DX 1.0 §4.1 (OVERRIDE_STATE enum).</summary>
public enum OverrideState
{
    /// <summary>The override mechanism is disabled — last-good values flow through.</summary>
    Disabled = 0,
    /// <summary>The configured <see cref="DxConnection.DefaultOverrideValue"/> is being substituted for live data.</summary>
    Enabled = 1,
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// OPC server runtime state, as reported by the <c>OPCSERVERSTATUS::dwServerState</c>
/// field (DA), <c>OPCEVENTSERVERSTATUS</c> (AE), and <c>OPCHDA_SERVERSTATUS</c> (HDA).
/// </summary>
public enum OpcServerState
{
    /// <summary>The state is not known (no GetStatus call has succeeded yet).</summary>
    Unknown = 0,

    /// <summary>The server is running normally.</summary>
    Running = 1,

    /// <summary>The server has detected an internal error and is unable to function.</summary>
    Failed = 2,

    /// <summary>The server is running but has no configuration loaded (no items / no tag tree).</summary>
    NoConfig = 3,

    /// <summary>The server is in a suspended state — not communicating with the field.</summary>
    Suspended = 4,

    /// <summary>The server is in a test / simulation mode (data is synthetic).</summary>
    Test = 5,

    /// <summary>The server has lost communication with the underlying device(s). DA 3.0+.</summary>
    CommFault = 6,
}

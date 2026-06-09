//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Descriptor for a single OPC DA standard property (ID + data type + description).
/// </summary>
public sealed record OpcStandardProperty(int Id, VarType DataType, string Description);

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Serilog-API-shaped shim that routes through Microsoft.Extensions.Logging.
// Source-compatible drop-in for `Log.Logger.{Information|Warning|Error|Debug|Verbose|Fatal}`
// calls — see LogHost.cs file header for the migration plan.
//

namespace Opc.Classic.Dcom.Internal;

/// <summary>
/// Shim providing the Serilog static-API surface (<c>Log.Logger.X(...)</c>)
/// but routing through Microsoft.Extensions.Logging.ILogger.
/// </summary>
public static class Log
{
    /// <summary>The shim logger — call <c>Log.Logger.Information(...)</c> etc.</summary>
    public static IShimLogger Logger { get; } = new ShimLogger();
}

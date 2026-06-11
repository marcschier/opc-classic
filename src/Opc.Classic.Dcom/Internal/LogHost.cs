//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Phase 2G bootstrap — Microsoft.Extensions.Logging-backed shim that
// mimics Serilog's "Log.Logger.Information(...)" surface. Lets existing
// Serilog call sites migrate file-by-file with a single using-directive
// swap (`using Opc.Classic.Dcom.Internal;` -> `using Opc.Classic.Dcom.Internal;`) instead
// of a big-bang rewrite of all 33+ logging consumers.
//
// Migration plan:
//   1. (this commit) Add LogHost + Log shim. Existing Serilog code keeps
//      working unchanged.
//   2. Per-file PR: replace `using Opc.Classic.Dcom.Internal;` with `using Opc.Classic.Dcom.Internal;`
//      and verify the file compiles + tests still pass.
//   3. Once all files migrated, remove the Serilog package reference.
//

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Dcom.Internal;

/// <summary>
/// Process-wide host for the ILoggerFactory used by Opc.Classic.Dcom.
/// Consumers configure the factory via <see cref="ConfigureFactory"/>;
/// when unconfigured, all log calls become no-ops (NullLoggerFactory).
/// </summary>
public static class LogHost
{
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;

    /// <summary>The current logger factory; defaults to <see cref="NullLoggerFactory"/>.</summary>
    public static ILoggerFactory Factory => _factory;

    /// <summary>Installs a new logger factory. Pass <see langword="null"/> to revert to no-op logging.</summary>
    public static void ConfigureFactory(ILoggerFactory? factory)
    {
        _factory = factory ?? NullLoggerFactory.Instance;
    }

    /// <summary>Creates a category logger from the configured factory.</summary>
    public static ILogger CreateLogger(string category) => _factory.CreateLogger(category);

    /// <summary>Creates a typed category logger from the configured factory.</summary>
    public static ILogger<T> CreateLogger<T>() => _factory.CreateLogger<T>();
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;

namespace Opc.Classic.Dcom.Channels;

/// <summary>
/// Tracks the ORPC causality identifier for the current asynchronous call chain.
/// </summary>
public static class CausalityContext {
    /// <summary>Gets the current call-chain causality identifier.</summary>
    public static AsyncLocal<Guid?> Current { get; } = new();

    /// <summary>
    /// Starts a causality scope, creating a new identifier only when no parent scope exists.
    /// </summary>
    public static IDisposable BeginCall() {
        Guid? previous = Current.Value;
        Current.Value = previous ?? Guid.NewGuid();
        return new Scope(previous);
    }

    /// <summary>Starts a causality scope with an explicit parent identifier.</summary>
    public static IDisposable BeginCall(Guid causalityId) {
        Guid? previous = Current.Value;
        Current.Value = causalityId;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable {
        private readonly Guid? _previous;
        private bool _disposed;

        public Scope(Guid? previous) => _previous = previous;

        public void Dispose() {
            if (_disposed) {
                return;
            }

            Current.Value = _previous;
            _disposed = true;
        }
    }
}

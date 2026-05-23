//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Opc.Classic.Hosting;

/// <summary>
/// Concurrent in-memory <see cref="IClsidRegistry"/> implementation.
/// </summary>
public sealed class InMemoryClsidRegistry : IClsidRegistry
{
    private readonly ConcurrentDictionary<Guid, OpcClsidRegistration> _byClsid;
    private readonly ConcurrentDictionary<string, OpcClsidRegistration> _byProgId;

    /// <summary>
    /// Initializes an empty registry.
    /// </summary>
    public InMemoryClsidRegistry()
    {
        _byClsid = new ConcurrentDictionary<Guid, OpcClsidRegistration>();
        _byProgId = new ConcurrentDictionary<string, OpcClsidRegistration>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a registry from an existing registration sequence.
    /// </summary>
    public InMemoryClsidRegistry(IEnumerable<OpcClsidRegistration> initialRegistrations)
        : this()
    {
        ArgumentNullException.ThrowIfNull(initialRegistrations);

        foreach (var registration in initialRegistrations)
        {
            Register(registration);
        }
    }

    /// <inheritdoc />
    public bool TryResolve(Guid clsid, out OpcClsidRegistration registration) =>
        _byClsid.TryGetValue(clsid, out registration!);

    /// <inheritdoc />
    public bool TryResolveProgId(string progId, out OpcClsidRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(progId);

        return _byProgId.TryGetValue(progId, out registration!);
    }

    /// <inheritdoc />
    public IEnumerable<OpcClsidRegistration> Enumerate() => _byClsid.Values;

    /// <inheritdoc />
    public void Register(OpcClsidRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (string.IsNullOrWhiteSpace(registration.ProgId))
        {
            throw new ArgumentException("A ProgID is required.", nameof(registration));
        }

        if (_byProgId.TryGetValue(registration.ProgId, out var previousForProgId)
            && previousForProgId.Clsid != registration.Clsid)
        {
            _byClsid.TryRemove(previousForProgId.Clsid, out _);
        }

        if (_byClsid.TryGetValue(registration.Clsid, out var previousForClsid)
            && !string.Equals(previousForClsid.ProgId, registration.ProgId, StringComparison.OrdinalIgnoreCase))
        {
            _byProgId.TryRemove(previousForClsid.ProgId, out _);
        }

        _byClsid[registration.Clsid] = registration;
        _byProgId[registration.ProgId] = registration;
    }

    /// <inheritdoc />
    public void Unregister(Guid clsid)
    {
        if (_byClsid.TryRemove(clsid, out var registration))
        {
            _byProgId.TryRemove(registration.ProgId, out _);
        }
    }
}

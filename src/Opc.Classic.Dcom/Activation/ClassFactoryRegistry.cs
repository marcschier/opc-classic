//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Thread-safe registry mapping CLSIDs to managed class factories used by the
/// server-side IRemoteSCMActivator implementation.
/// </summary>
public sealed class ClassFactoryRegistry
{
    private readonly ConcurrentDictionary<Guid, IClassFactory> _factories = new();

    /// <summary>Number of registered factories.</summary>
    public int Count => _factories.Count;

    /// <summary>Registers or replaces a factory delegate that returns a managed instance.</summary>
    public void Register(
        Guid clsid,
        Func<ClassFactoryActivationContext, object> factory,
        bool supportsGetClassObject = true)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Register(clsid, new DelegateClassFactory(factory, supportsGetClassObject));
    }

    /// <summary>Registers or replaces a factory delegate that returns a full activation result.</summary>
    public void Register(
        Guid clsid,
        Func<ClassFactoryActivationContext, ClassFactoryActivationResult> factory,
        bool supportsGetClassObject = true)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Register(clsid, new DelegateClassFactory(factory, supportsGetClassObject));
    }

    /// <summary>Registers or replaces a factory object.</summary>
    public void Register(Guid clsid, IClassFactory factory)
    {
        if (clsid == Guid.Empty)
        {
            throw new ArgumentException("CLSID cannot be empty.", nameof(clsid));
        }

        _factories[clsid] = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>Attempts to resolve a CLSID to its factory.</summary>
    public bool TryResolve(Guid clsid, out IClassFactory factory) => _factories.TryGetValue(clsid, out factory);

    /// <summary>Removes a registered factory.</summary>
    public bool Unregister(Guid clsid) => _factories.TryRemove(clsid, out _);

    private sealed class DelegateClassFactory : IClassFactory
    {
        private readonly Func<ClassFactoryActivationContext, object>? _instanceFactory;
        private readonly Func<ClassFactoryActivationContext, ClassFactoryActivationResult>? _resultFactory;

        public DelegateClassFactory(Func<ClassFactoryActivationContext, object> factory, bool supportsGetClassObject)
        {
            _instanceFactory = factory;
            SupportsGetClassObject = supportsGetClassObject;
        }

        public DelegateClassFactory(Func<ClassFactoryActivationContext, ClassFactoryActivationResult> factory, bool supportsGetClassObject)
        {
            _resultFactory = factory;
            SupportsGetClassObject = supportsGetClassObject;
        }

        public bool SupportsGetClassObject { get; }

        public ClassFactoryActivationResult CreateInstance(ClassFactoryActivationContext context)
        {
            if (_resultFactory is not null)
            {
                return _resultFactory(context) ?? throw new InvalidOperationException("Class factory returned null activation result.");
            }

            if (_instanceFactory is null)
            {
                throw new InvalidOperationException("Class factory result factory was not configured.");
            }

            object instance = _instanceFactory(context) ?? throw new InvalidOperationException("Class factory returned null instance.");
            return new ClassFactoryActivationResult(instance, CreateDefaultInterfaceDefinition(context.RequestedIid));
        }
    }

    internal static LocalInterfaceDefinition CreateDefaultInterfaceDefinition(Guid requestedIid)
    {
        Guid iid = requestedIid == Guid.Empty ? Guid.Parse(Opc.Classic.Dcom.Interfaces.IID_IUnknown) : requestedIid;
        return new LocalInterfaceDefinition(iid.ToString(), isDispInterface: false);
    }
}

//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace SharpInterop.Core;

/// <summary>Managed COM class factory used by <see cref="RemoteSCMActivatorServer" />.</summary>
public interface IClassFactory {
    /// <summary>Whether this factory can be returned from RemoteGetClassObject.</summary>
    bool SupportsGetClassObject { get; }

    /// <summary>Creates an instance for RemoteCreateInstance.</summary>
    ClassFactoryActivationResult CreateInstance(ClassFactoryActivationContext context);
}

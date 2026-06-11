//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Core;

/// <summary>Result produced by a class factory for DCOM export.</summary>
public sealed class ClassFactoryActivationResult
{
    /// <summary>Creates a result with the managed instance and interface definition to export.</summary>
    public ClassFactoryActivationResult(object instance, LocalInterfaceDefinition interfaceDefinition)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        InterfaceDefinition = interfaceDefinition ?? throw new ArgumentNullException(nameof(interfaceDefinition));
    }

    /// <summary>The managed object instance implementing the requested interface.</summary>
    public object Instance { get; }

    /// <summary>The DCOM interface definition used by <see cref="LocalCoClass" />.</summary>
    public LocalInterfaceDefinition InterfaceDefinition { get; }
}

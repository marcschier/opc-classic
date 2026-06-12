//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hosting;

/// <summary>
/// Cross-platform CLSID registry. Resolves a CLSID to the (assembly, type)
/// implementing it, plus optional ProgID alias.
/// </summary>
public interface IClsidRegistry
{
    /// <summary>Resolves a CLSID to its server registration.</summary>
    bool TryResolve(Guid clsid, out OpcClsidRegistration registration);

    /// <summary>Resolves a ProgID alias to its server registration.</summary>
    bool TryResolveProgId(string progId, out OpcClsidRegistration registration);

    /// <summary>Enumerates all registered server-class entries.</summary>
    IEnumerable<OpcClsidRegistration> Enumerate();

    /// <summary>Registers or replaces a server-class registration.</summary>
    void Register(OpcClsidRegistration registration);

    /// <summary>Removes a server-class registration by CLSID.</summary>
    void Unregister(Guid clsid);
}

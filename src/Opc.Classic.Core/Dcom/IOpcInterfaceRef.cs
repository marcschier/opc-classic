// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom;

/// <summary>
/// Managed handle for a DCOM OBJREF_STANDARD interface pointer decoded from an OPC Classic response.
/// </summary>
/// <remarks>
/// A future DCOM call-channel factory can resolve this handle to a channel for the returned sub-object.
/// </remarks>
public interface IOpcInterfaceRef
{
    /// <summary>
    /// Interface identifier carried by the OBJREF iid field.
    /// </summary>
    Guid Iid { get; }

    /// <summary>
    /// STDOBJREF flags.
    /// </summary>
    uint Flags { get; }

    /// <summary>
    /// STDOBJREF public reference count.
    /// </summary>
    uint PublicRefs { get; }

    /// <summary>
    /// Object exporter identifier (OXID).
    /// </summary>
    ulong Oxid { get; }

    /// <summary>
    /// Object identifier (OID).
    /// </summary>
    ulong Oid { get; }

    /// <summary>
    /// Interface pointer identifier (IPID).
    /// </summary>
    Guid Ipid { get; }

    /// <summary>
    /// DUALSTRINGARRAY security-offset value.
    /// </summary>
    ushort SecurityOffset { get; }

    /// <summary>
    /// Raw DUALSTRINGARRAY entries used to resolve OXID bindings.
    /// </summary>
    IReadOnlyList<ushort> ResolverBindings { get; }
}

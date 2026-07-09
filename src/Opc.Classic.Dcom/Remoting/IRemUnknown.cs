// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Generators;

namespace Opc.Classic.Dcom.Remoting;

/// <summary>
/// Source-generated proxy/dispatcher surface for the <c>IRemUnknown</c>
/// interface (IID <c>00000131-0000-0000-c000-000000000046</c>) defined in
/// <c>[MS-DCOM] §3.1.1.5.6</c>. Exposed on every DCOM object's OXID; the IPID
/// for IRemUnknown on a given OXID is returned by
/// <c>IActivation::RemoteActivation</c> as <c>ipidRemUnknown</c>.
/// </summary>
/// <remarks>
/// <para>
/// IRemUnknown is the runtime navigation interface used to query for, add
/// references to, and release additional interfaces on an already-resolved
/// remote object. Without it, the only way to obtain a new IPID is to ask the
/// activation layer for a different one at construction time.
/// </para>
/// </remarks>
[OpcInterface(Opc.Classic.Dcom.Interfaces.IID_IRemUnknown)]
[GenerateOpcProxy]
public partial interface IRemUnknown
{
    /// <summary>
    /// <c>IRemUnknown::RemQueryInterface</c> (opnum 3). For each requested IID
    /// in <paramref name="iids"/>, returns the per-IID HRESULT and (on success)
    /// the <c>STDOBJREF</c> identifying the new IPID. The caller routes
    /// subsequent calls to those interfaces using the returned IPIDs on the
    /// same DCOM channel.
    /// </summary>
    /// <remarks>
    /// IDL signature per [MS-DCOM] §3.1.1.5.6.1:
    /// <code>
    /// HRESULT RemQueryInterface(
    ///     [in] REFIPID ripid,
    ///     [in] unsigned long cRefs,
    ///     [in] unsigned short cIids,
    ///     [in, size_is(cIids)] IID* iids,
    ///     [out, size_is(,cIids)] REMQIRESULT** ppQIResults);
    /// </code>
    /// The <c>ppQIResults</c> output is a unique-pointer-prefixed conformant
    /// array — the <c>[return: OpcUniquePointer]</c> attribute directs the
    /// generator to emit the 4-byte referent before the array max_count +
    /// REMQIRESULT[<paramref name="cIids"/>] elements.
    /// </remarks>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<OpcRemQIResult[]> RemQueryInterfaceAsync(
        Guid ripid,
        uint cRefs,
        ushort cIids,
        Guid[] iids,
        CancellationToken cancellationToken = default);
}

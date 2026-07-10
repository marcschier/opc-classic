// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Human-readable labels for the well-known COM/OLE32 interface IDs that the
/// Windows SCM (RPCSS) probes on an object and its class factory during DCOM
/// activation and marshaling. Used only by <see cref="ComActivationDiagnostics"/>
/// tracing so <c>ccw-trace.log</c> can be diffed across hosts (for example a CI
/// runner versus a developer workstation) without a manual GUID lookup.
/// </summary>
/// <remarks>
/// These IIDs are the standard marshaling/security/agility interfaces from
/// <c>Unknwn.idl</c>, <c>ObjIdl.idl</c> and <c>ctxtcall.idl</c>. A managed CCW
/// legitimately answers <c>E_NOINTERFACE</c> for most of them (RPCSS then falls
/// back to standard marshaling), so seeing them in the trace is expected; the
/// value is in revealing <em>which</em> IIDs a given host probes and in what
/// order relative to the OPC interfaces.
/// </remarks>
internal static class WellKnownComIid
{
    private static readonly Guid IMarshal = new("00000003-0000-0000-c000-000000000046");
    private static readonly Guid IStdMarshalInfo = new("00000018-0000-0000-c000-000000000046");
    private static readonly Guid IExternalConnection = new("00000019-0000-0000-c000-000000000046");
    private static readonly Guid IMultiQI = new("00000020-0000-0000-c000-000000000046");
    private static readonly Guid INoMarshal = new("00000021-0000-0000-c000-000000000046");
    private static readonly Guid IClientSecurity = new("0000013d-0000-0000-c000-000000000046");
    private static readonly Guid IServerSecurity = new("0000013e-0000-0000-c000-000000000046");
    private static readonly Guid IRpcOptions = new("00000144-0000-0000-c000-000000000046");
    private static readonly Guid IContext = new("000001c0-0000-0000-c000-000000000046");
    private static readonly Guid ICallFactory = new("1c733a30-2a1c-11ce-ade5-00aa0044773d");
    private static readonly Guid IAgileObject = new("94ea2b94-e9cc-49e0-c0ff-ee64ca8f5b90");
    private static readonly Guid IFastRundown = new("00000034-0000-0000-c000-000000000046");

    /// <summary>
    /// Returns a human-readable name for <paramref name="iid"/> when it is a
    /// well-known COM marshaling IID, otherwise the GUID in registry form.
    /// </summary>
    internal static string Describe(Guid iid)
    {
        if (iid == IMarshal) { return "IMarshal"; }
        if (iid == IStdMarshalInfo) { return "IStdMarshalInfo"; }
        if (iid == IExternalConnection) { return "IExternalConnection"; }
        if (iid == IMultiQI) { return "IMultiQI"; }
        if (iid == INoMarshal) { return "INoMarshal"; }
        if (iid == IClientSecurity) { return "IClientSecurity"; }
        if (iid == IServerSecurity) { return "IServerSecurity"; }
        if (iid == IRpcOptions) { return "IRpcOptions"; }
        if (iid == IContext) { return "IContext"; }
        if (iid == ICallFactory) { return "ICallFactory"; }
        if (iid == IAgileObject) { return "IAgileObject"; }
        if (iid == IFastRundown) { return "IFastRundown"; }
        return iid.ToString();
    }
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// HRESULT facility code constants per [MS-ERREF] §2.1 facility table.
/// </summary>
/// <remarks>
/// Facility codes occupy bits 16-26 of an HRESULT and identify the
/// originating subsystem of an error. Use these constants when
/// constructing per-spec HRESULTs to ensure consistency with the
/// Microsoft Open Specifications.
/// </remarks>
public static class OpcFacility
{
    /// <summary><c>FACILITY_NULL</c> (0) — default facility code.</summary>
    public const int Null = 0;

    /// <summary><c>FACILITY_RPC</c> (1) — RPC subsystem.</summary>
    public const int Rpc = 1;

    /// <summary><c>FACILITY_DISPATCH</c> (2) — COM Dispatch.</summary>
    public const int Dispatch = 2;

    /// <summary><c>FACILITY_STORAGE</c> (3) — OLE Storage.</summary>
    public const int Storage = 3;

    /// <summary><c>FACILITY_ITF</c> (4) — interface-specific errors (shared by OPC vendor extensions).</summary>
    /// <remarks>OPC uses this facility for OPC-specific HRESULTs; this is the same numeric value as <see cref="OpcResultId.FacilityOpc"/>.</remarks>
    public const int Itf = 4;

    /// <summary>Alias for <see cref="Itf"/> in OPC contexts.</summary>
    public const int Opc = Itf;

    /// <summary><c>FACILITY_WIN32</c> (7) — Win32 system errors promoted to HRESULT space.</summary>
    public const int Win32 = 7;

    /// <summary><c>FACILITY_WINDOWS</c> (8) — Windows subsystem.</summary>
    public const int Windows = 8;

    /// <summary><c>FACILITY_SECURITY</c> (9) — SSPI / security subsystem.</summary>
    public const int Security = 9;

    /// <summary><c>FACILITY_CONTROL</c> (10) — control subsystem.</summary>
    public const int Control = 10;

    /// <summary><c>FACILITY_CERT</c> (11) — certificate subsystem.</summary>
    public const int Cert = 11;

    /// <summary><c>FACILITY_INTERNET</c> (12) — WinInet subsystem.</summary>
    public const int Internet = 12;

    /// <summary><c>FACILITY_MEDIASERVER</c> (13).</summary>
    public const int MediaServer = 13;

    /// <summary><c>FACILITY_MSMQ</c> (14) — MSMQ subsystem.</summary>
    public const int Msmq = 14;

    /// <summary><c>FACILITY_SETUPAPI</c> (15) — SetupAPI subsystem.</summary>
    public const int SetupApi = 15;

    /// <summary><c>FACILITY_SCARD</c> (16) — smartcard subsystem.</summary>
    public const int SmartCard = 16;

    /// <summary><c>FACILITY_COMPLUS</c> (17) — COM+ subsystem.</summary>
    public const int ComPlus = 17;

    /// <summary><c>FACILITY_AAF</c> (18) — Authentication / Access Framework.</summary>
    public const int Aaf = 18;

    /// <summary><c>FACILITY_URT</c> (19) — Universal Runtime (.NET CLR).</summary>
    public const int Urt = 19;

    /// <summary><c>FACILITY_ACS</c> (20) — Audit Collection Service.</summary>
    public const int Acs = 20;

    /// <summary><c>FACILITY_DPLAY</c> (21) — DirectPlay subsystem.</summary>
    public const int DPlay = 21;

    /// <summary><c>FACILITY_UMI</c> (22) — Universal Management Interface.</summary>
    public const int Umi = 22;

    /// <summary><c>FACILITY_SXS</c> (23) — side-by-side configuration.</summary>
    public const int Sxs = 23;
}

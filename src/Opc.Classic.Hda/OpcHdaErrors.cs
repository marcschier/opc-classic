//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707 // HDA HRESULT names preserve Appendix C / OpcHda_Error.h identifiers.

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA 1.x HRESULT constants from Appendix C / <c>interop\inc\OpcHda_Error.h</c>.
/// </summary>
/// <remarks>
/// The OPC Foundation header names these <c>OPC_E_*</c>/<c>OPC_S_*</c>. This type exposes
/// HDA-scoped <c>OPCHDA_*</c> aliases and matching header-name aliases for interop code.
/// </remarks>
public static class OpcHdaErrors {
    /// <summary><c>OPC_E_MAXEXCEEDED</c>: maximum value count exceeds the server limit.</summary>
    public const int OPCHDA_E_MAXEXCEEDED = unchecked((int)0xC0041001u);

    /// <summary><c>OPC_S_NODATA</c>: no data exists within the specified parameters.</summary>
    public const int OPCHDA_S_NODATA = 0x40041002;

    /// <summary><c>OPC_S_MOREDATA</c>: more data satisfies the query than was returned.</summary>
    public const int OPCHDA_S_MOREDATA = 0x40041003;

    /// <summary><c>OPC_E_INVALIDAGGREGATE</c>: aggregate ID is not valid.</summary>
    public const int OPCHDA_E_INVALIDAGGREGATE = unchecked((int)0xC0041004u);

    /// <summary><c>OPC_S_CURRENTVALUE</c>: only current values are available for requested item attributes.</summary>
    public const int OPCHDA_S_CURRENTVALUE = 0x40041005;

    /// <summary><c>OPC_S_EXTRADATA</c>: additional data satisfying the query was found.</summary>
    public const int OPCHDA_S_EXTRADATA = 0x40041006;

    /// <summary><c>OPC_W_NOFILTER</c>: the server does not support this filter.</summary>
    public const int OPCHDA_W_NOFILTER = unchecked((int)0x80041007u);

    /// <summary><c>OPC_E_UNKNOWNATTRID</c>: the server does not support this attribute.</summary>
    public const int OPCHDA_E_UNKNOWNATTRID = unchecked((int)0xC0041008u);

    /// <summary><c>OPC_E_NOT_AVAIL</c>: requested aggregate is not available for the item.</summary>
    public const int OPCHDA_E_NOT_AVAIL = unchecked((int)0xC0041009u);

    /// <summary><c>OPC_E_INVALIDDATATYPE</c>: supplied attribute value has an incorrect data type.</summary>
    public const int OPCHDA_E_INVALIDDATATYPE = unchecked((int)0xC004100Au);

    /// <summary><c>OPC_E_DATAEXISTS</c>: data already exists at the requested timestamp.</summary>
    public const int OPCHDA_E_DATAEXISTS = unchecked((int)0xC004100Bu);

    /// <summary><c>OPC_E_INVALIDATTRID</c>: supplied attribute ID is not valid.</summary>
    public const int OPCHDA_E_INVALIDATTRID = unchecked((int)0xC004100Cu);

    /// <summary><c>OPC_E_NODATAEXISTS</c>: no value exists for the specified time and item ID.</summary>
    public const int OPCHDA_E_NODATAEXISTS = unchecked((int)0xC004100Du);

    /// <summary><c>OPC_S_INSERTED</c>: the requested insert occurred.</summary>
    public const int OPCHDA_S_INSERTED = 0x4004100E;

    /// <summary><c>OPC_S_REPLACED</c>: the requested replace occurred.</summary>
    public const int OPCHDA_S_REPLACED = 0x4004100F;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_MAXEXCEEDED" />.</summary>
    public const int OPC_E_MAXEXCEEDED = OPCHDA_E_MAXEXCEEDED;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_NODATA" />.</summary>
    public const int OPC_S_NODATA = OPCHDA_S_NODATA;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_MOREDATA" />.</summary>
    public const int OPC_S_MOREDATA = OPCHDA_S_MOREDATA;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_INVALIDAGGREGATE" />.</summary>
    public const int OPC_E_INVALIDAGGREGATE = OPCHDA_E_INVALIDAGGREGATE;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_CURRENTVALUE" />.</summary>
    public const int OPC_S_CURRENTVALUE = OPCHDA_S_CURRENTVALUE;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_EXTRADATA" />.</summary>
    public const int OPC_S_EXTRADATA = OPCHDA_S_EXTRADATA;

    /// <summary>Header-name alias for <see cref="OPCHDA_W_NOFILTER" />.</summary>
    public const int OPC_W_NOFILTER = OPCHDA_W_NOFILTER;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_UNKNOWNATTRID" />.</summary>
    public const int OPC_E_UNKNOWNATTRID = OPCHDA_E_UNKNOWNATTRID;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_NOT_AVAIL" />.</summary>
    public const int OPC_E_NOT_AVAIL = OPCHDA_E_NOT_AVAIL;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_INVALIDDATATYPE" />.</summary>
    public const int OPC_E_INVALIDDATATYPE = OPCHDA_E_INVALIDDATATYPE;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_DATAEXISTS" />.</summary>
    public const int OPC_E_DATAEXISTS = OPCHDA_E_DATAEXISTS;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_INVALIDATTRID" />.</summary>
    public const int OPC_E_INVALIDATTRID = OPCHDA_E_INVALIDATTRID;

    /// <summary>Header-name alias for <see cref="OPCHDA_E_NODATAEXISTS" />.</summary>
    public const int OPC_E_NODATAEXISTS = OPCHDA_E_NODATAEXISTS;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_INSERTED" />.</summary>
    public const int OPC_S_INSERTED = OPCHDA_S_INSERTED;

    /// <summary>Header-name alias for <see cref="OPCHDA_S_REPLACED" />.</summary>
    public const int OPC_S_REPLACED = OPCHDA_S_REPLACED;
}

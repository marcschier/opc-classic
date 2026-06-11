// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Users can implement this class to provide for custom handling of there objects
/// </summary>
public abstract class CustomCallBuilder : CallBuilder
{

    /// <summary>
    /// Write
    /// </summary>
    /// <param name="ndr"></param>
    public abstract void WriteObject(NdrCodec ndr);

    /// <summary>
    /// Read
    /// </summary>
    /// <param name="ndr"></param>
    public abstract void ReadObject(NdrCodec ndr);

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="dispatchNotSupported"></param>
    protected CustomCallBuilder(bool dispatchNotSupported) :
        base(dispatchNotSupported)
    {
    }

    /// <summary>
    /// Create
    /// </summary>
    protected CustomCallBuilder()
    {
    }

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr) => WriteObject(ndr);

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) => ReadObject(ndr);
}

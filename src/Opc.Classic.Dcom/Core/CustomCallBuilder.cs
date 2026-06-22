// Copyright (c) 2026 marcschier. Licensed under the MIT License.

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
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public abstract void WriteObject(NdrCodec ndr);

    /// <summary>
    /// Read
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public abstract void ReadObject(NdrCodec ndr);

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="dispatchNotSupported">Value indicating whether IDispatch invocation should be rejected for this call.</param>
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

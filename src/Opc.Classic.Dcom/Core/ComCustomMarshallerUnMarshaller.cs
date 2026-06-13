// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Must be implemented by Classes providing marshall, unmarshall support
/// for OBJREF_CUSTOM.
/// </summary>
public abstract class ComCustomMarshallerUnMarshaller
{
    /// <summary>
    /// Clsid
    /// </summary>
    public string CLSID { get; }

    /// <summary>
    /// Create marshaller
    /// </summary>
    /// <param name="clsid">CLSID identifying the COM class or OPC server to activate.</param>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    protected ComCustomMarshallerUnMarshaller(string clsid, IComObject comObject) :
        this(clsid, comObject, false)
    {
    }

    /// <summary>
    /// Create marshaller
    /// </summary>
    /// <param name="clsid">CLSID identifying the COM class or OPC server to activate.</param>
    /// <param name="comObject">COM object instance whose exported interfaces are being managed.</param>
    /// <param name="isTemplate">Value indicating whether the structure instance is a template used for custom marshaling.</param>
    protected ComCustomMarshallerUnMarshaller(string clsid,
        IComObject comObject, bool isTemplate)
    {
        CLSID = clsid;
        if (isTemplate)
        {
            ComObject = new ComObjectImpl(comObject.AssociatedSession,
                ((IComObjectInternal)comObject).GetInterfacePointer());

            ((ComObjectImpl)ComObject).CustomObject = this;
        }
        else
        {
            ComObject = comObject;
        }
    }

    /// <summary>
    /// Me
    /// </summary>
    public IComObject ComObject { get; }

    /// <summary>
    /// Implement for custom encoding. Called by the framework.
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    public abstract void Encode(NdrCodec ndr, CodecContext context);

    /// <summary>
    /// Implement for custom decoding. Called by the framework.
    /// </summary>
    /// <param name="newMe">Value used while decoding or.</param>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>A new <see cref="ComCustomMarshallerUnMarshaller"/> instance built from <paramref name="newMe"/>.</returns>
    public abstract ComCustomMarshallerUnMarshaller Decode(IComObject newMe,
        NdrCodec ndr, CodecContext context);

    /// <summary>
    /// Serialize
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="value">Value being stored, encoded, or assigned.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    protected void Serialize(NdrCodec ndr, Type c, object value, CodecContext context) =>
        MarshalUnMarshalHelper.Serialize(ndr, c, value, context);

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="context">Codec context that tracks deferred pointers and per-call buffers.</param>
    /// <returns>The object reconstructed from the serialized COM wire representation.</returns>
    protected object Deserialize(NdrCodec ndr, object obj, CodecContext context) =>
        MarshalUnMarshalHelper.Deserialize(ndr, obj, context);

    /// <summary>
    /// Length in bytes
    /// </summary>
    /// <param name="c">Character value being tested or transformed.</param>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="flag">Flag value that controls the requested operation.</param>
    /// <returns>The requested length in bytes value.</returns>
    protected static int GetLengthInBytes(Type c, object obj, int flag = InteropFlags.FLAG_NULL) =>
        MarshalUnMarshalHelper.GetLengthInBytes(c, obj, flag);
}

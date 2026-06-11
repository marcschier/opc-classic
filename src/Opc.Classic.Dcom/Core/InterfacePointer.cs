// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Class representing a Marshalled Interface Pointer. You will never
/// use the members of this class directly, but always as an implementation
/// of <code><see cref="IComObject"/></code> interface.
/// Sample Usage:
/// <code>
///  var connectionPointContainer =
///      (<see cref="IComObject"/>)ieObject.WueryInterface("B196B284-BAB4-101A-B69C-00AA00341D07");
///  var object =
///      new CallBuilder(connectionPointContainer.Ipid,true);
///  object.Opnum = 1;
///  object.AddInParamAsUUID("34A715A0-6587-11D0-924A-0020AFC7AC4D");
///  object.AddOutParamAsObject(typeof(<see cref="InterfacePointer"/>));
///  // find connection point
///  object[] objects = (object[])connectionPointContainer.Call(object);
///  varconnectionPtr = (<see cref="InterfacePointer"/>)objects[0];
///  var connectionPointer =
///      <see cref="ObjectFactory"/>.CreateCOMInstance(connectionPointContainer, connectionPtr);
/// </code>
/// </summary>
[Serializable]
public /*internal*/ sealed class InterfacePointer
{

    /// <summary>
    /// Custom object
    /// </summary>
    internal bool IsCustomObjRef => ((InterfacePointerBody)_member.Referent)
        .CustomObjRef;

    /// <summary>
    /// Custom class id
    /// </summary>
    internal string CustomCLSID => ((InterfacePointerBody)_member.Referent)
        .CustomCLSID;

    /// <summary>
    /// Deferred
    /// </summary>
    internal bool Deffered
    {
        set => _member.Deffered = true;
    }

    /// <summary>
    /// Object type
    /// </summary>
    internal int ObjectType => ((InterfacePointerBody)_member.Referent).ObjectType;

    /// <summary>
    /// Object reference of specified type
    /// </summary>
    /// <param name="objectType"></param>
    internal object GetObjectReference(int objectType) =>
        ((InterfacePointerBody)_member.Referent).GetObjectReference(objectType);

    /// <summary>
    /// Returns the Interface Identifier for this MIP.
    /// </summary>
    public string IID => ((InterfacePointerBody)_member.Referent).IID;

    /// <summary>
    /// IP Id
    /// </summary>
    public string IPID => ((InterfacePointerBody)_member.Referent).IPID;

    /// <summary>
    /// Oid
    /// </summary>
    public byte[] OID => ((StdObjRef)((InterfacePointerBody)_member.Referent)
        .GetObjectReference(OBJREF_STANDARD)).ObjectId;

    /// <summary>
    /// Oxid
    /// </summary>
    internal byte[] OXID => ((StdObjRef)((InterfacePointerBody)_member.Referent)
        .GetObjectReference(OBJREF_STANDARD)).Oxid;

    /// <summary>
    /// String bindings
    /// </summary>
    internal DualStringArray StringBindings =>
        ((InterfacePointerBody)_member.Referent).StringBindings;

    /// <summary>
    /// Length
    /// </summary>
    internal int Length => ((InterfacePointerBody)_member.Referent).Length;

    /// <summary>
    /// Hidden constructor
    /// </summary>
    private InterfacePointer()
    {
    }

    /// <summary>
    /// Called from Oxid Resolver master, the resolver address are put in here itself
    /// </summary>
    /// <param name="iid"> </param>
    /// <param name="port"></param>
    /// <param name="objref"></param>
    internal InterfacePointer(string iid, int port, StdObjRef objref) =>
        _member = new ComPointer(new InterfacePointerBody(iid, port, objref), false);

    /// <summary>
    /// Create interface pointer
    /// </summary>
    /// <param name="iid"></param>
    /// <param name="interfacePointer"></param>
    internal InterfacePointer(string iid, InterfacePointer interfacePointer) =>
        _member = new ComPointer(new InterfacePointerBody(iid, interfacePointer), false);

    /// <inheritdoc/>
    public override string ToString()
    {
        var retVal = "InterfacePointer[IID:" + IID + ", ObjRef: " +
            GetObjectReference(OBJREF_STANDARD) + "]";
        return retVal;
    }

    /// <summary>
    /// Helper to compare to interface pointers
    /// </summary>
    /// <param name="src"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsOxidEqual(InterfacePointer src, InterfacePointer target)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(target);
        return src.OXID.SequenceEqual(target.OXID);
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    internal static InterfacePointer Decode(NdrCodec ndr, CodecContext context)
    {
        var ptr = new InterfacePointer();
        if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) ==
                    InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2)
        {
            ptr._member = (ComPointer)MarshalUnMarshalHelper.Deserialize(ndr,
                new ComPointer(typeof(InterfacePointerBody), true), context);
        }
        else
        {
            ptr._member = (ComPointer)MarshalUnMarshalHelper.Deserialize(ndr,
                new ComPointer(typeof(InterfacePointerBody)), context);
        }
        // the pointer is null, no point of it's wrapper being present, so return null from here as well
        if (ptr._member.IsNull)
        {
            ptr = null;
        }
        return ptr;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="context"></param>
    internal void Encode(NdrCodec ndr, CodecContext context)
    {
        if ((context.Flag & InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT) ==
                    InteropFlags.FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT)
        {
            // just encode a null.
            MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0, context);
            return;
        }
        MarshalUnMarshalHelper.Serialize(ndr, _member.GetType(), _member, context);
    }

    internal static readonly byte[] OBJREF_SIGNATURE = { 0x4d, 0x45, 0x4f, 0x57 }; // 'MEOW'
    internal const int OBJREF_STANDARD = 0x1; // standard marshaled objref
    internal const int OBJREF_HANDLER = 0x2; // handler marshaled objref
    internal const int OBJREF_CUSTOM = 0x4; // custom marshaled objref
    internal const int OBJREF_EXTENDED = 0x8; // extended standard marshaled objref

    // Flag values for a STDOBJREF (standard part of an OBJREF).
    // SORF_OXRES1 - SORF_OXRES8 are reserved for the object exporters
    // use only, object importers must ignore them and must not enforce MBZ.
    internal const int SORF_OXRES1 = 0x1; // reserved for exporter
    internal const int SORF_OXRES2 = 0x20; // reserved for exporter
    internal const int SORF_OXRES3 = 0x40; // reserved for exporter
    internal const int SORF_OXRES4 = 0x80; // reserved for exporter
    internal const int SORF_OXRES5 = 0x100; // reserved for exporter
    internal const int SORF_OXRES6 = 0x200; // reserved for exporter
    internal const int SORF_OXRES7 = 0x400; // reserved for exporter
    internal const int SORF_OXRES8 = 0x800; // reserved for exporter
    internal const int SORF_NULL = 0x0; // convenient for initializing SORF
    internal const int SORF_NOPING = 0x1000; // Pinging is not required

    private ComPointer _member;
}

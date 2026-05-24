//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal;
using SharpInterop.Rpc.Core;
using System;
using System.Collections.Generic;

namespace SharpInterop.Core;

[Serializable]
public sealed class HandlerInterfacePointerBody : InterfacePointerBody {
    internal HandlerInterfacePointerBody(string iid, StdObjRef stdObjRef, string handlerClsid, DualStringArray stringBindings)
        : base(InterfacePointer.OBJREF_HANDLER, iid, stdObjRef, stringBindings) {
        HandlerCLSID = handlerClsid;
        Length = GetEncodedLength();
    }

    public string HandlerCLSID { get; private set; }

    internal static HandlerInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength) {
        var stdObjRef = StdObjRef.Decode(ndr);
        var handlerClsid = ReadUuid(ndr, "OBJREF_HANDLER decode");
        var stringBindings = DualStringArray.Decode(ndr);
        var body = new HandlerInterfacePointerBody(iid, stdObjRef, handlerClsid, stringBindings);
        body.Length = ObjRefHeaderLength + bodyLength;
        return body;
    }

    protected override int GetEncodedLength() =>
        ObjRefHeaderLength + StdObjRefLength + 16 + StringBindings.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        ((StdObjRef)GetObjectReference(InterfacePointer.OBJREF_STANDARD)).Encode(ndr);
        WriteUuid(ndr, HandlerCLSID, "OBJREF_HANDLER encode");
        StringBindings.Encode(ndr);
    }
}

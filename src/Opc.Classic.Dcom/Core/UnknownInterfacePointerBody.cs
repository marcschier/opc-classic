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
public sealed class UnknownInterfacePointerBody : InterfacePointerBody {
    internal UnknownInterfacePointerBody(int objectType, string iid, byte[] rawBytes)
        : base(objectType, iid, null, null) {
        RawBytes = rawBytes ?? Array.Empty<byte>();
        Length = GetEncodedLength();
    }

    public byte[] RawBytes { get; }

    internal static UnknownInterfacePointerBody Decode(NdrCodec ndr, int objectType, string iid, int bodyLength) =>
        new UnknownInterfacePointerBody(objectType, iid, ReadRemainingBytes(ndr, bodyLength));

    protected override int GetEncodedLength() => ObjRefHeaderLength + RawBytes.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        if (RawBytes.Length > 0) {
            ndr.WriteOctetArray(RawBytes, 0, RawBytes.Length);
        }
    }
}

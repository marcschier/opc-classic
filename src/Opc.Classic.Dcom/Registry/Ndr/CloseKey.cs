//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Common;
using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace SharpInterop.Registry; 
/// <inheritdoc/>
public class CloseKey : NdrOp {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    public PolicyHandle key;
    public byte[] policyhandle = new byte[20];
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <inheritdoc/>
    public override int Opnum => 5;

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr) =>
        ndr.WriteOctetArray(key.Handle, 0, 20);

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr) {
        ndr.ReadOctetArray(policyhandle, 0, 20);
        var hresult = ndr.ReadUnsignedLong();
        if (hresult != 0) {
            throw new InteropRuntimeException(hresult);
        }
    }
}

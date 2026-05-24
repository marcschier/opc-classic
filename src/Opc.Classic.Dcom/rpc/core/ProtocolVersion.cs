//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace SharpInterop.Rpc.Core; 
/// <summary>
/// Protocol version
/// </summary>
public class ProtocolVersion : NdrOp {

    /// <summary>
    /// Major version
    /// </summary>
    public int MajorVersion { get; set; }

    /// <summary>
    /// Minor version
    /// </summary>
    public int MinorVersion { get; set; }

    /// <inheritdoc/>
    public override void Encode(NdrCodec ndr, NdrBuffer dst) {
        dst.Enc_ndr_small(MajorVersion);
        dst.Enc_ndr_small(MinorVersion);
    }

    /// <inheritdoc/>
    public override void Decode(NdrCodec ndr, NdrBuffer src) {
        MajorVersion = src.Dec_ndr_small();
        MinorVersion = src.Dec_ndr_small();
    }
}

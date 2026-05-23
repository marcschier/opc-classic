//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using OpcClassic.Dcom.Internal;
    using OpcClassic.Dcom.Internal.LegacyNdr;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Oxid ping
    /// </summary>
    internal class ComOxidPingObject : NdrOp {

        /// <summary>
        /// Set id
        /// </summary>
        internal byte[] SetId { get; set; }

        /// <inhertidoc/>
        public override void Write(NdrCodec ndr) {
            switch (Opnum) {
                case 2: // complex ping

                    var newlength = 8 + 6 + 8 + (_listOfAdds.Count * 8) + 8 +
                        (_listOfDels.Count * 8) + 16;
                    if (newlength > ndr.Buffer.Buf.Length) {
                        ndr.Buffer.Buf = new byte[newlength + 16];
                    }

                    if (SetId == null) {
                        Log.Logger.Information(
                            "Complex Ping going for the first time, will get the setId as response of this call ");
                        SetId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    }

                    if (Log.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information)) {
                        Log.Logger.Information("Complex ping going : listOfAdds -> Size : " +
                            _listOfAdds.Count + ", " + _listOfAdds);
                        Log.Logger.Information("listOfDels -> Size : " +
                            _listOfDels.Count + ", " + _listOfDels);
                    }

                    MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, SetId);
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(short), (short)_seqNum); // seq
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(short), (short)_listOfAdds.Count); // add
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(short), (short)_listOfDels.Count); // del
                    if (_listOfAdds.Count > 0) {
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), new object().GetHashCode()); // pointer
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), _listOfAdds.Count);
                        for (var i = 0; i < _listOfAdds.Count; i++) {
                            var oid = _listOfAdds[i];
                            MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, oid.OID);
                            Log.Logger.Information("[" + oid.ToString() + "]");
                        }
                    }
                    else {
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0); // null pointer
                    }

                    if (_listOfDels.Count > 0) {
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), new object().GetHashCode()); // pointer
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), _listOfDels.Count);
                        // now align for array
                        ndr.FillAligned(8);
                        for (var i = 0; i < _listOfDels.Count; i++) {
                            var oid = _listOfDels[i];
                            MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, oid.OID);
                        }
                    }
                    else {
                        MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0); // null pointer
                    }
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0);
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0);
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0);
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(int), 0);
                    break;

                case 1: // simple ping
                    if (SetId != null) {
                        MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, SetId); // setid
                        Log.Logger.Information("Simple Ping going for setId: " + Utils.HexString(SetId, 0, SetId.Length));
                    }
                    else {
                        Log.Logger.Information("Some error ! Simple ping requested, but has no setID ");
                    }
                    break;
                default:
                    // nothing.
                    break;
            }
        }

        /// <inhertidoc/>
        public override void Read(NdrCodec ndr) {
            // read response and fill DSs accordingly
            switch (Opnum) {
                case 2: // complex ping
                    SetId = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8);
                    // ping factor
                    MarshalUnMarshalHelper.Deserialize(ndr, typeof(short));
                    // hresult
                    var hresult = (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int));
                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Complex ping failed, hresult: " + hresult);
                    }
                    break;
                case 1: // simple ping
                    // hresult
                    hresult = (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int));
                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Simple ping failed, hresult: " + hresult);
                    }
                    else {
                        Log.Logger.Information("Simple Ping Succeeded");
                    }
                    break;
                default:
                    // nothing.
                    break;
            }
        }

        internal List<ObjectId> _listOfAdds = new List<ObjectId>();
        internal List<ObjectId> _listOfDels = new List<ObjectId>();
        internal int _seqNum;
    }
}
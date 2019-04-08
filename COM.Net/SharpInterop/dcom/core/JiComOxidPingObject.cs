//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Oxid ping
    /// </summary>
    internal class JiComOxidPingObject : NdrOp {

        /// <inhertidoc/>
        public override int Opnum => _opnum;

        /// <inhertidoc/>
        public override void Write(NdrCodec ndr) {
            switch (_opnum) {
                case 2: // complex ping

                    var newlength = 8 + 6 + 8 + (_listOfAdds.Count * 8) + 8 + (_listOfDels.Count * 8) + 16;
                    if (newlength > ndr.Buffer.Buf.Length) {
                        ndr.Buffer.Buf = new byte[newlength + 16];
                    }

                    if (_setId == null) {
                        Log.Logger.Information("Complex Ping going for the first time, will get the setId as response of this call ");
                        _setId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    }

                    if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        Log.Logger.Information("Complex ping going : listOfAdds -> Size : " + _listOfAdds.Count + " , " + _listOfAdds);
                        Log.Logger.Information("listOfDels -> Size : " + _listOfDels.Count + " , " + _listOfDels);
                    }

                    JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, _setId);

                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(short?), (short)_seqNum, null, JIFlags.FLAG_NULL); //seq
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(short?), (short)_listOfAdds.Count, null, JIFlags.FLAG_NULL); //add
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(short?), (short)_listOfDels.Count, null, JIFlags.FLAG_NULL); //del

                    if (_listOfAdds.Count > 0) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), new object().GetHashCode(), null, JIFlags.FLAG_NULL); //pointer
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), _listOfAdds.Count, null, JIFlags.FLAG_NULL);


                        for (var i = 0; i < _listOfAdds.Count; i++) {
                            var oid = (JIObjectId)_listOfAdds[i];
                            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, oid.OID);
                            Log.Logger.Information("[" + oid.ToString() + "]");
                        }
                    }
                    else {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL); //null pointer
                    }

                    if (_listOfDels.Count > 0) {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), new object().GetHashCode(), null, JIFlags.FLAG_NULL); //pointer
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), _listOfDels.Count, null, JIFlags.FLAG_NULL);

                        //now align for array
                        var index = (double)ndr.Buffer.Index;
                        long k = (k = Math.Round(index % 8.0)) == 0 ? 0 : 8 - k;
                        ndr.WriteOctetArray(new byte[(int)k], 0, (int)k);

                        for (var i = 0; i < _listOfDels.Count; i++) {
                            var oid = (JIObjectId)_listOfDels[i];
                            JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, oid.OID);
                            //JISystem.getLogger().info("[" + oid + "]");
                        }
                    }
                    else {
                        JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL); //null pointer
                    }

                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.Serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    break;

                case 1: // simple ping
                    if (_setId != null) {
                        JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr, _setId); //setid
                        Log.Logger.Information("Simple Ping going for setId: " + Utils.HexString(_setId, 0, _setId.Length));
                    }
                    else {
                        Log.Logger.Information("Some error ! Simple ping requested , but has no setID ");
                    }
                    break;

                default:
                    //nothing.
                    break;
            }
        }

        /// <inhertidoc/>
        public override void Read(NdrCodec ndr) {
            //read response and fill DSs accordingly
            switch (_opnum) {
                case 2: //complex ping
                    _setId = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8);
                    //ping factor
                    JIMarshalUnMarshalHelper.Deserialize(ndr, typeof(short?), null, JIFlags.FLAG_NULL, null);

                    //hresult
                    var hresult = (int)JIMarshalUnMarshalHelper.Deserialize(ndr,
                        typeof(int), null, JIFlags.FLAG_NULL, null);

                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Complex ping failed , hresult: " + hresult);
                    }

                    break;
                case 1: // simple ping
                    //hresult
                    hresult = (int)JIMarshalUnMarshalHelper.Deserialize(ndr, 
                        typeof(int), null, JIFlags.FLAG_NULL, null);

                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Simple ping failed , hresult: " + hresult);
                    }
                    else {
                        Log.Logger.Information("Simple Ping Succeeded");
                    }
                    break;

                default:
                    //nothing.
                    break;
            }
        }

        internal int _opnum = -1;
        internal List<object> _listOfAdds = new List<object>();
        internal List<object> _listOfDels = new List<object>();
        internal byte[] _setId;
        internal int _seqNum;
    }
}
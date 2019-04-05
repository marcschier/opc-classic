// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using System.Collections;
    using System;
    using Serilog;

    /// <summary>
    /// Oxid ping
    /// </summary>
    internal class JiComOxidPingObject : NdrOp {

        /// <inhertidoc/>
        public override int Opnum => opnum;

        /// <inhertidoc/>
        public override void Write(NdrCodec ndr) {
            switch (opnum) {
                case 2: // complex ping

                    var newlength = 8 + 6 + 8 + listOfAdds.Count * 8 + 8 + listOfDels.Count * 8 + 16;
                    if (newlength > ndr.Buffer.buf.Length) {
                        ndr.Buffer.buf = new byte[newlength + 16];
                    }

                    if (setId == null) {
                        Log.Logger.Information("Complex Ping going for the first time, will get the setId as response of this call ");
                        setId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    }

                    if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        Log.Logger.Information("Complex ping going : listOfAdds -> Size : " + listOfAdds.Count + " , " + listOfAdds);
                        Log.Logger.Information("listOfDels -> Size : " + listOfDels.Count + " , " + listOfDels);
                    }

                    JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr, setId);

                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(short?), (short)seqNum, null, JIFlags.FLAG_NULL); //seq
                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(short?), (short)listOfAdds.Count, null, JIFlags.FLAG_NULL); //add
                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(short?), (short)listOfDels.Count, null, JIFlags.FLAG_NULL); //del

                    if (listOfAdds.Count > 0) {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), new object().GetHashCode(), null, JIFlags.FLAG_NULL); //pointer
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), listOfAdds.Count, null, JIFlags.FLAG_NULL);


                        for (var i = 0; i < listOfAdds.Count; i++) {
                            var oid = (JIObjectId)listOfAdds[i];
                            JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr, oid.OID);
                            Log.Logger.Information("[" + oid.ToString() + "]");
                        }
                    }
                    else {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL); //null pointer
                    }

                    if (listOfDels.Count > 0) {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), new object().GetHashCode(), null, JIFlags.FLAG_NULL); //pointer
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), listOfDels.Count, null, JIFlags.FLAG_NULL);

                        //now align for array
                        var index = (double)ndr.Buffer.Index;
                        long k = (k = Math.Round(index % 8.0)) == 0 ? 0 : 8 - k;
                        ndr.writeOctetArray(new sbyte[(int)k], 0, (int)k);

                        for (var i = 0; i < listOfDels.Count; i++) {
                            var oid = (JIObjectId)listOfDels[i];
                            JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr, oid.OID);
                            //JISystem.getLogger().info("[" + oid + "]");
                        }
                    }
                    else {
                        JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL); //null pointer
                    }

                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    JIMarshalUnMarshalHelper.serialize(ndr, typeof(int?), 0, null, JIFlags.FLAG_NULL);
                    break;

                case 1: // simple ping
                    if (setId != null) {
                        JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr, setId); //setid
                        Log.Logger.Information("Simple Ping going for setId: " + Utils.HexString(setId, 0, setId.Length));
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
            switch (opnum) {
                case 2: //complex ping

                    setId = JIMarshalUnMarshalHelper.readOctetArrayLE(ndr, 8);
                    //ping factor
                    JIMarshalUnMarshalHelper.deSerialize(ndr, typeof(short?), null, JIFlags.FLAG_NULL, null);

                    //hresult
                    var hresult = (int)(int?)JIMarshalUnMarshalHelper.deSerialize(ndr, typeof(int?), null, JIFlags.FLAG_NULL, null);

                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Complex ping failed , hresult: " + hresult);
                    }

                    break;
                case 1: // simple ping

                    //hresult
                    hresult = (int)(int?)JIMarshalUnMarshalHelper.deSerialize(ndr, typeof(int?), null, JIFlags.FLAG_NULL, null);

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

        internal int opnum = -1;
        internal ArrayList listOfAdds = new ArrayList();
        internal ArrayList listOfDels = new ArrayList();
        internal byte[] setId;
        internal int seqNum;
    }
}
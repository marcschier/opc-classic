// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core
{


	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JISystem = common.JISystem;
	using JIComTransportFactory = transport.JIComTransportFactory;

	using Endpoint = rpc.Endpoint;
	using Stub = rpc.Stub;
    using System.Collections;
    using System.IO;
    using System;
    using Serilog;

    /// <summary>
    ///Class only used for Oxid ping requests between the Java client and the COM server. This is not for 
    /// reverse operations i.e COM client and Java server. That is handled at the OxidResolverImpl level in JIComOxidRuntimeHelper,
    /// since each of the Oxid Resolver has a separate thread for COM client.  
    /// </summary>
    internal sealed class JIComOxidStub : Stub
	{

		private static Properties defaults = new Properties();

		static JIComOxidStub()
		{

				defaults.put("rpc.ntlm.lanManagerKey","false");
				defaults.put("rpc.ntlm.sign","false");
				defaults.put("rpc.ntlm.seal","false");
				defaults.put("rpc.ntlm.keyExchange","false");
				defaults.put("rpc.connectionContext","rpc.security.ntlm.NtlmConnectionContext");

		}

        protected override string Syntax => "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";

        public JIComOxidStub(string address, string domain, string username, string password, bool useNTLMv2, bool isSSO) : base()
		{
            TransportFactory = JIComTransportFactory.SingleTon;
            Properties = new Properties(defaults);

			if (isSSO)
			{
                Properties.setProperty("rpc.ntlm.sso", "true");
			}
			else
			{
                Properties.setProperty("rpc.security.username", username);
                Properties.setProperty("rpc.security.password", password);
                Properties.setProperty("rpc.ntlm.domain", domain);
			}

            Address = "ncacn_ip_tcp:" + address + "[135]";
            Properties.setProperty("rpc.ntlm.ntlmv2", useNTLMv2.ToString());
		}

		public sbyte[] call(bool isSimplePing, sbyte[] setId, ArrayList listOfAdds, ArrayList listOfDels, int seqNum)
		{
            var pingObject = new PingObject {
                setId = setId,
                listOfAdds = listOfAdds,
                listOfDels = listOfDels,
                seqNum = seqNum
            };

            if (isSimplePing)
			{
				pingObject.opnum = 1;
			}
			else
			{
				pingObject.opnum = 2;
			}

			try
			{
				call(Endpoint.IDEMPOTENT,pingObject);
			}
			catch (IOException e)
			{
				Log.Logger.Error(e, "JIComOxidStub","call",e);
			}

			//returns setId.
			return pingObject.setId;
		}

		public void close()
		{
			try
			{
				detach();
			}
			catch (Exception)
			{
				//JISystem.getLogger().throwing("JIComOxidStub","close",e);  
			}
		}

	}

	internal class PingObject : NdrObject
	{
		internal int opnum = -1;

		internal ArrayList listOfAdds = new ArrayList();
		internal ArrayList listOfDels = new ArrayList();
		internal sbyte[] setId;
		internal int seqNum;

        public override int Opnum => opnum;

        //read follows write...please remember
        public override void write(NetworkDataRepresentation ndr)
		{
			switch (opnum)
			{
				case 2: //complex ping

					int newlength = 8 + 6 + 8 + listOfAdds.Count * 8 + 8 + listOfDels.Count * 8 + 16;
					if (newlength > ndr.Buffer.buf.Length)
					{
						ndr.Buffer.buf = new sbyte[newlength + 16];
					}

                    if (setId == null) {
                        Log.Logger.Information("Complex Ping going for the first time, will get the setId as response of this call ");
                        setId = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    }

                    if (Log.Logger.IsEnabled(Serilog.Events.LogEventLevel.Information))
					{
						Log.Logger.Information("Complex ping going : listOfAdds -> Size : " + listOfAdds.Count + " , " + listOfAdds);
						Log.Logger.Information("listOfDels -> Size : " + listOfDels.Count + " , " + listOfDels);
					}

					JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr,setId);

					JIMarshalUnMarshalHelper.serialize(ndr,typeof(short?), (short)seqNum, null,JIFlags.FLAG_NULL); //seq
					JIMarshalUnMarshalHelper.serialize(ndr,typeof(short?), (short)listOfAdds.Count, null,JIFlags.FLAG_NULL); //add
					JIMarshalUnMarshalHelper.serialize(ndr,typeof(short?), (short)listOfDels.Count, null,JIFlags.FLAG_NULL); //del

					if (listOfAdds.Count > 0)
					{
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), new object().GetHashCode(), null,JIFlags.FLAG_NULL); //pointer
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), listOfAdds.Count, null,JIFlags.FLAG_NULL);


						for (var i = 0;i < listOfAdds.Count;i++)
						{
							var oid = (JIObjectId)listOfAdds[i];
							JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr,oid.OID);
							Log.Logger.Information("[" + oid.ToString() + "]");
						}
					}
					else
					{
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL); //null pointer
					}

					if (listOfDels.Count > 0)
					{
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), new object().GetHashCode(), null,JIFlags.FLAG_NULL); //pointer
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), listOfDels.Count, null,JIFlags.FLAG_NULL);

						//now align for array
						var index = (double)ndr.Buffer.Index;
						long k = (k = Math.Round(index % 8.0)) == 0 ? 0 : 8 - k;
						ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

						for (var i = 0;i < listOfDels.Count;i++)
						{
							var oid = (JIObjectId)listOfDels[i];
							JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr,oid.OID);
							//JISystem.getLogger().info("[" + oid + "]");
						}
					}
					else
					{
						JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL); //null pointer
					}

					JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.serialize(ndr,typeof(int?), 0, null,JIFlags.FLAG_NULL);
					break;

				case 1: // simple ping

					if (setId != null)
					{
						JIMarshalUnMarshalHelper.writeOctetArrayLE(ndr,setId); //setid
						  var byteArrayOutputStream = new ByteArrayOutputStream();
						   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), setId, 0, setId.Length);
							Log.Logger.Information("Simple Ping going for setId: " + byteArrayOutputStream.ToString());
					}
					else
					{
						Log.Logger.Information("Some error ! Simple ping requested , but has no setID ");
					}
					break;

				default:
					//nothing.
			break;
			}
		}

		public override void read(NetworkDataRepresentation ndr)
		{
			//read response and fill DSs accordingly
			switch (opnum)
			{
				case 2: //complex ping

					setId = JIMarshalUnMarshalHelper.readOctetArrayLE(ndr,8);
					//ping factor
					JIMarshalUnMarshalHelper.deSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);

					//hresult
					var hresult = (int)(int?)JIMarshalUnMarshalHelper.deSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null);

                    if (hresult != 0) {
                        Log.Logger.Error("Some error ! Complex ping failed , hresult: " + hresult);
                    }

                    break;
				case 1: // simple ping

					//hresult
					hresult = (int)(int?)JIMarshalUnMarshalHelper.deSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null);

					if (hresult != 0)
					{
                        Log.Logger.Error("Some error ! Simple ping failed , hresult: " + hresult);
                    }
                    else
					{
                        Log.Logger.Information("Simple Ping Succeeded");
                    }
                    break;

				default:
					//nothing.
			break;
			}
		}
	}
}
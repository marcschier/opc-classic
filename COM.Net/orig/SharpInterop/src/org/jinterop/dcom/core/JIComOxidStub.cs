using System;
using System.Collections.Generic;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {


	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JISystem = org.jinterop.dcom.common.JISystem;
	using JIComTransportFactory = org.jinterop.dcom.transport.JIComTransportFactory;

	using Endpoint = rpc.Endpoint;
	using Stub = rpc.Stub;

	/// <summary>
	///Class only used for Oxid ping requests between the Java client and the COM server. This is not for 
	/// reverse operations i.e COM client and Java server. That is handled at the OxidResolverImpl level in JIComOxidRuntimeHelper,
	/// since each of the Oxid Resolver has a separate thread for COM client.  
	/// 
	/// 
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	internal sealed class JIComOxidStub : Stub {

		private static Properties Defaults = new Properties();

		static JIComOxidStub() {

				Defaults.put("rpc.ntlm.lanManagerKey","false");
				Defaults.put("rpc.ntlm.sign","false");
				Defaults.put("rpc.ntlm.seal","false");
				Defaults.put("rpc.ntlm.keyExchange","false");
				Defaults.put("rpc.connectionContext","rpc.security.ntlm.NtlmConnectionContext");

		}

		public string Syntax {
			get {
				return "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
			}
		}

		public JIComOxidStub(string address, string domain, string username, string password, bool useNTLMv2, bool isSSO) : base() {
			base.TransportFactory = JIComTransportFactory.SingleTon;
			base.Properties = new Properties(Defaults);

			if (isSSO) {
				base.Properties.setProperty("rpc.ntlm.sso", "true");
			}
			else {
				base.Properties.setProperty("rpc.security.username", username);
				base.Properties.setProperty("rpc.security.password", password);
				base.Properties.setProperty("rpc.ntlm.domain", domain);
			}

			base.Address = "ncacn_ip_tcp:" + address + "[135]";
			base.Properties.setProperty("rpc.ntlm.ntlmv2", Convert.ToString(useNTLMv2));
		}

		public sbyte[] Call(bool isSimplePing, sbyte[] setId, List<object> listOfAdds, List<object> listOfDels, int seqNum) {
			PingObject pingObject = new PingObject();
			pingObject.SetId = setId;
			pingObject.ListOfAdds = listOfAdds;
			pingObject.ListOfDels = listOfDels;
			pingObject.SeqNum = seqNum;

			if (isSimplePing) {
				pingObject.Opnum_Renamed = 1;
			}
			else {
				pingObject.Opnum_Renamed = 2;
			}

			try {
				call(Endpoint.IDEMPOTENT,pingObject);
			}
			catch (IOException e) {
				JISystem.Logger.throwing("JIComOxidStub","call",e);
			}

			//returns setId.
			return pingObject.SetId;
		}

		public void Close() {
			try {
				detach();
			}
			catch (Exception) {
				//JISystem.getLogger().throwing("JIComOxidStub","close",e);  
			}
		}

	}

	internal class PingObject : NdrObject {
		internal int Opnum_Renamed = -1;

		internal List<object> ListOfAdds = new List<object>();
		internal List<object> ListOfDels = new List<object>();
		internal sbyte[] SetId = null;
		internal int SeqNum = 0;

		public virtual int Opnum {
			get {
				return Opnum_Renamed;
			}
		}

		//read follows write...please remember
		public virtual void Write(NetworkDataRepresentation ndr) {
			switch (Opnum_Renamed) {
				case 2: //complex ping

					int newlength = 8 + 6 + 8 + ListOfAdds.Count * 8 + 8 + ListOfDels.Count * 8 + 16;
					if (newlength > ndr.Buffer.buf.length) {
						ndr.Buffer.buf = new sbyte[newlength + 16];
					}

					if (SetId == null) {
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Complex Ping going for the first time, will get the setId as response of this call ");
						}
						SetId = new sbyte[]{ 0,0,0,0,0,0,0,0 };
					}
					else {
						ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
						   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), SetId, 0, SetId.Length);
						   if (JISystem.Logger.isLoggable(Level.INFO)) {
							   JISystem.Logger.info("Complex Ping going for setId: " + byteArrayOutputStream.ToString());
						   }
					}

					if (JISystem.Logger.isLoggable(Level.INFO)) {
						JISystem.Logger.info("Complex ping going : listOfAdds -> Size : " + ListOfAdds.Count + " , " + ListOfAdds);
						JISystem.Logger.info("listOfDels -> Size : " + ListOfDels.Count + " , " + ListOfDels);
					}

					JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,SetId);

					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(short?),new short?((short)SeqNum),null,JIFlags.FLAG_NULL); //seq
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(short?),new short?((short)ListOfAdds.Count),null,JIFlags.FLAG_NULL); //add
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(short?),new short?((short)ListOfDels.Count),null,JIFlags.FLAG_NULL); //del

					if (ListOfAdds.Count > 0) {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?((new object()).GetHashCode()),null,JIFlags.FLAG_NULL); //pointer
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(ListOfAdds.Count),null,JIFlags.FLAG_NULL);


						for (int i = 0;i < ListOfAdds.Count;i++) {
							JIObjectId oid = (JIObjectId)ListOfAdds[i];
							JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,oid.OID);
							//JISystem.getLogger().info("[" + oid.toString() + "]");
						}
					}
					else {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL); //null pointer
					}

					if (ListOfDels.Count > 0) {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?((new object()).GetHashCode()),null,JIFlags.FLAG_NULL); //pointer
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(ListOfDels.Count),null,JIFlags.FLAG_NULL);

						//now align for array
						double index = (double)(new int?(ndr.Buffer.Index));
						long k = (k = Math.Round(index % 8.0)) == 0 ? 0 : 8 - k;
						ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

						for (int i = 0;i < ListOfDels.Count;i++) {
							JIObjectId oid = (JIObjectId)ListOfDels[i];
							JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,oid.OID);
							//JISystem.getLogger().info("[" + oid + "]");
						}
					}
					else {
						JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL); //null pointer
					}

					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL);
					JIMarshalUnMarshalHelper.Serialize(ndr,typeof(int?),new int?(0),null,JIFlags.FLAG_NULL);
					break;

				case 1: // simple ping

					if (SetId != null) {
						JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,SetId); //setid
						  ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
						   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), SetId, 0, SetId.Length);
						   if (JISystem.Logger.isLoggable(Level.INFO)) {
							   JISystem.Logger.info("Simple Ping going for setId: " + byteArrayOutputStream.ToString());
						   }
					}
					else {
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Some error ! Simple ping requested , but has no setID ");
						}
					}
					break;

				default:
					//nothing.
			break;
			}
		}

		public virtual void Read(NetworkDataRepresentation ndr) {
			//read response and fill DSs accordingly
			switch (Opnum_Renamed) {
				case 2: //complex ping

					SetId = JIMarshalUnMarshalHelper.ReadOctetArrayLE(ndr,8);
					//ping factor
					JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(short?),null,JIFlags.FLAG_NULL,null);

					//hresult
					int hresult = (int)((int?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null)));

					if (hresult != 0) {
						if (JISystem.Logger.isLoggable(Level.SEVERE)) {
							JISystem.Logger.severe("Some error ! Complex ping failed , hresult: " + hresult);
						}
					}
					else {
						ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
						   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), SetId, 0, SetId.Length);
						   if (JISystem.Logger.isLoggable(Level.INFO)) {
							   JISystem.Logger.info("Complex Ping Succeeded,  setId is : " + byteArrayOutputStream.ToString());
						   }
					}

					break;
				case 1: // simple ping

					//hresult
					hresult = (int)((int?)(JIMarshalUnMarshalHelper.DeSerialize(ndr,typeof(int?),null,JIFlags.FLAG_NULL,null)));

					if (hresult != 0) {
						if (JISystem.Logger.isLoggable(Level.SEVERE)) {
							JISystem.Logger.severe("Some error ! Simple ping failed , hresult: " + hresult);
						}
					}
					else {
						if (JISystem.Logger.isLoggable(Level.INFO)) {
							JISystem.Logger.info("Simple Ping Succeeded");
						}
					}
					break;

				default:
					//nothing.
			break;
			}
		}
	}






}
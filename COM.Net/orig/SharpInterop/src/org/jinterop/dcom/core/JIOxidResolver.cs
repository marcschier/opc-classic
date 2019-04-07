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

	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using JIComVersion = org.jinterop.dcom.common.JIComVersion;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using UUID = rpc.core.UUID;

	/// <summary>
	/// Partially implements IOxidResolver interface, used only for ResolveOxid calls.
	/// 
	/// 
	/// @since 1.23
	/// 
	/// </summary>
	internal sealed class JIOxidResolver : NdrObject {
		private readonly sbyte[] Oxid;

		private JIDualStringArray OxidBindings_Renamed = null;
		private string Ipid = null;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: JIOxidResolver(final byte[] oxid)
		public JIOxidResolver(sbyte[] oxid) {
			this.Oxid = oxid;
		}

		public int Opnum {
			get {
				return 4;
			}
		}

		public void Write(NetworkDataRepresentation ndr) {
			JIMarshalUnMarshalHelper.WriteOctetArrayLE(ndr,Oxid);
			JIMarshalUnMarshalHelper.Serialize(ndr, typeof(short?), new short?((short)1), new List<object>(), JIFlags.FLAG_NULL);
			JIMarshalUnMarshalHelper.Serialize(ndr, typeof(JIArray), new JIArray(new short?[]{ new short?((short)7) },true), new List<object>(), JIFlags.FLAG_REPRESENTATION_ARRAY);
		}

		public void Read(NetworkDataRepresentation ndr) {
			ndr.readUnsignedLong(); //pointer
			ndr.readUnsignedLong(); //some length component, irrelevant for us right now
			OxidBindings_Renamed = JIDualStringArray.Decode(ndr);
			try {
				UUID ipid2 = new UUID();
				ipid2.decode(ndr,ndr.Buffer);
				Ipid = (ipid2.ToString());
			}
			catch (NdrException e) {

				JISystem.Logger.throwing("JIRemActivation","read",e);
			}

			//read the auth hint
			int authenticationHint = ndr.readUnsignedLong();

			JIComVersion comVersion = new JIComVersion();
			comVersion.MajorVersion = ndr.readUnsignedShort();
			comVersion.MinorVersion = ndr.readUnsignedShort();

			int hresult = ndr.readUnsignedLong();

			if (hresult != 0) {
				//System.out.println("EXCEPTION FROM SERVER ! --> " + "0x" + Long.toHexString(hresult).substring(8));
				throw new JIRuntimeException(hresult);
			}

		}

		public JIDualStringArray OxidBindings {
			get {
				return OxidBindings_Renamed;
			}
		}

		public string IPID {
			get {
				return Ipid;
			}
		}

	}

}
/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
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
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>



namespace rpc.pdu {

	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using ProtocolVersion = rpc.core.ProtocolVersion;

	public class BindNoAcknowledgePdu : ConnectionOrientedPdu {

		public const int BIND_NO_ACKNOWLEDGE_TYPE = 0x0d;

		public const int REASON_NOT_SPECIFIED = 0;

		public const int TEMPORARY_CONGESTION = 1;

		public const int LOCAL_LIMIT_EXCEEDED = 2;

		public const int CALLED_PADDR_UNKNOWN = 3; // not used

		public const int PROTOCOL_VERSION_NOT_SUPPORTED = 4;

		public const int DEFAULT_CONTEXT_NOT_SUPPORTED = 5; // not used

		public const int USER_DATA_NOT_READABLE = 6; // not used

		public const int NO_PSAP_AVAILABLE = 7; // not used

		private ProtocolVersion[] VersionList_Renamed;

		private int RejectReason_Renamed = REASON_NOT_SPECIFIED;

		public override int Type {
			get {
				return BIND_NO_ACKNOWLEDGE_TYPE;
			}
		}

		public virtual int RejectReason {
			get {
				return RejectReason_Renamed;
			}
			set {
				this.RejectReason_Renamed = value;
			}
		}


		public virtual ProtocolVersion[] VersionList {
			get {
				return VersionList_Renamed;
			}
			set {
				this.VersionList_Renamed = value;
			}
		}


		public override void ReadBody(NetworkDataRepresentation ndr) {
			int reason = ndr.ReadUnsignedSmall();
			RejectReason = reason;
			ProtocolVersion[] versionList = null;
			if (reason == PROTOCOL_VERSION_NOT_SUPPORTED) {
				int count = ndr.ReadUnsignedSmall();
				versionList = new ProtocolVersion[count];
				for (int i = 0; i < count; i++) {
					versionList[i] = new ProtocolVersion();
					versionList[i].Read(ndr);
				}
			}
			VersionList = versionList;
		}

		public override void WriteBody(NetworkDataRepresentation ndr) {
			int reason = RejectReason;
			ndr.WriteUnsignedSmall((short) reason);
			if (reason != PROTOCOL_VERSION_NOT_SUPPORTED) {
				return;
			}
			ProtocolVersion[] versionList = VersionList;
			int count = (versionList != null) ? versionList.Length : 0;
			for (int i = 0; i < count; i++) {
				versionList[i].Write(ndr);
			}
		}

	}

}
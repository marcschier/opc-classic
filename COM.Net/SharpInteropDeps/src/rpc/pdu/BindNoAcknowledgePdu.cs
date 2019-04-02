// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace rpc.pdu
{

	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using ProtocolVersion = core.ProtocolVersion;

	public class BindNoAcknowledgePdu : ConnectionOrientedPdu
	{

		public const int BIND_NO_ACKNOWLEDGE_TYPE = 0x0d;

		public const int REASON_NOT_SPECIFIED = 0;

		public const int TEMPORARY_CONGESTION = 1;

		public const int LOCAL_LIMIT_EXCEEDED = 2;

		public const int CALLED_PADDR_UNKNOWN = 3; // not used

		public const int PROTOCOL_VERSION_NOT_SUPPORTED = 4;

		public const int DEFAULT_CONTEXT_NOT_SUPPORTED = 5; // not used

		public const int USER_DATA_NOT_READABLE = 6; // not used

		public const int NO_PSAP_AVAILABLE = 7; // not used

		private ProtocolVersion[] versionList;

		private int rejectReason;

        public override int Type => BIND_NO_ACKNOWLEDGE_TYPE;

        public virtual int RejectReason {
            get => rejectReason;
            set => rejectReason = value;
        }


        public virtual ProtocolVersion[] VersionList {
            get => versionList;
            set => versionList = value;
        }


        protected internal override void readBody(NetworkDataRepresentation ndr)
		{
			var reason = ndr.readUnsignedSmall();
			RejectReason = reason;
			ProtocolVersion[] versionList = null;
			if (reason == PROTOCOL_VERSION_NOT_SUPPORTED)
			{
				var count = ndr.readUnsignedSmall();
				versionList = new ProtocolVersion[count];
				for (var i = 0; i < count; i++)
				{
					versionList[i] = new ProtocolVersion();
					versionList[i].read(ndr);
				}
			}
			VersionList = versionList;
		}

		protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			var reason = RejectReason;
			ndr.writeUnsignedSmall((short) reason);
			if (reason != PROTOCOL_VERSION_NOT_SUPPORTED)
			{
				return;
			}
			var versionList = VersionList;
			var count = (versionList != null) ? versionList.Length : 0;
			for (var i = 0; i < count; i++)
			{
				versionList[i].write(ndr);
			}
		}

	}

}
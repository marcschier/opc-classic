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



namespace rpc {

	public class FaultException : RpcException, FaultCodes {

		private readonly sbyte[] Stub_Renamed;
		public int Status = -1;
		public FaultException() : base() {
			Stub_Renamed = null;
		}

		public FaultException(string message) : base(message) {
			Stub_Renamed = null;
		}

		public FaultException(string message, int status) : base(Message(message, status)) {
			this.Status = status;
			Stub_Renamed = null;
		}

		public FaultException(string message, int status, sbyte[] stub) : base(Message(message, status)) {
			this.Status = status;
			this.Stub_Renamed = stub;
		}

		public virtual sbyte[] Stub {
			get {
				return Stub_Renamed;
			}
		}

		private static string Message(string message, int status) {
			return (message != null) ? message + " (" + Message(status) + ")" : Message(status);
		}

		private static string Message(int status) {
			switch (status) {
			case FaultCodes_Fields.RPC_VERSION_MISMATCH:
				return "RPC_VERSION_MISMATCH";
			case FaultCodes_Fields.UNSPECIFIED_REJECTION:
				return "UNSPECIFIED_REJECTION";
			case FaultCodes_Fields.BAD_ACTIVITY_ID:
				return "BAD_ACTIVITY_ID";
			case FaultCodes_Fields.WHO_ARE_YOU_FAILED:
				return "WHO_ARE_YOU_FAILED";
			case FaultCodes_Fields.MANAGER_NOT_ENTERED:
				return "MANAGER_NOT_ENTERED";
			case FaultCodes_Fields.OPERATION_RANGE_ERROR:
				return "OPERATION_RANGE_ERROR";
			case FaultCodes_Fields.UNKNOWN_INTERFACE:
				return "UNKNOWN_INTERFACE";
			case FaultCodes_Fields.WRONG_BOOT_TIME:
				return "WRONG_BOOT_TIME";
			case FaultCodes_Fields.YOU_CRASHED:
				return "YOU_CRASHED";
			case FaultCodes_Fields.PROTOCOL_ERROR:
				return "PROTOCOL_ERROR";
			case FaultCodes_Fields.OUTPUT_ARGUMENTS_TOO_BIG:
				return "OUTPUT_ARGUMENTS_TOO_BIG";
			case FaultCodes_Fields.SERVER_TOO_BUSY:
				return "SERVER_TOO_BUSY";
			case FaultCodes_Fields.UNSUPPORTED_TYPE:
				return "UNSUPPORTED_TYPE";
			case FaultCodes_Fields.INVALID_PRESENTATION_CONTEXT_ID:
				return "INVALID_PRESENTATION_CONTEXT_ID";
			case FaultCodes_Fields.UNSUPPORTED_AUTHENTICATION_LEVEL:
				return "UNSUPPORTED_AUTHENTICATION_LEVEL";
			case FaultCodes_Fields.INVALID_CHECKSUM:
				return "INVALID_CHECKSUM";
			case FaultCodes_Fields.INVALID_CRC:
				return "INVALID_CRC";
			default:
				return "unknown";
			}
		}

	}

}
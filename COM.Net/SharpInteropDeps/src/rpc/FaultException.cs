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



namespace rpc
{

	public class FaultException : RpcException, FaultCodes
	{

		private readonly sbyte[] stub;
		public int status = -1;
		public FaultException() : base()
		{
			stub = null;
		}

		public FaultException(string message) : base(message_Renamed)
		{
			stub = null;
		}

		public FaultException(string message, int status) : base(message(message_Renamed, status))
		{
			this.status = status;
			stub = null;
		}

		public FaultException(string message, int status, sbyte[] stub) : base(message(message_Renamed, status))
		{
			this.status = status;
			this.stub = stub;
		}

        public virtual sbyte[] Stub => stub;

        private static string message(string message, int status)
		{
			return (message_Renamed != null) ? message_Renamed + " (" + message(status) + ")" : message(status);
		}

		private static string message(int status)
		{
			switch (status)
			{
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
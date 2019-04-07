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

	public interface FaultCodes {

		/// <summary>
		/// Fault status indicating the server does not support the RPC protocol
		/// version specified in the request.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server is rejecting the request for
		/// unspecified reasons.
		/// </summary>

		/// <summary>
		/// Connectionless fault status indicating the server has no state
		/// corresponding to the specified activity identifier.
		/// </summary>

		/// <summary>
		/// Connectionless fault status indicating the conversation manager
		/// callback failed.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server manager routine has not been
		/// entered and executed.
		/// </summary>

		/// <summary>
		/// Fault status indicating the requested operation number is out of
		/// range.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server does not export the interface
		/// requested by the client.
		/// </summary>

		/// <summary>
		/// Connectionless fault status indicating the specified boot time does
		/// not match the actual server boot time.
		/// </summary>

		/// <summary>
		/// Connectionless fault status indicating a restarted server called
		/// back a client.
		/// </summary>

		/// <summary>
		/// Fault status indicating a protocol violation.
		/// </summary>

		/// <summary>
		/// Fault status indicating the operation's output parameters are larger
		/// than their declared maximum size.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server is currently too busy to service
		/// the request.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server does not implement the requested
		/// operation for the requested object's type.
		/// </summary>

		/// <summary>
		/// Connection-oriented fault status indicating the requested presentation
		/// context ID is invalid.
		/// </summary>

		/// <summary>
		/// Fault status indicating the server does not support the authentication
		/// level requested.
		/// </summary>

		/// <summary>
		/// Fault status indicating an invalid checksum.
		/// </summary>

		/// <summary>
		/// Fault status indicating an invalid CRC.
		/// </summary>

	}

	public static class FaultCodes_Fields {
		public const int RPC_VERSION_MISMATCH = 0x1c000008;
		public const int UNSPECIFIED_REJECTION = 0x1c000009;
		public const int BAD_ACTIVITY_ID = 0x1c00000a;
		public const int WHO_ARE_YOU_FAILED = 0x1c00000b;
		public const int MANAGER_NOT_ENTERED = 0x1c00000c;
		public const int OPERATION_RANGE_ERROR = 0x1c010002;
		public const int UNKNOWN_INTERFACE = 0x1c010003;
		public const int WRONG_BOOT_TIME = 0x1c010006;
		public const int YOU_CRASHED = 0x1c010009;
		public const int PROTOCOL_ERROR = 0x1c01000b;
		public const int OUTPUT_ARGUMENTS_TOO_BIG = 0x1c010013;
		public const int SERVER_TOO_BUSY = 0x1c010014;
		public const int UNSUPPORTED_TYPE = 0x1c010017;
		public const int INVALID_PRESENTATION_CONTEXT_ID = 0x1c00001c;
		public const int UNSUPPORTED_AUTHENTICATION_LEVEL = 0x1c00001d;
		public const int INVALID_CHECKSUM = 0x1c00001f;
		public const int INVALID_CRC = 0x1c000020;
	}

}
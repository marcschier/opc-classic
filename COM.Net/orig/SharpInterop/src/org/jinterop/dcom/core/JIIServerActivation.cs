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

	internal interface JIIServerActivation {

		bool ActivationSuccessful { get; }

		JIDualStringArray DualStringArrayForOxid { get; }

		JIInterfacePointer MInterfacePointer { get; }

		string IPID { get; }

		bool Dual { get; }

		string DispIpid { get;set; }

		int DispRefs { get; }


	}

	public static class JIIServerActivation_Fields {
		public const int RPC_C_IMP_LEVEL_IDENTIFY = 2;
		public const int RPC_C_IMP_LEVEL_IMPERSONATE = 3;
	}
}
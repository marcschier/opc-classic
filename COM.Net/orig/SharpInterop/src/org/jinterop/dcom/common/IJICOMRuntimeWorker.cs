using System.Collections;

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

namespace org.jinterop.dcom.common {

	using UUID = rpc.core.UUID;

	/// <summary>
	/// Framework Internal.
	/// 
	/// @exclude
	/// @since 1.0
	/// </summary>
	public interface IJICOMRuntimeWorker {
		int Opnum { set; }
		string CurrentIID { set; }
		UUID CurrentObjectID { set;get; }
		IList QIedIIDs { get; }
		bool Resolver { get; }
		bool WorkerOver();
	}

}
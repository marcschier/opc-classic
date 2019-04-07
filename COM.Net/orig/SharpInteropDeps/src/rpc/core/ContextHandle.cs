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

namespace rpc.core {

	using NdrObject = ndr.NdrObject;

	public class ContextHandle : NdrObject {

		internal int Attributes_Renamed;
		internal UUID Uuid_Renamed;

		public ContextHandle(int attributes, UUID uuid) {
			Attributes = attributes;
			Uuid = uuid;
		}

		public virtual int Attributes {
			get {
				return Attributes_Renamed;
			}
			set {
				this.Attributes_Renamed = value;
			}
		}


		public virtual UUID Uuid {
			get {
				return Uuid_Renamed;
			}
			set {
				this.Uuid_Renamed = value;
			}
		}


	}

}
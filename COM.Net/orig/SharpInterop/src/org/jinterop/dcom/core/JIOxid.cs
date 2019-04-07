using System;

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


	[Serializable]
	internal sealed class JIOxid {

		private const long SerialVersionUID = 3456725801334190150L;
		internal sbyte[] Oxid = null;

		public JIOxid(sbyte[] oxid) {
			this.Oxid = oxid;
		}

		public sbyte[] OXID {
			get {
				return Oxid;
			}
		}

		public override int GetHashCode() {
			int result = 1;
			//from SUN
			for (int i = 0;i < Oxid.Length;i++) {
				result = 31 * result + Oxid[i];
			}
			return result;
			//return Arrays.hashCode(oxid);
		}

		 public override bool Equals(object obj) {
			 if (!(obj is JIOxid)) {
				return false;
			 }

			 return Arrays.Equals(Oxid,((JIOxid)obj).OXID);
		 }

	}

}
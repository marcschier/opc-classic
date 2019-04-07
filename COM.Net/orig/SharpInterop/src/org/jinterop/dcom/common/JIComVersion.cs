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

namespace org.jinterop.dcom.common {

	/// <summary>
	///<para> Framework Internal.
	/// This class represents the <code>COM</code> version of the currently 
	/// supported COM protocol. Default version is 5.4.
	/// </para>
	/// @exclude
	/// @since 1.0
	/// </summary>
	[Serializable]
	public sealed class JIComVersion {


		private const long SerialVersionUID = -1252228963385487909L;
		private int MajorVersion_Renamed = 5;
		private int MinorVersion_Renamed = 4;

		public JIComVersion() {
		}

		public JIComVersion(int majorVersion, int minorVersion) {
			this.MajorVersion_Renamed = majorVersion;
			this.MinorVersion_Renamed = minorVersion;
		}

		public int MajorVersion {
			set {
				this.MajorVersion_Renamed = value;
			}
			get {
				return MajorVersion_Renamed;
			}
		}


		public int MinorVersion {
			set {
				this.MinorVersion_Renamed = value;
			}
			get {
				return MinorVersion_Renamed;
			}
		}


	}

}
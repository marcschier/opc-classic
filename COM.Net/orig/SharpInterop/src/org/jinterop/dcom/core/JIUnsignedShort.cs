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

	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JISystem = org.jinterop.dcom.common.JISystem;

	/// <summary>
	/// Class representing the unsigned c++ short.
	/// 
	/// @since 1.15(b)
	/// 
	/// </summary>
	public sealed class JIUnsignedShort : IJIUnsigned {

		private readonly int? ShortValue;

		public JIUnsignedShort(int? shortValue) {
			if (shortValue == null || (int)shortValue < 0) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_UNSIGNED_NEGATIVE));
			}
			this.ShortValue = shortValue;
		}

		public int Type {
			get {
				return JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT;
			}
		}

		public Number Value {
			get {
				return ShortValue;
			}
		}

	}

}
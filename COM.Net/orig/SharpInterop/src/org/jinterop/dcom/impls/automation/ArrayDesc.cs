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

namespace org.jinterop.dcom.impls.automation {

	using JIArray = org.jinterop.dcom.core.JIArray;
	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIStruct = org.jinterop.dcom.core.JIStruct;

	/// <summary>
	/// Implements the <i>ARRAYDESC</i> structure of COM Automation.
	/// <para>
	/// Definition from MSDN: <i> Contained within the TYPEDESC, which describes the
	/// type of the array's elements, and information about the array's dimensions.
	/// </i>
	/// 
	/// @since 1.0
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class ArrayDesc {

		private const long SerialVersionUID = 8801586899375554929L;
		/// <summary>
		/// Element Type.
		/// </summary>
		public readonly TypeDesc TypeDesc;
		/// <summary>
		/// Dimension Count.
		/// </summary>
		public readonly short CDims;
		/// <summary>
		/// Variable length array containing one element for each dimension.
		/// </summary>
		public readonly SafeArrayBounds[] SafeArrayBounds;

		public ArrayDesc(JIStruct values) {
			if (values == null) {
				TypeDesc = null;
				CDims = -1;
				SafeArrayBounds = null;
				return;
			}

			TypeDesc = new TypeDesc((JIStruct)values.GetMember(0));
			CDims = (short)((short?)values.GetMember(1));
			JIArray arry = (JIArray)values.GetMember(2);
			object[] arry2 = (object [])arry.ArrayInstance;

			if (arry2 != null) {
				SafeArrayBounds = new SafeArrayBounds[arry2.Length];
				for (int i = 0;i < arry2.Length; i++) {
					SafeArrayBounds[i] = new SafeArrayBounds((JIStruct)arry2[i]);
				}
			}
			else {
				SafeArrayBounds = null;
			}
		}

		public ArrayDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.GetReferent()) {
		}
	}

}
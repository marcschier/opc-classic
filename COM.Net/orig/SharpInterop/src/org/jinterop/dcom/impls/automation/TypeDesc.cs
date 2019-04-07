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

	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIStruct = org.jinterop.dcom.core.JIStruct;
	using JIUnion = org.jinterop.dcom.core.JIUnion;

	/// <summary>
	///Implements the <i>TYPEDESC</i> structure of COM Automation and
	/// describes the type of a variable, the return type of a function,
	/// or the type of a function parameter.
	/// 
	/// @since 1.0
	/// </summary>
	[Serializable]
	public sealed class TypeDesc {

		private const long SerialVersionUID = 6276233095707726579L;
		public static readonly short? VT_PTR = new short?((short)0x1a);
		public static readonly short? VT_SAFEARRAY = new short?((short)0x1b);
		public static readonly short? VT_CARRAY = new short?((short)0x1c);
		public static readonly short? VT_USERDEFINED = new short?((short)0x1d);

		public readonly JIPointer TypeDesc_Renamed;
		public readonly JIPointer ArrayDesc;
		public readonly int HrefType;
		public readonly short Vt;

		public TypeDesc(JIStruct values) {
			if (values == null) {
				TypeDesc_Renamed = null;
				ArrayDesc = null;
				HrefType = -1;
				Vt = -1;
				return;
			}

			Vt = (short)((short?)values.GetMember(1));
			JIUnion union = (JIUnion)values.GetMember(0);

			if ((new short?(Vt)).Equals(VT_PTR) || (new short?(Vt)).Equals(VT_SAFEARRAY)) {
				JIPointer pointer = (pointer = (JIPointer)union.Members.GetValueOrNull(VT_PTR)) == null ? (JIPointer)union.Members.GetValueOrNull(VT_SAFEARRAY) : pointer;
				TypeDesc_Renamed = new JIPointer(new TypeDesc(pointer),false);
				ArrayDesc = null;
				HrefType = -1;
			}
			else if ((new short?(Vt)).Equals(VT_CARRAY)) {
				HrefType = -1;
				TypeDesc_Renamed = null;
				ArrayDesc = new JIPointer(new ArrayDesc((JIPointer)union.Members.GetValueOrNull(VT_CARRAY)));
			}
			else if ((new short?(Vt)).Equals(VT_USERDEFINED)) {
				TypeDesc_Renamed = null;
				ArrayDesc = null;
				HrefType = (int)((int?)union.Members.GetValueOrNull(VT_USERDEFINED));
			}
			else {
				TypeDesc_Renamed = null;
				ArrayDesc = null;
				HrefType = -1;
			}

		}

		public TypeDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.GetReferent()) {
		}

	}

}
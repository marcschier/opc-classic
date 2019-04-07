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


	/// 
	/// <summary>
	/// @since 1.0
	/// 
	/// </summary>
	[Serializable]
	internal sealed class JIOrpcExtentArray {

		private const long SerialVersionUID = -3594184670915738836L;
		private string Uuid = null;
		private int Size = -1;
		private sbyte?[] Data_Renamed = null;

		public JIOrpcExtentArray(string guid, int size, sbyte?[] data) {
			Uuid = guid;
			this.Size = size;
			this.Data_Renamed = data;
		}

		public string GUID {
			get {
				return Uuid;
			}
		}

		public int SizeOfData {
			get {
				return Size;
			}
		}

		public sbyte[] Data {
			get {
				sbyte[] newData = new sbyte[Data_Renamed.Length];
				for (int i = 0;i < Data_Renamed.Length;i++) {
					newData[i] = (sbyte)Data_Renamed[i];
				}
				return newData;
			}
		}
	}

}
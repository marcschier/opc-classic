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

	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
	using JIComObjectImplWrapper = org.jinterop.dcom.core.JIComObjectImplWrapper;
	using JIFlags = org.jinterop.dcom.core.JIFlags;
	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIStruct = org.jinterop.dcom.core.JIStruct;

	using UUID = rpc.core.UUID;
	/// <summary>
	/// @exclude
	/// @since 1.0
	/// </summary>
	[Serializable]
	internal sealed class JITypeLibImpl : JIComObjectImplWrapper, IJITypeLib {

		/// 
		private const long SerialVersionUID = -7090247136574816759L;

		//IJIComObject comObject = null;
		//JIRemUnknown unknown = null;
		public JITypeLibImpl(IJIComObject comObject) : base(comObject) { //, JIRemUnknown unknown
			//this.comObject = comObject;
		}

		public IJIComObject COMObject {
			get {
				return ComObject;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException
		public int TypeInfoCount {
			get {
				JICallBuilder callObject = new JICallBuilder(true);
				callObject.Opnum = 0;
				callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
				object[] result = ComObject.Call(callObject);
				return (int)((int?)result[0]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int index) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo GetTypeInfo(int index) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 1;
			callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
			return (IJITypeInfo) JIObjectFactory.NarrowObject((IJIComObject)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoType(int index) throws org.jinterop.dcom.common.JIException
		public int GetTypeInfoType(int index) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 2;
			callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
			return (int)((int?)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfoOfGuid(String uuid) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo GetTypeInfoOfGuid(string uuid) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 3;
			callObject.AddInParamAsUUID(uuid,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
			return (IJITypeInfo) JIObjectFactory.NarrowObject((IJIComObject)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getLibAttr() throws org.jinterop.dcom.common.JIException
		public void GetLibAttr() {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 4;

			JIStruct tlibattr = new JIStruct();
			tlibattr.AddMember(typeof(UUID));
			tlibattr.AddMember(typeof(int?));
			tlibattr.AddMember(typeof(int?));
			tlibattr.AddMember(typeof(short?));
			tlibattr.AddMember(typeof(short?));
			tlibattr.AddMember(typeof(short?));

			callObject.AddOutParamAsObject(new JIPointer(tlibattr),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL); //CLEANUPSTORAGE
			object[] result = ComObject.Call(callObject);
			int i = 0;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDocumentation(int memberId) throws org.jinterop.dcom.common.JIException
		public object[] GetDocumentation(int memberId) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.AddInParamAsInt(memberId,JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(0xb,JIFlags.FLAG_NULL); //refPtrFlags , as per the oaidl.idl...
			callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			callObject.Opnum = 6;
			return ComObject.Call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] findName(org.jinterop.dcom.core.JIString nameBuf,int hashValue,short found) throws org.jinterop.dcom.common.JIException
		public object[] FindName(JIString nameBuf, int hashValue, short found) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 8;
			callObject.AddInParamAsString((nameBuf).String,nameBuf.Type);
			callObject.AddInParamAsInt(hashValue,JIFlags.FLAG_NULL);
			callObject.AddInParamAsShort(found,JIFlags.FLAG_NULL);

			callObject.AddOutParamAsObject(new JIArray(typeof(IJIComObject),null,1,true,true),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(new JIArray(typeof(int?),null,1,true,true),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(short?),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);

			return ComObject.Call(callObject);
		}
	}

}
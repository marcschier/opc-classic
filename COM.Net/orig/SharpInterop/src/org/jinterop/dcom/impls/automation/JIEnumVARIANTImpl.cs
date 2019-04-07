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
	using JIVariant = org.jinterop.dcom.core.JIVariant;

	/// <summary>
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	internal sealed class JIEnumVARIANTImpl : JIComObjectImplWrapper, IJIEnumVariant {

		//IJIComObject comObject = null;

		/// 
		private const long SerialVersionUID = -8405188611519724869L;

		public JIEnumVARIANTImpl(IJIComObject comObject) : base(comObject) {
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] next(int celt) throws org.jinterop.dcom.common.JIException
		public object[] Next(int celt) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 0;
			callObject.AddInParamAsInt(celt,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsObject(new JIArray(typeof(JIVariant),null,1,true,true),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skip(int celt) throws org.jinterop.dcom.common.JIException
		public void Skip(int celt) {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 1;
			callObject.AddInParamAsInt(celt,JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws org.jinterop.dcom.common.JIException
		public void Reset() {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 2;
			object[] result = ComObject.Call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIEnumVariant Clone() throws org.jinterop.dcom.common.JIException
		public IJIEnumVariant Clone() {
			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 3;
			callObject.AddOutParamAsObject(typeof(IJIComObject),JIFlags.FLAG_NULL);
			object[] result = ComObject.Call(callObject);
			return (IJIEnumVariant)JIObjectFactory.NarrowObject((IJIComObject)result[0]);
		}


	}

}
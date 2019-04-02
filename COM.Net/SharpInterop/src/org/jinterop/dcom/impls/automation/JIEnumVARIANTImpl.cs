// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComObjectImplWrapper = core.JIComObjectImplWrapper;
    using JIFlags = core.JIFlags;
    using JIVariant = core.JIVariant;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    internal sealed class JIEnumVARIANTImpl : JIComObjectImplWrapper, IJIEnumVariant
	{

		//IJIComObject comObject = null;

		/// 
		private const long serialVersionUID = -8405188611519724869L;

		internal JIEnumVARIANTImpl(IJIComObject comObject) : base(comObject)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] next(int celt) throws org.jinterop.dcom.common.JIException
		public object[] next(int celt)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 0
            };
            callObject.addInParamAsInt(celt,JIFlags.FLAG_NULL);
			callObject.addOutParamAsObject(new JIArray(typeof(JIVariant),null,1,true,true),JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skip(int celt) throws org.jinterop.dcom.common.JIException
		public void skip(int celt)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 1
            };
            callObject.addInParamAsInt(celt,JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws org.jinterop.dcom.common.JIException
		public void reset()
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 2
            };
            var result = comObject.call(callObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIEnumVariant Clone() throws org.jinterop.dcom.common.JIException
		public IJIEnumVariant Clone()
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.addOutParamAsObject(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = comObject.call(callObject);
			return (IJIEnumVariant)JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}


	}

}
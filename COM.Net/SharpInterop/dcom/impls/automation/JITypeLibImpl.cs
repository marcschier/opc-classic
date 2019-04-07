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
    using JIPointer = core.JIPointer;
    using JIString = core.JIString;
    using JIStruct = core.JIStruct;

    using UUID = rpc.core.UUID;
    /// <summary>
    /// @exclude
    /// @since 1.0
    /// </summary>
    [Serializable]
	internal sealed class JITypeLibImpl : JIComObjectImplWrapper, IJITypeLib
	{

		/// 
		private const long serialVersionUID = -7090247136574816759L;

		//IJIComObject comObject = null;
		//JIRemUnknown unknown = null;
		internal JITypeLibImpl(IJIComObject comObject) : base(comObject) //, JIRemUnknown unknown
		{
			//this.comObject = comObject;
		}

        public IJIComObject COMObject => ComObject;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException
        public int TypeInfoCount
		{
			get
			{
                var callObject = new JICallBuilder(true) {
                    Opnum = 0
                };
                callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
				var result = ComObject.Call(callObject);
				return (int)(int?)result[0];
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int index) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo getTypeInfo(int index)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 1
            };
            callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = ComObject.Call(callObject);
			return (IJITypeInfo) JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoType(int index) throws org.jinterop.dcom.common.JIException
		public int getTypeInfoType(int index)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 2
            };
            callObject.AddInParamAsInt(index,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			var result = ComObject.Call(callObject);
			return (int)(int?)result[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfoOfGuid(String uuid) throws org.jinterop.dcom.common.JIException
		public IJITypeInfo getTypeInfoOfGuid(string uuid)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 3
            };
            callObject.AddInParamAsUUID(uuid,JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = ComObject.Call(callObject);
			return (IJITypeInfo) JIObjectFactory.narrowObject((IJIComObject)result[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getLibAttr() throws org.jinterop.dcom.common.JIException
		public void getLibAttr()
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 4
            };

            var tlibattr = new JIStruct();
			tlibattr.AddMember(typeof(UUID));
			tlibattr.AddMember(typeof(int?));
			tlibattr.AddMember(typeof(int?));
			tlibattr.AddMember(typeof(short?));
			tlibattr.AddMember(typeof(short?));
			tlibattr.AddMember(typeof(short?));

			callObject.AddOutParamAsObject(new JIPointer(tlibattr),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL); //CLEANUPSTORAGE
			var result = ComObject.Call(callObject);
			var i = 0;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDocumentation(int memberId) throws org.jinterop.dcom.common.JIException
		public object[] getDocumentation(int memberId)
		{
			var callObject = new JICallBuilder(true);
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
		public object[] findName(JIString nameBuf, int hashValue, short found)
		{
            var callObject = new JICallBuilder(true) {
                Opnum = 8
            };
            callObject.AddInParamAsString(nameBuf.String,nameBuf.Type);
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
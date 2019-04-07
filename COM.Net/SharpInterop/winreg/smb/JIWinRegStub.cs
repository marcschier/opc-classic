// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace org.jinterop.winreg.smb {



    using SmbException = SharpCifs.smb.SmbException;

    using IJIAuthInfo = dcom.common.IJIAuthInfo;
    using JIErrorCodes = dcom.common.JIErrorCodes;
    using JIException = dcom.common.JIException;
    using JIRuntimeException = dcom.common.JIRuntimeException;
    using JISystem = dcom.common.JISystem;

    using Endpoint = rpc.IEndpoint;
    using Stub = rpc.Stub;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    public class JIWinRegStub : Stub, IJIWinReg
	{


		//"ncacn_np:" + servername + "[\\PIPE\\winreg]"
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIWinRegStub(org.jinterop.dcom.common.IJIAuthInfo authInfo, String serverName) throws java.net.UnknownHostException
		public JIWinRegStub(IJIAuthInfo authInfo, string serverName) 		{
			if (authInfo == null)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
			}

			base.TransportFactory = new rpc.ncacn_np.TransportFactory();
			base.SharpCifs.Util.Sharpen.Properties = new SharpCifs.Util.Sharpen.Properties();
			base.SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ncacn_np.username", authInfo.UserName);
			string password = null;
			try
			{
				password = URLEncoder.encode(authInfo.Password,"utf-8");
			}
			catch (UnsupportedEncodingException)
			{
				try
				{
					password = URLEncoder.encode(authInfo.Password,System.getProperty("file.encoding"));
				}
				catch (UnsupportedEncodingException)
				{
					throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION2);
				}
			}
			//some strange issue with the space character, it gets encoded to '+' (which is right) , but Windows refuses it.
			//Manually changing + to %20
			var password_ = new StringBuilder();
			for (var i = 0 ; i < password.Length; i++)
			{
				var ch = password[i];
				if (ch == '+')
				{
					password_.Append("%20");
					continue;
				}

				password_.Append(ch);
			}

			base.SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ncacn_np.password", password_.ToString());
			base.SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ncacn_np.domain", authInfo.Domain);
			serverName = serverName.Trim();
			serverName = InetAddress.getByName(serverName).HostAddress;
			base.Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIWinRegStub(String serverName) throws java.net.UnknownHostException
		public JIWinRegStub(string serverName) 		{
			base.TransportFactory = new rpc.ncacn_np.TransportFactory();
			base.SharpCifs.Util.Sharpen.Properties = new SharpCifs.Util.Sharpen.Properties();
			base.SharpCifs.Util.Sharpen.Properties.setProperty("rpc.ntlm.sso", "true");
			serverName = serverName.Trim();
			serverName = InetAddress.getByName(serverName).HostAddress;
			base.Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKLM() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_OpenHKLM()
		{
			var openhklm = new IJIWinReg_openHKLM();
			var handle = new JIPolicyHandle(false);
			try
			{
				call(Endpoint.IDEMPOTENT,openhklm);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			Array.Copy(openhklm.policyhandle,0,handle.handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKCR() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_OpenHKCR()
		{
			var openhkcr = new IJIWinReg_openHKCR();
			var handle = new JIPolicyHandle(false);
			try
			{
				call(Endpoint.IDEMPOTENT,openhkcr);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			Array.Copy(openhkcr.policyhandle,0,handle.handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKCU() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_OpenHKCU()
		{
			var openhkcu = new IJIWinReg_openHKCU();
			var handle = new JIPolicyHandle(false);
			try
			{
				call(Endpoint.IDEMPOTENT,openhkcu);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			Array.Copy(openhkcu.policyhandle,0,handle.handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKU() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_OpenHKU()
		{
			var openhku = new IJIWinReg_openHKU();
			var handle = new JIPolicyHandle(false);
			try
			{
				call(Endpoint.IDEMPOTENT,openhku);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			Array.Copy(openhku.policyhandle,0,handle.handle,0,20);

			return handle;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenKey(org.jinterop.winreg.JIPolicyHandle handle,String key, int accessMask) throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_OpenKey(JIPolicyHandle handle, string key, int accessMask)
		{
            var openkey = new IJIWinReg_openKey {
                accessMask = accessMask,
                key = key,
                parentKey = handle
            };
            var newHandle = new JIPolicyHandle(false);
			try
			{
				call(Endpoint.IDEMPOTENT,openkey);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			Array.Copy(openkey.policyhandle,0,newHandle.handle,0,20);

			return newHandle;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_CloseKey(org.jinterop.winreg.JIPolicyHandle handle) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_CloseKey(JIPolicyHandle handle)
		{
            var closekey = new IJIWinReg_closeKey {
                key = handle
            };
            try
			{
				call(Endpoint.IDEMPOTENT,closekey);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_DeleteKeyOrValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, bool isKey) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_DeleteKeyOrValue(JIPolicyHandle handle, string valueName, bool isKey)
		{
            var delete = new IJIWinReg_deleteValueOrKey {
                parentKey = handle,
                valueName = valueName,
                isKey = isKey
            };
            try
			{
				call(Endpoint.IDEMPOTENT,delete);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] winreg_QueryValue(org.jinterop.winreg.JIPolicyHandle handle,int bufferSize) throws org.jinterop.dcom.common.JIException
		public virtual sbyte[] winreg_QueryValue(JIPolicyHandle handle, int bufferSize)
		{
            var queryvalue = new IJIWinReg_queryValue {
                parentKey = handle,
                bufferLength = bufferSize
            };
            try
			{
				call(Endpoint.IDEMPOTENT,queryvalue);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			//return queryvalue.key;
			return queryvalue.buffer;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_QueryValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName,int bufferSize) throws org.jinterop.dcom.common.JIException
		public virtual object[] winreg_QueryValue(JIPolicyHandle handle, string valueName, int bufferSize)
		{
            var queryvalue = new IJIWinReg_queryValue {
                parentKey = handle,
                bufferLength = bufferSize,
                key = valueName
            };

            try
			{
				call(Endpoint.IDEMPOTENT,queryvalue);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			return new object[]{ queryvalue.type, queryvalue.buffer != null ? (object)queryvalue.buffer : (object)queryvalue.buffer2};
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SaveFile(org.jinterop.winreg.JIPolicyHandle handle, String fileName) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_SaveFile(JIPolicyHandle handle, string fileName)
		{
            var savefile = new IJIWinReg_saveFile {
                parentKey = handle,
                fileName = fileName
            };

            try
			{
				call(Endpoint.IDEMPOTENT,savefile);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_CreateKey(org.jinterop.winreg.JIPolicyHandle handle, String subKey, int options,int accessMask) throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle winreg_CreateKey(JIPolicyHandle handle, string subKey, int options, int accessMask)
		{
            var createkey = new IJIWinReg_createKey {
                accessMask = accessMask,
                key = subKey,
                parentKey = handle,
                options = options
            };

            try
			{
				call(Endpoint.IDEMPOTENT,createkey);
			}
			catch (SmbException e)
			{
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			var newHandle = new JIPolicyHandle(createkey.actiontaken == 1 ? true : false);
			Array.Copy(createkey.policyhandle,0,newHandle.handle,0,20);

			return newHandle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName,byte[][] data) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[][] data)
		{
			if (data == null)
			{
				throw new System.ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_WINREG_EXCEPTION5));
			}

			//calculate length of all strings + extra null in the end
			var totalStrings = data.Length;
			var length = 0;
			for (var i = 0;i < totalStrings;i++)
			{
				var j = data[i].Length;
				length += (j + 1) * 2; //including null termination
			}

			length += 2; //final termination

            var setvalue = new IJIWinReg_setValue {
                clazzType = IJIWinReg_Fields.REG_MULTI_SZ,
                data2 = data,
                lengthInBytes = length,
                parentKey = handle,
                valueName = valueName
            };
            Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_SetValue(JIPolicyHandle handle, string valueName)
		{
            var setvalue = new IJIWinReg_setValue {
                clazzType = IJIWinReg_Fields.REG_NONE,
                parentKey = handle,
                valueName = valueName
            };
            Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, byte[] data, bool isBinary, bool expand_sz) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[] data, bool isBinary, bool expand_sz)
		{
			var setvalue = new IJIWinReg_setValue();
			if (isBinary)
			{
				setvalue.clazzType = IJIWinReg_Fields.REG_BINARY;
			}
			else
			{
				if (expand_sz)
				{
					setvalue.clazzType = IJIWinReg_Fields.REG_EXPAND_SZ;
				}
				else
				{
					setvalue.clazzType = IJIWinReg_Fields.REG_SZ;
				}
			}

			setvalue.data = data;
			setvalue.lengthInBytes = data.Length;
			setvalue.parentKey = handle;
			setvalue.valueName = valueName;
			Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, int data) throws org.jinterop.dcom.common.JIException
		public virtual void winreg_SetValue(JIPolicyHandle handle, string valueName, int data)
		{
            var setvalue = new IJIWinReg_setValue {
                clazzType = IJIWinReg_Fields.REG_DWORD,
                lengthInBytes = 4,
                dword = data,
                parentKey = handle,
                valueName = valueName
            };
            Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] winreg_EnumKey(org.jinterop.winreg.JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException
		public virtual string[] winreg_EnumKey(JIPolicyHandle handle, int index)
		{
            var enumkey = new IJIWinReg_enumKey {
                parentKey = handle,
                index = index
            };

            try
			{
				call(Endpoint.IDEMPOTENT,enumkey);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			return enumkey.retval;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_EnumValue(org.jinterop.winreg.JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException
		public virtual object[] winreg_EnumValue(JIPolicyHandle handle, int index)
		{
            var enumvalue = new IJIWinReg_enumValue {
                parentKey = handle,
                index = index
            };

            try
			{
				call(Endpoint.IDEMPOTENT,enumvalue);
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e)
			{
				throw new JIException(e);
			}

			return enumvalue.retval;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setValue(org.jinterop.winreg.IJIWinReg_setValue setvalue) throws org.jinterop.dcom.common.JIException
		private IJIWinReg_setValue Value
		{
			set
			{
				try
				{
					call(Endpoint.IDEMPOTENT,value);
				}
				catch (IOException e)
				{
					throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
				}
				catch (JIRuntimeException e)
				{
					throw new JIException(e);
				}
			}
		}

        protected internal virtual string Syntax =>
                // WinReg Service
                "338cd001-2244-31f1-aaaa-900038001003:1.0";

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void closeConnection() throws org.jinterop.dcom.common.JIException
        public virtual void closeConnection()
		{
			try
			{
				base.Detach();
			}
			catch (IOException e)
			{
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
		}

	}

}
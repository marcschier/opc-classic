using System;
using System.Text;

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


namespace org.jinterop.winreg.smb {



	using SmbException = jcifs.smb.SmbException;

	using IJIAuthInfo = org.jinterop.dcom.common.IJIAuthInfo;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIException = org.jinterop.dcom.common.JIException;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using Endpoint = rpc.Endpoint;
	using Stub = rpc.Stub;

	/// <summary>
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	public class JIWinRegStub : Stub, IJIWinReg {


		//"ncacn_np:" + servername + "[\\PIPE\\winreg]"
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIWinRegStub(org.jinterop.dcom.common.IJIAuthInfo authInfo, String serverName) throws java.net.UnknownHostException
		public JIWinRegStub(IJIAuthInfo authInfo, string serverName) : base() {
			if (authInfo == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
			}

			base.TransportFactory = new rpc.ncacn_np.TransportFactory();
			base.Properties = new Properties();
			base.Properties.setProperty("rpc.ncacn_np.username", authInfo.UserName);
			string password = null;
			try {
				password = URLEncoder.encode(authInfo.Password,"utf-8");
			}
			catch (UnsupportedEncodingException) {
				try {
					password = URLEncoder.encode(authInfo.Password,System.getProperty("file.encoding"));
				}
				catch (UnsupportedEncodingException) {
					throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION2);
				}
			}
			//some strange issue with the space character, it gets encoded to '+' (which is right) , but Windows refuses it.
			//Manually changing + to %20
			StringBuilder password_ = new StringBuilder();
			for (int i = 0 ; i < password.Length; i++) {
				char ch = password[i];
				if (ch == '+') {
					password_.Append("%20");
					continue;
				}

				password_.Append(ch);
			}

			base.Properties.setProperty("rpc.ncacn_np.password", password_.ToString());
			base.Properties.setProperty("rpc.ncacn_np.domain", authInfo.Domain);
			serverName = serverName.Trim();
			serverName = InetAddress.getByName(serverName).HostAddress;
			base.Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIWinRegStub(String serverName) throws java.net.UnknownHostException
		public JIWinRegStub(string serverName) : base() {
			base.TransportFactory = new rpc.ncacn_np.TransportFactory();
			base.Properties = new Properties();
			base.Properties.setProperty("rpc.ntlm.sso", "true");
			serverName = serverName.Trim();
			serverName = InetAddress.getByName(serverName).HostAddress;
			base.Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKLM() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_OpenHKLM() {
			org.jinterop.winreg.IJIWinReg_openHKLM openhklm = new org.jinterop.winreg.IJIWinReg_openHKLM();
			JIPolicyHandle handle = new JIPolicyHandle(false);
			try {
				call(Endpoint.IDEMPOTENT,openhklm);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			Array.Copy(openhklm.Policyhandle,0,handle.Handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKCR() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_OpenHKCR() {
			org.jinterop.winreg.IJIWinReg_openHKCR openhkcr = new org.jinterop.winreg.IJIWinReg_openHKCR();
			JIPolicyHandle handle = new JIPolicyHandle(false);
			try {
				call(Endpoint.IDEMPOTENT,openhkcr);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			Array.Copy(openhkcr.Policyhandle,0,handle.Handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKCU() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_OpenHKCU() {
			org.jinterop.winreg.IJIWinReg_openHKCU openhkcu = new org.jinterop.winreg.IJIWinReg_openHKCU();
			JIPolicyHandle handle = new JIPolicyHandle(false);
			try {
				call(Endpoint.IDEMPOTENT,openhkcu);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			Array.Copy(openhkcu.Policyhandle,0,handle.Handle,0,20);

			return handle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenHKU() throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_OpenHKU() {
			org.jinterop.winreg.IJIWinReg_openHKU openhku = new org.jinterop.winreg.IJIWinReg_openHKU();
			JIPolicyHandle handle = new JIPolicyHandle(false);
			try {
				call(Endpoint.IDEMPOTENT,openhku);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			Array.Copy(openhku.Policyhandle,0,handle.Handle,0,20);

			return handle;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_OpenKey(org.jinterop.winreg.JIPolicyHandle handle,String key, int accessMask) throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_OpenKey(JIPolicyHandle handle, string key, int accessMask) {
			org.jinterop.winreg.IJIWinReg_openKey openkey = new org.jinterop.winreg.IJIWinReg_openKey();
			openkey.AccessMask = accessMask;
			openkey.Key = key;
			openkey.ParentKey = handle;
			JIPolicyHandle newHandle = new JIPolicyHandle(false);
			try {
				call(Endpoint.IDEMPOTENT,openkey);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			Array.Copy(openkey.Policyhandle,0,newHandle.Handle,0,20);

			return newHandle;
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_CloseKey(org.jinterop.winreg.JIPolicyHandle handle) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_CloseKey(JIPolicyHandle handle) {
			org.jinterop.winreg.IJIWinReg_closeKey closekey = new org.jinterop.winreg.IJIWinReg_closeKey();
			closekey.Key = handle;
			try {
				call(Endpoint.IDEMPOTENT,closekey);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_DeleteKeyOrValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, boolean isKey) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_DeleteKeyOrValue(JIPolicyHandle handle, string valueName, bool isKey) {
			org.jinterop.winreg.IJIWinReg_deleteValueOrKey delete = new org.jinterop.winreg.IJIWinReg_deleteValueOrKey();
			delete.ParentKey = handle;
			delete.ValueName = valueName;
			delete.IsKey = isKey;
			try {
				call(Endpoint.IDEMPOTENT,delete);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] winreg_QueryValue(org.jinterop.winreg.JIPolicyHandle handle,int bufferSize) throws org.jinterop.dcom.common.JIException
		public virtual sbyte[] Winreg_QueryValue(JIPolicyHandle handle, int bufferSize) {
			org.jinterop.winreg.IJIWinReg_queryValue queryvalue = new org.jinterop.winreg.IJIWinReg_queryValue();
			queryvalue.ParentKey = handle;
			queryvalue.BufferLength = bufferSize;
			try {
				call(Endpoint.IDEMPOTENT,queryvalue);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			//return queryvalue.key;
			return queryvalue.Buffer;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_QueryValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName,int bufferSize) throws org.jinterop.dcom.common.JIException
		public virtual object[] Winreg_QueryValue(JIPolicyHandle handle, string valueName, int bufferSize) {
			org.jinterop.winreg.IJIWinReg_queryValue queryvalue = new org.jinterop.winreg.IJIWinReg_queryValue();
			queryvalue.ParentKey = handle;
			queryvalue.BufferLength = bufferSize;
			queryvalue.Key = valueName;

			try {
				call(Endpoint.IDEMPOTENT,queryvalue);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			return new object[]{ new int?(queryvalue.Type),(queryvalue.Buffer != null ? (object)queryvalue.Buffer : (object)queryvalue.Buffer2) };
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SaveFile(org.jinterop.winreg.JIPolicyHandle handle, String fileName) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_SaveFile(JIPolicyHandle handle, string fileName) {
			org.jinterop.winreg.IJIWinReg_saveFile savefile = new org.jinterop.winreg.IJIWinReg_saveFile();
			savefile.ParentKey = handle;
			savefile.FileName = fileName;

			try {
				call(Endpoint.IDEMPOTENT,savefile);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.winreg.JIPolicyHandle winreg_CreateKey(org.jinterop.winreg.JIPolicyHandle handle, String subKey, int options,int accessMask) throws org.jinterop.dcom.common.JIException
		public virtual JIPolicyHandle Winreg_CreateKey(JIPolicyHandle handle, string subKey, int options, int accessMask) {
			org.jinterop.winreg.IJIWinReg_createKey createkey = new org.jinterop.winreg.IJIWinReg_createKey();
			createkey.AccessMask = accessMask;
			createkey.Key = subKey;
			createkey.ParentKey = handle;
			createkey.Options = options;

			try {
				call(Endpoint.IDEMPOTENT,createkey);
			}
			catch (SmbException e) {
				throw new JIException(e.NtStatus,e);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			JIPolicyHandle newHandle = new JIPolicyHandle(createkey.Actiontaken == 1 ? true : false);
			Array.Copy(createkey.Policyhandle,0,newHandle.Handle,0,20);

			return newHandle;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName,byte[][] data) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[][] data) {
			if (data == null) {
				throw new System.ArgumentException(JISystem.GetLocalizedMessage(JIErrorCodes.JI_WINREG_EXCEPTION5));
			}

			//calculate length of all strings + extra null in the end
			int totalStrings = data.Length;
			int length = 0;
			for (int i = 0;i < totalStrings;i++) {
				int j = data[i].Length;
				length = length + (j + 1) * 2; //including null termination
			}

			length = length + 2; //final termination

			org.jinterop.winreg.IJIWinReg_setValue setvalue = new org.jinterop.winreg.IJIWinReg_setValue();
			setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_MULTI_SZ;
			setvalue.Data2 = data;
			setvalue.LengthInBytes = length;
			setvalue.ParentKey = handle;
			setvalue.ValueName = valueName;
			Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_SetValue(JIPolicyHandle handle, string valueName) {
			org.jinterop.winreg.IJIWinReg_setValue setvalue = new org.jinterop.winreg.IJIWinReg_setValue();
			setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_NONE;
			setvalue.ParentKey = handle;
			setvalue.ValueName = valueName;
			Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, byte[] data, boolean isBinary, boolean expand_sz) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[] data, bool isBinary, bool expand_sz) {
			org.jinterop.winreg.IJIWinReg_setValue setvalue = new org.jinterop.winreg.IJIWinReg_setValue();
			if (isBinary) {
				setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_BINARY;
			}
			else {
				if (expand_sz) {
					setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_EXPAND_SZ;
				}
				else {
					setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_SZ;
				}
			}

			setvalue.Data = data;
			setvalue.LengthInBytes = data.Length;
			setvalue.ParentKey = handle;
			setvalue.ValueName = valueName;
			Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(org.jinterop.winreg.JIPolicyHandle handle,String valueName, int data) throws org.jinterop.dcom.common.JIException
		public virtual void Winreg_SetValue(JIPolicyHandle handle, string valueName, int data) {
			org.jinterop.winreg.IJIWinReg_setValue setvalue = new org.jinterop.winreg.IJIWinReg_setValue();
			setvalue.ClazzType = org.jinterop.winreg.IJIWinReg_Fields.REG_DWORD;
			setvalue.LengthInBytes = 4;
			setvalue.Dword = data;
			setvalue.ParentKey = handle;
			setvalue.ValueName = valueName;
			Value = setvalue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] winreg_EnumKey(org.jinterop.winreg.JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException
		public virtual string[] Winreg_EnumKey(JIPolicyHandle handle, int index) {
			org.jinterop.winreg.IJIWinReg_enumKey enumkey = new org.jinterop.winreg.IJIWinReg_enumKey();
			enumkey.ParentKey = handle;
			enumkey.Index = index;

			try {
				call(Endpoint.IDEMPOTENT,enumkey);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			return enumkey.Retval;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_EnumValue(org.jinterop.winreg.JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException
		public virtual object[] Winreg_EnumValue(JIPolicyHandle handle, int index) {
			org.jinterop.winreg.IJIWinReg_enumValue enumvalue = new org.jinterop.winreg.IJIWinReg_enumValue();
			enumvalue.ParentKey = handle;
			enumvalue.Index = index;

			try {
				call(Endpoint.IDEMPOTENT,enumvalue);
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
			catch (JIRuntimeException e) {
				throw new JIException(e);
			}

			return enumvalue.Retval;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setValue(org.jinterop.winreg.IJIWinReg_setValue setvalue) throws org.jinterop.dcom.common.JIException
		private org.jinterop.winreg.IJIWinReg_setValue Value {
			set {
				try {
					call(Endpoint.IDEMPOTENT,value);
				}
				catch (IOException e) {
					throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
				}
				catch (JIRuntimeException e) {
					throw new JIException(e);
				}
			}
		}

		public virtual string Syntax {
			get {
				// WinReg Service
				return "338cd001-2244-31f1-aaaa-900038001003:1.0";
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closeConnection() throws org.jinterop.dcom.common.JIException
		public virtual void CloseConnection() {
			try {
				base.detach();
			}
			catch (IOException e) {
				throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED,e);
			}
		}

	}

}
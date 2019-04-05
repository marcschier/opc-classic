// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.winreg {



    using Encdec = SharpCifs.util.Encdec;
    using NdrOp = SharpCifs.Dcerpc.Ndr.NdrOp;
    using NdrCodec = SharpCifs.Dcerpc.Ndr.NdrCodec;

    using JIErrorCodes = dcom.common.JIErrorCodes;
    using JIException = dcom.common.JIException;
    using JIRuntimeException = dcom.common.JIRuntimeException;

    /// <summary>
    /// Perform C-R-U-D on the Windows Registry.
    /// 
    /// <para>This interface uses "Windows Remote Registry" and "Server" services and these must be running on target workstation.
    /// 
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    public interface IJIWinReg
	{

		/// <summary>
		/// Type specifying String
		/// </summary>
		/// <summary>
		/// Type specifying Binary
		/// </summary>
		/// <summary>
		/// Type specifying DWORD
		/// </summary>
		/// <summary>
		/// Type specifying environment string
		/// </summary>
		/// <summary>
		/// Type specifying mutliple strings (array)
		/// </summary>
		/// <summary>
		/// Type specifying empty type
		/// </summary>


		/// <summary>
		/// Opens the HKEY_CLASSES_ROOT key
		/// </summary>
		/// <returns> handle representing the opened key </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKCR() throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_OpenHKCR();

		/// <summary>
		/// Opens the HKEY_CURRENT_USER key
		/// </summary>
		/// <returns> handle representing the opened key </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKCU() throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_OpenHKCU();

		/// <summary>
		/// Opens the HKEY_USERS key
		/// </summary>
		/// <returns> handle representing the opened key </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKU() throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_OpenHKU();

		/// <summary>
		/// Opens the HKEY_LOCAL_MACHINE key
		/// </summary>
		/// <returns> handle representing the opened key </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKLM() throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_OpenHKLM();

		/// <summary>
		/// Opens the subkey of key specified by handle.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="key"> </param>
		/// <param name="accessMask"> type of access required.
		/// </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenKey(JIPolicyHandle handle,String key,int accessMask) throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_OpenKey(JIPolicyHandle handle, string key, int accessMask);


		/// <summary>
		/// Closes the key.
		/// </summary>
		/// <param name="handle"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_CloseKey(JIPolicyHandle handle) throws org.jinterop.dcom.common.JIException;
		void winreg_CloseKey(JIPolicyHandle handle);

		/// <summary>
		/// Query the key for it's name. Please put buffer size more than the estimated expected value. In this case
		/// 1024 would do.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="bufferSize">
		/// </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] winreg_QueryValue(JIPolicyHandle handle,int bufferSize) throws org.jinterop.dcom.common.JIException;
		sbyte[] winreg_QueryValue(JIPolicyHandle handle, int bufferSize);

		/// <summary>
		/// Query the key-value for it's value.Please put buffer size more than the estimated expected value.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="bufferSize"> </param>
		/// <param name="valueName"> </param>
		/// <returns> first param contains the class type as an Integer, second param contains the value as a 1 dimensional byte array,if any. In case of REG_MULTI_SZ
		/// you will get a 2 dimensional byte array as the second param. </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_QueryValue(JIPolicyHandle handle,String valueName,int bufferSize) throws org.jinterop.dcom.common.JIException;
		object[] winreg_QueryValue(JIPolicyHandle handle, string valueName, int bufferSize);

		/// <summary>
		///Creates a new key by name subKey under the handle. If REG_OPTION_NON_VOLATILE option is used then the key is preserved
		/// in the registry when the machine shutsdown, otherwise it is stored only in memory.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="subKey"> </param>
		/// <param name="options"> </param>
		/// <param name="accessMask">
		/// </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_CreateKey(JIPolicyHandle handle, String subKey,int options,int accessMask) throws org.jinterop.dcom.common.JIException;
		JIPolicyHandle winreg_CreateKey(JIPolicyHandle handle, string subKey, int options, int accessMask);

		/// <summary>
		/// Sets name-value for a REG_MULTI_SZ type. data is a 2 dimensional array, each primary dimension representing
		/// one string. Please make sure that the encoding is correct while doing String.getBytes(...).
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="valueName"> </param>
		/// <param name="data"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName,byte[][] data) throws org.jinterop.dcom.common.JIException;
		void winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[][] data);

		/// <summary>
		///Sets an empty name-value for a REG_NONE type.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="valueName"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName) throws org.jinterop.dcom.common.JIException;
		void winreg_SetValue(JIPolicyHandle handle, string valueName);

		/// <summary>
		/// Sets name-value for a REG_SZ\REG_EXPAND_SZ\REG_BINARY type. The data will be considered as String if the binary flag is not set to true.
		/// In case of non binary data, please make sure that the encoding is correct while doing String.getBytes(...). Set expand_sz to true if the String
		/// contains environment variables. When both binary and expand_sz are set , binary will take precedence.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="valueName"> </param>
		/// <param name="data"> </param>
		/// <param name="binary"> </param>
		/// <param name="expand_sz"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName, byte[] data, bool binary,bool expand_sz) throws org.jinterop.dcom.common.JIException;
		void winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[] data, bool binary, bool expand_sz);

		/// <summary>
		///Sets name-value for a REG_DWORD type.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="valueName"> </param>
		/// <param name="data"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName, int data) throws org.jinterop.dcom.common.JIException;
		void winreg_SetValue(JIPolicyHandle handle, string valueName, int data);

		/// <summary>
		///Deletes a key or value specified by valueName.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="valueName"> </param>
		/// <param name="isKey"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_DeleteKeyOrValue(JIPolicyHandle handle,String valueName, bool isKey) throws org.jinterop.dcom.common.JIException;
		void winreg_DeleteKeyOrValue(JIPolicyHandle handle, string valueName, bool isKey);

		/// <summary>
		/// Saves registry entries from handle location to local fileName. This path is local to the target machine.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="fileName"> </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SaveFile(JIPolicyHandle handle,String fileName) throws org.jinterop.dcom.common.JIException;
		void winreg_SaveFile(JIPolicyHandle handle, string fileName);

		/// <summary>
		/// Returns name and class (in that order) for the key identified by index under parent handle.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="index">
		/// </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] winreg_EnumKey(JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException;
		string[] winreg_EnumKey(JIPolicyHandle handle, int index);

		/// <summary>
		///Returns name and type (in that order) for the value identified by index under parent handle.
		/// </summary>
		/// <param name="handle"> </param>
		/// <param name="index"> </param>
		/// <returns> First is a String (valueName) and second param is an Integer (type) </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_EnumValue(JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException;
		object[] winreg_EnumValue(JIPolicyHandle handle, int index);

		/// <summary>
		/// Closes this connection, but a word of caution, it does not close any OPEN Key. Just releases the NP resources it is holding.
		/// </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closeConnection() throws org.jinterop.dcom.common.JIException;
		void closeConnection();
	}

	public static class IJIWinReg_Fields
	{
		public const int KEY_ALL_ACCESS = 0x000f003f;
		public const int KEY_CREATE_LINK = 0x00000020;
		public const int KEY_CREATE_SUB_KEY = 0x00000004;
		public const int KEY_ENUMERATE_SUB_KEYS = 0x00000008;
		public const int KEY_EXECUTE = 0x00020019;
		public const int KEY_NOTIFY = 0x00000010;
		public const int KEY_QUERY_VALUE = 0x00000001;
		public const int KEY_READ = 0x00020019;
		public const int KEY_SET_VALUE = 0x00000002;
		public const int KEY_WRITE = 0x00020006;
		public const int REG_SZ = 1;
		public const int REG_BINARY = 3;
		public const int REG_DWORD = 4;
		public const int REG_EXPAND_SZ = 2;
		public const int REG_MULTI_SZ = 7;
		public const int REG_NONE = 0;
		public const int REG_OPTION_NON_VOLATILE = 0;
		public const int REG_OPTION_VOLATILE = 1;
	}

	public class IJIWinReg_closeKey : NdrOp
	{
		public JIPolicyHandle key;
        public virtual int Opnum => 5;

        public virtual void write(NdrCodec ndr)
		{
			ndr.writeOctetArray(key.handle,0,20);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_openHKLM : NdrOp
	{
        public virtual int Opnum => 2;

        public virtual void write(NdrCodec ndr)
		{
			//it's a pointer

			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());

			//system name
			ndr.WriteUnsignedShort(40736);

			//length
			ndr.WriteUnsignedShort(1);

			ndr.WriteUnsignedLong(0x2000000);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_openHKCU : NdrOp
	{
        public virtual int Opnum => 1;

        public virtual void write(NdrCodec ndr)
		{
			//it's a pointer

			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());

			//system name
			ndr.WriteUnsignedShort(49736);

			//length
			ndr.WriteUnsignedShort(1);

			ndr.WriteUnsignedLong(0x2000000);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_openHKU : NdrOp
	{
        public virtual int Opnum => 4;

        public virtual void write(NdrCodec ndr)
		{
			//it's a pointer

			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());

			//system name
			ndr.WriteUnsignedShort(49736);

			//length
			ndr.WriteUnsignedShort(1);

			ndr.WriteUnsignedLong(0x2000000);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_openHKCR : NdrOp
	{
        public virtual int Opnum => 0;

        public virtual void write(NdrCodec ndr)
		{
			//it's a pointer

			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());

			//system name
			ndr.WriteUnsignedShort(49736);

			//length
			ndr.WriteUnsignedShort(1);

			ndr.WriteUnsignedLong(0x2000000);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_deleteValueOrKey : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string valueName;
		public bool isKey;
		public virtual int Opnum
		{
			get
			{
                if (isKey) {
                    return 7;
                }
                return 8;
            }
		}

		public virtual void write(NdrCodec ndr)
		{
			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((valueName.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((valueName.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(valueName.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(valueName.Length + 1);

			var i = 0;
			while (i < valueName.Length)
			{
				ndr.WriteUnsignedShort(valueName[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);
		}

		public virtual void read(NdrCodec ndr)
		{
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}


	}

	public class IJIWinReg_saveFile : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string fileName;
        public virtual int Opnum => 20;

        public virtual void write(NdrCodec ndr)
		{
			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((fileName.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((fileName.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(fileName.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(fileName.Length + 1);

			var i = 0;
			while (i < fileName.Length)
			{
				ndr.WriteUnsignedShort(fileName[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);
			//now align for int
			var index = (double)ndr.Buffer.Index;
			long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			ndr.WriteUnsignedLong(0);
		}

		public virtual void read(NdrCodec ndr)
		{
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}


	}

	public class IJIWinReg_createKey : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string key;
		public int accessMask = -1;
		public int options = -1;
		public int actiontaken = -1;
        public virtual int Opnum => 6;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(key.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(key.Length + 1);

			var i = 0;
			while (i < key.Length)
			{
				ndr.WriteUnsignedShort(key[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);

			//now align for int
			var index = (double)ndr.Buffer.Index;
			long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			//write the class
			var clazz = "REG_SZ";
			//clazz len , since it is uint16
			ndr.WriteUnsignedShort((clazz.Length + 1) * 2);
			//clazz size, since it is uint16
			ndr.WriteUnsignedShort((clazz.Length + 1) * 2);

			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(clazz.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(clazz.Length + 1);

			i = 0;
			while (i < clazz.Length)
			{
				ndr.WriteUnsignedShort(clazz[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);

			//now align for int
			index = (double)ndr.Buffer.Index;
			k = 0;
			k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			//options
			ndr.WriteUnsignedLong(options);

			ndr.WriteUnsignedLong(accessMask);

			//ptr to sec desc , null
			ndr.WriteUnsignedLong(0);
			//pointer to action taken
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			//pointer to action taken
			ndr.ReadUnsignedLong();
			actiontaken = ndr.ReadUnsignedLong();
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_setValue : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string valueName;
		public int clazzType = -1;
		public int lengthInBytes = -1;
		public sbyte[] data; //should be in the right encoding for Strings.
		public sbyte[][] data2; //reg_
		public int dword;
        public virtual int Opnum => 22;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((valueName.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((valueName.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(valueName.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(valueName.Length + 1);

			var i = 0;
			while (i < valueName.Length)
			{
				ndr.WriteUnsignedShort(valueName[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);

			//now align for int
			var index = (double)ndr.Buffer.Index;
			long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			//write the type.
			ndr.WriteUnsignedLong(clazzType);

			i = 0;
			if (lengthInBytes != 0)
			{
				switch (clazzType)
				{
					case IJIWinReg_Fields.REG_EXPAND_SZ: //for environment variable strings
					case IJIWinReg_Fields.REG_SZ: //for strings, strings are null terminated, length in bytes will NOT include the null termination
						//character
						//writing the max count
						ndr.WriteUnsignedLong((lengthInBytes + 1) * 2);

						while (i < data.Length)
						{
							ndr.WriteUnsignedShort(data[i]);
							i++;
						}

						//null termination
						ndr.WriteUnsignedShort(0);

						//now align for int
						index = (double)ndr.Buffer.Index;
						k = 0;
						k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
						ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

						ndr.WriteUnsignedLong((lengthInBytes + 1) * 2);

					break;
					case IJIWinReg_Fields.REG_DWORD:
						ndr.WriteUnsignedLong(lengthInBytes);
						ndr.WriteUnsignedLong(dword);
						ndr.WriteUnsignedLong(lengthInBytes);
					break;
					case IJIWinReg_Fields.REG_NONE:
						data = new sbyte[0];
						lengthInBytes = 0;
						goto case IJIWinReg_Fields.REG_BINARY;
					case IJIWinReg_Fields.REG_BINARY:
						ndr.WriteUnsignedLong(lengthInBytes);
						ndr.writeOctetArray(data,0,lengthInBytes);
						index = (double)ndr.Buffer.Index;
						k = 0;
						k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
						ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);
						ndr.WriteUnsignedLong(lengthInBytes);
					break;
					case IJIWinReg_Fields.REG_MULTI_SZ: //for strings, strings are null terminated, length in bytes will NOT include the null termination
						//character
						//writing the max count , this will be computed before hand
						ndr.WriteUnsignedLong(lengthInBytes);

						for (i = 0; i < data2.Length;i++)
						{
							for (var j = 0; j < data2[i].Length;j++)
							{
								ndr.WriteUnsignedShort(data2[i][j]);
							}
							//null termination for each string
							ndr.WriteUnsignedShort(0);
						}
						//null termination for the multi sz.
						ndr.WriteUnsignedShort(0);

						//now align for int
						index = (double)ndr.Buffer.Index;
						k = 0;
						k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
						ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

						ndr.WriteUnsignedLong(lengthInBytes);

					break;


					default:
						throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION4);
				}
			}
			else
			{
				//for data
				ndr.WriteUnsignedLong(0);
				//for length
				ndr.WriteUnsignedLong(0);
			}


		}

		public virtual void read(NdrCodec ndr)
		{
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}


	}

	public class IJIWinReg_enumKey : NdrOp
	{
		public JIPolicyHandle parentKey;
		public int index = -1;
		public string[] retval = new string[2];
        public virtual int Opnum => 9;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			ndr.WriteUnsignedLong(index);

			//buffer len , since it is uint16
			ndr.WriteUnsignedShort(0);
			//buffer size, since it is uint16
			ndr.WriteUnsignedShort(2048);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(1024);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(0);

			//pointer
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//buffer len , since it is uint16
			ndr.WriteUnsignedShort(0);
			//buffer size, since it is uint16
			ndr.WriteUnsignedShort(2048);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(1024);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(0);

			//pointer for time
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);
			ndr.WriteUnsignedLong(0);
		}

		public virtual void read(NdrCodec ndr)
		{
			//buffer len , since it is uint16
			ndr.ReadUnsignedShort();
			//buffer size, since it is uint16
			ndr.ReadUnsignedShort();

			//it's a pointer
			//referent
			ndr.ReadUnsignedLong();
			//max count
			ndr.ReadUnsignedLong();
			//offset
			ndr.ReadUnsignedLong();

			var actuallength = ndr.ReadUnsignedLong(); //actuallength
			var bytes = new sbyte[0];
			if (actuallength != 0)
			{
				bytes = new sbyte[actuallength - 1];
			}
			var i = 0;
			//last 2 bytes , null termination will be eaten outside the loop
			while (i < actuallength - 1)
			{
				var retVal = ndr.ReadUnsignedShort();
				bytes[i] = (sbyte)retVal;
				i++;
			}
			if (actuallength != 0)
			{
				ndr.ReadUnsignedShort();
			}

			retval[0] = StringHelperClass.NewString(bytes);

			long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
			ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

	//			it's a pointer
			//referent
			ndr.ReadUnsignedLong();

	//			buffer len , since it is uint16
			ndr.ReadUnsignedShort();
			//buffer size, since it is uint16
			ndr.ReadUnsignedShort();

			//it's a pointer
			//referent
			ndr.ReadUnsignedLong();
			//max count
			ndr.ReadUnsignedLong();
			//offset
			ndr.ReadUnsignedLong();

			actuallength = ndr.ReadUnsignedLong(); //actuallength
			bytes = new sbyte[0];
			if (actuallength != 0)
			{
				bytes = new sbyte[actuallength - 1];
			}
			i = 0;
			//last 2 bytes , null termination will be eaten outside the loop
			while (i < actuallength - 1)
			{
				var retVal = ndr.ReadUnsignedShort();
				bytes[i] = (sbyte)retVal;
				i++;
			}
			if (actuallength != 0)
			{
				ndr.ReadUnsignedShort();
			}

			retval[1] = StringHelperClass.NewString(bytes);

			l = 0;
			l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
			ndr.readOctetArray(new sbyte[(int)l],0,(int)l);
			//now to read the time
			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();

			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}


	}

	public class IJIWinReg_enumValue : NdrOp
	{
		public JIPolicyHandle parentKey;
		public int index = -1;
		public object[] retval = new object[2];
        public virtual int Opnum => 10;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			ndr.WriteUnsignedLong(index);

			//buffer len , since it is uint16
			ndr.WriteUnsignedShort(0);
			//buffer size, since it is uint16
			ndr.WriteUnsignedShort(2048);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(1024);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(0);

			//pointer
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);

			ndr.WriteUnsignedLong(0);

			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);

			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);



		}

		public virtual void read(NdrCodec ndr)
		{
			//buffer len , since it is uint16
			ndr.ReadUnsignedShort();
			//buffer size, since it is uint16
			ndr.ReadUnsignedShort();

			//it's a pointer
			//referent
			ndr.ReadUnsignedLong();
			//max count
			ndr.ReadUnsignedLong();
			//offset
			ndr.ReadUnsignedLong();

			var actuallength = ndr.ReadUnsignedLong(); //actuallength
			var bytes = new sbyte[0];
			if (actuallength != 0)
			{
				bytes = new sbyte[actuallength - 1];
			}
			var i = 0;
			//last 2 bytes , null termination will be eaten outside the loop
			while (i < actuallength - 1)
			{
				var retVal = ndr.ReadUnsignedShort();
				bytes[i] = (sbyte)retVal;
				i++;
			}
			if (actuallength != 0)
			{
				ndr.ReadUnsignedShort();
			}

			retval[0] = StringHelperClass.NewString(bytes);

			long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
			ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

	//			it's a pointer
			//referent
			ndr.ReadUnsignedLong();

			var type = ndr.ReadUnsignedLong();
			retval[1] = type;

			ndr.ReadUnsignedLong();

			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();

			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();

			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}


	}

	public class IJIWinReg_openKey : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string key;
		public int accessMask = IJIWinReg_Fields.KEY_READ;

        public virtual int Opnum => 15;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(key.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(key.Length + 1);

			var i = 0;
			while (i < key.Length)
			{
				ndr.WriteUnsignedShort(key[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);

			//now align for int
			var index = (double)ndr.Buffer.Index;
			long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			//reserved
			ndr.WriteUnsignedLong(0);

			ndr.WriteUnsignedLong(accessMask);
		}

		public virtual void read(NdrCodec ndr)
		{
			ndr.readOctetArray(policyhandle,0,20);
			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}
		}

		public sbyte[] policyhandle = new sbyte[20];
	}

	public class IJIWinReg_queryValue : NdrOp
	{
		public JIPolicyHandle parentKey;
		public string key = "";
		public int bufferLength = -1;
		public int type = -1;
		public sbyte[] buffer;
		public sbyte[][] buffer2 = new sbyte[2048][];
        public virtual int Opnum => 17;

        public virtual void write(NdrCodec ndr)
		{

			//write parent handle
			ndr.writeOctetArray(parentKey.handle,0,20);

			//key len , since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);
			//key size, since it is uint16
			ndr.WriteUnsignedShort((key.Length + 1) * 2);

			//it's a pointer
			//referent
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(key.Length + 1);
			//offset
			ndr.WriteUnsignedLong(0);
			//actual count
			ndr.WriteUnsignedLong(key.Length + 1);

			var i = 0;
			while (i < key.Length)
			{
				ndr.WriteUnsignedShort(key[i]);
				i++;
			}

			//null termination
			ndr.WriteUnsignedShort(0);

			//now align for int
			var index = (double)ndr.Buffer.Index;
			long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
			ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

			//pointer to type
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);

			//pointer to data
			ndr.WriteUnsignedLong(new object().GetHashCode());
			//max count
			ndr.WriteUnsignedLong(bufferLength);
			ndr.WriteUnsignedLong(0); //offset
			ndr.WriteUnsignedLong(0); //actual

			//pointer to size
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(bufferLength);

			//pointer to length
			ndr.WriteUnsignedLong(new object().GetHashCode());
			ndr.WriteUnsignedLong(0);
		}

		public virtual void read(NdrCodec ndr)
		{
			var i = 0;
			//pointer
			ndr.ReadUnsignedLong();
			type = ndr.ReadUnsignedLong(); //type
			var retval = new sbyte[bufferLength];
			//StringBuffer buffer = new StringBuffer();
			//pointer to data
			ndr.ReadUnsignedLong();
			var maxcount = ndr.ReadUnsignedLong(); //maxcount
			var offset = ndr.ReadUnsignedLong(); //offset
			switch (type)
			{
				case IJIWinReg_Fields.REG_EXPAND_SZ: //for environment variable strings
				case IJIWinReg_Fields.REG_SZ:

					var actuallength = (int)Math.Round((double)ndr.ReadUnsignedLong() / 2.0); //actuallength

					//last 2 bytes , null termination will be eaten outside the loop
					while (i < actuallength - 1)
					{
						var retVal = ndr.ReadUnsignedShort();
						//even though this is a unicode string , but will not have anything else
						//other than ascii charset, which is supported by all encodings.
						//buffer.append(new String(new byte[]{(byte)retVal}));
						retval[i] = (sbyte)retVal;
						i++;
					}
					if (actuallength != 0)
					{
						ndr.ReadUnsignedShort();
					}

				break;
				case IJIWinReg_Fields.REG_DWORD:
					i = ndr.ReadUnsignedLong();
					var value = ndr.ReadUnsignedLong();
					Encdec.enc_uint32le(value, retval, 0);
				break;
				case IJIWinReg_Fields.REG_NONE:
				case IJIWinReg_Fields.REG_BINARY:
					i = ndr.ReadUnsignedLong();
					ndr.readOctetArray(retval,0,i);
				break;
				case IJIWinReg_Fields.REG_MULTI_SZ:

					actuallength = (int)Math.Round((double)ndr.ReadUnsignedLong() / 2.0); //actuallength
					int kk = 0, ll = 0;
					i = 0;
					//last 2 bytes , null termination will be eaten outside the loop
					while (i < actuallength - 1)
					{
						var retVal = ndr.ReadUnsignedShort();
						if (retVal == 0)
						{
							//reached end of one string
							buffer2[kk] = new sbyte[ll];
							Array.Copy(retval,0,buffer2[kk],0,ll);
							kk++;
							ll = -1; //it will become 0 next
							retval = new sbyte[bufferLength];
						}
						else
						{
							retval[ll] = (sbyte)retVal;
						}
						i++;
						ll++;
					}
					if (actuallength != 0)
					{
						ndr.ReadUnsignedShort();
					}

					break;
				default:
					throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION4);


			}

			long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
			ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

			//pointer to size
			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();

			//pointer to length
			ndr.ReadUnsignedLong();
			ndr.ReadUnsignedLong();

			var hresult = ndr.ReadUnsignedLong();
			if (hresult != 0)
			{
				throw new JIRuntimeException(hresult);
			}

            if (type != IJIWinReg_Fields.REG_MULTI_SZ) {
                buffer = new sbyte[i];
                Array.Copy(retval, 0, buffer, 0, i);
            }
            //key = buffer.toString();
        }

		public sbyte[] policyhandle = new sbyte[20];
	}

}
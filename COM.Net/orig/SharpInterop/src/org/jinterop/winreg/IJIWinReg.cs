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

namespace org.jinterop.winreg {



    using Encdec = jcifs.util.Encdec;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
    using JIException = org.jinterop.dcom.common.JIException;
    using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;

    /// <summary>
    /// Perform C-R-U-D on the Windows Registry.
    /// 
    /// <para>This interface uses "Windows Remote Registry" and "Server" services and these must be running on target workstation.
    /// 
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    public interface IJIWinReg {

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
        JIPolicyHandle Winreg_OpenHKCR();

        /// <summary>
        /// Opens the HKEY_CURRENT_USER key
        /// </summary>
        /// <returns> handle representing the opened key </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKCU() throws org.jinterop.dcom.common.JIException;
        JIPolicyHandle Winreg_OpenHKCU();

        /// <summary>
        /// Opens the HKEY_USERS key
        /// </summary>
        /// <returns> handle representing the opened key </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKU() throws org.jinterop.dcom.common.JIException;
        JIPolicyHandle Winreg_OpenHKU();

        /// <summary>
        /// Opens the HKEY_LOCAL_MACHINE key
        /// </summary>
        /// <returns> handle representing the opened key </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenHKLM() throws org.jinterop.dcom.common.JIException;
        JIPolicyHandle Winreg_OpenHKLM();

        /// <summary>
        /// Opens the subkey of key specified by handle.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="key"> </param>
        /// <param name="accessMask"> type of access required.
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_OpenKey(JIPolicyHandle handle,String key,int accessMask) throws org.jinterop.dcom.common.JIException;
        JIPolicyHandle Winreg_OpenKey(JIPolicyHandle handle, string key, int accessMask);


        /// <summary>
        /// Closes the key.
        /// </summary>
        /// <param name="handle"> </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_CloseKey(JIPolicyHandle handle) throws org.jinterop.dcom.common.JIException;
        void Winreg_CloseKey(JIPolicyHandle handle);

        /// <summary>
        /// Query the key for it's name. Please put buffer size more than the estimated expected value. In this case
        /// 1024 would do.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="bufferSize">
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] winreg_QueryValue(JIPolicyHandle handle,int bufferSize) throws org.jinterop.dcom.common.JIException;
        sbyte[] Winreg_QueryValue(JIPolicyHandle handle, int bufferSize);

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
        object[] Winreg_QueryValue(JIPolicyHandle handle, string valueName, int bufferSize);

        /// <summary>
        ///Creates a new key by name subKey under the handle. If REG_OPTION_NON_VOLATILE option is used then the key is preserved
        /// in the registry when the machine shutsdown, otherwise it is stored only in memory.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="subKey"> </param>
        /// <param name="options"> </param>
        /// <param name="accessMask">
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIPolicyHandle winreg_CreateKey(JIPolicyHandle handle, String subKey,int options,int accessMask) throws org.jinterop.dcom.common.JIException;
        JIPolicyHandle Winreg_CreateKey(JIPolicyHandle handle, string subKey, int options, int accessMask);

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
        void Winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[][] data);

        /// <summary>
        ///Sets an empty name-value for a REG_NONE type.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="valueName"> </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName) throws org.jinterop.dcom.common.JIException;
        void Winreg_SetValue(JIPolicyHandle handle, string valueName);

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
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName, byte[] data, boolean binary,boolean expand_sz) throws org.jinterop.dcom.common.JIException;
        void Winreg_SetValue(JIPolicyHandle handle, string valueName, sbyte[] data, bool binary, bool expand_sz);

        /// <summary>
        ///Sets name-value for a REG_DWORD type.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="valueName"> </param>
        /// <param name="data"> </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SetValue(JIPolicyHandle handle,String valueName, int data) throws org.jinterop.dcom.common.JIException;
        void Winreg_SetValue(JIPolicyHandle handle, string valueName, int data);

        /// <summary>
        ///Deletes a key or value specified by valueName.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="valueName"> </param>
        /// <param name="isKey"> </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_DeleteKeyOrValue(JIPolicyHandle handle,String valueName, boolean isKey) throws org.jinterop.dcom.common.JIException;
        void Winreg_DeleteKeyOrValue(JIPolicyHandle handle, string valueName, bool isKey);

        /// <summary>
        /// Saves registry entries from handle location to local fileName. This path is local to the target machine.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="fileName"> </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void winreg_SaveFile(JIPolicyHandle handle,String fileName) throws org.jinterop.dcom.common.JIException;
        void Winreg_SaveFile(JIPolicyHandle handle, string fileName);

        /// <summary>
        /// Returns name and class (in that order) for the key identified by index under parent handle.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="index">
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] winreg_EnumKey(JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException;
        string[] Winreg_EnumKey(JIPolicyHandle handle, int index);

        /// <summary>
        ///Returns name and type (in that order) for the value identified by index under parent handle.
        /// </summary>
        /// <param name="handle"> </param>
        /// <param name="index"> </param>
        /// <returns> First is a String (valueName) and second param is an Integer (type) </returns>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] winreg_EnumValue(JIPolicyHandle handle,int index) throws org.jinterop.dcom.common.JIException;
        object[] Winreg_EnumValue(JIPolicyHandle handle, int index);

        /// <summary>
        /// Closes this connection, but a word of caution, it does not close any OPEN Key. Just releases the NP resources it is holding.
        /// </summary>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closeConnection() throws org.jinterop.dcom.common.JIException;
        void CloseConnection();
    }

    public static class IJIWinReg_Fields {
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

    public class IJIWinReg_closeKey : NdrObject {
        public JIPolicyHandle Key = null;
        public virtual int Opnum {
            get {
                return 5;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            ndr.writeOctetArray(Key.Handle,0,20);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_openHKLM : NdrObject {
        public virtual int Opnum {
            get {
                return 2;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //it's a pointer

            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());

            //system name
            ndr.writeUnsignedShort(40736);

            //length
            ndr.writeUnsignedShort(1);

            ndr.writeUnsignedLong(0x2000000);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_openHKCU : NdrObject {
        public virtual int Opnum {
            get {
                return 1;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //it's a pointer

            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());

            //system name
            ndr.writeUnsignedShort(49736);

            //length
            ndr.writeUnsignedShort(1);

            ndr.writeUnsignedLong(0x2000000);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_openHKU : NdrObject {
        public virtual int Opnum {
            get {
                return 4;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //it's a pointer

            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());

            //system name
            ndr.writeUnsignedShort(49736);

            //length
            ndr.writeUnsignedShort(1);

            ndr.writeUnsignedLong(0x2000000);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_openHKCR : NdrObject {
        public virtual int Opnum {
            get {
                return 0;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //it's a pointer

            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());

            //system name
            ndr.writeUnsignedShort(49736);

            //length
            ndr.writeUnsignedShort(1);

            ndr.writeUnsignedLong(0x2000000);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_deleteValueOrKey : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string ValueName = null;
        public bool IsKey = false;
        public virtual int Opnum {
            get {
                if (IsKey) {
                    return 7;
                }
                else {
                    return 8;
                }
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((ValueName.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((ValueName.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(ValueName.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(ValueName.Length + 1);

            int i = 0;
            while (i < ValueName.Length) {
                ndr.writeUnsignedShort(ValueName[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }


    }

    public class IJIWinReg_saveFile : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string FileName = null;
        public virtual int Opnum {
            get {
                return 20;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {
            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((FileName.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((FileName.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(FileName.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(FileName.Length + 1);

            int i = 0;
            while (i < FileName.Length) {
                ndr.writeUnsignedShort(FileName[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);
            //now align for int
            double index = (double)(new int?(ndr.Buffer.Index));
            long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            ndr.writeUnsignedLong(0);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }


    }

    public class IJIWinReg_createKey : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string Key = null;
        public int AccessMask = -1;
        public int Options = -1;
        public int Actiontaken = -1;
        public virtual int Opnum {
            get {
                return 6;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(Key.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(Key.Length + 1);

            int i = 0;
            while (i < Key.Length) {
                ndr.writeUnsignedShort(Key[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);

            //now align for int
            double index = (double)(new int?(ndr.Buffer.Index));
            long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            //write the class
            string clazz = "REG_SZ";
            //clazz len , since it is uint16
            ndr.writeUnsignedShort((clazz.Length + 1) * 2);
            //clazz size, since it is uint16
            ndr.writeUnsignedShort((clazz.Length + 1) * 2);

            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(clazz.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(clazz.Length + 1);

            i = 0;
            while (i < clazz.Length) {
                ndr.writeUnsignedShort(clazz[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);

            //now align for int
            index = (double)(new int?(ndr.Buffer.Index));
            k = 0;
            k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            //options
            ndr.writeUnsignedLong(Options);

            ndr.writeUnsignedLong(AccessMask);

            //ptr to sec desc , null
            ndr.writeUnsignedLong(0);
            //pointer to action taken
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            //pointer to action taken
            ndr.readUnsignedLong();
            Actiontaken = ndr.readUnsignedLong();
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_setValue : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string ValueName = null;
        public int ClazzType = -1;
        public int LengthInBytes = -1;
        public sbyte[] Data = null; //should be in the right encoding for Strings.
        public sbyte[][] Data2 = null; //reg_
        public int Dword;
        public virtual int Opnum {
            get {
                return 22;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((ValueName.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((ValueName.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(ValueName.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(ValueName.Length + 1);

            int i = 0;
            while (i < ValueName.Length) {
                ndr.writeUnsignedShort(ValueName[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);

            //now align for int
            double index = (double)(new int?(ndr.Buffer.Index));
            long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            //write the type.
            ndr.writeUnsignedLong(ClazzType);

            i = 0;
            if (LengthInBytes != 0) {
                switch (ClazzType) {
                    case IJIWinReg_Fields.REG_EXPAND_SZ: //for environment variable strings
                    case IJIWinReg_Fields.REG_SZ: //for strings, strings are null terminated, length in bytes will NOT include the null termination
                        //character
                        //writing the max count
                        ndr.writeUnsignedLong((LengthInBytes + 1) * 2);

                        while (i < Data.Length) {
                            ndr.writeUnsignedShort(Data[i]);
                            i++;
                        }

                        //null termination
                        ndr.writeUnsignedShort(0);

                        //now align for int
                        index = (double)(new int?(ndr.Buffer.Index));
                        k = 0;
                        k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
                        ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

                        ndr.writeUnsignedLong((LengthInBytes + 1) * 2);

                    break;
                    case IJIWinReg_Fields.REG_DWORD:
                        ndr.writeUnsignedLong(LengthInBytes);
                        ndr.writeUnsignedLong(Dword);
                        ndr.writeUnsignedLong(LengthInBytes);
                    break;
                    case IJIWinReg_Fields.REG_NONE:
                        Data = new sbyte[0];
                        LengthInBytes = 0;
                        goto case IJIWinReg_Fields.REG_BINARY;
                    case IJIWinReg_Fields.REG_BINARY:
                        ndr.writeUnsignedLong(LengthInBytes);
                        ndr.writeOctetArray(Data,0,LengthInBytes);
                        index = (double)(new int?(ndr.Buffer.Index));
                        k = 0;
                        k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
                        ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);
                        ndr.writeUnsignedLong(LengthInBytes);
                    break;
                    case IJIWinReg_Fields.REG_MULTI_SZ: //for strings, strings are null terminated, length in bytes will NOT include the null termination
                        //character
                        //writing the max count , this will be computed before hand
                        ndr.writeUnsignedLong(LengthInBytes);

                        for (i = 0; i < Data2.Length;i++) {
                            for (int j = 0; j < Data2[i].Length;j++) {
                                ndr.writeUnsignedShort(Data2[i][j]);
                            }
                            //null termination for each string
                            ndr.writeUnsignedShort(0);
                        }
                        //null termination for the multi sz.
                        ndr.writeUnsignedShort(0);

                        //now align for int
                        index = (double)(new int?(ndr.Buffer.Index));
                        k = 0;
                        k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
                        ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

                        ndr.writeUnsignedLong(LengthInBytes);

                    break;


                    default:
                        throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION4);
                }
            }
            else {
                //for data
                ndr.writeUnsignedLong(0);
                //for length
                ndr.writeUnsignedLong(0);
            }


        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }


    }

    public class IJIWinReg_enumKey : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public int Index = -1;
        public string[] Retval = new string[2];
        public virtual int Opnum {
            get {
                return 9;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            ndr.writeUnsignedLong(Index);

            //buffer len , since it is uint16
            ndr.writeUnsignedShort(0);
            //buffer size, since it is uint16
            ndr.writeUnsignedShort(2048);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(1024);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(0);

            //pointer
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //buffer len , since it is uint16
            ndr.writeUnsignedShort(0);
            //buffer size, since it is uint16
            ndr.writeUnsignedShort(2048);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(1024);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(0);

            //pointer for time
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);
            ndr.writeUnsignedLong(0);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            //buffer len , since it is uint16
            ndr.readUnsignedShort();
            //buffer size, since it is uint16
            ndr.readUnsignedShort();

            //it's a pointer
            //referent
            ndr.readUnsignedLong();
            //max count
            ndr.readUnsignedLong();
            //offset
            ndr.readUnsignedLong();

            int actuallength = ndr.readUnsignedLong(); //actuallength
            sbyte[] bytes = new sbyte[0];
            if (actuallength != 0) {
                bytes = new sbyte[actuallength - 1];
            }
            int i = 0;
            //last 2 bytes , null termination will be eaten outside the loop
            while (i < actuallength - 1) {
                int retVal = ndr.readUnsignedShort();
                bytes[i] = (sbyte)retVal;
                i++;
            }
            if (actuallength != 0) {
                ndr.readUnsignedShort();
            }

            Retval[0] = StringHelperClass.NewString(bytes);

            long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
            ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

    //            it's a pointer
            //referent
            ndr.readUnsignedLong();

    //            buffer len , since it is uint16
            ndr.readUnsignedShort();
            //buffer size, since it is uint16
            ndr.readUnsignedShort();

            //it's a pointer
            //referent
            ndr.readUnsignedLong();
            //max count
            ndr.readUnsignedLong();
            //offset
            ndr.readUnsignedLong();

            actuallength = ndr.readUnsignedLong(); //actuallength
            bytes = new sbyte[0];
            if (actuallength != 0) {
                bytes = new sbyte[actuallength - 1];
            }
            i = 0;
            //last 2 bytes , null termination will be eaten outside the loop
            while (i < actuallength - 1) {
                int retVal = ndr.readUnsignedShort();
                bytes[i] = (sbyte)retVal;
                i++;
            }
            if (actuallength != 0) {
                ndr.readUnsignedShort();
            }

            Retval[1] = StringHelperClass.NewString(bytes);

            l = 0;
            l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
            ndr.readOctetArray(new sbyte[(int)l],0,(int)l);
            //now to read the time
            ndr.readUnsignedLong();
            ndr.readUnsignedLong();
            ndr.readUnsignedLong();

            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }


    }

    public class IJIWinReg_enumValue : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public int Index = -1;
        public object[] Retval = new object[2];
        public virtual int Opnum {
            get {
                return 10;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            ndr.writeUnsignedLong(Index);

            //buffer len , since it is uint16
            ndr.writeUnsignedShort(0);
            //buffer size, since it is uint16
            ndr.writeUnsignedShort(2048);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(1024);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(0);

            //pointer
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);

            ndr.writeUnsignedLong(0);

            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);

            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);



        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            //buffer len , since it is uint16
            ndr.readUnsignedShort();
            //buffer size, since it is uint16
            ndr.readUnsignedShort();

            //it's a pointer
            //referent
            ndr.readUnsignedLong();
            //max count
            ndr.readUnsignedLong();
            //offset
            ndr.readUnsignedLong();

            int actuallength = ndr.readUnsignedLong(); //actuallength
            sbyte[] bytes = new sbyte[0];
            if (actuallength != 0) {
                bytes = new sbyte[actuallength - 1];
            }
            int i = 0;
            //last 2 bytes , null termination will be eaten outside the loop
            while (i < actuallength - 1) {
                int retVal = ndr.readUnsignedShort();
                bytes[i] = (sbyte)retVal;
                i++;
            }
            if (actuallength != 0) {
                ndr.readUnsignedShort();
            }

            Retval[0] = StringHelperClass.NewString(bytes);

            long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
            ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

    //            it's a pointer
            //referent
            ndr.readUnsignedLong();

            int type = ndr.readUnsignedLong();
            Retval[1] = new int?(type);

            ndr.readUnsignedLong();

            ndr.readUnsignedLong();
            ndr.readUnsignedLong();

            ndr.readUnsignedLong();
            ndr.readUnsignedLong();

            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }


    }

    public class IJIWinReg_openKey : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string Key = null;
        public int AccessMask = IJIWinReg_Fields.KEY_READ;

        public virtual int Opnum {
            get {
                return 15;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(Key.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(Key.Length + 1);

            int i = 0;
            while (i < Key.Length) {
                ndr.writeUnsignedShort(Key[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);

            //now align for int
            double index = (double)(new int?(ndr.Buffer.Index));
            long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            //reserved
            ndr.writeUnsignedLong(0);

            ndr.writeUnsignedLong(AccessMask);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            ndr.readOctetArray(Policyhandle,0,20);
            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

    public class IJIWinReg_queryValue : NdrObject {
        public JIPolicyHandle ParentKey = null;
        public string Key = "";
        public int BufferLength = -1;
        public int Type = -1;
        public sbyte[] Buffer = null;
        public sbyte[][] Buffer2 = new sbyte[2048][];
        public virtual int Opnum {
            get {
                return 17;
            }
        }

        public virtual void Write(NetworkDataRepresentation ndr) {

            //write parent handle
            ndr.writeOctetArray(ParentKey.Handle,0,20);

            //key len , since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);
            //key size, since it is uint16
            ndr.writeUnsignedShort((Key.Length + 1) * 2);

            //it's a pointer
            //referent
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(Key.Length + 1);
            //offset
            ndr.writeUnsignedLong(0);
            //actual count
            ndr.writeUnsignedLong(Key.Length + 1);

            int i = 0;
            while (i < Key.Length) {
                ndr.writeUnsignedShort(Key[i]);
                i++;
            }

            //null termination
            ndr.writeUnsignedShort(0);

            //now align for int
            double index = (double)(new int?(ndr.Buffer.Index));
            long k = (k = Math.Round(index % 4.0)) == 0 ? 0 : 4 - k;
            ndr.writeOctetArray(new sbyte[(int)k],0,(int)k);

            //pointer to type
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);

            //pointer to data
            ndr.writeUnsignedLong((new object()).GetHashCode());
            //max count
            ndr.writeUnsignedLong(BufferLength);
            ndr.writeUnsignedLong(0); //offset
            ndr.writeUnsignedLong(0); //actual

            //pointer to size
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(BufferLength);

            //pointer to length
            ndr.writeUnsignedLong((new object()).GetHashCode());
            ndr.writeUnsignedLong(0);
        }

        public virtual void Read(NetworkDataRepresentation ndr) {
            int i = 0;
            //pointer
            ndr.readUnsignedLong();
            Type = ndr.readUnsignedLong(); //type
            sbyte[] retval = new sbyte[BufferLength];
            //StringBuffer buffer = new StringBuffer();
            //pointer to data
            ndr.readUnsignedLong();
            int maxcount = ndr.readUnsignedLong(); //maxcount
            int offset = ndr.readUnsignedLong(); //offset
            switch (Type) {
                case IJIWinReg_Fields.REG_EXPAND_SZ: //for environment variable strings
                case IJIWinReg_Fields.REG_SZ:

                    int actuallength = (int)Math.Round((double)(new int?(ndr.readUnsignedLong())) / 2.0); //actuallength

                    //last 2 bytes , null termination will be eaten outside the loop
                    while (i < actuallength - 1) {
                        int retVal = ndr.readUnsignedShort();
                        //even though this is a unicode string , but will not have anything else
                        //other than ascii charset, which is supported by all encodings.
                        //buffer.append(new String(new byte[]{(byte)retVal}));
                        retval[i] = (sbyte)retVal;
                        i++;
                    }
                    if (actuallength != 0) {
                        ndr.readUnsignedShort();
                    }

                break;
                case IJIWinReg_Fields.REG_DWORD:
                    i = ndr.readUnsignedLong();
                    int value = ndr.readUnsignedLong();
                    Encdec.enc_uint32le(value, retval, 0);
                break;
                case IJIWinReg_Fields.REG_NONE:
                case IJIWinReg_Fields.REG_BINARY:
                    i = ndr.readUnsignedLong();
                    ndr.readOctetArray(retval,0,i);
                break;
                case IJIWinReg_Fields.REG_MULTI_SZ:

                    actuallength = (int)Math.Round((double)(new int?(ndr.readUnsignedLong())) / 2.0); //actuallength
                    int kk = 0, ll = 0;
                    i = 0;
                    //last 2 bytes , null termination will be eaten outside the loop
                    while (i < actuallength - 1) {
                        int retVal = ndr.readUnsignedShort();
                        if (retVal == 0) {
                            //reached end of one string
                            Buffer2[kk] = new sbyte[ll];
                            Array.Copy(retval,0,Buffer2[kk],0,ll);
                            kk++;
                            ll = -1; //it will become 0 next
                            retval = new sbyte[BufferLength];
                        }
                        else {
                            retval[ll] = (sbyte)retVal;
                        }
                        i++;
                        ll++;
                    }
                    if (actuallength != 0) {
                        ndr.readUnsignedShort();
                    }

                    break;
                default:
                    throw new JIRuntimeException(JIErrorCodes.JI_WINREG_EXCEPTION4);


            }

            long l = (l = Math.Round(ndr.Buffer.Index % 4.0)) == 0 ? 0 : 4 - l;
            ndr.readOctetArray(new sbyte[(int)l],0,(int)l);

            //pointer to size
            ndr.readUnsignedLong();
            ndr.readUnsignedLong();

            //pointer to length
            ndr.readUnsignedLong();
            ndr.readUnsignedLong();

            int hresult = ndr.readUnsignedLong();
            if (hresult != 0) {
                throw new JIRuntimeException(hresult);
            }

            if (Type != IJIWinReg_Fields.REG_MULTI_SZ) {
                this.Buffer = new sbyte[i];
                Array.Copy(retval,0,this.Buffer,0,i);
            }
            else {
                //we have the data already in buffer2.
            }
            //key = buffer.toString();
        }

        public sbyte[] Policyhandle = new sbyte[20];
    }

}
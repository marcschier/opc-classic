//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.winreg.smb {
    using SharpCifs.Util.Sharpen;
    using SharpCifs.Smb;
    using rpc;
    using org.jinterop.dcom.common;
    using System.Text;
    using System.Net;
    using System.Linq;
    using System.IO;
    using System;
    using System.Web;

    /// <summary>
    /// Registry strub
    /// </summary>
    public class JIWinRegStub : Stub, IJIWinReg {

        /// <inheritdoc/>
        protected override string Syntax => // WinReg Service
                "338cd001-2244-31f1-aaaa-900038001003:1.0";

        /// <summary>
        /// Create stub
        /// "ncacn_np:" + servername + "[\\PIPE\\winreg]"
        /// </summary>
        /// <param name="authInfo"></param>
        /// <param name="serverName"></param>
        /// <exception cref="UnknownHostException"></exception>
        public JIWinRegStub(IJIAuthInfo authInfo, string serverName) {
            if (authInfo == null) {
                throw new ArgumentException(
                    JISystem.GetLocalizedMessage(JIErrorCodes.JI_AUTH_NOT_SUPPLIED));
            }
            TransportFactory = new rpc.ncacn_np.TransportFactory();
            Properties = new Properties();
            Properties.SetProperty("rpc.ncacn_np.username", authInfo.UserName);
            var password =  HttpUtility.UrlEncode(authInfo.Password, Encoding.UTF8);

            // TODO: Needed?
            // some strange issue with the space character, it gets encoded to '+'
            // (which is right), but Windows refuses it.
            // Manually changing + to %20
            var password_ = new StringBuilder();
            for (var i = 0; i < password.Length; i++) {
                var ch = password[i];
                if (ch == '+') {
                    password_.Append("%20");
                    continue;
                }
                password_.Append(ch);
            }

            Properties.SetProperty("rpc.ncacn_np.password", password_.ToString());
            Properties.SetProperty("rpc.ncacn_np.domain", authInfo.Domain);
            serverName = serverName.Trim();
            serverName = Dns.GetHostAddresses(serverName).First().ToString();
            Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";

        }

        /// <summary>
        /// Create stub
        /// "ncacn_np:" + servername + "[\\PIPE\\winreg]"
        /// </summary>
        /// <param name="serverName"></param>
        /// <exception cref="UnknownHostException"></exception>
        public JIWinRegStub(string serverName) {
            TransportFactory = new rpc.ncacn_np.TransportFactory();
            Properties = new Properties();
            Properties.SetProperty("rpc.ntlm.sso", "true");
            serverName = serverName.Trim();
            serverName = Dns.GetHostAddresses(serverName).First().ToString();
            Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";
        }

        /// <inheritdoc/>
        public JIPolicyHandle OpenHKLM() {
            var openhklm = new OpenHKLM();
            var handle = new JIPolicyHandle(false);
            try {
                Call(Semantics.IDEMPOTENT, openhklm);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            Array.Copy(openhklm.policyhandle, 0, handle.Handle, 0, 20);
            return handle;
        }

        /// <inheritdoc/>
        public JIPolicyHandle OpenHKCR() {
            var openhkcr = new OpenHKCR();
            var handle = new JIPolicyHandle(false);
            try {
                Call(Semantics.IDEMPOTENT, openhkcr);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            Array.Copy(openhkcr.policyhandle, 0, handle.Handle, 0, 20);

            return handle;
        }

        /// <inheritdoc/>
        public JIPolicyHandle OpenHKCU() {
            var openhkcu = new OpenHKCU();
            var handle = new JIPolicyHandle(false);
            try {
                Call(Semantics.IDEMPOTENT, openhkcu);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            Array.Copy(openhkcu.policyhandle, 0, handle.Handle, 0, 20);
            return handle;
        }

        /// <inheritdoc/>
        public JIPolicyHandle OpenHKU() {
            var openhku = new OpenHKU();
            var handle = new JIPolicyHandle(false);
            try {
                Call(Semantics.IDEMPOTENT, openhku);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.JI_WINREG_EXCEPTION, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            Array.Copy(openhku.policyhandle, 0, handle.Handle, 0, 20);

            return handle;
        }

        /// <inheritdoc/>
        public JIPolicyHandle OpenKey(JIPolicyHandle handle, string key,
            RegKeyAccess accessMask) {
            var openkey = new OpenKey {
                accessMask = accessMask,
                key = key,
                parentKey = handle
            };
            var newHandle = new JIPolicyHandle(false);
            try {
                Call(Semantics.IDEMPOTENT, openkey);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            Array.Copy(openkey.policyhandle, 0, newHandle.Handle, 0, 20);

            return newHandle;
        }

        /// <inheritdoc/>
        public void CloseKey(JIPolicyHandle handle) {
            var closekey = new CloseKey {
                key = handle
            };
            try {
                Call(Semantics.IDEMPOTENT, closekey);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }
        }

        /// <inheritdoc/>
        public void DeleteKeyOrValue(JIPolicyHandle handle, string valueName, bool isKey) {
            var delete = new DeleteValueOrKey {
                parentKey = handle,
                valueName = valueName,
                isKey = isKey
            };
            try {
                Call(Semantics.IDEMPOTENT, delete);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }
        }

        /// <inheritdoc/>
        public byte[] QueryValue(JIPolicyHandle handle, int bufferSize) {
            var queryvalue = new QueryValue {
                parentKey = handle,
                bufferLength = bufferSize
            };
            try {
                Call(Semantics.IDEMPOTENT, queryvalue);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            // return queryvalue.key;
            return queryvalue.buffer;
        }

        /// <inheritdoc/>
        public object[] QueryValue(JIPolicyHandle handle, string valueName, int bufferSize) {
            var queryvalue = new QueryValue {
                parentKey = handle,
                bufferLength = bufferSize,
                key = valueName
            };

            try {
                Call(Semantics.IDEMPOTENT, queryvalue);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            return new object[] { queryvalue.type, queryvalue.buffer ?? (object)queryvalue.buffer2 };
        }

        /// <inheritdoc/>
        public void SaveFile(JIPolicyHandle handle, string fileName) {
            var savefile = new SaveFile {
                parentKey = handle,
                fileName = fileName
            };

            try {
                Call(Semantics.IDEMPOTENT, savefile);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

        }

        /// <inheritdoc/>
        public JIPolicyHandle CreateKey(JIPolicyHandle handle, string subKey,
            RegOption options, RegKeyAccess accessMask) {
            var createkey = new CreateKey {
                accessMask = accessMask,
                key = subKey,
                parentKey = handle,
                options = options
            };

            try {
                Call(Semantics.IDEMPOTENT, createkey);
            }
            catch (SmbException e) {
                throw new JIException(e.GetNtStatus(), e);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            var newHandle = new JIPolicyHandle(createkey.actiontaken == 1 ? true : false);
            Array.Copy(createkey.policyhandle, 0, newHandle.Handle, 0, 20);

            return newHandle;
        }

        /// <inheritdoc/>
        public void SetValue(JIPolicyHandle handle, string valueName, byte[][] data) {
            if (data == null) {
                throw new ArgumentException(JISystem.GetLocalizedMessage(
                    JIErrorCodes.JI_WINREG_EXCEPTION5));
            }

            // calculate length of all strings + extra null in the end
            var totalStrings = data.Length;
            var length = 0;
            for (var i = 0; i < totalStrings; i++) {
                var j = data[i].Length;
                length += (j + 1) * 2; // including null termination
            }

            length += 2; // final termination

            var setvalue = new SetValue {
                clazzType = RegValueType.REG_MULTI_SZ,
                data2 = data,
                lengthInBytes = length,
                parentKey = handle,
                valueName = valueName
            };
            SetValue(setvalue);
        }

        /// <inheritdoc/>
        public void SetValue(JIPolicyHandle handle, string valueName) {
            var setvalue = new SetValue {
                clazzType = RegValueType.REG_NONE,
                parentKey = handle,
                valueName = valueName
            };
            SetValue(setvalue);
        }

        /// <inheritdoc/>
        public void SetValue(JIPolicyHandle handle, string valueName, byte[] data, bool isBinary, bool expand_sz) {
            var setvalue = new SetValue {
                data = data,
                lengthInBytes = data.Length,
                parentKey = handle,
                valueName = valueName
            };
            if (isBinary) {
                setvalue.clazzType = RegValueType.REG_BINARY;
            }
            else {
                if (expand_sz) {
                    setvalue.clazzType = RegValueType.REG_EXPAND_SZ;
                }
                else {
                    setvalue.clazzType = RegValueType.REG_SZ;
                }
            }
            SetValue(setvalue);
        }

        /// <inheritdoc/>
        public void SetValue(JIPolicyHandle handle, string valueName, int data) {
            var setvalue = new SetValue {
                clazzType = RegValueType.REG_DWORD,
                lengthInBytes = 4,
                dword = data,
                parentKey = handle,
                valueName = valueName
            };
            SetValue(setvalue);
        }

        /// <inheritdoc/>
        public string[] EnumKey(JIPolicyHandle handle, int index) {
            var enumkey = new EnumKey {
                parentKey = handle,
                index = index
            };

            try {
                Call(Semantics.IDEMPOTENT, enumkey);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            return enumkey.retval;
        }

        /// <inheritdoc/>
        public object[] EnumValue(JIPolicyHandle handle, int index) {
            var enumvalue = new EnumValue {
                parentKey = handle,
                index = index
            };
            try {
                Call(Semantics.IDEMPOTENT, enumvalue);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }

            return enumvalue.retval;
        }

        /// <inheritdoc/>
        public void CloseConnection() {
            try {
                Detach();
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
        }

        /// <summary>
        /// Set value
        /// </summary>
        private void SetValue(SetValue value) {
            try {
                Call(Semantics.IDEMPOTENT, value);
            }
            catch (IOException e) {
                throw new JIException(JIErrorCodes.RPC_E_UNEXPECTED, e);
            }
            catch (JIRuntimeException e) {
                throw new JIException(e);
            }
        }
    }
}
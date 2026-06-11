// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using SharpCifs.Smb;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Common;
using System.Text;
using System.Net;
using System.Linq;
using System.IO;
using System;
using System.Web;

namespace Opc.Classic.Dcom.Registry.Smb;

/// <summary>
/// Registry strub
/// </summary>
public class RegistryStub : Stub, IRegistry
{

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
    public RegistryStub(IAuthInfo authInfo, string serverName)
    {
        if (authInfo == null)
        {
            throw new ArgumentException(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_AUTH_NOT_SUPPLIED),
                nameof(authInfo));
        }
        TransportFactory = new Opc.Classic.Dcom.Rpc.Ncacn_Np.TransportFactory();
        Properties = new PropertyBag();
        Properties.SetProperty("rpc.ncacn_np.username", authInfo.UserName);
        var password = HttpUtility.UrlEncode(authInfo.Password, Encoding.UTF8);

        // TODO: Needed?
        // some strange issue with the space character, it gets encoded to '+'
        // (which is right), but Windows refuses it.
        // Manually changing + to %20
        var password_ = new StringBuilder();
        for (var i = 0; i < password.Length; i++)
        {
            var ch = password[i];
            if (ch == '+')
            {
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
    public RegistryStub(string serverName)
    {
        TransportFactory = new Opc.Classic.Dcom.Rpc.Ncacn_Np.TransportFactory();
        Properties = new PropertyBag();
        Properties.SetProperty("rpc.ntlm.sso", "true");
        serverName = serverName.Trim();
        serverName = Dns.GetHostAddresses(serverName).First().ToString();
        Address = "ncacn_np:" + serverName + "[\\PIPE\\winreg]";
    }

    /// <inheritdoc/>
    public PolicyHandle OpenHKLM()
    {
        var openhklm = new OpenHKLM();
        var handle = new PolicyHandle(false);
        try
        {
            Call(Semantics.IDEMPOTENT, openhklm);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        Array.Copy(openhklm.policyhandle, 0, handle.Handle, 0, 20);
        return handle;
    }

    /// <inheritdoc/>
    public PolicyHandle OpenHKCR()
    {
        var openhkcr = new OpenHKCR();
        var handle = new PolicyHandle(false);
        try
        {
            Call(Semantics.IDEMPOTENT, openhkcr);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        Array.Copy(openhkcr.policyhandle, 0, handle.Handle, 0, 20);

        return handle;
    }

    /// <inheritdoc/>
    public PolicyHandle OpenHKCU()
    {
        var openhkcu = new OpenHKCU();
        var handle = new PolicyHandle(false);
        try
        {
            Call(Semantics.IDEMPOTENT, openhkcu);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        Array.Copy(openhkcu.policyhandle, 0, handle.Handle, 0, 20);
        return handle;
    }

    /// <inheritdoc/>
    public PolicyHandle OpenHKU()
    {
        var openhku = new OpenHKU();
        var handle = new PolicyHandle(false);
        try
        {
            Call(Semantics.IDEMPOTENT, openhku);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        Array.Copy(openhku.policyhandle, 0, handle.Handle, 0, 20);

        return handle;
    }

    /// <inheritdoc/>
    public PolicyHandle OpenKey(PolicyHandle handle, string key,
        RegKeyAccess accessMask)
    {
        var openkey = new OpenKey
        {
            accessMask = accessMask,
            key = key,
            parentKey = handle
        };
        var newHandle = new PolicyHandle(false);
        try
        {
            Call(Semantics.IDEMPOTENT, openkey);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        Array.Copy(openkey.policyhandle, 0, newHandle.Handle, 0, 20);

        return newHandle;
    }

    /// <inheritdoc/>
    public void CloseKey(PolicyHandle handle)
    {
        var closekey = new CloseKey
        {
            key = handle
        };
        try
        {
            Call(Semantics.IDEMPOTENT, closekey);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }
    }

    /// <inheritdoc/>
    public void DeleteKeyOrValue(PolicyHandle handle, string valueName, bool isKey)
    {
        var delete = new DeleteValueOrKey
        {
            parentKey = handle,
            valueName = valueName,
            isKey = isKey
        };
        try
        {
            Call(Semantics.IDEMPOTENT, delete);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }
    }

    /// <inheritdoc/>
    public byte[] QueryValue(PolicyHandle handle, int bufferSize)
    {
        var queryvalue = new QueryValue
        {
            parentKey = handle,
            bufferLength = bufferSize
        };
        try
        {
            Call(Semantics.IDEMPOTENT, queryvalue);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        // return queryvalue.key;
        return queryvalue.buffer;
    }

    /// <inheritdoc/>
    public object[] QueryValue(PolicyHandle handle, string valueName, int bufferSize)
    {
        var queryvalue = new QueryValue
        {
            parentKey = handle,
            bufferLength = bufferSize,
            key = valueName
        };

        try
        {
            Call(Semantics.IDEMPOTENT, queryvalue);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        return new object[] { queryvalue.type, queryvalue.buffer ?? (object)queryvalue.buffer2 };
    }

    /// <inheritdoc/>
    public void SaveFile(PolicyHandle handle, string fileName)
    {
        var savefile = new SaveFile
        {
            parentKey = handle,
            fileName = fileName
        };

        try
        {
            Call(Semantics.IDEMPOTENT, savefile);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

    }

    /// <inheritdoc/>
    public PolicyHandle CreateKey(PolicyHandle handle, string subKey,
        RegOption options, RegKeyAccess accessMask)
    {
        var createkey = new CreateKey
        {
            accessMask = accessMask,
            key = subKey,
            parentKey = handle,
            options = options
        };

        try
        {
            Call(Semantics.IDEMPOTENT, createkey);
        }
        catch (SmbException e)
        {
            throw new InteropException(e.GetNtStatus(), e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        var newHandle = new PolicyHandle(createkey.actiontaken == 1 ? true : false);
        Array.Copy(createkey.policyhandle, 0, newHandle.Handle, 0, 20);

        return newHandle;
    }

    /// <inheritdoc/>
    public void SetValue(PolicyHandle handle, string valueName, byte[][] data)
    {
        if (data == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_WINREG_EXCEPTION5), nameof(data));
        }

        // calculate length of all strings + extra null in the end
        var totalStrings = data.Length;
        var length = 0;
        for (var i = 0; i < totalStrings; i++)
        {
            var j = data[i].Length;
            length += (j + 1) * 2; // including null termination
        }

        length += 2; // final termination

        var setvalue = new SetValue
        {
            clazzType = RegValueType.REG_MULTI_SZ,
            data2 = data,
            lengthInBytes = length,
            parentKey = handle,
            valueName = valueName
        };
        SetValue(setvalue);
    }

    /// <inheritdoc/>
    public void SetValue(PolicyHandle handle, string valueName)
    {
        var setvalue = new SetValue
        {
            clazzType = RegValueType.REG_NONE,
            parentKey = handle,
            valueName = valueName
        };
        SetValue(setvalue);
    }

    /// <inheritdoc/>
    public void SetValue(PolicyHandle handle, string valueName, byte[] data, bool isBinary, bool expand_sz)
    {
        var setvalue = new SetValue
        {
            data = data,
            lengthInBytes = data.Length,
            parentKey = handle,
            valueName = valueName
        };
        if (isBinary)
        {
            setvalue.clazzType = RegValueType.REG_BINARY;
        }
        else
        {
            if (expand_sz)
            {
                setvalue.clazzType = RegValueType.REG_EXPAND_SZ;
            }
            else
            {
                setvalue.clazzType = RegValueType.REG_SZ;
            }
        }
        SetValue(setvalue);
    }

    /// <inheritdoc/>
    public void SetValue(PolicyHandle handle, string valueName, int data)
    {
        var setvalue = new SetValue
        {
            clazzType = RegValueType.REG_DWORD,
            lengthInBytes = 4,
            dword = data,
            parentKey = handle,
            valueName = valueName
        };
        SetValue(setvalue);
    }

    /// <inheritdoc/>
    public string[] EnumKey(PolicyHandle handle, int index)
    {
        var enumkey = new EnumKey
        {
            parentKey = handle,
            index = index
        };

        try
        {
            Call(Semantics.IDEMPOTENT, enumkey);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        return enumkey.retval;
    }

    /// <inheritdoc/>
    public object[] EnumValue(PolicyHandle handle, int index)
    {
        var enumvalue = new EnumValue
        {
            parentKey = handle,
            index = index
        };
        try
        {
            Call(Semantics.IDEMPOTENT, enumvalue);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }

        return enumvalue.retval;
    }

    /// <inheritdoc/>
    public void CloseConnection()
    {
        try
        {
            Detach();
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
    }

    /// <summary>
    /// Set value
    /// </summary>
    private void SetValue(SetValue value)
    {
        try
        {
            Call(Semantics.IDEMPOTENT, value);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e)
        {
            throw new InteropException(e);
        }
    }
}

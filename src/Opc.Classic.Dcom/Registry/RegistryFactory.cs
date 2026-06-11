// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Registry.Smb;
using Opc.Classic.Dcom.Common.Ntlm;
using System.Threading;

namespace Opc.Classic.Dcom.Registry;

/// <summary>
/// Factory to get an implementation of <code>IRegistry</code>.
/// This interface uses "Windows Remote Registry" and "Server"
/// services and these must be running on target workstation.
/// based upon the transport intended to be used this factory
/// provides either the smb impl or the tcp/ip one.
/// </summary>
public class RegistryFactory
{

    /// <summary>
    /// Private constructor
    /// </summary>
    private RegistryFactory()
    {
    }

    /// <summary>
    /// Instantiates the Factory.
    /// </summary>
    public static RegistryFactory Instance
    {
        get
        {
            if (_factory == null)
            {
                lock (s_factoryLock)
                {
                    if (_factory == null)
                    {
                        _factory = new RegistryFactory();
                    }
                }
            }
            return _factory;
        }
    }

    /// <summary>
    /// Gets an Implementation of IRegistry interface,
    /// currently only SMB transport is supported.
    /// </summary>
    /// <param name="authInfo"> credentials for access
    /// to Windows Remote Registry service </param>
    /// <param name="serverName"> target server </param>
    /// <param name="smbTransport"> true if SMB transport
    /// is required, false will return null.
    /// </param>
    /// <exception cref="UnknownHostException"> </exception>
    public IRegistry GetRegistryClient(IAuthInfo authInfo, string serverName,
        bool smbTransport)
    {
        if (smbTransport)
        {
            return new RegistryStub(authInfo, serverName);
        }
        return null;
    }

    /// <summary>
    /// Gets an Implementation of IRegistry interface,
    /// currently only SMB transport is supported.
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="smbTransport"> true if SMB transport
    /// is required, false will return null.
    /// </param>
    /// <exception cref="UnknownHostException"> </exception>
    public IRegistry GetRegistryClient(string serverName, bool smbTransport)
    {
        if (smbTransport)
        {
            return new RegistryStub(serverName);
        }
        return null;
    }

    private static readonly Lock s_factoryLock = new();
    private static RegistryFactory _factory;
}

// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Automation;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Registry;
using Opc.Classic.Dcom.Transport;
using System.Net;
using Opc.Classic.Dcom.Rpc.Core;
using System.Globalization;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;
/// <summary>
/// Startup class representing a COM Server.
/// Sample Usage :
/// <code>
///  <seealso cref="Session"/> session =
///     <see cref="Session"/>.createSession("DOMAIN","USERNAME","PASSWORD");
///  <see cref="ComServer"/> excelServer = 
///     new <see cref="ComServer"/>(<seealso cref="ProgId"/>.ValueOf("Excel.Application"),address,session);
///  <see cref="IComObject"/> comObject = excelServer.CreateInstance();
///  // Obtaining the <see cref="IDispatch"/> (if supported)
///  <seealso cref="IDispatch"/> dispatch =
///    (<see cref="IDispatch"/>)<seealso cref="ObjectFactory"/>.NarrowObject(
///     comObject.queryInterface(<see cref="IDispatch"/>.IID));
///  </code>
/// Each instance of this class is associated with a single session only.
/// </summary>
public sealed class ComServer : Stub
{
    private static readonly PropertyBag kDefaults = new PropertyBag();
    static ComServer()
    {
        kDefaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
        kDefaults.SetProperty("rpc.ntlm.sign", "false");
        kDefaults.SetProperty("rpc.ntlm.seal", "false");
        kDefaults.SetProperty("rpc.ntlm.keyExchange", "false");
        kDefaults.SetProperty("rpc.ntlm.sso", "false");
        kDefaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
        kDefaults.SetProperty("rpc.socketTimeout", 0.ToString(CultureInfo.InvariantCulture));
        kDefaults.SetProperty(RpcTransportQuotas.MaxNdrPayloadSizeProperty, RpcTransportQuotas.DefaultMaxNdrPayloadSize.ToString(CultureInfo.InvariantCulture));
        kDefaults.SetProperty(RpcTransportQuotas.MaxNtlmMessageSizeProperty, RpcTransportQuotas.DefaultMaxNtlmMessageSize.ToString(CultureInfo.InvariantCulture));
        kDefaults.SetProperty(RpcTransportQuotas.MaxSmb2MessageSizeProperty, RpcTransportQuotas.DefaultMaxSmb2MessageSize.ToString(CultureInfo.InvariantCulture));
        //        rpc.connectionContext = rpc.security.ntlm.NtlmConnectionContext
        //        rpc.ntlm.sign = false
        //        rpc.ntlm.seal = false
        //        rpc.ntlm.keyExchange = false

    }

    private ComServer()
    {
    }

    /// <summary>
    /// <para> Instantiates a <see cref="ComServer"/> represented by the interfacePointer param. There are cases where a COM server may hand
    /// down a
    /// reference to a different COM server(which may or may not be on the same machine) and we would like to hook in between.
    /// The <code><see cref="IComObject"/></code> interface is usable only in the context of the current <see cref="ComServer"/>, but when
    /// the interfacePointer
    /// is of a completely different COM server, the <see cref="ObjectFactory"/> APIs will not work. The reason is the interface pointer
    /// passed to those
    /// APIs expects to belong only to a single and same COM server (say 'A'). If by any chance, that COM server passes a reference to you
    /// of another COM server (say 'B') on a different machine, the <code><see cref="IComObject"/></code> so returned from
    /// <code><see cref="ObjectFactory"/></code> APIs
    /// will result in "Method not found" Exceptions (or others) since the pointer returned via that will always place calls to 'A' instead
    /// of 'B'.
    /// Under such scenarios you must use this API. This is not a usual case and for reasons related to nature of DCOM, will be very well
    /// documented
    /// in the Developers guide of your COM server.
    /// <para>
    /// </para>
    /// <para>The DCOM specs refer to this as the "middleman" case. (Section 3.3.1) </para>
    /// </para>
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    /// <param name="session"> Please use a new session and not an already bounded one. The <code><see cref="Session"/>.createSession(<see cref="Session"/>)</code> can be used to create a new session. </param>
    /// <param name="interfacePointer"> reference to a different COM server pointer. </param>
    /// <param name="ipAddress"> Can be <code>null</code>. Sometimes there are many adapters (virtual as well) on the Target machine to which this interface pointer belongs,
    /// which may get sent as part of the interface pointer and consequently this call will fail since it is a possibility that IP is not reachable via this machine.
    /// The developer can send in the valid IP and if found in the interface pointer list will be used to talk to the target machine, overriding the other IP addresses present in the interface pointer.
    /// If this IP is not found then the "machine name" binding will be used. If this param is <code>null</code> then the first binding obtained from the interface pointer is used. </param>
    internal ComServer(Session session, InterfacePointer interfacePointer, string ipAddress)
    {
        if (interfacePointer == null || session == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(ErrorCode.INTEROP_COMSTUB_ILLEGAL_ARGUMENTS), nameof(session));
        }

        if (session.Stub != null)
        {
            throw new InteropException(ErrorCode.INTEROP_SESSION_ALREADY_ESTABLISHED);
        }

        if (Log.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            Interop.Internal_dumpMap();
        }

        //        ipAddress="192.168.1.104";
        if (ipAddress != null && !ipAddress.Trim().Equals("", StringComparison.CurrentCultureIgnoreCase))
        {
            kListOfIps.Add(ipAddress);
        }

        TransportFactory = ComTransportFactory.Instance;
        // now read the session and prepare information for the stub.
        Properties = new PropertyBag(kDefaults);
        Properties.SetProperty("rpc.security.username", session.UserName);
        Properties.SetProperty("rpc.security.password", session.Password);
        Properties.SetProperty("rpc.ntlm.domain", session.Domain);
        Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString(CultureInfo.InvariantCulture));
        if (session.NTLMv2Enabled)
        {
            Properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        }
        if (session.SSOEnabled)
        {
            Properties.SetProperty("rpc.ntlm.sso", "true");
        }

        var addressBindings = interfacePointer.StringBindings.StringBindings;

        var i = 0;
        StringBinding binding = null;
        StringBinding nameBinding = null;
        var targetAddress = ipAddress == null ? "" : ipAddress.Trim();

        {
            //        if (!targetAddress.equals(""))
            // now we choose, otherwise the first one we get.
            while (i < addressBindings.Length)
            {
                binding = addressBindings[i];
                if (binding.TowerId != 0x07)
                {
                    // this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                    i++;
                    continue;
                }
                // get the one with IP address
                var idx = binding.NetworkAddress.IndexOf('.');
                if (idx != -1)
                {
                    try
                    {
                        if (kListOfIps.Contains(binding.NetworkAddress))
                        {
                            nameBinding = null;
                            break;
                        }

                        // now check for the one with port
                        idx = binding.NetworkAddress.IndexOf('['); // this contains the port
                        if (idx != -1 && kListOfIps.Contains(binding.NetworkAddress.Substring(0, idx)))
                        {
                            nameBinding = null;
                            break;
                        }
                    }
                    catch (FormatException)
                    {
                    }
                }
                else
                {
                    // can only come for the name, saving it incase nothing matches the target address
                    nameBinding = binding;
                }
                i++;
            }

            binding = nameBinding ?? binding;
        }

        // will use this last binding .
        // and currently only TCPIP is supported.
        var address = binding.NetworkAddress;
        if (address.IndexOf('[') == -1)
        { // this does not contain the port
            var addr = Interop.GetIPForHostName(address); // to use the binding supplied by the user.
            if (addr != null)
            {
                address = addr;
            }
            // use 135
            address += "[135]";
        }
        else
        {
            var idx = address.IndexOf('[');
            var host = binding.NetworkAddress.Substring(0, idx);
            var addr = Interop.GetIPForHostName(host); // to use the binding supplied by the user.
            if (addr != null)
            {
                address = string.Concat(addr, address.AsSpan(idx));
            }
        }
        Address = "ncacn_ip_tcp:" + address;
        _session = session;
        _session.TargetServer = Address.SubstringSpecial(
            Address.IndexOf(':') + 1,
            Address.IndexOf('['));
        _oxidResolver = new OxidResolver(((StdObjRef)
            interfacePointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD)).Oxid);
        try
        {
            _syntax = "99fcfec4-5260-101b-bbcb-00aa0021347a:0.0";
            Attach();
            // first send an AlterContext to the IID of the IOxidResolver
            Endpoint.Syntax.Uuid = new Opc.Classic.Dcom.Rpc.Core.UUID(Interfaces.IID_IObjectExporter);
            Endpoint.Syntax.Version = 0;
            ((ComEndpoint)Endpoint).RebindEndPoint();

            Call(Semantics.IDEMPOTENT, _oxidResolver);
        }
        catch (FaultException e)
        {
            throw new InteropException((int)e.Code, e);
        }
        catch (IOException e)
        {
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e1)
        {
            throw new InteropException(e1);
        }

        // Now will setup syntax for IRemUnknown and the address.
        _syntax = interfacePointer.IID + ":0.0";

        // now for the new ip and the port.

        var bindings = _oxidResolver.OxidBindings.StringBindings;

        binding = null;
        nameBinding = null;
        i = 0;
        //        if (!targetAddress.equals(""))
        {
            // now we choose, otherwise the first one we get.
            while (i < bindings.Length)
            {
                binding = bindings[i];
                if (binding.TowerId != 0x07) // this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
                {
                    i++;
                    continue;
                }
                // get the one with IP address
                var idx = binding.NetworkAddress.IndexOf('.');
                if (idx != -1)
                {
                    try
                    {
                        if (kListOfIps.Contains(binding.NetworkAddress))
                        {
                            nameBinding = null;
                            break;
                        }

                        // now check for the one with port
                        idx = binding.NetworkAddress.IndexOf('['); // this contains the port
                        if (idx != -1 && kListOfIps.Contains(binding.NetworkAddress.Substring(0, idx)))
                        {
                            nameBinding = null;
                            break;
                        }
                    }
                    catch (FormatException)
                    {
                    }
                }
                else
                {
                    // can only come for the name, saving it incase nothing matches the target address
                    nameBinding = binding;
                }
                i++;
            }

            binding = nameBinding ?? binding;
        }

        // now set the NTLMv2 Session Security.
        if (session.SessionSecurityEnabled)
        {
            Properties.SetProperty("rpc.ntlm.seal", "true");
            Properties.SetProperty("rpc.ntlm.sign", "true");
            Properties.SetProperty("rpc.ntlm.keyExchange", "true");
            Properties.SetProperty("rpc.ntlm.keyLength", "128");
            Properties.SetProperty("rpc.ntlm.ntlm2", "true");
        }

        address = binding.NetworkAddress; // this will always have the port.
        var index = address.IndexOf('[');
        var hostname = binding.NetworkAddress.Substring(0, index);
        var ipAddr = Interop.GetIPForHostName(hostname); // to use the binding supplied by the user.
        if (ipAddr != null)
        {
            address = string.Concat(ipAddr, address.AsSpan(index));
        }

        // and currently only TCPIP is supported.
        Address = "ncacn_ip_tcp:" + address;
        _remunknownIPID = _oxidResolver.IPID;
        _interfacePtrCtor = interfacePointer;
        _session.Stub = this;
        _session.Stub2 = new RemUnknown2ServerStub(session, _remunknownIPID, Address);
    }

    /// <summary>
    /// <code><seealso cref="ProgId"/></code> based constructor with the host machine for COM
    /// server being <i>LOCALHOST</i>.
    /// </summary>
    /// <param name="progId"> user-friendly string such as "Excel.Application",
    /// "TestCOMServer.Test123" etc. </param>
    /// <param name="session"> session to be associated with. </param>
    /// <exception cref="InteropException"> will <i>also</i> get thrown in case the 
    /// <code>session</code> is associated with another server already. </exception>
    /// <exception cref="ArgumentException"> raised when either <code>progId</code>
    /// or <code>session</code> is <code>null</code>. </exception>
    /// <exception cref="System.Net.Sockets.SocketException">Thrown when the remote host cannot be resolved or the connection is refused.</exception>
    public ComServer(ProgId progId, Session session) :
        this(progId, Dns.GetHostName(), session)
    {
    }

    /// <summary>
    /// <code><seealso cref="Clsid"/></code> based constructor with the host
    /// machine for COM server being <i>LOCALHOST</i>.
    /// </summary>
    /// <param name="clsid">128 bit string such as "00024500-0000-0000-C000-000000000046".
    /// </param>
    /// <param name="session"> session to be associated with. </param>
    /// <exception cref="InteropException"> will <i>also</i> get thrown in case the
    /// <code>session</code> is associated with another server already. </exception>
    /// <exception cref="ArgumentException"> raised when either <code>clsid</code>
    /// or <code>session</code> is <code>null</code>. </exception>
    /// <exception cref="System.Net.Sockets.SocketException">Thrown when the remote host cannot be resolved or the connection is refused.</exception>
    public ComServer(Clsid clsid, Session session) :
        this(clsid, Dns.GetHostName(), session)
    {
    }

    /// <summary>
    /// Refer <seealso cref="ComServer(ProgId, Session)"/> for details.
    /// </summary>
    /// <param name="progId"> user-friendly string such as "Excel.Application",
    /// "TestCOMServer.Test123" etc. </param>
    /// <param name="address"> address of the host where the <code>COM</code> object resides.
    /// This should be in the IEEE IP format (e.g. 192.168.170.6) or a resolvable HostName. 
    /// </param>
    /// <param name="session"> session to be associated with. </param>
    /// <exception cref="InteropException"> will <i>also</i> get thrown in case the
    /// <code>session</code> is associated with another server already. </exception>
    /// <exception cref="ArgumentException"> raised when any of the parameters
    /// is <code>null</code>. </exception>
    /// <exception cref="System.Net.Sockets.SocketException">Thrown when the remote host cannot be resolved or the connection is refused.</exception>
    public ComServer(ProgId progId, string address, Session session)
    {
        if (progId == null || address == null || session == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COMSTUB_ILLEGAL_ARGUMENTS), nameof(session));
        }
        if (session.Stub != null)
        {
            throw new InteropException(ErrorCode.INTEROP_SESSION_ALREADY_ESTABLISHED);
        }

        if (session.SSOEnabled)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COMSTUB_ILLEGAL_ARGUMENTS2), nameof(session));
        }

        address = address.Trim();
        address = Dns.GetHostAddresses(address).First()?.ToString();
        var clsid = progId.GetCorrespondingClsid(address, session);
        address = "ncacn_ip_tcp:" + address + "[135]";
        Initialise(clsid, address, session);
    }

    /// <summary>
    /// Refer <seealso cref="ComServer(Clsid, Session)"/> for details.
    /// </summary>
    /// <param name="clsid">128 bit string such as "00024500-0000-0000-C000-000000000046".
    /// </param>
    /// <param name="address"> address of the host where the <code>COM</code> object
    /// resides.This should be in the IEEE IP format (e.g. 192.168.170.6) or a
    /// resolvable HostName. </param>
    /// <param name="session"> session to be associated with. </param>
    /// <exception cref="InteropException"> will <i>also</i> get thrown in case the
    /// <code>session</code> is associated with another server already. </exception>
    /// <exception cref="ArgumentException"> raised when any of the parameters
    /// is <code>null</code>. </exception>
    /// <exception cref="System.Net.Sockets.SocketException">Thrown when the remote host cannot be resolved or the connection is refused.</exception>
    public ComServer(Clsid clsid, string address, Session session)
    {
        if (clsid == null || address == null || session == null)
        {
            throw new ArgumentException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COMSTUB_ILLEGAL_ARGUMENTS), nameof(session));
        }
        if (session.Stub != null)
        {
            throw new InteropException(ErrorCode.INTEROP_SESSION_ALREADY_ESTABLISHED);
        }
        address = address.Trim();
        address = Dns.GetHostAddresses(address).First()?.ToString();
        address = "ncacn_ip_tcp:" + address + "[135]";
        Initialise(clsid, address, session);
    }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="clsid">CLSID identifying the COM class or OPC server to activate.</param>
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="session">Session that owns the COM object, transport, and authentication state.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    private void Initialise(Clsid clsid, string address, Session session)
    {
        TransportFactory = ComTransportFactory.Instance;
        // now read the session and prepare information for the stub.
        Properties = new PropertyBag(kDefaults);
        Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString(CultureInfo.InvariantCulture));
        Address = address;

        if (session.NTLMv2Enabled)
        {
            Properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        }

        if (session.SSOEnabled)
        {
            Properties.SetProperty("rpc.ntlm.sso", "true");
        }
        else
        {
            Properties.SetProperty("rpc.security.username", session.UserName);
            Properties.SetProperty("rpc.security.password", session.Password);
            Properties.SetProperty("rpc.ntlm.domain", session.Domain);
        }

        ProtectionLevel activationProtectionLevel = ComOxidRuntime.ConfigureActivationProtection(
            Properties,
            session.SessionSecurityEnabled,
            session.SessionSecurityEnabled ? session.UserName : null,
            session.SessionSecurityEnabled ? session.Password : null);
        _activationAuthenticationLevel = (int)activationProtectionLevel;

        if (Log.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
        {
            Interop.Internal_dumpMap();
        }

        _clsid = clsid.CLSID.ToUpper(CultureInfo.InvariantCulture);
        _session = session;
        _session.TargetServer = address.SubstringSpecial(address.IndexOf(':') + 1, address.IndexOf('['));
        try
        {
            Init();
        }
        catch (InteropException e)
        {
            if ((uint)e.ErrorCode == 0x80040154)
            {
                Log.Logger.Warning("Got the class not registered exception, " +
                    "will attempt setting entries based on status flags...");

                // try registering the dll\ocx on our own
                // check for clsid.autoregister flag
                // check for jisystem.autoregister flag.
                // jisystem takes precedence over clsid.

                if (Interop.UseAutoRegistration || clsid.UseAutoRegistration)
                {
                    // first create the registry entries.
                    try
                    {
                        IRegistry registry = null;
                        if (session.SSOEnabled)
                        {
                            registry = RegistryFactory.Instance.GetRegistryClient(session.TargetServer, true);
                        }
                        else
                        {
                            registry = RegistryFactory.Instance.GetRegistryClient(new DefaultAuthInfoImpl(
                                session.Domain, session.UserName, session.Password), session.TargetServer, true);
                        }

                        PolicyHandle hklm = null;
                        PolicyHandle hkwow6432 = null;
                        try
                        {
                            // Try 64bit first...
                            hklm = registry.OpenHKLM();
                            hkwow6432 = registry.OpenKey(hklm, "SOFTWARE\\Classes\\Wow6432Node", RegKeyAccess.KEY_ALL_ACCESS);
                        }
                        catch (InteropException)
                        {
                        }

                        if (hklm != null)
                        {
                            registry.CloseKey(hklm);
                        }

                        if (hkwow6432 != null)
                        {
                            Log.Logger.Information("Attempting to register on 64 bit");

                            // HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\CLSID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\ -- "AppID"
                            var key = registry.CreateKey(hkwow6432, "CLSID\\{" + _clsid + "}",
                                RegOption.REG_OPTION_NON_VOLATILE, RegKeyAccess.KEY_ALL_ACCESS);
                            registry.SetValue(key, "AppId", ("{" + _clsid + "}").GetBytes(), false, false);
                            registry.CloseKey(key);
                            Log.Logger.Information("--- SetValue --- SOFTWARE\\Classes\\Wow6432Node\\CLSID\\" + _clsid + " -- AppID");

                            // HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node\AppID\{E4BE20A4-9EF1-4B05-9117-AF43EAB4B295}\AppID\ -- "DllSurrogate"
                            key = registry.CreateKey(hkwow6432, "AppID\\{" + _clsid + "}",
                                RegOption.REG_OPTION_NON_VOLATILE, RegKeyAccess.KEY_ALL_ACCESS);
                            registry.SetValue(key, "DllSurrogate", "".GetBytes(), false, false);
                            registry.CloseKey(key);

                            Log.Logger.Information("--- SetValue --- SOFTWARE\\Classes\\Wow6432Node\\AppID\\" +
                                _clsid + " -- DllSurrogate");
                            registry.CloseKey(hkwow6432);
                        }
                        else
                        {
                            Log.Logger.Information("Attempting to register on 32 bit");
                            var hkcr = registry.OpenHKCR();
                            var key = registry.CreateKey(hkcr, "CLSID\\{" + _clsid + "}",
                                RegOption.REG_OPTION_NON_VOLATILE, RegKeyAccess.KEY_ALL_ACCESS);
                            registry.SetValue(key, "AppID", ("{" + _clsid + "}").GetBytes(), false, false);
                            registry.CloseKey(key);
                            key = registry.CreateKey(hkcr, "AppID\\{" + _clsid + "}",
                                RegOption.REG_OPTION_NON_VOLATILE, RegKeyAccess.KEY_ALL_ACCESS);
                            registry.SetValue(key, "DllSurrogate", "  ".GetBytes(), false, false);

                            registry.CloseKey(key);
                            registry.CloseKey(hkcr);
                        }
                        registry.CloseConnection();
                    }
                    catch (System.Net.Sockets.SocketException e1)
                    {
                        // auto registration failed as well...
                        Log.Logger.Error(e, "ComServer initialise");
                        throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION3, e1);
                    }
                    // lets retry
                    Init();
                }
                else
                {
                    throw;
                }
            }
            else
            {
                throw;
            }
        }

        _session.Stub = this;
        _session.Stub2 = new RemUnknown2ServerStub(session, _remunknownIPID, Address);
    }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    private void Init()
    {
        if (_serverActivation != null && _serverActivation.ActivationSuccessful)
        {
            return;
        }

        var attachcomplete = false;
        try
        {
            _syntax = Interfaces.IID_IObjectExporter.ToLower(CultureInfo.InvariantCulture) + ":0.0";
            Attach();
            // socket to COM server is established
            attachcomplete = true;
            // first send an AlterContext to the IID of the IOxidResolver
            Endpoint.Syntax.Uuid = new Opc.Classic.Dcom.Rpc.Core.UUID(Interfaces.IID_IObjectExporter);
            Endpoint.Syntax.Version = 0;
            ((ComEndpoint)Endpoint).RebindEndPoint();

            // 3.2.4.1.1.1 Determining RPC Binding Information for Activation
            // Commenting the below to dynamically identify DCOM versions.
            //            CallBuilder serverAlive = new CallBuilder(true);
            //            serverAlive.attachSession(session);
            //            serverAlive.setOpnum(0);
            //            serverAlive.setReadOnlyHRESULT();
            //            Call(Semantics.IDEMPOTENT,serverAlive);

            var serverAlive = new CallBuilder(true);
            serverAlive.AttachSession(_session);
            serverAlive.Opnum = 2;
            serverAlive.Internal_COMVersion();
            try
            {
                Call(Semantics.IDEMPOTENT, serverAlive);
                Interop.COMVersion = serverAlive.Internal_getComVersion();
            }
            catch (InteropRuntimeException e)
            {
                if (e.HResult == unchecked((int)ErrorCode.RPC_S_PROCNUM_OUT_OF_RANGE))
                {
                    Interop.COMVersion.MajorVersion = 5;
                    Interop.COMVersion.MinorVersion = 1;
                }
            }

            if (Interop.COMVersion != null && Interop.COMVersion.MinorVersion > 1)
            {
                // Default path: IRemoteSCMActivator (DCOM v5.6 / Win XP SP2+ /
                // Win Server 2003+). Required by Microsoft's DCOM hardening
                // (KB5004442, mandatory since March 2023) which rejects activation
                // requests below RPC_C_AUTHN_LEVEL_PKT_INTEGRITY against hardened
                // Windows DCOM servers.
                _syntax = Interfaces.IID_IRemoteSCMActivator + ":0.0";
                Endpoint.Syntax.Uuid = new UUID(Interfaces.IID_IRemoteSCMActivator);
                Endpoint.Syntax.Version = 0;
                ((ComEndpoint)Endpoint).RebindEndPoint();
                _serverActivation = new RemoteSCMActivator.RemoteCreateInstance(_session.TargetServer, _clsid, _activationAuthenticationLevel);
                Call(Semantics.IDEMPOTENT, (RemoteSCMActivator.RemoteCreateInstance)_serverActivation);
            }
            else
            {
                // Legacy path: IRemoteActivation (DCOM v5.4 / Win 2000 / XP RTM).
                // Opt-in only — set Interop.COMVersion = new ComVersion(5, 1).
                // Hardened Windows DCOM servers will reject this path with
                // Event ID 10036 unless explicitly relaxed on the server.
                _syntax = Interfaces.IID_IActivation + ":0.0";
                Endpoint.Syntax.Uuid = new UUID(Interfaces.IID_IActivation);
                Endpoint.Syntax.Version = 0;
                ((ComEndpoint)Endpoint).RebindEndPoint();
                _serverActivation = new RemActivation(_clsid);
                Call(Semantics.IDEMPOTENT, (RemActivation)_serverActivation);
            }
        }
        catch (FaultException e)
        {
            _serverActivation = null;
            throw new InteropException((int)e.Code, e);
        }
        catch (IOException e)
        {
            _serverActivation = null;
            throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
        }
        catch (InteropRuntimeException e1)
        {
            _serverActivation = null;
            throw new InteropException(e1);
        }
        finally
        {
            // the only time remactivation will be null will be case of an exception.
            if (attachcomplete && _serverActivation == null)
            {
                try
                {
                    Detach();
                }
                catch (IOException e)
                {
                    Log.Logger.Warning(e, "Unable to detach during init");
                }
            }
        }

        // Now will setup syntax for IRemUnknown2 and the address.
        _syntax = Interfaces.IID_IRemUnknown2 + ":0.0";
        // now for the new ip and the port.

        var bindings = _serverActivation.DualStringArrayForOxid.StringBindings;
        var i = 0;
        StringBinding binding = null;
        StringBinding nameBinding = null;
        var targetAddress = Address;
        targetAddress = targetAddress.SubstringSpecial(targetAddress.IndexOf(':') + 1, targetAddress.IndexOf('['));
        while (i < bindings.Length)
        {
            binding = bindings[i];
            if (binding.TowerId != 0x07) // this means, even though I asked for TCPIP something else was supplied, noticed this in win2k.
            {
                i++;
                continue;
            }
            // get the one with IP address
            var idx = binding.NetworkAddress.IndexOf('.');
            if (idx != -1)
            {
                try
                {
                    idx = binding.NetworkAddress.IndexOf('['); // this contains the port
                    if (idx != -1 && binding.NetworkAddress.Substring(0, idx).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase))
                    {
                        break;
                    }
                }
                catch (FormatException)
                {
                }
            }
            else
            {
                // can only come for the name, saving it incase nothing matches the target address
                // then we are not sure which is the right IP and which might be virtual, refer to
                // issue faced by Igor.
                nameBinding = binding;
                idx = binding.NetworkAddress.IndexOf('['); // this contains the port
                if (binding.NetworkAddress.Substring(0, idx).Equals(targetAddress, StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }
            i++;
        }

        if (binding == null)
        {
            binding = nameBinding;
        }

        // will use this last binding .
        // and currently only TCPIP is supported.
        // now set the NTLMv2 Session Security.
        if (_session.SessionSecurityEnabled)
        {
            Properties.SetProperty("rpc.ntlm.seal", "true");
            Properties.SetProperty("rpc.ntlm.sign", "true");
            Properties.SetProperty("rpc.ntlm.keyExchange", "true");
            Properties.SetProperty("rpc.ntlm.keyLength", "128");
            Properties.SetProperty("rpc.ntlm.ntlm2", "true");
        }

        var address = binding.NetworkAddress; // this will always have the port.
        var index = address.IndexOf('[');
        var hostname = binding.NetworkAddress.Substring(0, index);
        var ipAddr = Interop.GetIPForHostName(hostname); // to use the binding supplied by the user.
        if (ipAddr != null)
        {
            address = string.Concat(ipAddr, address.AsSpan(index));
        }

        // and currently only TCPIP is supported.
        Address = "ncacn_ip_tcp:" + address;
        _remunknownIPID = _serverActivation.IPID;
    }

    /// <summary>
    /// Will give a call to IRemUnknown for the passed IID
    /// </summary>
    /// <param name="iid">Interface IID identifying the COM interface being queried or marshaled.</param>
    /// <param name="ipidOfTheTargetUnknown">IPID of the target IUnknown interface used for remote queries.</param>
    /// <returns>The requested interface value.</returns>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal IComObject GetInterface(string iid, string ipidOfTheTargetUnknown)
    {
        IComObject retval = null;
        // this is still essentially serial, since all threads will have to wait for mutex before
        // entering addToSession.
        lock (_mutex)
        {
            // now also set the Object ID for IRemUnknown call this will be the IPID of the returned RemActivation
            Object = _remunknownIPID;
            var reqUnknown = new RemUnknown2(ipidOfTheTargetUnknown, iid);
            try
            {
                _session.Stub2.Call(Semantics.IDEMPOTENT, reqUnknown);
            }
            catch (FaultException e)
            {
                throw new InteropException((int)e.Code, e);
            }
            catch (IOException e)
            {
                throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
            }
            catch (InteropRuntimeException e1)
            {
                // remoteActivation = null;
                throw new InteropException(e1);
            }

            retval = FrameworkHelper.InstantiateComObject(_session, reqUnknown.InterfacePointer);
            // increasing the reference count.
            retval.AddRef();
            // for querying dispatch we can't send another call
            if (!iid.Equals(Interfaces.IID_IDispatch, StringComparison.CurrentCultureIgnoreCase))
            {
                var success = true;
                ((ComObjectImpl)retval).IsDual = true;
                // now to check whether it supports IDispatch
                // IDispatch 00020400-0000-0000-c000-000000000046
                var dispatch = new RemUnknown2(retval.Ipid, Interfaces.IID_IDispatch);
                try
                {
                    _session.Stub2.Call(Semantics.IDEMPOTENT, dispatch);
                }
                catch (FaultException e)
                {
                    throw new InteropException((int)e.Code, e);
                }
                catch (IOException e)
                {
                    throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
                }
                catch (InteropRuntimeException)
                {
                    // will eat this exception here.
                    ((ComObjectImpl)retval).IsDual = false;
                    success = false;
                }

                if (success)
                {
                    // which means that IDispatch is supported
                    _session.ReleaseRef(dispatch.InterfacePointer.IPID, ((StdObjRef)dispatch.InterfacePointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD)).PublicRefs);
                }
            }
        }

        return retval;
    }

    /// <summary>
    /// Returns an <code><see cref="IComObject"/></code> representing the COM Server.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    public IComObject CreateInstance()
    {
        if (_interfacePtrCtor != null)
        {
            throw new InvalidOperationException(Interop.GetLocalizedMessage(
                ErrorCode.INTEROP_COMSTUB_WRONGCALLCREATEINSTANCE));
        }
        IComObject comObject = null;

        // This method is still essentially serial, since all threads will have to stop at mutex and then
        // go to addToSession after it (since there is no condition).
        lock (_mutex)
        {
            if (_serverInstantiated)
            {
                throw new InteropException(ErrorCode.INTEROP_OBJECT_ALREADY_INSTANTIATED);
            }
            comObject = FrameworkHelper.InstantiateComObject(_session, _serverActivation.MInterfacePointer);
            if (_serverActivation.Dual)
            {
                // <see cref="IComObject"/> comObject2 = getObject(remoteActivation.dispIpid,Interfaces.IID_IDispatch);
                // this will get garbage collected and then removed.
                // session.addToSession(comObject2,remoteActivation.dispOid);
                _session.ReleaseRef(_serverActivation.DispIpid, _serverActivation.DispRefs);
                _serverActivation.DispIpid = null;
                ((ComObjectImpl)comObject).IsDual = true;
            }
            else
            {
                ((ComObjectImpl)comObject).IsDual = false;
            }
            // increasing the reference count.
            comObject.AddRef();
            _serverInstantiated = true;
        }

        return comObject;
    }

    /// <summary>
    /// Returns a <code><see cref="IComObject"/></code> representing the <code>COM</code> Server.
    /// To be used only with <code><see cref="ComServer"/>(<see cref="Session"/>,<see cref="InterfacePointer"/>,String)</code> ctor,
    /// otherwise use createInstance() instead.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal IComObject Instance
    {
        get
        {
            if (_interfacePtrCtor == null)
            {
                throw new InvalidOperationException(Interop.GetLocalizedMessage(
                    ErrorCode.INTEROP_COMSTUB_WRONGCALLGETINSTANCE));
            }

            IComObject comObject = null;
            // This method is still essentially serial, since all threads will have to stop at mutex and then
            // go to addToSession after it (since there is no condition).
            lock (_mutex)
            {
                if (_serverInstantiated)
                {
                    throw new InteropException(ErrorCode.INTEROP_OBJECT_ALREADY_INSTANTIATED);
                }
                comObject = FrameworkHelper.InstantiateComObject(_session, _interfacePtrCtor);
                // increasing the reference count.
                comObject.AddRef();
                _serverInstantiated = true;
            }

            return comObject;
        }
    }

    /// <summary>
    /// Syntax
    /// </summary>
    protected override string Syntax => _syntax;

    /// <summary>
    /// Execute a Method on the COM Interface identified by the IID.
    /// </summary>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="targetIID">
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal object[] Call(CallBuilder obj, string targetIID) =>
        Call(obj, targetIID, _session.GlobalSocketTimeout);

    /// <summary>
    /// Execute a Method on the COM Interface identified by the IID
    /// </summary>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <param name="targetIID">
    /// </param>
    /// <param name="socketTimeout">Socket timeout, in milliseconds, used for blocking transport operations.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal object[] Call(CallBuilder obj, string targetIID, int socketTimeout)
    {
        lock (_mutex)
        {
            if (_session.SessionInDestroy && !obj.FromDestroySession)
            {
                throw new InteropException(ErrorCode.INTEROP_SESSION_DESTROYED);
            }

            if (socketTimeout != 0)
            {
                SocketTimeOut = socketTimeout;
            }
            else // for cases where it was something earlier, but is now being set to 0.
            {
                if (_timeoutModifiedfrom0)
                {
                    SocketTimeOut = socketTimeout;
                }
            }
            try
            {
                Attach();
                if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID, StringComparison.CurrentCultureIgnoreCase))
                {
                    // first send an AlterContext to the IID of the interface
                    Endpoint.Syntax.Uuid = new Opc.Classic.Dcom.Rpc.Core.UUID(targetIID);
                    Endpoint.Syntax.Version = 0;
                    ((ComEndpoint)Endpoint).RebindEndPoint();
                }

                Object = obj.ParentIpid;
                Call(Semantics.IDEMPOTENT, obj);
            }
            catch (FaultException e)
            {
                throw new InteropException((int)e.Code, e);
            }
            catch (IOException e)
            {
                throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
            }
            catch (InteropRuntimeException e1)
            {
                throw new InteropException(e1);
            }
            return obj.Results;
        }
    }

    /// <summary>
    /// Server interface pointer
    /// </summary>
    internal InterfacePointer ServerInterfacePointer =>
            // remoteactivation can be null only incase of OxidResolver ctor getting called.
            _serverActivation == null ? _interfacePtrCtor : _serverActivation.MInterfacePointer;

    /// <summary>
    /// Add ref release
    /// </summary>
    /// <param name="obj">Object instance being marshaled, unmarshaled, or invoked.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    internal void AddRef_ReleaseRef(CallBuilder obj)
    {
        lock (_mutex)
        {
            if (_remunknownIPID == null)
            {
                return;
            }
            // now also set the Object ID for IRemUnknown call this will be 
            // the IPID of the returned RemActivation or IOxidResolver
            obj.ParentIpid = _remunknownIPID;
            obj.AttachSession(_session);
            try
            {
                Call(obj, Interfaces.IID_IRemUnknown2);
            }
            catch (InteropRuntimeException e1)
            {
                throw new InteropException(e1);
            }
        }
    }

    /// <summary>
    /// Close
    /// </summary>
    internal void CloseStub()
    {
        try
        {
            Detach();
        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
        catch
        {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }
    }

    /// <summary>
    /// Socket timeout
    /// </summary>
    internal int SocketTimeOut
    {
        set
        {
            if (value == 0)
            {
                _timeoutModifiedfrom0 = false;
            }
            else
            {
                _timeoutModifiedfrom0 = true;
            }

            Properties.SetProperty("rpc.socketTimeout", value.ToString(CultureInfo.InvariantCulture));
        }
    }

    private IServerActivation _serverActivation;
    private readonly OxidResolver _oxidResolver;
    private string _clsid;
    private int _activationAuthenticationLevel = (int)ProtectionLevel.PROTECTION_LEVEL_INTEGRITY;
    private Session _session;
    private bool _serverInstantiated;
    private string _remunknownIPID;
    private readonly Lock _mutex = new();
    private string _syntax;
    private bool _timeoutModifiedfrom0;
    private readonly InterfacePointer _interfacePtrCtor;
    private static readonly HashSet<string> kListOfIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

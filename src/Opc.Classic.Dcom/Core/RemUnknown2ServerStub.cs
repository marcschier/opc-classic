// SPDX-License-Identifier: MIT
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Dcom.Rpc;
using SharpCifs.Util.Sharpen;
using System;
using System.IO;
using System.Globalization;

namespace Opc.Classic.Dcom.Core; 
/// <summary>
/// Represents unknown server
/// </summary>
internal sealed class RemUnknown2ServerStub : Stub {

    private static readonly PropertyBag kDefaults = new PropertyBag();
    static RemUnknown2ServerStub() {
        kDefaults.SetProperty("rpc.ntlm.lanManagerKey", "false");
        kDefaults.SetProperty("rpc.ntlm.sign", "false");
        kDefaults.SetProperty("rpc.ntlm.seal", "false");
        kDefaults.SetProperty("rpc.ntlm.keyExchange", "false");
        kDefaults.SetProperty("rpc.connectionContext", "rpc.security.ntlm.NtlmConnectionContext");
        kDefaults.SetProperty("rpc.socketTimeout", 0.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Socket timeout
    /// </summary>
    internal int SocketTimeOut {
        set {
            if (value == 0) {
                _timeoutModifiedfrom0 = false;
            }
            else {
                _timeoutModifiedfrom0 = true;
            }
            Properties.SetProperty("rpc.socketTimeout", value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Syntax
    /// </summary>
    protected override string Syntax { get; }

    /// <summary>
    /// Interface pointer to the initialized COM server, must be
    /// called immediately after the <see cref="ComServer"/> has been
    /// initialized. And closeStub must be called where we
    /// call closeStub of <see cref="ComServer"/>.
    /// </summary>
    /// <param name="session"> </param>
    /// <param name="remUnknownIpid"> </param>
    /// <param name="address"> in the "ncacn_ip_tcp:host[port]" format </param>
    /// <exception cref="InteropException"> </exception>
    internal RemUnknown2ServerStub(Session session, string remUnknownIpid,
        string address) {

        _session = session;
        TransportFactory = ComTransportFactory.Instance;
        Properties = new PropertyBag(kDefaults);
        Properties.SetProperty("rpc.socketTimeout", session.GlobalSocketTimeout.ToString(CultureInfo.InvariantCulture));

        if (session.NTLMv2Enabled) {
            Properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        }

        if (session.SSOEnabled) {
            Properties.SetProperty("rpc.ntlm.sso", "true");
        }
        else {
            Properties.SetProperty("rpc.security.username", session.UserName);
            Properties.SetProperty("rpc.security.password", session.Password);
            Properties.SetProperty("rpc.ntlm.domain", session.Domain);
        }

        // now set the NTLMv2 Session Security.
        if (session.SessionSecurityEnabled) {
            Properties.SetProperty("rpc.ntlm.seal", "true");
            Properties.SetProperty("rpc.ntlm.sign", "true");
            Properties.SetProperty("rpc.ntlm.keyExchange", "true");
            Properties.SetProperty("rpc.ntlm.keyLength", "128");
            Properties.SetProperty("rpc.ntlm.ntlm2", "true");
        }

        // Now will setup syntax for IRemUnknown and the address.
        Syntax = Interfaces.IID_IRemUnknown2 + ":0.0";
        // and currently only TCPIP is supported.
        Address = address;
        _remunknownIPID = remUnknownIpid;
        _session.Stub2 = this;
    }

    /// <summary>
    /// Execute a Method on the COM Interface identified by the IID
    /// <param name="obj"> </param>
    /// <param name="targetIID"></param>
    /// <param name="socketTimeout"></param>
    /// </summary>
    /// <exception cref="InteropException"> </exception>
    /// <returns></returns>
    internal object[] Call(CallBuilder obj, string targetIID, int socketTimeout) {
        lock (_mutex) {
            if (_session.SessionInDestroy && !obj.FromDestroySession) {
                throw new InteropException(ErrorCode.INTEROP_SESSION_DESTROYED);
            }

            if (socketTimeout != 0) {
                SocketTimeOut = socketTimeout;
            }
            else {
                // for cases where it was something earlier, but is now being set to 0.
                if (_timeoutModifiedfrom0) {
                    SocketTimeOut = socketTimeout;
                }
            }
            try {
                Attach();
                if (!Endpoint.Syntax.Uuid.ToString().Equals(targetIID,
                    StringComparison.CurrentCultureIgnoreCase)) {
                    // first send an AlterContext to the IID of the interface
                    Endpoint.Syntax.Uuid = new Opc.Classic.Dcom.Rpc.Core.UUID(targetIID);
                    Endpoint.Syntax.Version = 0;
                    ((ComEndpoint)Endpoint).RebindEndPoint();
                }

                Object = obj.ParentIpid;
                Call(Semantics.IDEMPOTENT, obj);
            }
            catch (FaultException e) {
                throw new InteropException((int)e.Code, e);
            }
            catch (IOException e) {
                throw new InteropException(ErrorCode.RPC_E_UNEXPECTED, e);
            }
            catch (InteropRuntimeException e1) {
                throw new InteropException(e1);
            }

            return obj.Results;
        }
    }

    /// <summary>
    /// Add ref release
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="InteropException"></exception>
    internal void AddRef_ReleaseRef(CallBuilder obj) {
        lock (_mutex) {
            if (_remunknownIPID == null) {
                return;
            }
            // now also set the Object ID for IRemUnknown call this will be the
            // IPID of the returned RemActivation or IOxidResolver
            obj.ParentIpid = _remunknownIPID;
            obj.AttachSession(_session);
            try {
                Call(obj, Interfaces.IID_IRemUnknown2, _session.GlobalSocketTimeout);
            }
            catch (InteropRuntimeException e1) {
                throw new InteropException(e1);
            }
        }
    }

    /// <summary>
    /// Close
    /// </summary>
    internal void CloseStub() {
        try {
            Detach();
        }
        catch (IOException) {
        }
    }

    private readonly Session _session;
    private readonly string _remunknownIPID;
    private readonly System.Threading.Lock _mutex = new();
    private bool _timeoutModifiedfrom0;
}

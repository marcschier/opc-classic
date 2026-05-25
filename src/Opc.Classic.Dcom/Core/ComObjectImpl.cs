// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core; 
/// <summary>
/// Implementation for <see cref="IComObject"/>.
/// There is a 1 to 1 mapping between this and a <code>COM</code> interface.
/// </summary>
[Serializable]
internal sealed class ComObjectImpl : IComObject, IComObjectInternal {

    /// <summary>
    /// Dual interface
    /// </summary>
    internal bool IsDual {
        set {
            _dualInfo = true;
            _isDual = value;
        }
    }

    /// <inheritdoc/>
    public bool LocalReference { get; }

    /// <inheritdoc/>
    public ComCustomMarshallerUnMarshaller CustomObject { get; set; }

    /// <inheritdoc/>
    public int LengthOfInterfacePointer => _ptr.Length;

    /// <inheritdoc/>
    public string Ipid => _ptr.IPID;

    /// <inheritdoc/>
    public Session AssociatedSession => _session;

    /// <inheritdoc/>
    public string InterfaceIdentifier => _ptr.IID;

    /// <inheritdoc/>
    public bool DispatchSupported {
        get {
            lock (_syncRoot) {
                CheckLocal();
                if (!_dualInfo) {
                    // query interface for it and then release it.
                    try {
                        var comObject = QueryInterface(Interfaces.IID_IDispatch);
                        comObject.Release();
                        IsDual = true;
                    }
                    catch (InteropException) {
                        IsDual = false;
                    }
                }
                return _isDual;
            }
        }
    }

    /// <inheritdoc/>
    public IUnreferenced UnreferencedHandler {
        get {
            CheckLocal();
            return _session.GetUnreferencedHandler(Ipid);
        }
    }

    /// <inheritdoc/>
    public int InstanceLevelSocketTimeout {
        get {
            CheckLocal();
            return _timeout;
        }
        set {
            CheckLocal();
            _timeout = value;
        }
    }

    /// <summary>
    /// Create object
    /// </summary>
    /// <param name="session"></param>
    /// <param name="ptr"></param>
    internal ComObjectImpl(Session session, InterfacePointer ptr) :
        this(session, ptr, false) {
    }

    /// <summary>
    /// Create object
    /// </summary>
    /// <param name="session"></param>
    /// <param name="ptr"></param>
    /// <param name="isLocal"></param>
    internal ComObjectImpl(Session session, InterfacePointer ptr, bool isLocal) {
        _session = session;
        _ptr = ptr;
        LocalReference = isLocal;
    }

    /// <inheritdoc/>
    public IComObject QueryInterface(string iid) {
        CheckLocal();
        return _session.Stub.GetInterface(iid, _ptr.IPID);
    }

    /// <inheritdoc/>
    public void AddRef() {
        CheckLocal();
        var obj = new CallBuilder(true) {
            ParentIpid = _ptr.IPID,
            Opnum = 1 // addRef
        };

        // length
        obj.AddInParamAsShort(1);
        // ipid to addfref on
        var array = new ComArray(new UUID[] { new UUID(_ptr.IPID) }, true);
        obj.AddInParamAsArray(array);
        // TODO requesting 5 for now, will later build caching mechnaism to exhaust
        // 5 refs first before asking for more
        // same with release.
        obj.AddInParamAsInt(5);
        obj.AddInParamAsInt(0); // private refs = 0

        obj.AddOutParamAsType(typeof(short)); // size
        obj.AddOutParamAsType(typeof(int)); // Hresult for size
        Log.Logger.Warning("addRef: Adding 5 references for " + _ptr.IPID + " session: " +
            _session.SessionIdentifier);

        // <see cref="Session"/>.debug_addIpids(_ptr.IPID, 5);

        //        session.getStub2().addRef_ReleaseRef(obj);
        _session.AddRef_ReleaseRef(_ptr.IPID, obj, 5);

        if (obj.GetResultAsIntAt(1) != 0) {
            throw new InteropException(obj.GetResultAsIntAt(1), (Exception)null);
        }
    }

    /// <inheritdoc/>
    public void Release() {
        CheckLocal();
        var obj = new CallBuilder(true) {
            ParentIpid = _ptr.IPID,
            Opnum = 2 // release
        };
        // length
        obj.AddInParamAsShort(1);
        // ipid to addfref on
        var array = new ComArray(new UUID[] { new UUID(_ptr.IPID) }, true);
        obj.AddInParamAsArray(array);
        // TODO requesting 5 for now, will later build caching mechnaism to exhaust 5 refs first before asking for more
        // same with release.
        obj.AddInParamAsInt(5);
        obj.AddInParamAsInt(0); // private refs = 0
        if (Log.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information)) {
            Log.Logger.Warning("RELEASE called directly ! removing 5 references for " + _ptr.IPID + " session: " + _session.SessionIdentifier);
            // <see cref="Session"/>.debug_delIpids(_ptr.IPID, 5);
        }
        _session.AddRef_ReleaseRef(_ptr.IPID, obj, -5);
    }

    /// <inheritdoc/>
    public object[] Call(CallBuilder obj) {
        CheckLocal();
        return Call(obj, _timeout);
    }

    /// <inheritdoc/>
    public void RegisterUnreferencedHandler(IUnreferenced unreferenced) {
        CheckLocal();
        _session.RegisterUnreferencedHandler(Ipid, unreferenced);
    }

    /// <inheritdoc/>
    public void UnregisterUnreferencedHandler() {
        CheckLocal();
        _session.UnregisterUnreferencedHandler(Ipid);
    }

    /// <inheritdoc/>
    public object[] Call(CallBuilder obj, int socketTimeout) {
        CheckLocal();
        obj.AttachSession(_session);
        obj.ParentIpid = _ptr.IPID;
        // Call is always made on your stub.
        if (socketTimeout != 0) { // using instance level timeout
            return _session.Stub.Call(obj, _ptr.IID, socketTimeout);
        }
        return _session.Stub.Call(obj, _ptr.IID);
    }

    /// <inheritdoc/>
    public InterfacePointer GetInterfacePointer() => _ptr ?? _session.Stub.ServerInterfacePointer;

    /// <inheritdoc/>
    public string SetConnectionInfo(IComObject connectionPoint, int? cookie) {
        lock (_syncRoot) {
            CheckLocal();
            if (_connectionPointInfo == null) { // lazy creation, since this is used by event callbacks only.
                _connectionPointInfo = new Dictionary<string, object[]>(StringComparer.Ordinal);
            }
            var uniqueId = /*UUID.randomUUID()*/ Guid.NewGuid().ToString();
            _connectionPointInfo[uniqueId] = new object[] { connectionPoint, cookie };
            return uniqueId;
        }
    }

    /// <inheritdoc/>
    public object[] GetConnectionInfo(string identifier) {
        lock (_syncRoot) {
            CheckLocal();
            return _connectionPointInfo[identifier];
        }
    }

    /// <inheritdoc/>
    public object[] RemoveConnectionInfo(string identifier) {
        lock (_syncRoot) {
            CheckLocal();
            var result = _connectionPointInfo[identifier];
            _connectionPointInfo.Remove(identifier);
            return result;
        }
    }

    /// <inheritdoc/>
    public void SetDeffered(bool deffered) => _ptr.Deffered = deffered;

    /// <inheritdoc/>
    public override string ToString() => "ComObject[" + GetInterfacePointer() +
        ", session: " + AssociatedSession.SessionIdentifier +
        ", isLocal: " + LocalReference + "]";

    /// <inheritdoc/>
    public override bool Equals(object obj) {
        if (!(obj is ComObjectImpl other)) {
            return false;
        }
        return _ptr.IPID.Equals(other.Ipid, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Ipid);

    /// <summary>
    /// Replace members
    /// </summary>
    /// <param name="comObject"></param>
    internal void ReplaceMembers(IComObject comObject) {
        _session = comObject.AssociatedSession;
        _ptr = ((IComObjectInternal)comObject).GetInterfacePointer();
    }

    /// <summary>
    /// Check local
    /// </summary>
    private void CheckLocal() {
        if (_session == null) {
            throw new InvalidOperationException(
                Interop.GetLocalizedMessage(ErrorCode.INTEROP_SESSION_NOT_ATTACHED));
        }
        if (LocalReference) {
            throw new InvalidOperationException(
                Interop.GetLocalizedMessage(ErrorCode.E_NOTIMPL));
        }
    }

    private readonly System.Threading.Lock _syncRoot = new();
    private bool _isDual;
    private bool _dualInfo;
    [NonSerialized] private Session _session;
    private InterfacePointer _ptr;
    private Dictionary<string, object[]> _connectionPointInfo;
    private int _timeout;
}

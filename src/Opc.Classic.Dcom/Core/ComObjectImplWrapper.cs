// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using System;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Internal Framework class.
/// </summary>
[Serializable]
public class ComObjectImplWrapper : IComObject, IComObjectInternal {

    /// <summary>
    /// Wrapped Com object
    /// </summary>
    protected IComObject ComObject { get; }

    /// <inheritdoc/>
    public virtual string Ipid => ComObject.Ipid;

    /// <inheritdoc/>
    public virtual Session AssociatedSession =>
        ComObject.AssociatedSession;

    /// <inheritdoc/>
    public virtual string InterfaceIdentifier =>
        ComObject.InterfaceIdentifier;

    /// <inheritdoc/>
    public virtual bool DispatchSupported =>
        ComObject.DispatchSupported;

    /// <inheritdoc/>
    public virtual IUnreferenced UnreferencedHandler =>
        ComObject.UnreferencedHandler;

    /// <inheritdoc/>
    public virtual bool LocalReference =>
        ComObject.LocalReference;

    /// <inheritdoc/>
    public virtual ComCustomMarshallerUnMarshaller CustomObject =>
        ComObject.CustomObject;

    /// <inheritdoc/>
    public virtual int LengthOfInterfacePointer =>
        ComObject.LengthOfInterfacePointer;

    /// <inheritdoc/>
    public virtual int InstanceLevelSocketTimeout {
        get => ComObject.InstanceLevelSocketTimeout;
        set => ComObject.InstanceLevelSocketTimeout = value;
    }

    /// <summary>
    /// Create wrapper
    /// </summary>
    /// <param name="comObject"></param>
    protected internal ComObjectImplWrapper(IComObject comObject) =>
        ComObject = comObject;

    /// <inheritdoc/>
    public virtual IComObject QueryInterface(string iid) =>
        ComObject.QueryInterface(iid);

    /// <inheritdoc/>
    public virtual void AddRef() =>
        ComObject.AddRef();

    /// <inheritdoc/>
    public virtual void Release() =>
        ComObject.Release();

    /// <inheritdoc/>
    public virtual object[] Call(CallBuilder obj) =>
        ComObject.Call(obj);

    /// <inheritdoc/>
    public virtual void RegisterUnreferencedHandler(IUnreferenced unreferenced) =>
        ComObject.RegisterUnreferencedHandler(unreferenced);

    /// <inheritdoc/>
    public virtual void UnregisterUnreferencedHandler() =>
        ComObject.UnregisterUnreferencedHandler();

    /// <inheritdoc/>
    public virtual object[] Call(CallBuilder obj, int timeout) =>
        ComObject.Call(obj, timeout);

    /// <inheritdoc/>
    public virtual InterfacePointer GetInterfacePointer() =>
        ((IComObjectInternal)ComObject).GetInterfacePointer();

    /// <inheritdoc/>
    public virtual string SetConnectionInfo(IComObject connectionPoint, int? cookie) =>
        ((IComObjectInternal)ComObject).SetConnectionInfo(connectionPoint, cookie);

    /// <inheritdoc/>
    public virtual object[] GetConnectionInfo(string identifier) =>
        ((IComObjectInternal)ComObject).GetConnectionInfo(identifier);

    /// <inheritdoc/>
    public virtual object[] RemoveConnectionInfo(string identifier) =>
        ((IComObjectInternal)ComObject).RemoveConnectionInfo(identifier);

    /// <inheritdoc/>
    public virtual void SetDeffered(bool deffered) =>
        ((IComObjectInternal)ComObject).SetDeffered(deffered);

    /// <inheritdoc/>
    public override string ToString() =>
        ComObject.ToString();
}

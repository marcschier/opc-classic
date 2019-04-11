//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using System;

    /// <summary>
    /// Internal Framework class.
    /// </summary>
    [Serializable]
    public class JIComObjectImplWrapper : IComObject {

        /// <summary>
        /// Wrapped Com object
        /// </summary>
        protected IComObject ComObject { get; }

        /// <inheritdoc/>
        public virtual string Ipid => ComObject.Ipid;

        /// <inheritdoc/>
        public virtual JISession AssociatedSession =>
            ComObject.AssociatedSession;

        /// <inheritdoc/>
        public virtual string InterfaceIdentifier =>
            ComObject.InterfaceIdentifier;

        /// <inheritdoc/>
        public virtual bool DispatchSupported =>
            ComObject.DispatchSupported;

        /// <inheritdoc/>
        public virtual IJIUnreferenced UnreferencedHandler =>
            ComObject.UnreferencedHandler;

        /// <inheritdoc/>
        public virtual bool LocalReference =>
            ComObject.LocalReference;

        /// <inheritdoc/>
        public virtual JIComCustomMarshallerUnMarshaller CustomObject =>
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
        protected internal JIComObjectImplWrapper(IComObject comObject) =>
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
        public virtual object[] Call(JICallBuilder obj) =>
            ComObject.Call(obj);

        /// <inheritdoc/>
        public virtual void RegisterUnreferencedHandler(IJIUnreferenced unreferenced) =>
            ComObject.RegisterUnreferencedHandler(unreferenced);

        /// <inheritdoc/>
        public virtual void UnregisterUnreferencedHandler() =>
            ComObject.UnregisterUnreferencedHandler();

        /// <inheritdoc/>
        public virtual object[] Call(JICallBuilder obj, int timeout) =>
            ComObject.Call(obj, timeout);

        /// <inheritdoc/>
        public virtual JIInterfacePointer Internal_getInterfacePointer() =>
            ComObject.Internal_getInterfacePointer();

        /// <inheritdoc/>
        public virtual string Internal_setConnectionInfo(IComObject connectionPoint, int? cookie) =>
            ComObject.Internal_setConnectionInfo(connectionPoint, cookie);

        /// <inheritdoc/>
        public virtual object[] Internal_getConnectionInfo(string identifier) =>
            ComObject.Internal_getConnectionInfo(identifier);

        /// <inheritdoc/>
        public virtual object[] Internal_removeConnectionInfo(string identifier) =>
            ComObject.Internal_removeConnectionInfo(identifier);

        /// <inheritdoc/>
        public virtual void Internal_setDeffered(bool deffered) =>
            ComObject.Internal_setDeffered(deffered);

        /// <inheritdoc/>
        public override string ToString() =>
            ComObject.ToString();
    }
}
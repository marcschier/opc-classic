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
    public class JIComObjectImplWrapper : IJIComObject {
        /// <summary>
        /// Com object
        /// </summary>
        protected internal readonly IJIComObject comObject;

        /// <summary>
        /// Create wrapper
        /// </summary>
        /// <param name="comObject"></param>
		protected internal JIComObjectImplWrapper(IJIComObject comObject) {
            this.comObject = comObject;
        }

        /// <inheritdoc/>
		public virtual IJIComObject queryInterface(string iid) {

            return comObject.queryInterface(iid);
        }

        /// <inheritdoc/>
		public virtual void addRef() {
            comObject.addRef();
        }

        /// <inheritdoc/>
		public virtual void release() {
            comObject.release();
        }

        /// <inheritdoc/>
        public virtual string Ipid => comObject.Ipid;

        /// <inheritdoc/>
        public virtual object[] call(JICallBuilder obj) {
            return comObject.call(obj);
        }

        /// <inheritdoc/>
        public virtual JIInterfacePointer internal_getInterfacePointer() {
            return comObject.internal_getInterfacePointer();
        }

        /// <inheritdoc/>
        public virtual JISession AssociatedSession => comObject.AssociatedSession;

        /// <inheritdoc/>
        public virtual string InterfaceIdentifier => comObject.InterfaceIdentifier;

        /// <inheritdoc/>
        public virtual bool DispatchSupported => comObject.DispatchSupported;

        /// <inheritdoc/>
        public virtual string internal_setConnectionInfo(IJIComObject connectionPoint, int? cookie) {
            return comObject.internal_setConnectionInfo(connectionPoint, cookie);
        }

        /// <inheritdoc/>
		public virtual object[] internal_getConnectionInfo(string identifier) {
            return comObject.internal_getConnectionInfo(identifier);
        }

        /// <inheritdoc/>
		public virtual object[] internal_removeConnectionInfo(string identifier) {
            return comObject.internal_removeConnectionInfo(identifier);
        }

        /// <inheritdoc/>
        public virtual IJIUnreferenced UnreferencedHandler => comObject.UnreferencedHandler;

        /// <inheritdoc/>
        public virtual void registerUnreferencedHandler(IJIUnreferenced unreferenced) {
            comObject.registerUnreferencedHandler(unreferenced);
        }

        /// <inheritdoc/>
		public virtual void unregisterUnreferencedHandler() {
            comObject.unregisterUnreferencedHandler();
        }

        /// <inheritdoc/>
		public virtual object[] call(JICallBuilder obj, int timeout) {
            return comObject.call(obj, timeout);
        }

        /// <inheritdoc/>
		public virtual int InstanceLevelSocketTimeout {
            get => comObject.InstanceLevelSocketTimeout;
            set => comObject.InstanceLevelSocketTimeout = value;
        }

        /// <inheritdoc/>
        public virtual void internal_setDeffered(bool deffered) {
            comObject.internal_setDeffered(deffered);
        }

        /// <inheritdoc/>
        public virtual bool LocalReference => comObject.LocalReference;

        /// <inheritdoc/>
        public virtual JIComCustomMarshallerUnMarshaller CustomObject => comObject.CustomObject;


        /// <inheritdoc/>
        public virtual int LengthOfInterfacePointer => comObject.LengthOfInterfacePointer;

        /// <inheritdoc/>
        public override string ToString() {
            return comObject.ToString();
        }
    }
}
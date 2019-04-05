// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace rpc {
    using SharpCifs.Dcerpc.Ndr;
    using rpc.core;
    using System;
    using System.IO;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Stub
    /// </summary>
    public abstract class Stub {

        /// <summary>
        /// Address
        /// </summary>
        public virtual string Address {
            get => _address;
            set {
                if ((value == null) ? _address == null : value.Equals(_address)) {
                    return;
                }
                _address = value;
                try {
                    Detach();
                }
                catch (IOException) {
                }
            }
        }

        /// <summary>
        /// Object
        /// </summary>
        public string Object { get; set; }

        /// <summary>
        /// Transport factory
        /// </summary>
        public TransportFactory TransportFactory { get; set; }

        /// <summary>
        /// SharpCifs.Util.Sharpen.Properties
        /// </summary>
        public Properties Properties { get; set; }

        /// <summary>
        /// Endpoint
        /// </summary>
        protected IEndpoint Endpoint { get; set; }

        /// <summary>
        /// Detach
        /// </summary>
        /// <exception cref="IOException"></exception>
        protected void Detach() {
            var endpoint = Endpoint;
            if (endpoint == null) {
                return;
            }
            try {
                endpoint.Detach();
            }
            finally {
                Endpoint = null;
            }
        }

        /// <summary>
        /// Attach
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="T:rpc.RpcException"></exception>
        protected void Attach() {
            var endpoint = Endpoint;
            if (endpoint != null) {
                return;
            }
            var address = Address;
            if (address == null) {
                throw new RpcException("No address specified.");
            }
            Endpoint = TransportFactory.CreateTransport(address, Properties).Attach(new PresentationSyntax(Syntax));
        }

        /// <summary>
        /// call
        /// </summary>
        /// <param name="semantics"></param>
        /// <param name="ndrobj"></param>
        /// <exception cref="IOException"></exception>
        public virtual void Call(int semantics, NdrOp ndrobj) {
            Attach();
            var obj = Object;
            var uuid = (obj == null) ? null : new UUID(obj);
            Endpoint.Call(semantics, uuid, ndrobj.Opnum, ndrobj);
        }

        /// <summary>
        /// Syntax
        /// </summary>
        protected internal abstract string Syntax { get; }

        private string _address;
    }
}
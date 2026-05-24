//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal;
using SharpInterop.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System.IO;

namespace SharpInterop.Rpc; 
/// <summary>
/// Stub
/// </summary>
public abstract class Stub {

    /// <summary>
    /// Address
    /// </summary>
    public string Address {
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
    /// Opc.Classic.Dcom.Internal.PropertyBag
    /// </summary>
    public PropertyBag Properties { get; set; }

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
        var tp = TransportFactory.CreateTransport(address, Properties);
        Endpoint = tp.Attach(new PresentationSyntax(Syntax));
    }

    /// <summary>
    /// Call on the endpoint using the ndr operation
    /// </summary>
    /// <param name="semantics"></param>
    /// <param name="ndrobj"></param>
    /// <exception cref="IOException"></exception>
    public void Call(Semantics semantics, NdrOp ndrobj) {
        Attach();
        var obj = Object;
        var uuid = (obj == null) ? null : new UUID(obj);
        Endpoint.Call(semantics, uuid, ndrobj.Opnum, ndrobj);
    }

    /// <summary>
    /// Syntax
    /// </summary>
    protected abstract string Syntax { get; }

    private string _address;
}

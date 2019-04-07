/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>



namespace rpc {


	using NdrObject = ndr.NdrObject;
	using PresentationSyntax = rpc.core.PresentationSyntax;
	using UUID = rpc.core.UUID;

	public abstract class Stub {

		private TransportFactory TransportFactory_Renamed;

		private Endpoint Endpoint_Renamed;

		private string @object;

		private string Address_Renamed;

		private Properties Properties_Renamed;

		public virtual string Address {
			get {
				return Address_Renamed;
			}
			set {
				if ((value == null) ? this.Address_Renamed == null : value.Equals(this.Address_Renamed)) {
					return;
				}
				this.Address_Renamed = value;
				try {
					Detach();
				}
				catch (IOException) {
				}
			}
		}


		public virtual string Object {
			get {
				return @object;
			}
			set {
				this.@object = value;
			}
		}


		public virtual TransportFactory TransportFactory {
			get {
		//        return (transportFactory != null) ? transportFactory :
		//                (transportFactory = TransportFactory.getInstance());
				return TransportFactory_Renamed; //Will never be null
			}
			set {
				this.TransportFactory_Renamed = value;
			}
		}


		public virtual Properties Properties {
			get {
				return Properties_Renamed;
			}
			set {
				this.Properties_Renamed = value;
			}
		}


		public virtual Endpoint Endpoint {
			get {
				return Endpoint_Renamed;
			}
			set {
				this.Endpoint_Renamed = value;
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void detach() throws java.io.IOException
		public virtual void Detach() {
			Endpoint endpoint = Endpoint;
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void attach() throws java.io.IOException
		public virtual void Attach() {
			Endpoint endpoint = Endpoint;
			if (endpoint != null) {
				return;
			}
			string address = Address;
			if (address == null) {
				throw new RpcException("No address specified.");
			}
			Endpoint = TransportFactory.CreateTransport(address, Properties).Attach(new PresentationSyntax(Syntax));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(int semantics, ndr.NdrObject ndrobj) throws java.io.IOException
		public virtual void Call(int semantics, NdrObject ndrobj) {
			Attach();
			string @object = object;
			UUID uuid = (@object == null) ? null : new UUID(@object);
			Endpoint.Call(semantics, uuid, ndrobj.Opnum, ndrobj);
		}

		public abstract string Syntax { get; }

	}

}
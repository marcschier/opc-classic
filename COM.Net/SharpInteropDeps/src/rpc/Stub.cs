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



namespace rpc
{


	using NdrObject = ndr.NdrObject;
	using PresentationSyntax = core.PresentationSyntax;
	using UUID = core.UUID;

	public abstract class Stub
	{

		private TransportFactory transportFactory;

		private Endpoint endpoint;

		private string @object;

		private string address;

		private Properties properties;

		public virtual string Address {
            get => address;
            set {
                if ((value == null) ? address == null : value.Equals(address)) {
                    return;
                }
                address = value;
                try {
                    detach();
                }
                catch (IOException) {
                }
            }
        }


        public virtual string Object {
            get => @object;
            set => @object = value;
        }


        public virtual TransportFactory TransportFactory {
            get =>
                //        return (transportFactory != null) ? transportFactory :
                //                (transportFactory = TransportFactory.getInstance());
                transportFactory; //Will never be null
            set => transportFactory = value;
        }


        public virtual Properties Properties {
            get => properties;
            set => properties = value;
        }


        protected internal virtual Endpoint Endpoint {
            get => endpoint;
            set => endpoint = value;
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected void detach() throws java.io.IOException
        protected internal virtual void detach()
		{
			var endpoint = Endpoint;
			if (endpoint == null)
			{
				return;
			}
			try
			{
				endpoint.detach();
			}
			finally
			{
				Endpoint = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void attach() throws java.io.IOException
		protected internal virtual void attach()
		{
			var endpoint = Endpoint;
			if (endpoint != null)
			{
				return;
			}
			var address = Address;
			if (address == null)
			{
				throw new RpcException("No address specified.");
			}
			Endpoint = TransportFactory.createTransport(address, Properties).attach(new PresentationSyntax(Syntax));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(int semantics, ndr.NdrObject ndrobj) throws java.io.IOException
		public virtual void call(int semantics, NdrObject ndrobj)
		{
			attach();
			string @object = object;
			var uuid = (@object == null) ? null : new UUID(@object);
			Endpoint.call(semantics, uuid, ndrobj.Opnum, ndrobj);
		}

		protected internal abstract string Syntax {get;}

	}

}
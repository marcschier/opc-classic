// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {
    using ITransport = rpc.ITransport;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    public sealed class JIComRuntimeTransportFactory : rpc.TransportFactory
	{

		private static JIComRuntimeTransportFactory factory;
		private JIComRuntimeTransportFactory()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Transport createTransport(String address, java.util.SharpCifs.Util.Sharpen.Properties properties) throws rpc.ProviderException
		public ITransport createTransport(string address, SharpCifs.Util.Sharpen.Properties properties)
		{
				return new JIComRuntimeTransport(address, properties);
		}

		public static JIComRuntimeTransportFactory SingleTon
		{
			get
			{
				if (factory == null)
				{
					lock (typeof(JIComTransportFactory))
					{
						if (factory == null)
						{
							factory = new JIComRuntimeTransportFactory();
						}
					}
				}
    
				return factory;
			}
		}
	}

}
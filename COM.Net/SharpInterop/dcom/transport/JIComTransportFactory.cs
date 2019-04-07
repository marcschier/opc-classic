// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {


    using SelectorManager = niosupport.SelectorManager;
    using ITransport = rpc.ITransport;

    /// <summary>
    /// Factory for <seealso cref="JIComTransport"/>
    /// </summary>
    public sealed class JIComTransportFactory : rpc.TransportFactory
	{
		private static JIComTransportFactory instance;

		private readonly SelectorManager selectorManager;

		/// <summary>
		/// Constructor for JIComTransportFactory.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private JIComTransportFactory() throws java.io.IOException
		private JIComTransportFactory()
		{
			selectorManager = new SelectorManager();
		}

		/// <seealso cref= rpc.TransportFactory#createTransport(java.lang.String,
		///      java.util.SharpCifs.Util.Sharpen.Properties) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Transport createTransport(String address, java.util.SharpCifs.Util.Sharpen.Properties properties) throws rpc.ProviderException
		public ITransport createTransport(string address, SharpCifs.Util.Sharpen.Properties properties)
		{
			return new JIComTransport(address, selectorManager, properties);
		}

		/// <returns> the singleton instance </returns>
		public static JIComTransportFactory Singleton
		{
			get
			{
				lock (typeof(JIComTransportFactory))
				{
					if (instance == null)
					{
						try
						{
							instance = new JIComTransportFactory();
						}
						catch (IOException e)
						{
							throw new ExceptionInInitializerError(e);
						}
					}
					return instance;
				}
			}
		}

        public static JIComTransportFactory SingleTon => Singleton;
    }

}
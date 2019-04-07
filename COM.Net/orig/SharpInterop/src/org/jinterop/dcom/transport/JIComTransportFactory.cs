/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.transport {


	using SelectorManager = org.jinterop.dcom.transport.niosupport.SelectorManager;

	using ProviderException = rpc.ProviderException;
	using Transport = rpc.Transport;

	/// <summary>
	/// Factory for <seealso cref="JIComTransport"/>
	/// </summary>
	public sealed class JIComTransportFactory : rpc.TransportFactory {
		private static JIComTransportFactory Instance;

		private readonly SelectorManager SelectorManager;

		/// <summary>
		/// Constructor for JIComTransportFactory.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private JIComTransportFactory() throws java.io.IOException
		private JIComTransportFactory() {
			SelectorManager = new SelectorManager();
		}

		/// <seealso cref= rpc.TransportFactory#createTransport(java.lang.String,
		///      java.util.Properties) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Transport createTransport(String address, java.util.Properties properties) throws rpc.ProviderException
		public Transport CreateTransport(string address, Properties properties) {
			return new JIComTransport(address, SelectorManager, properties);
		}

		/// <returns> the singleton instance </returns>
		public static JIComTransportFactory Singleton {
			get {
				lock (typeof(JIComTransportFactory)) {
					if (Instance == null) {
						try {
							Instance = new JIComTransportFactory();
						}
						catch (IOException e) {
							throw new ExceptionInInitializerError(e);
						}
					}
					return Instance;
				}
			}
		}

		public static JIComTransportFactory SingleTon {
			get {
				return Singleton;
			}
		}
	}

}
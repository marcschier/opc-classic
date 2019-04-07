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

	using ProviderException = rpc.ProviderException;
	using Transport = rpc.Transport;

	/// <summary>
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	public sealed class JIComRuntimeTransportFactory : rpc.TransportFactory {

		private static JIComRuntimeTransportFactory Factory = null;
		private JIComRuntimeTransportFactory() {
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Transport createTransport(String address, java.util.Properties properties) throws rpc.ProviderException
		public Transport CreateTransport(string address, Properties properties) {
				return new JIComRuntimeTransport(address, properties);
		}

		public static JIComRuntimeTransportFactory SingleTon {
			get {
				if (Factory == null) {
					lock (typeof(JIComTransportFactory)) {
						if (Factory == null) {
							Factory = new JIComRuntimeTransportFactory();
						}
					}
				}
    
				return Factory;
			}
		}
	}

}
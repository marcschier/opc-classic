using System;
using System.Collections;

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



namespace rpc.security.ntlm {


	using Type1Message = jcifs.ntlmssp.Type1Message;
	using Type2Message = jcifs.ntlmssp.Type2Message;
	using Type3Message = jcifs.ntlmssp.Type3Message;

	public abstract class AuthenticationSource {

		private static readonly AuthenticationSource INSTANCE;

		static AuthenticationSource() {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string service = "META-INF/services/" + typeof(AuthenticationSource).FullName;
			URL location = null;
			ClassLoader loader = typeof(AuthenticationSource).ClassLoader;
			if (loader != null) {
				location = loader.getResource(service);
			}
			if (location == null) {
				location = ClassLoader.getSystemResource(service);
			}
			AuthenticationSource instance = null;
			if (location != null) {
				try {
					Properties properties = new Properties();
					properties.load(location.openStream());
					System.Collections.IEnumerator classNames = properties.propertyNames();
					if (classNames.hasMoreElements()) {
						Type sourceClass = Type.GetType((string) classNames.nextElement());
						instance = (AuthenticationSource) sourceClass.newInstance();
					}
				}
				catch (Exception ex) {
					Console.Error.WriteLine("WARNING: Unable to instantiate source.");
					Console.WriteLine(ex.ToString());
					Console.Write(ex.StackTrace);
				}
			}
			INSTANCE = instance;
		}

		public static AuthenticationSource DefaultInstance {
			get {
				return INSTANCE;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract byte[] createChallenge(java.util.Properties properties, jcifs.ntlmssp.Type1Message type1) throws java.io.IOException;
		public abstract sbyte[] CreateChallenge(Properties properties, Type1Message type1);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract byte[] authenticate(java.util.Properties properties, jcifs.ntlmssp.Type2Message type2, jcifs.ntlmssp.Type3Message type3) throws java.io.IOException;
		public abstract sbyte[] Authenticate(Properties properties, Type2Message type2, Type3Message type3);

	}

}
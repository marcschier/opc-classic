//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Auth.ntlm {
    using SharpCifs.Ntlmssp;
    using SharpCifs.Util.Sharpen;
    using System.IO;

    /// <summary>
    /// Auth source
    /// </summary>
    public abstract class AuthenticationSource {

        /// <summary>
        /// Initialize source
        /// </summary>
        static AuthenticationSource() {
            // JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:

            // TODO
            // TODO
            // TODO

          // var service = "META-INF/services/" + typeof(AuthenticationSource).FullName;
          // URL location = null;
          // ClassLoader loader = typeof(AuthenticationSource).ClassLoader;
          // if (loader != null) {
          //     location = loader.getResource(service);
          // }
          // if (location == null) {
          //     location = ClassLoader.getSystemResource(service);
          // }
          AuthenticationSource instance = null;
          // if (location != null) {
          //     try {
          //         var properties = new Properties();
          //         properties.load(location.openStream());
          //         IEnumerator classNames = properties.propertyNames();
          //         if (classNames.hasMoreElements()) {
          //             var sourceClass = Type.GetType((string)classNames.nextElement());
          //             instance = (AuthenticationSource)sourceClass.newInstance();
          //         }
          //     }
          //     catch (Exception ex) {
          //         Console.Error.WriteLine("WARNING: Unable to instantiate source.");
          //         Console.WriteLine(ex.ToString());
          //         Console.Write(ex.StackTrace);
          //     }
          // }
            DefaultInstance = instance;
        }


        /// <summary>
        /// Default
        /// </summary>
        public static AuthenticationSource DefaultInstance { get; private set; }

        /// <summary>
        /// Create challenge
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="type1"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public abstract byte[] CreateChallenge(Properties properties,
            Type1Message type1);

        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="type2"></param>
        /// <param name="type3"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public abstract sbyte[] Authenticate(Properties properties,
            Type2Message type2, Type3Message type3);
    }
}
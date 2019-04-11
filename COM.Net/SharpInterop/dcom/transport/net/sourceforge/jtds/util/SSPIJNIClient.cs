using System;
using System.Runtime.InteropServices;

// jTDS JDBC Driver for Microsoft SQL Server and Sybase
// Copyright (C) 2004 The jTDS Project
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307    USA
//
namespace net.sourceforge.jtds.util {

    /// <summary>
    /// COPIED FROM jtds PROJECT FOR SSO CAPABILITIES.
    /// returns the user credentials for NTLM authentication.
    /// </summary>
    public class SSPIJNIClient {

        /// <summary>
        /// Singleton instance. </summary>
        private static SSPIJNIClient _thisInstance;

        /// <summary>
        /// SSPI native library loaded flag. 
        /// </summary>
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0169  // Add readonly modifier
        private static bool _libraryLoaded;
#pragma warning restore CS0169  // Add readonly modifier
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// SSPI client initialized flag. 
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Initializes the SSPI client. 
        /// </summary>
        [DllImport("ntlmauth.dll")]
        private static extern void Initialize();

        /// <summary>
        /// Uninitializes the SSPI client. 
        /// </summary>
        [DllImport("ntlmauth.dll")]
        private static extern void UnInitialize();

        /// <summary>
        /// Prepares the NTLM TYPE-1 message and returns it as a
        /// <code>byte[]</code>.
        /// </summary>
        [DllImport("ntlmauth.dll")]
        private static extern byte[] PrepareSSORequest();

        /// <summary>
        /// Prepares the NTLM TYPE-3 message using the current user's credentials.
        /// It needs the challenge BLOB and it's size as input. The challenge BLOB
        /// is nothig but the TYPE-2 message that is received 
        /// </summary>
        /// <param name="buf">  challenge BLOB </param>
        /// <param name="size"> challenge BLOB size </param>
        /// <returns> NTLM TYPE-3 message </returns>
        [DllImport("ntlmauth.dll")]
        private static extern byte[] PrepareSSOSubmit(byte[] buf, long size);

        /// <summary>
        /// Private constructor for singleton.
        /// </summary>
        private SSPIJNIClient() {
          //  try {
          //      if (System.getProperty("os.name").ToLower().StartsWith("windows", StringComparison.Ordinal)) {
          //          //JAVA TO C# CONVERTER TODO TASK: The library is specified in the 'DllImport' attribute for .NET:
          //          //					System.loadLibrary("ntlmauth");
          //          libraryLoaded = true;
          //      }
          //      else {
          //          throw new ArgumentException("This functionality is available only under \"Microsoft Windows\" line of Operating systems.");
          //      }
          //  }
          //  catch (UnsatisfiedLinkError err) {
          //      Logger.getLogger("org.jinterop").severe("Unable to load library: " + err);
          //      throw new InvalidOperationException("Native SSPI library not loaded. " + "Check the java.library.path system property." + "This functionality is available only under \"Microsoft Windows\" line of Operating systems.");
          //
          //  }
        }

        /// <summary>
        /// Returns the singleton <code>SSPIJNIClient</code> instance.
        /// </summary>
        public static SSPIJNIClient Instance {
            get {

                if (_thisInstance == null) {
                    //            if (!libraryLoaded) {
                    //                throw new System.InvalidOperationException("Native SSPI library not loaded. "
                    //                        + "Check the java.library.path system property."
                    //                        + "This functionality is available only under \"Microsoft Windows\" line of Operating systems.");
                    //            }
                    _thisInstance = new SSPIJNIClient();
                    _thisInstance.InvokeInitialize();
                }
                return _thisInstance;
            }
        }

        /// <summary>
        /// Calls <code>#initialize()</code> if the SSPI client is not already inited.
        /// </summary>
        public virtual void InvokeInitialize() {
            if (!_initialized) {
                Initialize();
                _initialized = true;
            }
        }

        /// <summary>
        /// Calls <code>#unInitialize()</code> if the SSPI client is inited.
        /// </summary>
        public virtual void InvokeUnInitialize() {
            if (_initialized) {
                UnInitialize();
                _initialized = false;
            }
        }

        /// <summary>
        /// Calls <code>#prepareSSORequest()</code> to prepare the NTLM TYPE-1 message.
        /// </summary>
        /// <exception cref="Exception"> if an error occurs during the call or the SSPI client
        ///                   is uninitialized </exception>
        public virtual byte[] InvokePrepareSSORequest() {
            if (!_initialized) {
                throw new InvalidOperationException("SSPI Not Initialized");
            }
            return PrepareSSORequest();
        }

        /// <summary>
        /// Calls <code>#prepareSSOSubmit(byte[], long)</code> to prepare the NTLM TYPE-3
        /// message.
        /// </summary>
        /// <exception cref="Exception"> if an error occurs during the call or the SSPI client
        ///                   is uninitialized </exception>
        public virtual byte[] InvokePrepareSSOSubmit(byte[] buf) {
            if (!_initialized) {
                throw new InvalidOperationException("SSPI Not Initialized");
            }
            return PrepareSSOSubmit(buf, buf.Length);
        }
    }
}
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
    /// 
    /// A JNI client to SSPI based CPP program (DLL) that returns the user
    /// credentials for NTLM authentication.
    /// <p/>
    /// The DLL name is ntlmauth.dll.
    /// 
    /// @author Magendran Sathaiah (mahi@aztec.soft.net)
    /// </summary>
    public class SSPIJNIClient {
        /// <summary>
        /// Singleton instance. </summary>
        private static SSPIJNIClient ThisInstance;

        /// <summary>
        /// SSPI native library loaded flag. </summary>
        private static bool LibraryLoaded;

        /// <summary>
        /// SSPI client initialized flag. </summary>
        private bool Initialized;

        /// <summary>
        /// Initializes the SSPI client. </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void initialize();

        /// <summary>
        /// Uninitializes the SSPI client. </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void unInitialize();

        /// <summary>
        /// Prepares the NTLM TYPE-1 message and returns it as a
        /// <code>byte[]</code>.
        /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern byte[] prepareSSORequest();

        /// <summary>
        /// Prepares the NTLM TYPE-3 message using the current user's credentials.
        /// <para>
        /// It needs the challenge BLOB and it's size as input. The challenge BLOB
        /// is nothig but the TYPE-2 message that is received from the SQL Server.
        /// 
        /// </para>
        /// </summary>
        /// <param name="buf">  challenge BLOB </param>
        /// <param name="size"> challenge BLOB size </param>
        /// <returns> NTLM TYPE-3 message </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern byte[] prepareSSOSubmit(sbyte[] buf, long size);


        /// <summary>
        /// Private constructor for singleton.
        /// </summary>
        private SSPIJNIClient() {
            try {
                if (System.getProperty("os.name").ToLower().StartsWith("windows", StringComparison.Ordinal)) {
//JAVA TO C# CONVERTER TODO TASK: The library is specified in the 'DllImport' attribute for .NET:
//                    System.loadLibrary("ntlmauth");
                    SSPIJNIClient.LibraryLoaded = true;
                }
                else {
                    throw new System.ArgumentException("This functionality is available only under \"Microsoft Windows\" line of Operating systems.");
                }
            }
            catch (UnsatisfiedLinkError err) {
                Logger.getLogger("org.jinterop").severe("Unable to load library: " + err);
                throw new System.InvalidOperationException("Native SSPI library not loaded. " + "Check the java.library.path system property." + "This functionality is available only under \"Microsoft Windows\" line of Operating systems.");

            }
        }

        /// <summary>
        /// Returns the singleton <code>SSPIJNIClient</code> instance.
        /// </summary>
        /// <exception cref="SQLException"> if an error occurs during initialization </exception>
        public static SSPIJNIClient Instance {
            get {
    
                if (ThisInstance == null) {
        //            if (!libraryLoaded) {
        //                throw new IllegalStateException("Native SSPI library not loaded. "
        //                        + "Check the java.library.path system property."
        //                        + "This functionality is available only under \"Microsoft Windows\" line of Operating systems.");
        //            }
                    ThisInstance = new SSPIJNIClient();
                    ThisInstance.InvokeInitialize();
                }
                return ThisInstance;
            }
        }

        /// <summary>
        /// Calls <code>#initialize()</code> if the SSPI client is not already inited.
        /// </summary>
        public virtual void InvokeInitialize() {
            if (!Initialized) {
                initialize();
                Initialized = true;
            }
        }

        /// <summary>
        /// Calls <code>#unInitialize()</code> if the SSPI client is inited.
        /// </summary>
        public virtual void InvokeUnInitialize() {
            if (Initialized) {
                unInitialize();
                Initialized = false;
            }
        }

        /// <summary>
        /// Calls <code>#prepareSSORequest()</code> to prepare the NTLM TYPE-1 message.
        /// </summary>
        /// <exception cref="Exception"> if an error occurs during the call or the SSPI client
        ///                   is uninitialized </exception>
        public virtual sbyte[] InvokePrepareSSORequest() {
            if (!Initialized) {
                throw new System.InvalidOperationException("SSPI Not Initialized");
            }
            return prepareSSORequest();
        }

        /// <summary>
        /// Calls <code>#prepareSSOSubmit(byte[], long)</code> to prepare the NTLM TYPE-3
        /// message.
        /// </summary>
        /// <exception cref="Exception"> if an error occurs during the call or the SSPI client
        ///                   is uninitialized </exception>
        public virtual sbyte[] InvokePrepareSSOSubmit(sbyte[] buf) {
            if (!Initialized) {
                throw new System.InvalidOperationException("SSPI Not Initialized");
            }
            return prepareSSOSubmit(buf, buf.Length);
        }
    }

}
using System;

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


    using NdrBuffer = ndr.NdrBuffer;

    using JISystem = org.jinterop.dcom.common.JISystem;

    using Endpoint = rpc.Endpoint;
    using ProviderException = rpc.ProviderException;
    using RpcException = rpc.RpcException;
    using Transport = rpc.Transport;
    using PresentationSyntax = rpc.core.PresentationSyntax;
    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    internal sealed class JIComRuntimeTransport : Transport {


        public const string PROTOCOL = "ncacn_ip_tcp";

        private Properties Properties_Renamed;


        private Socket Socket;

        private System.IO.Stream Output;

        private System.IO.Stream Input;

        private bool Attached;


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComRuntimeTransport(String address, java.util.Properties properties) throws rpc.ProviderException
        public JIComRuntimeTransport(string address, Properties properties) {
            this.Properties_Renamed = properties;
            //address is ignored
        }

        public string Protocol {
            get {
                return PROTOCOL;
            }
        }

        public Properties Properties {
            get {
                return Properties_Renamed;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Endpoint attach(rpc.core.PresentationSyntax syntax) throws java.io.IOException
        public Endpoint Attach(PresentationSyntax syntax) {
            if (Attached) {
                throw new RpcException("Transport already attached.");
            }

            Endpoint endPoint = null;
            try {
                Socket = (Socket)JISystem.Internal_getSocket();
                Output = null;
                Input = null;
                Attached = true;
                endPoint = new JIComRuntimeEndpoint(this, syntax);
            }
            catch (Exception) {
                try {
                    Close();
                }
                catch (Exception) {
                }
            }
            return endPoint;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
        public void Close() {
            try {
                if (Socket != null) {
                    Socket.close();
                }
            }
            finally {
                Attached = false;
                Socket = null;
                Output = null;
                Input = null;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void send(ndr.NdrBuffer buffer) throws java.io.IOException
        public void Send(NdrBuffer buffer) {
            if (!Attached) {
                throw new RpcException("Transport not attached.");
            }
            if (Output == null) {
                Output = Socket.OutputStream;
            }
            Output.Write(buffer.Buffer, 0, buffer.Length);
            Output.Flush();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void receive(ndr.NdrBuffer buffer) throws java.io.IOException
        public void Receive(NdrBuffer buffer) {
            if (!Attached) {
                throw new RpcException("Transport not attached.");
            }
            if (Input == null) {
                Input = Socket.InputStream;
            }
            buffer.length = (Input.Read(buffer.Buffer, 0, buffer.Capacity));
        }


    }

}
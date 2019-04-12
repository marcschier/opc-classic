//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//


namespace SharpInterop.Test {
    using SharpInterop.Common;
    using SharpInterop.Core;
    using SharpInterop;
    using SharpInterop.Automation;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    public class Program {

        public void Execute(ComString str) => Console.WriteLine(str.String);
        /// <param name="args"> </param>
        public static void RunTest(string[] args) {

            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }

            try {
                var domain = args[1];
                var username = args[2];
                var password = args[3];

                Interop.UseAutoRegistration = true;
                var session3 = Session.CreateSession(domain, username, password);
                session3.UseSessionSecurity(true);
                var virtualServer = new ComServer(ProgId.ValueOf("VirtualServer.Application"), args[0], session3);
                var unkVirtualServer = virtualServer.CreateInstance();
                var dispatchVirtualServer = (IDispatch)ObjectFactory.NarrowObject(unkVirtualServer.QueryInterface(Interfaces.IID_IDispatch));
            }
            catch (UnknownHostException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (InteropException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (IOException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
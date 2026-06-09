// SPDX-License-Identifier: MIT


namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
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

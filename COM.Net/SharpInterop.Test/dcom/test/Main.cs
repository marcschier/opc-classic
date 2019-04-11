/*
 * Main.java
 *
 * Created on 20 ������ 2007 �., 14:47
 *
 * To change this template, choose Tools | Template Manager
 * and open the template in the editor.
 */

namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    public class Program {

        public void Execute(JIString str) => Console.WriteLine(str.String);
        /// <param name="args"> </param>
        public static void Main(string[] args) {

            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }

            try {
                var domain = args[1];
                var username = args[2];
                var password = args[3];

                JISystem.UseAutoRegistration = true;
                var session3 = JISession.CreateSession(domain, username, password);
                session3.UseSessionSecurity(true);
                var virtualServer = new JIComServer(JIProgId.ValueOf("VirtualServer.Application"), args[0], session3);
                var unkVirtualServer = virtualServer.CreateInstance();
                var dispatchVirtualServer = (IJIDispatch)JIObjectFactory.NarrowObject(unkVirtualServer.QueryInterface(Interfaces.IID_IDispatch));
            }
            catch (UnknownHostException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (JIException e) {
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
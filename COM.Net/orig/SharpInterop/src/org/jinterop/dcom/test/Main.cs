using System;

/*
 * Main.java
 *
 * Created on 20 ������ 2007 �., 14:47
 *
 * To change this template, choose Tools | Template Manager
 * and open the template in the editor.
 */

namespace org.jinterop.dcom.test {


    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class Main {

        public virtual void Execute(JIString str) {
            Console.WriteLine(str.String);
        }
        /// <param name="args"> </param>
        public static void Main(string[] args) {

            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }



            try {

                string domain = args[1];
                string username = args[2];
                string password = args[3];

                JISystem.Logger.Level = Level.FINEST;
                JISystem.InBuiltLogHandler = false;
                JISystem.AutoRegisteration = true;
                JISession session3 = JISession.CreateSession(domain,username,password);
                session3.UseSessionSecurity(true);
                JIComServer virtualServer = new JIComServer(JIProgId.ValueOf("VirtualServer.Application"),args[0],session3);
                IJIComObject unkVirtualServer = virtualServer.CreateInstance();
                IJIDispatch dispatchVirtualServer = (IJIDispatch)JIObjectFactory.NarrowObject(unkVirtualServer.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));



            }
            catch (UnknownHostException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (JIException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (SecurityException e) {
                // TODO Auto-generated catch block
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
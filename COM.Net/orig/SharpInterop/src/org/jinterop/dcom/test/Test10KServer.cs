using System;
using System.Threading;

namespace org.jinterop.dcom.test {



    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class Test10KServer {

        private JIComServer ComStub = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;


        public static void Main(string[] args) {

            try {

                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    JISystem.InBuiltLogHandler = false;
                    JISystem.AutoRegisteration = true;
                    for (int i = 0;i < 10000;++i) {

                        JISession session = JISession.CreateSession(args[1],args[2],args[3]);
                        JIComServer comServer = new JIComServer(JIProgId.ValueOf("MSMQ.MSMQQueueInfo"),args[0],session);
                        IJIComObject unknown = comServer.CreateInstance();
                        IJIDispatch dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
                        //JISession.destroySession(session);
                        Thread.Sleep(150);
                        if (i % 100 == 0) {
                            Console.WriteLine(("").valueOf(i));
                        }
                        System.gc();
                    }

            }
            catch (Exception e) {
                    // TODO Auto-generated catch block
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
            }
        }





    }

}
namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class Test10KServer {

        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                JISystem.UseAutoRegistration = true;
                for (var i = 0; i < 10000; ++i) {
                    var session = JISession.CreateSession(args[1], args[2], args[3]);
                    var comServer = new JIComServer(JIProgId.ValueOf("MSMQ.MSMQQueueInfo"), args[0], session);
                    var unknown = comServer.CreateInstance();
                    var dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(Interfaces.IID_IDispatch));
                    //JISession.destroySession(session);
                    Thread.Sleep(150);
                    if (i % 100 == 0) {
                        Console.WriteLine(i);
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
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
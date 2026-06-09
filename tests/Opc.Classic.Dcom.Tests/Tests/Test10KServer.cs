// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public static class Test10KServer {

        public static void RunTest(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                Interop.UseAutoRegistration = true;
                for (var i = 0; i < 10000; ++i) {
                    var session = Session.CreateSession(args[1], args[2], args[3]);
                    var comServer = new ComServer(ProgId.ValueOf("MSMQ.MSMQQueueInfo"), args[0], session);
                    var unknown = comServer.CreateInstance();
                    var dispatch = (IDispatch)ObjectFactory.NarrowObject(unknown.QueryInterface(Interfaces.IID_IDispatch));
                    // <see cref="Session"/>.destroySession(session);
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

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
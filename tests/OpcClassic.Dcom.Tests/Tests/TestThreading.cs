//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using Serilog;
    using SharpCifs.Util.Sharpen;
    using System;
    using SharpInterop.Automation;
    using SharpInterop.Common;
    using SharpInterop.Core;

    public class TestThreading {

        internal const string Domain = "fdgnt";
        internal const string User = "roopchand";
        internal const string Password = "QweQwe007";
        internal const string Host = "estroopchandnb";

        internal const string ComServerName = "WbemScripting.SWbemLocator";
        internal const string ComObjectId = "76A6415B-CB41-11d1-8B02-00600806D9B6";


        internal const int TotalLoops = 500;
        internal const int NumThreads = 25;
        internal static int _loopsPerThread;
        internal const int WaitForThreadssleepTime = 1000;

        static TestThreading() => _loopsPerThread = TotalLoops / NumThreads;

        public void SetUp() => Interop.UseAutoRegistration = true;

        public void Test() {
            var group = new ThreadGroup("Test Threading Group");
            var threads = new Thread[NumThreads];
            for (var i = 0; i < NumThreads; i++) {
                threads[i] = new TestThread(group, "TestThread: " + i);
            }

            for (var i = 0; i < NumThreads; i++) {
                threads[i].Start();
                // log.info( "activeCount: "+ group.activeCount() );
                // group.list();
            }

            var keepSleeping = true;
            while (keepSleeping) {
                try {
                    for (var i = 0; i < threads.Length; i++) {
                        var thread = threads[i];
                        thread.Join();
                    }
                }
                catch (OperationCanceledException e) {
                    Log.Logger.Error(e, "InterruptedException caught");
                }

                break;
                /*
                bool threadsRunning = false;
                int aliveCount = 0;
                for ( int i = 0; i < threads.length; i++ ) {
                    Thread thread = threads[ i ];
                    if ( thread.isAlive() ) {
                        aliveCount++;
                        threadsRunning = true;
                        // break;
                    }
                }
                log.info( "threadsRunning: "+ threadsRunning +" aliveCount: "+ aliveCount );
                if ( threadsRunning == false ) {
                    keepSleeping = false;
                    break;
                }
                */
            }
        }

        public class TestThread : Thread {
            public TestThread(ThreadGroup group, string name) : base(group, name) {
            }
            public override void Run() {
                for (var i = 0; i < _loopsPerThread; i++) {
                    DoStuff();
                }
            }

            public void DoStuff() {

                try {
                    var session = Session.CreateSession(Domain, User, Password);

                    // this.session.setGlobalSocketTimeout( 60000 );

                    // by name, requires local access (for registry search), or a populated progIdVsClsidDB.properties
                    var progId = ProgId.ValueOf(ComServerName);

                    var baseComServer = new ComServer(progId, Host, session);

                    // Do it by clsid
                    // Clsid clsid = Clsid.valueOf( "76A6415B-CB41-11d1-8B02-00600806D9B6" );
                    // clsid.setAutoRegistration( true );
                    // baseComServer = new <see cref="ComServer"/>( clsid, host, session );

                    // I'm not really sure what the deal is with this
                    // Create an intermediary instance?
                    var unknown = baseComServer.CreateInstance();

                    var baseComObject = unknown.QueryInterface(ComObjectId);

                    var baseDispatch = (IDispatch)ObjectFactory.NarrowObject(baseComObject.QueryInterface(Interfaces.IID_IDispatch));

                    var connectServer = baseDispatch.CallMethodA("ConnectServer", new object[] {
                        new ComString(Host), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(),
                        Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(),
                        0, Variant.CreateOPTIONAL_PARAM() })[0];

                    Session.DestroySession(session);
                    Console.WriteLine("doStuff() run complete");
                }
                catch (Exception e) {
                    Log.Logger.Error(e, "Caught exception: ");
                }
            }
        }


#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public static void RunTest(string[] args) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter
            var test = new TestThreading();
            test.SetUp();
            test.Test();
        }
    }

}
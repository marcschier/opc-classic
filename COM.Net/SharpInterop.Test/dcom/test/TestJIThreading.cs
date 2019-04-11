namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.impls.automation;
    using Serilog;
    using SharpCifs.Util.Sharpen;
    using System;
    using IJIDispatch = impls.automation.IJIDispatch;
    using JIComServer = core.JIComServer;
    using JIObjectFactory = impls.JIObjectFactory;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JISystem = common.JISystem;
    using JIVariant = core.JIVariant;

    public class TestJIThreading {

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

        static TestJIThreading() => _loopsPerThread = TotalLoops / NumThreads;

        public void SetUp() => JISystem.UseAutoRegistration = true;

        public void TestThreading() {
            var group = new ThreadGroup("JIThreading Group");
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
                    var session = JISession.CreateSession(Domain, User, Password);

                    // this.session.setGlobalSocketTimeout( 60000 );

                    // by name, requires local access (for registry search), or a populated progIdVsClsidDB.properties
                    var progId = JIProgId.ValueOf(ComServerName);

                    var baseComServer = new JIComServer(progId, Host, session);

                    // Do it by clsid
                    // JIClsid clsid = JIClsid.valueOf( "76A6415B-CB41-11d1-8B02-00600806D9B6" );
                    // clsid.setAutoRegistration( true );
                    // baseComServer = new JIComServer( clsid, host, session );

                    // I'm not really sure what the deal is with this
                    // Create an intermediary instance?
                    var unknown = baseComServer.CreateInstance();

                    var baseComObject = unknown.QueryInterface(ComObjectId);

                    var baseDispatch = (IJIDispatch)JIObjectFactory.NarrowObject(baseComObject.QueryInterface(Interfaces.IID_IDispatch));

                    var connectServer = baseDispatch.CallMethodA("ConnectServer", new object[] {
                        new JIString(Host), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),
                        JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),
                        0, JIVariant.CreateOPTIONAL_PARAM() })[0];

                    JISession.DestroySession(session);
                    Console.WriteLine("doStuff() run complete");
                }
                catch (Exception e) {
                    Log.Logger.Error(e, "Caught exception: ");
                }
            }
        }


#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args) {
#pragma warning restore IDE0060 // Remove unused parameter
            var testJIThreading = new TestJIThreading();
            testJIThreading.SetUp();
            testJIThreading.TestThreading();
        }
    }

}
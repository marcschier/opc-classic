using System;
using System.Threading;

namespace org.jinterop.dcom.test {


    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class TestJIThreading {

        internal const string Domain = "fdgnt";
        internal const string User = "roopchand";
        internal const string Password = "QweQwe007";
        internal const string Host = "estroopchandnb";

        internal const string ComServerName = "WbemScripting.SWbemLocator";
        internal const string ComObjectId = "76A6415B-CB41-11d1-8B02-00600806D9B6";


        internal const int TotalLoops = 500;
        internal const int NumThreads = 25;
        internal static int LoopsPerThread;
        internal const int WaitForThreadssleepTime = 1000;

        static TestJIThreading() {
            LoopsPerThread = TotalLoops / NumThreads;
        }

        public virtual void SetUp() {

            try {
                JISystem.InBuiltLogHandler = false;
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
            JISystem.AutoRegisteration = true;
            JISystem.Logger.Level = Level.ALL;
        }

        public virtual void TestThreading() {
            ThreadGroup group = new ThreadGroup("JIThreading Group");
            Thread[] threads = new Thread[NumThreads];
            for (int i = 0; i < NumThreads; i++) {
                threads[i] = new TestThread(group, "TestThread: " + i);
            }

            for (int i = 0; i < NumThreads; i++) {
                threads[i].Start();
                //log.info( "activeCount: "+ group.activeCount() );
                //group.list();
            }

            bool keepSleeping = true;
            while (keepSleeping) {
                try {
                    for (int i = 0; i < threads.Length; i++) {
                        Thread thread = threads[i];
                        thread.Join();
                    }
                }
                catch (InterruptedException e) {
                    JISystem.Logger.log(Level.SEVERE, "InterruptedException caught", e);
                }

                break;
                /*
                boolean threadsRunning = false;
                int aliveCount = 0;
                for ( int i = 0; i < threads.length; i++ ) {
                    Thread thread = threads[ i ];
                    if ( thread.isAlive() ) {
                        aliveCount++;
                        threadsRunning = true;
                        //break;
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

        public class TestThread : System.Threading.Thread {
            public TestThread(ThreadGroup group, string name) : base(group, name) {
            }
            public virtual void Run() {
                for (int i = 0; i < LoopsPerThread; i++) {
                    DoStuff();
                }
            }

            public virtual void DoStuff() {

                try {
                    JISession session = JISession.CreateSession(Domain, User, Password);

                    //this.session.setGlobalSocketTimeout( 60000 );

                    // by name, requires local access (for registry search), or a populated progIdVsClsidDB.properties
                    JIProgId progId = JIProgId.ValueOf(ComServerName);

                    JIComServer baseComServer = new JIComServer(progId, Host, session);

                    // Do it by clsid
                    //JIClsid clsid = JIClsid.valueOf( "76A6415B-CB41-11d1-8B02-00600806D9B6" );
                    //clsid.setAutoRegistration( true );
                    //baseComServer = new JIComServer( clsid, host, session );

                    // I'm not really sure what the deal is with this
                    // Create an intermediary instance?
                    IJIComObject unknown = baseComServer.CreateInstance();

                    IJIComObject baseComObject = (IJIComObject) unknown.QueryInterface(ComObjectId);

                    IJIDispatch baseDispatch = (IJIDispatch) JIObjectFactory.NarrowObject(baseComObject.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));

                    JIVariant connectServer = (JIVariant) baseDispatch.callMethodA("ConnectServer", new object[] { new JIString(Host)
                                   , JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), new int?(0), JIVariant.OPTIONAL_PARAM()
                })[0];

                    JISession.DestroySession(session);
                    Console.WriteLine("doStuff() run complete");
            }
                public virtual catch (Exception e) {
                    JISystem.Logger.log(Level.SEVERE, "Caught exception: ", e);
                }
        }
    }


        public static void Main(string[] args) {
            TestJIThreading testJIThreading = new TestJIThreading();
            testJIThreading.SetUp();
            testJIThreading.TestThreading();
        }
}

}
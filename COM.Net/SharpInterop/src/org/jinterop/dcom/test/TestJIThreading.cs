namespace org.jinterop.dcom.test {


    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class TestJIThreading
	{

		internal const string domain = "fdgnt";
		internal const string user = "roopchand";
		internal const string password = "QweQwe007";
		internal const string host = "estroopchandnb";

		internal const string comServerName = "WbemScripting.SWbemLocator";
		internal const string comObjectId = "76A6415B-CB41-11d1-8B02-00600806D9B6";


		internal const int totalLoops = 500;
		internal const int numThreads = 25;
		internal static int loopsPerThread;
		internal const int waitForThreadssleepTime = 1000;

		static TestJIThreading()
		{
			loopsPerThread = totalLoops / numThreads;
		}

		public virtual void setUp()
		{

			try
			{
				JISystem.InBuiltLogHandler = false;
			}
			catch (SecurityException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			JISystem.AutoRegisteration = true;
			Log.Logger.Level = Level.ALL;
		}

		public virtual void testThreading()
		{
			var group = new ThreadGroup("JIThreading Group");
			var threads = new Thread[numThreads];
			for (var i = 0; i < numThreads; i++)
			{
				threads[i] = new TestThread(group, "TestThread: " + i);
			}

			for (var i = 0; i < numThreads; i++)
			{
				threads[i].Start();
				//log.info( "activeCount: "+ group.activeCount() );
				//group.list();
			}

			var keepSleeping = true;
			while (keepSleeping)
			{
				try
				{
					for (var i = 0; i < threads.Length; i++)
					{
						var thread = threads[i];
						thread.Join();
					}
				}
				catch (InterruptedException e)
				{
					Log.Logger.log(Level.SEVERE, "InterruptedException caught", e);
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

		public class TestThread : System.Threading.Thread
		{
			public TestThread(ThreadGroup group, string name) : base(group, name)
			{
			}
			public virtual void run()
			{
				for (var i = 0; i < loopsPerThread; i++)
				{
					doStuff();
				}
			}

			public virtual void doStuff()
			{

				try
				{
					var session = JISession.createSession(domain, user, password);

					//this.session.setGlobalSocketTimeout( 60000 );

					// by name, requires local access (for registry search), or a populated progIdVsClsidDB.properties
					var progId = JIProgId.valueOf(comServerName);

					var baseComServer = new JIComServer(progId, host, session);

					// Do it by clsid
					//JIClsid clsid = JIClsid.valueOf( "76A6415B-CB41-11d1-8B02-00600806D9B6" );
					//clsid.setAutoRegistration( true );
					//baseComServer = new JIComServer( clsid, host, session );

					// I'm not really sure what the deal is with this
					// Create an intermediary instance?
					var unknown = baseComServer.CreateInstance();

					var baseComObject = (IJIComObject) unknown.QueryInterface(comObjectId);

					var baseDispatch = (IJIDispatch) JIObjectFactory.narrowObject(baseComObject.QueryInterface(impls.automation.IJIDispatch_Fields.IID));

					var connectServer = (JIVariant) baseDispatch.callMethodA("ConnectServer", new object[] { new JIString(host)
								   , JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), 0, JIVariant.CreateOPTIONAL_PARAM()
				})[0];

					JISession.destroySession(session);
					Console.WriteLine("doStuff() run complete");
			}
				internal virtual catch (Exception e)
				{
					Log.Logger.log(Level.SEVERE, "Caught exception: ", e);
				}
		}
	}


		public static void Main(string[] args)
		{
			var testJIThreading = new TestJIThreading();
			testJIThreading.setUp();
			testJIThreading.testThreading();
		}
}

}
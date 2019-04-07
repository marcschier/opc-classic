namespace org.jinterop.dcom.test {



    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class Test10KServer
	{

		private readonly JIComServer comStub;
		private readonly IJIDispatch dispatch;
		private readonly IJIComObject unknown;


		public static void Main(string[] args)
		{

			try
			{

					if (args.Length < 4)
					{
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					JISystem.InBuiltLogHandler = false;
					JISystem.AutoRegisteration = true;
					for (var i = 0;i < 10000;++i)
					{

						var session = JISession.createSession(args[1],args[2],args[3]);
						var comServer = new JIComServer(JIProgId.ValueOf("MSMQ.MSMQQueueInfo"),args[0],session);
						var unknown = comServer.CreateInstance();
						var dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
						//JISession.destroySession(session);
						Thread.Sleep(150);
						if (i % 100 == 0)
						{
							Console.WriteLine("".valueOf(i));
						}
						System.gc();
					}

			}
			catch (Exception e)
			{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
			}
		}





	}

}
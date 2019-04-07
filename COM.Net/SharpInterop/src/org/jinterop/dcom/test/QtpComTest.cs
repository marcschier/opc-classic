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

    public class QtpComTest
	{

		private JIComServer comServer;

		private IJIDispatch dispatch;

		private IJIComObject unknown;

		private readonly JISession session;





//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public QtpComTest(String address, String domain, String username, String password) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public QtpComTest(string address, string domain, string username, string password)
		{

						Log.Logger.Level = Level.FINEST;

						/*Let the j-Interop library do this for you. You can set the "autoRegistration" flag in the
	
						  JISystem class. When the library encounters a "Class not registered" exception, it will
	
						  perform all the registry changes if the autoRegistration flag is set. And then re-attempt
	
						  loading the COM Server. Please have a look at MSSysInfo,MSWMI examples.*/

						JISystem.AutoRegisteration = true;

						session = JISession.createSession(domain,username,password);

						comServer = new JIComServer(JIProgId.valueOf("QuickTest.Application"), address, session);

	//                    session.setGlobalSocketTimeout(30000);

		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startQTP() throws org.jinterop.dcom.common.JIException
		public virtual void startQTP()
		{

						Console.WriteLine(comServer.SharpCifs.Util.Sharpen.Properties);

						unknown = comServer.CreateInstance();

						dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));

						//System.out.println(((JIVariant)dispatch.get("Version")).getObjectAsString().getString());

		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showQtp() throws org.jinterop.dcom.common.JIException
		public virtual void showQtp()
		{

						var dispId = dispatch.getIDsOfNames("Visible");

						var variant = new JIVariant(true);

						dispatch.put(dispId,variant);

		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void envQtp() throws org.jinterop.dcom.common.JIException
		public virtual void envQtp()
		{

						dispatch.callMethodA("Open", new object[]{new JIString("C:\\Programme\\Mercury Interactive\\QuickTest Professional\\Tests\\Test1"), new JIVariant(false), new JIVariant(true)});

						var variant = dispatch.get("Test");

						var test = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
						Console.WriteLine(test.get("Author"));

						//and this is the original session associated with dispatch.
						JISession.destroySession(session);

		}





		public static void Main(string[] args)
		{

						//"localhost", "ctron", "mpitonia", "ChrisSarah1"

						//"VPC003", "automation" , "automated_user", "@utom@tion"

						//"automationsvr01", "AUTOMATION", "Automated_User", "@utom@tion"

						try
						{

										var comQtp = new QtpComTest("localhost", "domain", "username", "password");

										comQtp.startQTP();

										comQtp.showQtp();

										comQtp.envQtp();

						}
						catch (Exception e)
						{

										Console.WriteLine(e.ToString());
										Console.Write(e.StackTrace);

						}

		}











	}
}
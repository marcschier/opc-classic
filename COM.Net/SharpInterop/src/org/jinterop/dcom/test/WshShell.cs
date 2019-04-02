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



    public class WshShell
	{



		  private readonly int xlWorksheet = -4167;

		  private readonly int xlXYScatterLinesNoMarkers = 75;

		  private readonly int xlColumns = 2;



		  private JIComServer comServer;

		  private IJIDispatch dispatch;

		  private IJIComObject unknown;

		  private readonly IJIDispatch dispatchOfWorkSheet;

		  private readonly IJIDispatch dispatchOfWorkBook;

		  private readonly JISession session;







//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public WshShell(String address, String domain, String username, String password) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		  public WshShell(string address, string domain, string username, string password)

		  {

				Log.Logger.Level = Level.SEVERE;

				session = JISession.createSession(domain,username,password);

				comServer = new JIComServer(JIProgId.valueOf("WScript.Shell"), address, session);

		  }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWScript() throws org.jinterop.dcom.common.JIException
		  public virtual void startWScript()

		  {









				Console.WriteLine(comServer.Properties);



				unknown = comServer.createInstance();

				dispatch = (IJIDispatch)JIObjectFactory.narrowObject((IJIComObject)unknown.queryInterface(impls.automation.IJIDispatch_Fields.IID));





				var jv = (JIVariant)dispatch.get("CurrentDirectory");

				Console.WriteLine(jv.ObjectAsString.String);



				var dispId = dispatch.getIDsOfNames("CurrentDirectory");

				Console.WriteLine(dispId);

				var variant = new JIVariant("C://WINDOWS");

				dispatch.put(dispId,variant);



				jv = (JIVariant)dispatch.get("CurrentDirectory");

				Console.WriteLine(jv.ObjectAsString.String);




				try
				{
					Thread.Sleep(60 * 1000 * 3);
				}
				catch (InterruptedException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				//WshShell.Exec

				Console.WriteLine(dispatch.callMethodA("Exec", new object[]{new JIString("calc")})[0]);


				try
				{
					Thread.Sleep(60 * 1000 * 3);
				}
				catch (InterruptedException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				//WshShell.Run

				Console.WriteLine(dispatch.callMethodA("Run", new object[]{new JIString("notepad"), new JIVariant(10),JIVariant.CreateOPTIONAL_PARAM()})[0]);







				//JISession.destroySession(session);





		  }





		  public static void Main(string[] args)
		  {

				try
				{

					  JISystem.AutoRegisteration = true;





					  var wScript = new WshShell("localhost", "domain", "username", "password");



					  wScript.startWScript();

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
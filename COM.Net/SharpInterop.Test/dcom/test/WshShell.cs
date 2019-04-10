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

				comServer = new JIComServer(JIProgId.ValueOf("WScript.Shell"), address, session);

		  }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWScript() throws org.jinterop.dcom.common.JIException
		  public virtual void startWScript()

		  {









				Console.WriteLine(comServer.SharpCifs.Util.Sharpen.Properties);



				unknown = comServer.CreateInstance();

				dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)unknown.QueryInterface(impls.automation.DispatchFlags.IID));





				var jv = (JIVariant)dispatch.Get("CurrentDirectory");

				Console.WriteLine(jv.ObjectAsString.String);



				var dispId = dispatch.GetIDsOfNames("CurrentDirectory");

				Console.WriteLine(dispId);

				var variant = new JIVariant("C://WINDOWS");

				dispatch.Put(dispId,variant);



				jv = (JIVariant)dispatch.Get("CurrentDirectory");

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

				Console.WriteLine(dispatch.CallMethodA("Exec", new object[]{new JIString("calc")})[0]);


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

				Console.WriteLine(dispatch.CallMethodA("Run", new object[]{new JIString("notepad"), new JIVariant(10),JIVariant.CreateOPTIONAL_PARAM()})[0]);







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
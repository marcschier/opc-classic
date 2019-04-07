using System;
using System.Threading;

namespace org.jinterop.dcom.test {


	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;



	public class WshShell {



		  private readonly int XlWorksheet = -4167;

		  private readonly int XlXYScatterLinesNoMarkers = 75;

		  private readonly int XlColumns = 2;



		  private JIComServer ComServer = null;

		  private IJIDispatch Dispatch = null;

		  private IJIComObject Unknown = null;

		  private IJIDispatch DispatchOfWorkSheet = null;

		  private IJIDispatch DispatchOfWorkBook = null;

		  private JISession Session = null;







//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public WshShell(String address, String domain, String username, String password) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		  public WshShell(string address, string domain, string username, string password)

		  {

				JISystem.Logger.Level = Level.SEVERE;

				Session = JISession.CreateSession(domain,username,password);

				ComServer = new JIComServer(JIProgId.ValueOf("WScript.Shell"), address, Session);

		  }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWScript() throws org.jinterop.dcom.common.JIException
		  public virtual void StartWScript()

		  {









				Console.WriteLine(ComServer.Properties);



				Unknown = ComServer.CreateInstance();

				Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));





				JIVariant jv = (JIVariant)Dispatch.Get("CurrentDirectory");

				Console.WriteLine(jv.ObjectAsString.String);



				int dispId = Dispatch.GetIDsOfNames("CurrentDirectory");

				Console.WriteLine(dispId);

				JIVariant variant = new JIVariant("C://WINDOWS");

				Dispatch.Put(dispId,variant);



				jv = (JIVariant)Dispatch.Get("CurrentDirectory");

				Console.WriteLine(jv.ObjectAsString.String);




				try {
					Thread.Sleep(60 * 1000 * 3);
				}
				catch (InterruptedException e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				//WshShell.Exec

				Console.WriteLine(Dispatch.CallMethodA("Exec", new object[]{ new JIString("calc") })[0]);


				try {
					Thread.Sleep(60 * 1000 * 3);
				}
				catch (InterruptedException e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				//WshShell.Run

				Console.WriteLine(Dispatch.CallMethodA("Run", new object[]{ new JIString("notepad"), new JIVariant(10),JIVariant.OPTIONAL_PARAM() })[0]);







				//JISession.destroySession(session);





		  }





		  public static void Main(string[] args) {

				try {

					  JISystem.AutoRegisteration = true;





					  WshShell wScript = new WshShell("localhost", "domain", "username", "password");



					  wScript.StartWScript();

				}
					  catch (Exception e) {

							// TODO Auto-generated catch block

							Console.WriteLine(e.ToString());
							Console.Write(e.StackTrace);

					  }

		  }











	}
}
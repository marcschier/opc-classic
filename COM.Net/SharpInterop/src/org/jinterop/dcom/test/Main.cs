/*
 * Main.java
 *
 * Created on 20 ������ 2007 �., 14:47
 *
 * To change this template, choose Tools | Template Manager
 * and open the template in the editor.
 */

namespace org.jinterop.dcom.test {


    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class Main
	{

		public virtual void Execute(JIString str)
		{
			Console.WriteLine(str.String);
		}
		/// <param name="args"> </param>
		public static void Main(string[] args)
		{

			if (args.Length < 4)
			{
				Console.WriteLine("Please provide address domain username password");
				return;
			}



			try
			{

				var domain = args[1];
				var username = args[2];
				var password = args[3];

				Log.Logger.Level = Level.FINEST;
				JISystem.InBuiltLogHandler = false;
				JISystem.AutoRegisteration = true;
				var session3 = JISession.createSession(domain,username,password);
				session3.useSessionSecurity(true);
				var virtualServer = new JIComServer(JIProgId.valueOf("VirtualServer.Application"),args[0],session3);
				var unkVirtualServer = virtualServer.createInstance();
				var dispatchVirtualServer = (IJIDispatch)JIObjectFactory.narrowObject(unkVirtualServer.queryInterface(impls.automation.IJIDispatch_Fields.IID));



			}
			catch (UnknownHostException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (JIException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
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


		}

	}
}
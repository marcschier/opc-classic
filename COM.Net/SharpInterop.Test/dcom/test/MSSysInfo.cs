namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JILocalCoClass = core.JILocalCoClass;
    using JILocalInterfaceDefinition = core.JILocalInterfaceDefinition;
    using JILocalMethodDescriptor = core.JILocalMethodDescriptor;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSSysInfo
	{

		internal JISession session;
		internal IJIComObject sysInfoObject;
		internal IJIComObject sysInfoServer;
		internal IJIDispatch dispatch;
		internal string identifier;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSSysInfo(String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		internal MSSysInfo(string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			session.useSessionSecurity(true);
			var comServer = new JIComServer(JIProgId.ValueOf("SYSINFO.SysInfo"),args[0],session);
			sysInfoServer = comServer.CreateInstance();
			sysInfoObject = (IJIComObject)sysInfoServer.QueryInterface("6FBA474C-43AC-11CE-9A0E-00AA0062BB4C");
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(sysInfoObject.QueryInterface(impls.automation.DispatchFlags.IID));

		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void displayValues() throws org.jinterop.dcom.common.JIException
		internal virtual void displayValues()
		{
			Console.WriteLine("ACStatus: " + dispatch.Get("ACStatus").ObjectAsShort);
			Console.WriteLine("BatteryFullTime: " + dispatch.Get("BatteryFullTime").ObjectAsInt);
			Console.WriteLine("BatteryLifePercent: " + dispatch.Get("BatteryLifePercent").ObjectAsShort);
			Console.WriteLine("BatteryLifeTime: " + dispatch.Get("BatteryLifeTime").ObjectAsInt);
			Console.WriteLine("BatteryStatus: " + dispatch.Get("BatteryStatus").ObjectAsShort);
			Console.WriteLine("OSVersion: " + dispatch.Get("OSVersion").ObjectAsFloat);
			//dispatch.callMethod("AboutBox");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void AttachEventListener() throws org.jinterop.dcom.common.JIException
		internal virtual void AttachEventListener()
		{
			//6FBA474D-43AC-11CE-9A0E-00AA0062BB4C

			var javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("6FBA474D-43AC-11CE-9A0E-00AA0062BB4C"),typeof(SysInfoEvents));
			javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("PowerStatusChanged",8,null));
			javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("TimeChanged",3,null));
			identifier = JIObjectFactory.AttachEventHandler(sysInfoServer,"6FBA474D-43AC-11CE-9A0E-00AA0062BB4C",JIObjectFactory.BuildObject(session,javaComponent));
			try
			{
				Thread.Sleep(3000);
			}
			catch (InterruptedException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			} //for call backs
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void DetachEventListener() throws org.jinterop.dcom.common.JIException
		internal virtual void DetachEventListener()
		{
			JIObjectFactory.DetachEventHandler(sysInfoServer,identifier);
			JISession.destroySession(dispatch.AssociatedSession);
		}

		public static void Main(string[] args)
		{
			try
			{
				if (args.Length < 4)
				{
					Console.WriteLine("Please provide address domain username password");
					return;
				}
				Log.Logger.Level = Level.OFF;
				JISystem.AutoRegisteration = true;
				var sysInfo = new MSSysInfo(args);
				sysInfo.displayValues();
				sysInfo.AttachEventListener();
				Thread.Sleep(20000); //now play around with power settings
				sysInfo.DetachEventListener();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}


	}


}
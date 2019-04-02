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
			var comServer = new JIComServer(JIProgId.valueOf("SYSINFO.SysInfo"),args[0],session);
			sysInfoServer = comServer.createInstance();
			sysInfoObject = (IJIComObject)sysInfoServer.queryInterface("6FBA474C-43AC-11CE-9A0E-00AA0062BB4C");
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(sysInfoObject.queryInterface(impls.automation.IJIDispatch_Fields.IID));

		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void displayValues() throws org.jinterop.dcom.common.JIException
		internal virtual void displayValues()
		{
			Console.WriteLine("ACStatus: " + dispatch.get("ACStatus").ObjectAsShort);
			Console.WriteLine("BatteryFullTime: " + dispatch.get("BatteryFullTime").ObjectAsInt);
			Console.WriteLine("BatteryLifePercent: " + dispatch.get("BatteryLifePercent").ObjectAsShort);
			Console.WriteLine("BatteryLifeTime: " + dispatch.get("BatteryLifeTime").ObjectAsInt);
			Console.WriteLine("BatteryStatus: " + dispatch.get("BatteryStatus").ObjectAsShort);
			Console.WriteLine("OSVersion: " + dispatch.get("OSVersion").ObjectAsFloat);
			//dispatch.callMethod("AboutBox");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void AttachEventListener() throws org.jinterop.dcom.common.JIException
		internal virtual void AttachEventListener()
		{
			//6FBA474D-43AC-11CE-9A0E-00AA0062BB4C

			var javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("6FBA474D-43AC-11CE-9A0E-00AA0062BB4C"),typeof(SysInfoEvents));
			javaComponent.InterfaceDefinition.addMethodDescriptor(new JILocalMethodDescriptor("PowerStatusChanged",8,null));
			javaComponent.InterfaceDefinition.addMethodDescriptor(new JILocalMethodDescriptor("TimeChanged",3,null));
			identifier = JIObjectFactory.attachEventHandler(sysInfoServer,"6FBA474D-43AC-11CE-9A0E-00AA0062BB4C",JIObjectFactory.buildObject(session,javaComponent));
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
			JIObjectFactory.detachEventHandler(sysInfoServer,identifier);
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
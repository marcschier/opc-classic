using System;
using System.Threading;

namespace org.jinterop.dcom.test {


	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JILocalCoClass = org.jinterop.dcom.core.JILocalCoClass;
	using JILocalInterfaceDefinition = org.jinterop.dcom.core.JILocalInterfaceDefinition;
	using JILocalMethodDescriptor = org.jinterop.dcom.core.JILocalMethodDescriptor;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	public class MSSysInfo {

		internal JISession Session = null;
		internal IJIComObject SysInfoObject = null;
		internal IJIComObject SysInfoServer = null;
		internal IJIDispatch Dispatch = null;
		internal string Identifier = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSSysInfo(String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSSysInfo(string[] args) {
			Session = JISession.CreateSession(args[1],args[2],args[3]);
			Session.UseSessionSecurity(true);
			JIComServer comServer = new JIComServer(JIProgId.ValueOf("SYSINFO.SysInfo"),args[0],Session);
			SysInfoServer = comServer.CreateInstance();
			SysInfoObject = (IJIComObject)SysInfoServer.QueryInterface("6FBA474C-43AC-11CE-9A0E-00AA0062BB4C");
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(SysInfoObject.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));

		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void displayValues() throws org.jinterop.dcom.common.JIException
		public virtual void DisplayValues() {
			Console.WriteLine("ACStatus: " + Dispatch.Get("ACStatus").ObjectAsShort);
			Console.WriteLine("BatteryFullTime: " + Dispatch.Get("BatteryFullTime").ObjectAsInt);
			Console.WriteLine("BatteryLifePercent: " + Dispatch.Get("BatteryLifePercent").ObjectAsShort);
			Console.WriteLine("BatteryLifeTime: " + Dispatch.Get("BatteryLifeTime").ObjectAsInt);
			Console.WriteLine("BatteryStatus: " + Dispatch.Get("BatteryStatus").ObjectAsShort);
			Console.WriteLine("OSVersion: " + Dispatch.Get("OSVersion").ObjectAsFloat);
			//dispatch.callMethod("AboutBox");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void AttachEventListener() throws org.jinterop.dcom.common.JIException
		public virtual void AttachEventListener() {
			//6FBA474D-43AC-11CE-9A0E-00AA0062BB4C

			JILocalCoClass javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("6FBA474D-43AC-11CE-9A0E-00AA0062BB4C"),typeof(SysInfoEvents));
			javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("PowerStatusChanged",8,null));
			javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("TimeChanged",3,null));
			Identifier = JIObjectFactory.AttachEventHandler(SysInfoServer,"6FBA474D-43AC-11CE-9A0E-00AA0062BB4C",JIObjectFactory.BuildObject(Session,javaComponent));
			try {
				Thread.Sleep(3000);
			}
			catch (InterruptedException e) {
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			} //for call backs
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void DetachEventListener() throws org.jinterop.dcom.common.JIException
		public virtual void DetachEventListener() {
			JIObjectFactory.DetachEventHandler(SysInfoServer,Identifier);
			JISession.DestroySession(Dispatch.AssociatedSession);
		}

		public static void Main(string[] args) {
			try {
				if (args.Length < 4) {
					Console.WriteLine("Please provide address domain username password");
					return;
				}
				JISystem.Logger.Level = Level.OFF;
				JISystem.AutoRegisteration = true;
				MSSysInfo sysInfo = new MSSysInfo(args);
				sysInfo.DisplayValues();
				sysInfo.AttachEventListener();
				Thread.Sleep(20000); //now play around with power settings
				sysInfo.DetachEventListener();
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}


	}


}
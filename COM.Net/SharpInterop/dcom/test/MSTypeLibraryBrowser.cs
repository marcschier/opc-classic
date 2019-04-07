namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIObjectFactory = impls.JIObjectFactory;
    using FuncDesc = impls.automation.FuncDesc;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJITypeInfo = impls.automation.IJITypeInfo;
    using IJITypeLib = impls.automation.IJITypeLib;
    using TypeAttr = impls.automation.TypeAttr;
    using VarDesc = impls.automation.VarDesc;

    public class MSTypeLibraryBrowser
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSTypeLibraryBrowser(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSTypeLibraryBrowser(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.ValueOf("InternetExplorer.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws org.jinterop.dcom.common.JIException
		public virtual void start()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
			var typeInfo = dispatch.getTypeInfo(0);
			var typeLib = (IJITypeLib)((object[])typeInfo.ContainingTypeLib)[0];
			var result = typeLib.getDocumentation(-1);
			Console.WriteLine(((JIString)result[0]).String);
			Console.WriteLine(((JIString)result[1]).String);
			Console.WriteLine(((JIString)result[3]).String);
			Console.WriteLine("-------------------------------");
			var typeInfoCount = typeLib.TypeInfoCount;
			var i = 0;
			string[] g_arrClassification = {};
			for (; i < typeInfoCount;i++)
			{
				result = typeLib.getDocumentation(i);
				var j = typeLib.getTypeInfoType(i);


				Console.WriteLine(((JIString)result[0]).String);
				Console.WriteLine(((JIString)result[1]).String);
				Console.WriteLine(((JIString)result[3]).String);
				Console.WriteLine(g_arrClassification[j]);

				var typeInfo2 = typeLib.getTypeInfo(i);
				var typeAttr = typeInfo2.TypeAttr;
				for (j = 0;j < typeAttr.cFuncs;j++)
				{
					var funcDesc = typeInfo2.getFuncDesc(j);
					result = typeInfo2.getDocumentation(funcDesc.memberId);
					Console.WriteLine(((JIString)result[0]).String);
					Console.WriteLine(((JIString)result[1]).String);
					Console.WriteLine(((JIString)result[3]).String);
				}

				for (j = 0;j < typeAttr.cVars;j++)
				{
					if (j == 77)
					{
						var kk = 0;
					}
					var varDesc = typeInfo2.getVarDesc(j);
					result = typeInfo2.getDocumentation(varDesc.memberId);
					Console.WriteLine(((JIString)result[0]).String);
					Console.WriteLine(((JIString)result[1]).String);
					Console.WriteLine(((JIString)result[3]).String);
					//System.out.println(j);
				}


				Console.WriteLine("***************************************");
			}
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
				var typeLibraryBrowser = new MSTypeLibraryBrowser(args[0],args);
				typeLibraryBrowser.start();
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
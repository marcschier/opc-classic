namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    public class MSTypeLibraryBrowser
	{

		private readonly JIComServer _comServer;
		private IJIDispatch _dispatch;
		private IJIComObject _unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSTypeLibraryBrowser(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSTypeLibraryBrowser(string address, string[] args)
		{
			var session = JISession.CreateSession(args[1],args[2],args[3]);
			_comServer = new JIComServer(JIProgId.ValueOf("InternetExplorer.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws org.jinterop.dcom.common.JIException
		public virtual void Start()
		{
			_unknown = _comServer.CreateInstance();
			_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
			var typeInfo = _dispatch.GetTypeInfo(0);
			var typeLib = (IJITypeLib)typeInfo.ContainingTypeLib[0];
			var result = typeLib.GetDocumentation(-1);
			Console.WriteLine(((JIString)result[0]).String);
			Console.WriteLine(((JIString)result[1]).String);
			Console.WriteLine(((JIString)result[3]).String);
			Console.WriteLine("-------------------------------");
			var typeInfoCount = typeLib.TypeInfoCount;
			var i = 0;
			string[] g_arrClassification = {};
			for (; i < typeInfoCount;i++)
			{
				result = typeLib.GetDocumentation(i);
				var j = typeLib.GetTypeInfoType(i);


				Console.WriteLine(((JIString)result[0]).String);
				Console.WriteLine(((JIString)result[1]).String);
				Console.WriteLine(((JIString)result[3]).String);
				Console.WriteLine(g_arrClassification[j]);

				var typeInfo2 = typeLib.GetTypeInfo(i);
				var typeAttr = typeInfo2.TypeAttr;
				for (j = 0;j < typeAttr.cFuncs;j++)
				{
					var funcDesc = typeInfo2.GetFuncDesc(j);
					result = typeInfo2.GetDocumentation(funcDesc.memberId);
					Console.WriteLine(((JIString)result[0]).String);
					Console.WriteLine(((JIString)result[1]).String);
					Console.WriteLine(((JIString)result[3]).String);
				}

				for (j = 0;j < typeAttr.cVars;j++)
				{
					var varDesc = typeInfo2.GetVarDesc(j);
					result = typeInfo2.GetDocumentation(varDesc.memberId);
					Console.WriteLine(((JIString)result[0]).String);
					Console.WriteLine(((JIString)result[1]).String);
					Console.WriteLine(((JIString)result[3]).String);
					//System.out.println(j);
				}


				Console.WriteLine("***************************************");
			}
			JISession.DestroySession(_dispatch.AssociatedSession);
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
				typeLibraryBrowser.Start();
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
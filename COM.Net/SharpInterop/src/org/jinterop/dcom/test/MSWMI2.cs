namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JIClsid = core.JIClsid;
    using JIComServer = core.JIComServer;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJIEnumVariant = impls.automation.IJIEnumVariant;

    /// <summary>
    /// WMI example showing how to use a new logger implementation.
    /// 
    /// @since 1.23
    /// 
    /// </summary>
    public class MSWMI2
	{

		private JIComServer comStub;
		private IJIComObject comObject;
		private IJIDispatch dispatch;
		private readonly string address;
		private readonly JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWMI2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWMI2(string address, string[] args)
		{
			this.address = address;
			session = JISession.createSession(args[1],args[2],args[3]);
	//		session.useSessionSecurity(true);
	//		session.setGlobalSocketTimeout(5000);
			comStub = new JIComServer(JIClsid.ValueOf("76a64158-cb41-11d1-8b02-00600806d9b6"),address,session);
			var unknown = comStub.CreateInstance();
			comObject = (IJIComObject)unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); //ISWbemLocator
			//This will obtain the dispatch interface
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(comObject.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
	//		IJIDispatch securityDisp = (IJIDispatch)JIObjectFactory.narrowObject(dispatch.get("Security_").getObjectAsComObject());
	//		securityDisp.put("ImpersonationLevel", new JIVariant(3));
			var results = dispatch.callMethodA("ConnectServer",new object[]{JIVariant.CreateOPTIONAL_PARAM(),new JIString("ROOT\\CIMV2"),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), 0, JIVariant.CreateOPTIONAL_PARAM()});

			var wbemServices_dispatch = (IJIDispatch)JIObjectFactory.narrowObject(results[0].ObjectAsComObject);
			results = wbemServices_dispatch.callMethodA("ExecQuery", new object[]{new JIString("select * from Win32_OperatingSystem where Primary=True"), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			var wbemObjectSet_dispatch = (IJIDispatch)JIObjectFactory.narrowObject(results[0].ObjectAsComObject);
			var variant = wbemObjectSet_dispatch.get("_NewEnum");
			var object2 = variant.ObjectAsComObject;

			var enumVARIANT = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.QueryInterface(impls.automation.IJIEnumVariant_Fields.IID));

			var Count = wbemObjectSet_dispatch.get("Count");
			var count = Count.ObjectAsInt;
			for (var i = 0; i < count; i++)
			{
				var values = enumVARIANT.next(1);
				var array = (JIArray)values[0];
				var arrayObj = (object[])array.ArrayInstance;
				for (var j = 0; j < arrayObj.Length; j++)
				{
					var wbemObject_dispatch = (IJIDispatch)JIObjectFactory.narrowObject(((JIVariant)arrayObj[j]).ObjectAsComObject);
					var variant2 = (JIVariant)wbemObject_dispatch.callMethodA("GetObjectText_",new object[]{ 1 })[0];
					Console.WriteLine(variant2.ObjectAsString.String);
					Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
				}
			}


		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void killme() throws org.jinterop.dcom.common.JIException
		private void killme()
		{
			JISession.destroySession(session);
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

					JISystem.InBuiltLogHandler = false;
					Log.Logger.Level = Level.FINEST;
					JISystem.AutoRegisteration = true;
					var test = new MSWMI2(args[0],args);
					for (var i = 0 ; i < 2; i++)
					{
						Console.WriteLine("Index i: " + i);
						test.performOp();
					}
					test.killme();
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
namespace org.jinterop.dcom.test {




    using IJIUnreferenced = common.IJIUnreferenced;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJIEnumVariant = impls.automation.IJIEnumVariant;


    public class MSWMI
	{

		private JIComServer comStub;
		private IJIComObject comObject;
		private IJIDispatch dispatch;
		private readonly string address;
		private JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWMI(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWMI(string address, string[] args)
		{
			this.address = address;
			session = JISession.createSession(args[1],args[2],args[3]);
			session.useSessionSecurity(true);
			session.GlobalSocketTimeout = 5000;
			comStub = new JIComServer(JIProgId.valueOf("WbemScripting.SWbemLocator"),address,session);
			var unknown = comStub.createInstance();
			comObject = (IJIComObject)unknown.queryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); //ISWbemLocator
			//This will obtain the dispatch interface
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(comObject.queryInterface(impls.automation.IJIDispatch_Fields.IID));
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			System.gc();
			var results = dispatch.callMethodA("ConnectServer",new object[]{new JIString(address),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), 0, JIVariant.CreateOPTIONAL_PARAM()});

			//using the dispatch results above you can use the "ConnectServer" api to retrieve a pointer to IJIDispatch
			//of ISWbemServices

			//OR
			//Make a direct call like below , in this case you would get back an interface pointer to ISWbemServices , NOT to it's IDispatch
			var callObject = new JICallBuilder();
			callObject.addInParamAsString(address,JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsInt(0,JIFlags.FLAG_NULL);
			callObject.addInParamAsPointer(null,JIFlags.FLAG_NULL);
			callObject.Opnum = 0;
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var wbemServices = JIObjectFactory.narrowObject((IJIComObject)((object[])comObject.call(callObject))[0]);
			wbemServices.InstanceLevelSocketTimeout = 1000;
			wbemServices.registerUnreferencedHandler(new IJIUnreferencedAnonymousInnerClassHelper(this));

			//Lets have a look at both.
			var wbemServices_dispatch = (IJIDispatch)JIObjectFactory.narrowObject(results[0].ObjectAsComObject);
			results = wbemServices_dispatch.callMethodA("InstancesOf", new object[]{new JIString("Win32_Process"), 0, JIVariant.CreateOPTIONAL_PARAM()});
			var wbemObjectSet_dispatch = (IJIDispatch)JIObjectFactory.narrowObject(results[0].ObjectAsComObject);
			var variant = wbemObjectSet_dispatch.get("_NewEnum");
			var object2 = variant.ObjectAsComObject;

			Console.WriteLine(object2.DispatchSupported);
			Console.WriteLine(object2.DispatchSupported);

			object2.registerUnreferencedHandler(new IJIUnreferencedAnonymousInnerClassHelper2(this));

			var enumVARIANT = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.queryInterface(impls.automation.IJIEnumVariant_Fields.IID));

			//This will return back a dispatch of ISWbemObjectSet

			//OR
			//It returns back the pointer to ISWbemObjectSet
			callObject = new JICallBuilder();
			callObject.addInParamAsString("Win32_Process",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addInParamAsInt(0,JIFlags.FLAG_NULL);
			callObject.addInParamAsPointer(null,JIFlags.FLAG_NULL);
			callObject.Opnum = 4;
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var wbemObjectSet = JIObjectFactory.narrowObject((IJIComObject)((object[])wbemServices.call(callObject))[0]);

			//okay seen enough of the other usage, lets just stick to disptach, it's lot simpler
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

		private class IJIUnreferencedAnonymousInnerClassHelper : IJIUnreferenced
		{
			private readonly MSWMI outerInstance;

			public IJIUnreferencedAnonymousInnerClassHelper(MSWMI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void unReferenced()
			{
				Console.WriteLine("wbemServices unreferenced... ");
			}
		}

		private class IJIUnreferencedAnonymousInnerClassHelper2 : IJIUnreferenced
		{
			private readonly MSWMI outerInstance;

			public IJIUnreferencedAnonymousInnerClassHelper2(MSWMI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void unReferenced()
			{
				Console.WriteLine("object2 unreferenced...");
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

					Log.Logger.Level = Level.INFO;
					JISystem.InBuiltLogHandler = false;
					JISystem.AutoRegisteration = true;
					var test = new MSWMI(args[0],args);
					for (var i = 0 ; i < 100; i++)
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
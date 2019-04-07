namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JICallBuilder = core.JICallBuilder;
    using JIClsid = core.JIClsid;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIPointer = core.JIPointer;
    using JISession = core.JISession;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class TestCOMServer
	{

		private JIComServer comStub;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TestCOMServer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public TestCOMServer(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);


			//instead of this the ProgID "TestCOMServer.ITestCOMServer"	can be used as well.
			//comStub = new JIComServer(JIProgId.valueOf(session,"TestCOMServer.ITestCOMServer"),address,session);
			//CLSID of ITestCOMServer
			comStub = new JIComServer(JIClsid.ValueOf("44A9CD09-0D9B-4FD2-9B8A-0151F2E0CAD1"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void execute() throws org.jinterop.dcom.common.JIException
		public virtual void execute()
		{
			unknown = comStub.CreateInstance();
			//CLSID of IITestCOMServer
			var comObject = (IJIComObject)unknown.QueryInterface("4AE62432-FD04-4BF9-B8AC-56AA12A47FF9");
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(comObject.QueryInterface(impls.automation.IJIDispatch_Fields.IID));

			//Now call via automation
			object[] results = dispatch.callMethodA("Add",new object[]{ 1, 2, new JIVariant(0,true)});
			Console.WriteLine(results[1]);

            //now without automation
            var callObject = new JICallBuilder {
                Opnum = 1 //obtained from the IDL or TypeLib.
            };
            callObject.AddInParamAsInt(1,JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(2,JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(new JIPointer(0),JIFlags.FLAG_NULL);
			//Since the retval is a top level pointer , it will get replaced with it's base type.
			callObject.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
			results = comObject.Call(callObject);
			Console.WriteLine(results[0]);
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
					var test = new TestCOMServer(args[0],args);
					test.execute();
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
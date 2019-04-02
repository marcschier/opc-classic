namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIPointer = core.JIPointer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;

    public class MetrikonOPC
	{

		private JIComServer comStub;
		private IJIComObject unknown;
		private IJIComObject opcServer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MetrikonOPC(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MetrikonOPC(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comStub = new JIComServer(JIProgId.valueOf("Matrikon.OPC.Simulation"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getOPC() throws org.jinterop.dcom.common.JIException
		public virtual void getOPC()
		{
			unknown = comStub.createInstance();
			opcServer = (IJIComObject)unknown.queryInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 0
            };

            callObject.addInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
			callObject.addInParamAsInt(unchecked((int)0xFFFFFFFF), JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(1000,JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(1234,JIFlags.FLAG_NULL);
			callObject.addInParamAsPointer(new JIPointer(0), JIFlags.FLAG_NULL);
			callObject.addInParamAsPointer(new JIPointer(0.0),JIFlags.FLAG_NULL);
			callObject.addInParamAsInt(0, JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.addInParamAsUUID("39C13A50-011E-11D0-9675-0020AFD8ADB3", JIFlags.FLAG_NULL);
			callObject.addOutParamAsType(typeof(IJIComObject), JIFlags.FLAG_NULL);

			var result = opcServer.call(callObject);



			JISession.destroySession(unknown.AssociatedSession);
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
					var test = new MetrikonOPC(args[0],args);
					test.OPC;
					test.performOp();
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
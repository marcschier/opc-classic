namespace org.jinterop.dcom.test {
    using System;
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
			comStub = new JIComServer(JIProgId.ValueOf("Matrikon.OPC.Simulation"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getOPC() throws org.jinterop.dcom.common.JIException
		public virtual void getOPC()
		{
			unknown = comStub.CreateInstance();
			opcServer = (IJIComObject)unknown.QueryInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 0
            };

            callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
			callObject.AddInParamAsInt(unchecked((int)0xFFFFFFFF), JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(1000,JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(1234,JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(new JIPointer(0), JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(new JIPointer(0.0),JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(0, JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.AddInParamAsUUID("39C13A50-011E-11D0-9675-0020AFD8ADB3", JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject), JIFlags.FLAG_NULL);

			var result = opcServer.Call(callObject);



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
					test.getOPC();
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
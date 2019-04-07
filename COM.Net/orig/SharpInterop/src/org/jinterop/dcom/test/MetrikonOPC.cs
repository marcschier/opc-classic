using System;

namespace org.jinterop.dcom.test {



	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIFlags = org.jinterop.dcom.core.JIFlags;
	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;

	public class MetrikonOPC {

		private JIComServer ComStub = null;
		private IJIComObject Unknown = null;
		private IJIComObject OpcServer = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MetrikonOPC(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MetrikonOPC(string address, string[] args) {
			JISession session = JISession.CreateSession(args[1],args[2],args[3]);
			ComStub = new JIComServer(JIProgId.ValueOf("Matrikon.OPC.Simulation"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getOPC() throws org.jinterop.dcom.common.JIException
		public virtual void GetOPC() {
			Unknown = ComStub.CreateInstance();
			OpcServer = (IJIComObject)Unknown.QueryInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
		}



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void PerformOp() {

			JICallBuilder callObject = new JICallBuilder(true);
			callObject.Opnum = 0;

			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
			callObject.AddInParamAsInt(unchecked((int)0xFFFFFFFF), JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(1000,JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(1234,JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(new JIPointer(new int?(0)), JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(new JIPointer(new float?(0.0)),JIFlags.FLAG_NULL);
			callObject.AddInParamAsInt(0, JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			callObject.AddInParamAsUUID("39C13A50-011E-11D0-9675-0020AFD8ADB3", JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject), JIFlags.FLAG_NULL);

			object[] result = OpcServer.Call(callObject);



			JISession.DestroySession(Unknown.AssociatedSession);
		}

		public static void Main(string[] args) {

			try {
					if (args.Length < 4) {
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					MetrikonOPC test = new MetrikonOPC(args[0],args);
					test.OPC;
					test.PerformOp();
			}
				catch (Exception e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
		}





	}

}
namespace org.jinterop.dcom.test {

    using IJIComObject = core.IJIComObject;
    using JIClsid = core.JIClsid;
    using JIComServer = core.JIComServer;
    using JISession = core.JISession;

    public class TestWinNativeSSO
	{

		public static void Main(string[] args)
		{

			try
			{

				var session = JISession.createSession();
				var comServer = new JIComServer(JIClsid.valueOf("00024500-0000-0000-C000-000000000046"), session);
				var comObject = comServer.createInstance();
				var h = 0;

	//			SSPIJNIClient jniClient = SSPIJNIClient.getInstance();
	//			byte[] type1Message = jniClient.invokePrepareSSORequest();
	//			jcifs.util.Hexdump.hexdump(System.out, type1Message, 0, type1Message.length);
	//			int h = 0;
	//			
	//			jniClient.invokeUnInitialize();
	//			
	//			type1Message = new Type1Message().toByteArray();
	//			jcifs.util.Hexdump.hexdump(System.out, type1Message, 0, type1Message.length);
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
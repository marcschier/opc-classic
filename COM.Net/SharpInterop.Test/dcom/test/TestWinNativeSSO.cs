namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using System;

    public class TestWinNativeSSO {

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args) {
#pragma warning restore IDE0060 // Remove unused parameter

            try {

                var session = JISession.CreateSession();
                var comServer = new JIComServer(JIClsid.ValueOf("00024500-0000-0000-C000-000000000046"), session);
                var comObject = comServer.CreateInstance();

                //			SSPIJNIClient jniClient = SSPIJNIClient.getInstance();
                //			byte[] type1Message = jniClient.invokePrepareSSORequest();
                //			Utils.HexString(type1Message, 0, type1Message.length);
                //			int h = 0;
                //			
                //			jniClient.invokeUnInitialize();
                //			
                //			type1Message = new Type1Message().toByteArray();
                //			Utils.HexString(type1Message, 0, type1Message.length);
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }
    }

}
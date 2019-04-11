using System;

namespace org.jinterop.dcom.test {

    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIClsid = org.jinterop.dcom.core.JIClsid;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JISession = org.jinterop.dcom.core.JISession;

    public class TestWinNativeSSO {

        public static void Main(string[] args) {

            try {

                JISession session = JISession.CreateSession();
                JIComServer comServer = new JIComServer(JIClsid.ValueOf("00024500-0000-0000-C000-000000000046"), session);
                IJIComObject comObject = comServer.CreateInstance();
                int h = 0;

    //            SSPIJNIClient jniClient = SSPIJNIClient.getInstance();
    //            byte[] type1Message = jniClient.invokePrepareSSORequest();
    //            jcifs.util.Hexdump.hexdump(System.out, type1Message, 0, type1Message.length);
    //            int h = 0;
    //            
    //            jniClient.invokeUnInitialize();
    //            
    //            type1Message = new Type1Message().toByteArray();
    //            jcifs.util.Hexdump.hexdump(System.out, type1Message, 0, type1Message.length);
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }
    }

}
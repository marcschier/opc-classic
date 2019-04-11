using System;

namespace org.jinterop.dcom.test {



    using JIException = org.jinterop.dcom.common.JIException;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
    using JIClsid = org.jinterop.dcom.core.JIClsid;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIFlags = org.jinterop.dcom.core.JIFlags;
    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class TestCOMServer {

        private JIComServer ComStub = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TestCOMServer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public TestCOMServer(string address, string[] args) {
            JISession session = JISession.CreateSession(args[1],args[2],args[3]);


            //instead of this the ProgID "TestCOMServer.ITestCOMServer"    can be used as well.
            //comStub = new JIComServer(JIProgId.valueOf(session,"TestCOMServer.ITestCOMServer"),address,session);
            //CLSID of ITestCOMServer
            ComStub = new JIComServer(JIClsid.ValueOf("44A9CD09-0D9B-4FD2-9B8A-0151F2E0CAD1"),address,session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void execute() throws org.jinterop.dcom.common.JIException
        public virtual void Execute() {
            Unknown = ComStub.CreateInstance();
            //CLSID of IITestCOMServer
            IJIComObject comObject = (IJIComObject)Unknown.QueryInterface("4AE62432-FD04-4BF9-B8AC-56AA12A47FF9");
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(comObject.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));

            //Now call via automation
            object[] results = Dispatch.CallMethodA("Add",new object[]{ new int?(1), new int?(2), new JIVariant(0,true) });
            Console.WriteLine(results[1]);

            //now without automation
            JICallBuilder callObject = new JICallBuilder();
            callObject.Opnum = 1; //obtained from the IDL or TypeLib.
            callObject.AddInParamAsInt(1,JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(2,JIFlags.FLAG_NULL);
            callObject.AddInParamAsPointer(new JIPointer(new int?(0)),JIFlags.FLAG_NULL);
            //Since the retval is a top level pointer , it will get replaced with it's base type.
            callObject.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
            results = comObject.Call(callObject);
            Console.WriteLine(results[0]);
            JISession.DestroySession(Dispatch.AssociatedSession);
        }



        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    TestCOMServer test = new TestCOMServer(args[0],args);
                    test.Execute();
            }
                catch (Exception e) {
                    // TODO Auto-generated catch block
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
        }





    }

}
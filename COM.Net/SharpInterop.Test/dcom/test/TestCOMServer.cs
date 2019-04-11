namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.impls.automation;
    using System;
    using IComObject = core.IComObject;
    using IJIDispatch = impls.automation.IJIDispatch;
    using JICallBuilder = core.JICallBuilder;
    using JIClsid = core.JIClsid;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIObjectFactory = impls.JIObjectFactory;
    using JIPointer = core.JIPointer;
    using JISession = core.JISession;
    using JIVariant = core.JIVariant;

    public class TestCOMServer {

        private readonly JIComServer _comStub;
        private IJIDispatch _dispatch;
        private IComObject _unknown;


        public TestCOMServer(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);


            // instead of this the ProgID "TestCOMServer.ITestCOMServer"    can be used as well.
            // comStub = new JIComServer(JIProgId.valueOf(session,"TestCOMServer.ITestCOMServer"),address,session);
            // CLSID of ITestCOMServer
            _comStub = new JIComServer(JIClsid.ValueOf("44A9CD09-0D9B-4FD2-9B8A-0151F2E0CAD1"), address, session);
        }


        public void Execute() {
            _unknown = _comStub.CreateInstance();
            // CLSID of IITestCOMServer
            var comObject = _unknown.QueryInterface("4AE62432-FD04-4BF9-B8AC-56AA12A47FF9");
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(comObject.QueryInterface(Interfaces.IID_IDispatch));

            // Now call via automation
            object[] results = _dispatch.CallMethodA("Add", new object[] { 1, 2, new JIVariant(0, true) });
            Console.WriteLine(results[1]);

            // now without automation
            var callObject = new JICallBuilder {
                Opnum = 1 // obtained from the IDL or TypeLib.
            };
            callObject.AddInParamAsInt(1, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(2, JIFlags.FLAG_NULL);
            callObject.AddInParamAsPointer(new JIPointer(0), JIFlags.FLAG_NULL);
            // Since the retval is a top level pointer, it will get replaced with it's base type.
            callObject.AddOutParamAsObject(typeof(int), JIFlags.FLAG_NULL);
            results = comObject.Call(callObject);
            Console.WriteLine(results[0]);
            JISession.DestroySession(_dispatch.AssociatedSession);
        }



        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new TestCOMServer(args[0], args);
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
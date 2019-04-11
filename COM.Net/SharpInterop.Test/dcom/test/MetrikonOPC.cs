namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class MetrikonOPC {

        private readonly JIComServer _comStub;
        private IJIComObject _unknown;
        private IJIComObject _opcServer;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public MetrikonOPC(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MetrikonOPC(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);
            _comStub = new JIComServer(JIProgId.ValueOf("Matrikon.OPC.Simulation"), address, session);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getOPC() throws org.jinterop.dcom.common.JIException
        public virtual void GetOPC() {
            _unknown = _comStub.CreateInstance();
            _opcServer = _unknown.QueryInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        }



        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {

            var callObject = new JICallBuilder(true) {
                Opnum = 0
            };

            callObject.AddInParamAsString("", JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
            callObject.AddInParamAsInt(unchecked((int)0xFFFFFFFF), JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(1000, JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(1234, JIFlags.FLAG_NULL);
            callObject.AddInParamAsPointer(new JIPointer(0), JIFlags.FLAG_NULL);
            callObject.AddInParamAsPointer(new JIPointer(0.0), JIFlags.FLAG_NULL);
            callObject.AddInParamAsInt(0, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            callObject.AddInParamAsUUID("39C13A50-011E-11D0-9675-0020AFD8ADB3", JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IJIComObject), JIFlags.FLAG_NULL);

            var result = _opcServer.Call(callObject);

            JISession.DestroySession(_unknown.AssociatedSession);
        }

        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MetrikonOPC(args[0], args);
                test.GetOPC();
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
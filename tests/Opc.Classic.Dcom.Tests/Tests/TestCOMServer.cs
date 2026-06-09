// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Automation;
    using System;
    using Opc.Classic.Dcom.Core;

    public class TestCOMServer {

        private readonly ComServer _comStub;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public TestCOMServer(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);

            // instead of this the ProgID "TestCOMServer.ITestCOMServer" can be used as well.
            // comStub = new ComServer(ProgId.valueOf(session,"TestCOMServer.ITestCOMServer"),address,session);
            // CLSID of ITestCOMServer
            _comStub = new ComServer(Clsid.ValueOf("44A9CD09-0D9B-4FD2-9B8A-0151F2E0CAD1"), address, session);
        }


        public void Execute() {
            _unknown = _comStub.CreateInstance();
            // CLSID of IITestCOMServer
            var comObject = _unknown.QueryInterface("4AE62432-FD04-4BF9-B8AC-56AA12A47FF9");
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(comObject.QueryInterface(Interfaces.IID_IDispatch));

            // Now call via automation
            object[] results = _dispatch.CallMethodA("Add", new object[] { 1, 2, new Variant(0, true) });
            Console.WriteLine(results[1]);

            // now without automation
            var callObject = new CallBuilder {
                Opnum = 1 // obtained from the IDL or TypeLib.
            };
            callObject.AddInParamAsInt(1);
            callObject.AddInParamAsInt(2);
            callObject.AddInParamAsPointer(new ComPointer(0));
            // Since the retval is a top level pointer, it will get replaced with it's base type.
            callObject.AddOutParamAsObject(typeof(int));
            results = comObject.Call(callObject);
            Console.WriteLine(results[0]);
            Session.DestroySession(_dispatch.AssociatedSession);
        }



        public static void RunTest(string[] args) {
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

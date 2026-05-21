//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using SharpInterop.Core;
    using System;

    public class OPC {

        private readonly ComServer _comStub;
        private IComObject _unknown;
        private IComObject _opcServer;

        public OPC(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            _comStub = new ComServer(ProgId.ValueOf("Matrikon.OPC.Simulation"), address, session);
        }

        public void GetOPC() {
            _unknown = _comStub.CreateInstance();
            _opcServer = _unknown.QueryInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        }

        public void PerformOp() {

            var callObject = new CallBuilder(true) {
                Opnum = 0
            };

            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
            callObject.AddInParamAsInt(unchecked((int)0xFFFFFFFF));
            callObject.AddInParamAsInt(1000);
            callObject.AddInParamAsInt(1234);
            callObject.AddInParamAsPointer(new ComPointer(0));
            callObject.AddInParamAsPointer(new ComPointer(0.0));
            callObject.AddInParamAsInt(0);
            callObject.AddOutParamAsType(typeof(int));
            callObject.AddOutParamAsType(typeof(int));
            callObject.AddInParamAsUUID("39C13A50-011E-11D0-9675-0020AFD8ADB3");
            callObject.AddOutParamAsType(typeof(IComObject));

            var result = _opcServer.Call(callObject);

            Session.DestroySession(_unknown.AssociatedSession);
        }

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new OPC(args[0], args);
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
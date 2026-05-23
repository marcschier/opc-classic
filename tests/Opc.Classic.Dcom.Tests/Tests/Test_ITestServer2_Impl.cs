//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using SharpInterop.Common;
    using SharpInterop.Core;
    using SharpInterop;
    using SharpInterop.Automation;
    using System;

    public class Test_ITestServer2_Impl {

        public void Execute(ComString str) => Console.WriteLine(str.String);

        public static void RunTest(string[] args) {
            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            try {
                Interop.UseAutoRegistration = true;

                var session1 = Session.CreateSession(args[1], args[2], args[3]);
                var session2 = Session.CreateSession(args[1], args[2], args[3]);
                var testServer1 = new ComServer(ProgId.ValueOf("TestJavaServer.TestServer1"), args[0], session1);
                var unkTestServer1 = testServer1.CreateInstance();
                var testServer1Intf = ObjectFactory.NarrowObject(unkTestServer1.QueryInterface("2A93A24D-59FE-4DE0-B67E-B8D41C9F57F8"));
                var dispatch1 = (IDispatch)ObjectFactory.NarrowObject(unkTestServer1.QueryInterface(Interfaces.IID_IDispatch));

                // First lets call the ITestServer1.Call_TestServer2_Java using the Dispatch interface
                // Acquire a reference to ITestServer2
                var testServer2 = new ComServer(ProgId.ValueOf("TestJavaServer.TestServer2"), args[0], session2);
                var unkTestServer2 = testServer2.CreateInstance();
                // Get the interface pointer to ITestServer2
                var iTestServer2 = ObjectFactory.NarrowObject(unkTestServer2.QueryInterface("9CCC5120-457D-49F3-8113-90F7E97B54A7"));

                var dispatch2 = (IDispatch)ObjectFactory.NarrowObject(unkTestServer2.QueryInterface(Interfaces.IID_IDispatch));

                // send it directly without IDispatch interface, please note that the "dispatchNotSupported" flag of CallBuilder is "false".
                var callObject = new CallBuilder(false);
                callObject.AddInParamAsComObject(iTestServer2);
                callObject.Opnum = 0;
                testServer1Intf.Call(callObject);

                // Send it to ITestServer.Call_TestServer2_Java2 via IDispatch of ITestServer1. Notice that pointer here id IDispatch.
                dispatch1.CallMethod("Call_TestServer2_Java2", new object[] { new Variant(dispatch2) });
                // Send it to ITestServer.Call_TestServer2_Java via IDispatch of ITestServer1.
                dispatch1.CallMethod("Call_TestServer2_Java", new object[] { new Variant(iTestServer2) });

                // Now for the Java Implementation of ITestServer2 interface (from the type library or IDL)
                // IID of ITestServer2 interface
                var interfaceDefinition = new LocalInterfaceDefinition("9CCC5120-457D-49F3-8113-90F7E97B54A7");
                // lets define the method "Execute" now. Please note that either this should be in the same order as defined in IDL
                // or use the addInParamAsObject with opnum parameter function.
                var parameterObject = new LocalParamsDescriptor();
                parameterObject.AddInParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR), InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
                var methodDescriptor = new LocalMethodDescriptor("Execute", 1, parameterObject);
                interfaceDefinition.AddMethodDescriptor(methodDescriptor);
                // Create the Java Server class. This contains the instance to be called by the COM Server ITestServer1.
                var _testServer2 = new LocalCoClass(interfaceDefinition, new Test_ITestServer2_Impl());
                // Get a interface pointer to the Java CO Class. The template could be any <see cref="IComObject"/> since only the session is reused.
                var __testServer2 = ObjectFactory.BuildObject(session1, _testServer2);
                // Call our Java server. The same message should be printed on the Java console.
                dispatch1.CallMethod("Call_TestServer2_Java", new object[] { new Variant(__testServer2) });

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
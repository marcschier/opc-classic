namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    public class Test_ITestServer2_Impl {

        public void Execute(JIString str) => Console.WriteLine(str.String);

        public static void Main(string[] args) {
            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            try {
                JISystem.UseAutoRegistration = true;

                var session1 = JISession.CreateSession(args[1], args[2], args[3]);
                var session2 = JISession.CreateSession(args[1], args[2], args[3]);
                var testServer1 = new JIComServer(JIProgId.ValueOf("TestJavaServer.TestServer1"), args[0], session1);
                var unkTestServer1 = testServer1.CreateInstance();
                var testServer1Intf = JIObjectFactory.NarrowObject(unkTestServer1.QueryInterface("2A93A24D-59FE-4DE0-B67E-B8D41C9F57F8"));
                var dispatch1 = (IJIDispatch)JIObjectFactory.NarrowObject(unkTestServer1.QueryInterface(Interfaces.IID_IDispatch));

                // First lets call the ITestServer1.Call_TestServer2_Java using the Dispatch interface
                // Acquire a reference to ITestServer2
                var testServer2 = new JIComServer(JIProgId.ValueOf("TestJavaServer.TestServer2"), args[0], session2);
                var unkTestServer2 = testServer2.CreateInstance();
                // Get the interface pointer to ITestServer2
                var iTestServer2 = JIObjectFactory.NarrowObject(unkTestServer2.QueryInterface("9CCC5120-457D-49F3-8113-90F7E97B54A7"));

                var dispatch2 = (IJIDispatch)JIObjectFactory.NarrowObject(unkTestServer2.QueryInterface(Interfaces.IID_IDispatch));

                // send it directly without IDispatch interface, please note that the "dispatchNotSupported" flag of JICallBuilder is "false".
                var callObject = new JICallBuilder(false);
                callObject.AddInParamAsComObject(iTestServer2, JIFlags.FLAG_NULL);
                callObject.Opnum = 0;
                testServer1Intf.Call(callObject);

                // Send it to ITestServer.Call_TestServer2_Java2 via IDispatch of ITestServer1. Notice that pointer here id IDispatch.
                dispatch1.CallMethod("Call_TestServer2_Java2", new object[] { new JIVariant(dispatch2) });
                // Send it to ITestServer.Call_TestServer2_Java via IDispatch of ITestServer1.
                dispatch1.CallMethod("Call_TestServer2_Java", new object[] { new JIVariant(iTestServer2) });

                // Now for the Java Implementation of ITestServer2 interface (from the type library or IDL)  
                // IID of ITestServer2 interface
                var interfaceDefinition = new JILocalInterfaceDefinition("9CCC5120-457D-49F3-8113-90F7E97B54A7");
                // lets define the method "Execute" now. Please note that either this should be in the same order as defined in IDL
                // or use the addInParamAsObject with opnum parameter function.
                var parameterObject = new JILocalParamsDescriptor();
                parameterObject.AddInParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR), JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
                var methodDescriptor = new JILocalMethodDescriptor("Execute", 1, parameterObject);
                interfaceDefinition.AddMethodDescriptor(methodDescriptor);
                // Create the Java Server class. This contains the instance to be called by the COM Server ITestServer1.
                var _testServer2 = new JILocalCoClass(interfaceDefinition, new Test_ITestServer2_Impl());
                // Get a interface pointer to the Java CO Class. The template could be any IJIComObject since only the session is reused.
                var __testServer2 = JIObjectFactory.BuildObject(session1, _testServer2);
                // Call our Java server. The same message should be printed on the Java console.
                dispatch1.CallMethod("Call_TestServer2_Java", new object[] { new JIVariant(__testServer2) });

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
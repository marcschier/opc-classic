namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    public class SampleTestServerCallback {

        private static void Append(string fileName, string data) {
            try {
                var pWriter = new PrintWriter(new System.IO.StreamWriter(fileName, true));
                pWriter.Write(data);
                pWriter.Flush();
                pWriter.Close();
            }
            catch (IOException) {
            }
        }

        public virtual void UpdateMe(JIUnsignedShort size, JIArray array) {
            Append("C:\\Test\\callback_j.log", "SampleTestServerCallback::UpdateMe entered with array size=" + size + "\n");
            Console.WriteLine("SampleTestServerCallback::UpdateMe entered with array size=" + size + "\n");
            var structArray = (JIStruct[])array.ArrayInstance;
            for (var i = 0; i < (int)size.Value; i++) {
                Append("C:\\Test\\callback_j.log", "Member 0= " + structArray[i].GetMember(0).ToString() + "\n");
                Console.WriteLine("Array elt=" + i + ",Member 0= " + structArray[i].GetMember(0).ToString() + "\n");
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private static org.jinterop.dcom.core.JILocalInterfaceDefinition registerInterface() throws org.jinterop.dcom.common.JIException
        private static JILocalInterfaceDefinition RegisterInterface() {
            //Now for the Java Implementation of SampleTestServer2 interface (from the type library or IDL)
            var interfaceDefinition = new JILocalInterfaceDefinition("D3F9CE10-686C-11d2-97BF-006008BD50B1", false); //IStatisUpdateMeSink

            var VarData = new JIStruct(); // Will add in the struct later on
            VarData.AddMember(typeof(JIUnsignedInteger));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(JIUnsignedShort));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(DateTime));
            VarData.AddMember(typeof(JIUnsignedShort));

            var NonVariableData = new JIStruct(); // Will add in the struct later on
            NonVariableData.AddMember(typeof(JIUnsignedInteger));
            NonVariableData.AddMember(typeof(JIUnsignedInteger));
            NonVariableData.AddMember(typeof(JIUnsignedByte));
            NonVariableData.AddMember(new JIPointer(new JIArray(VarData, null, 1, true), true)); //since this is an embedded pointer
            var NonVariableDataArray = new JIArray(NonVariableData, null, 1, true);

            var updateParamObj = new JILocalParamsDescriptor();
            updateParamObj.AddInParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
            updateParamObj.AddInParamAsObject(NonVariableDataArray, JIFlags.FLAG_NULL);
            var methodDescriptor = new JILocalMethodDescriptor("UpdateMe", updateParamObj);
            interfaceDefinition.AddMethodDescriptor(methodDescriptor);

            return interfaceDefinition;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void testStaticUpdateMeSink(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public static void TestStaticUpdateMeSink(string[] args) {

            var session = JISession.CreateSession(args[1], args[2], args[3]);
            var comStub = new JIComServer(JIProgId.ValueOf("TstMarsh.Test"), args[0], session);
            var unknown = comStub.CreateInstance();
            var ITest = unknown.QueryInterface("89D8C8BE-1E91-11D3-910F-00C04F9403C2"); //ITest

            //Create the Java Server class. This contains the instance to be called by the COM Server
            //
            var interfaceDefinition = RegisterInterface();
            if (_staticSinkJavaCoClass == null) {
                _staticSinkJavaCoClass = new JILocalCoClass(interfaceDefinition, new SampleTestServerCallback());
            }
            var iStaticSink = JIObjectFactory.BuildObject(session, _staticSinkJavaCoClass);

            var results = new object[1];
            // Create the session
            var javaCallback = new JICallBuilder(true) {
                Opnum = 0
            };
            javaCallback.AddInParamAsComObject(iStaticSink, JIFlags.FLAG_NULL);
            javaCallback.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL); //Long
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack about to call this...");
            results = ITest.Call(javaCallback); //<== same exception is thrown here as well
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack succeeded, session out =" + results[0]);
            var staticSession = (int)results[0];

            // set the refresh rate
            var rate = 4000;
            javaCallback.ReInit();
            javaCallback.Opnum = 4;
            javaCallback.AddInParamAsInt(staticSession, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            javaCallback.AddInParamAsInt(rate, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.SetSomethingInSomethingsRate about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.SetSomethingInSomethingsRate succeeded");

            //start the session
            javaCallback.ReInit();
            javaCallback.Opnum = 6;
            javaCallback.AddInParamAsInt(staticSession, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.StartSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.StartSomething succeeded");

            //stop the session
            Thread.Sleep(10000);
            javaCallback.ReInit();
            javaCallback.Opnum = 7;
            javaCallback.AddInParamAsInt(staticSession, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.StopSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.StopSomething succeeded");

            //destroy the session
            Thread.Sleep(1000);
            javaCallback.ReInit();
            javaCallback.Opnum = 1;
            javaCallback.AddInParamAsInt(staticSession, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.DestroySomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.DestroySomething succeeded");

            JISession.DestroySession(session);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void testSinkDebug(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public static void TestSinkDebug(string[] args) {

            var session = JISession.CreateSession(args[1], args[2], args[3]);
            var comStub = new JIComServer(JIProgId.ValueOf("TstMarsh.Test"), args[0], session);
            var unknown = comStub.CreateInstance();
            var ITest = unknown.QueryInterface("89D8C8BE-1E91-11D3-910F-00C04F9403C2"); //ITest

            //Create the Java Server class. This contains the instance to be called by the COM Server
            //
            var interfaceDefinition = RegisterInterface();
            if (_staticSinkJavaCoClass != null) {
                _staticSinkJavaCoClass = new JILocalCoClass(interfaceDefinition, new SampleTestServerCallback());
            }

            var iStaticSink = JIObjectFactory.BuildObject(session, _staticSinkJavaCoClass);

            var results = new object[1];
            // Create the session
            var javaCallback = new JICallBuilder(true) {
                Opnum = 8
            };
            javaCallback.AddInParamAsComObject(iStaticSink, JIFlags.FLAG_NULL);
            javaCallback.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL); //Long
            results = ITest.Call(javaCallback); //<== same exception is thrown here as well
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack succeeded, session out =" + results[0]);
            var staticSession = (int)results[0];

            Thread.Sleep(30000);

            javaCallback.ReInit();
            javaCallback.Opnum = 1;
            javaCallback.AddInParamAsInt(staticSession, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.UnDoSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.UnDoSomething succeeded");

            JISession.DestroySession(session);
        }

        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                JISystem.UseAutoRegistration = true;
                for (var i = 0; i < 100; i++) {
                    Console.WriteLine("**********************Invoking callback sequence....\n");
                    TestStaticUpdateMeSink(args);
                    Thread.Sleep(12000);
                }
                //            testSinkDebug(args);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

        internal static JILocalCoClass _staticSinkJavaCoClass;

    }

}
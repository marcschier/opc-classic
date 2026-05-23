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

        public void UpdateMe(ushort size, ComArray array) {
            Append("C:\\Test\\callback_j.log", "SampleTestServerCallback::UpdateMe entered with array size=" + size + "\n");
            Console.WriteLine("SampleTestServerCallback::UpdateMe entered with array size=" + size + "\n");
            var structArray = (Struct[])array.ArrayInstance;
            for (var i = 0; i < size; i++) {
                Append("C:\\Test\\callback_j.log", "Member 0= " + structArray[i].GetMember(0).ToString() + "\n");
                Console.WriteLine("Array elt=" + i + ",Member 0= " + structArray[i].GetMember(0).ToString() + "\n");
            }
        }

        private static LocalInterfaceDefinition RegisterInterface() {
            // Now for the Java Implementation of SampleTestServer2 interface (from the type library or IDL)
            var interfaceDefinition = new LocalInterfaceDefinition("D3F9CE10-686C-11d2-97BF-006008BD50B1", false); // IStatisUpdateMeSink

            var VarData = new Struct(); // Will add in the struct later on
            VarData.AddMember(typeof(uint));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(ushort));
            VarData.AddMember(typeof(float));
            VarData.AddMember(typeof(DateTime));
            VarData.AddMember(typeof(ushort));

            var NonVariableData = new Struct(); // Will add in the struct later on
            NonVariableData.AddMember(typeof(uint));
            NonVariableData.AddMember(typeof(uint));
            NonVariableData.AddMember(typeof(byte));
            NonVariableData.AddMember(new ComPointer(new ComArray(VarData, null, 1, true), true)); // since this is an embedded pointer
            var NonVariableDataArray = new ComArray(NonVariableData, null, 1, true);

            var updateParamObj = new LocalParamsDescriptor();
            updateParamObj.AddInParamAsType(typeof(ushort));
            updateParamObj.AddInParamAsObject(NonVariableDataArray);
            var methodDescriptor = new LocalMethodDescriptor("UpdateMe", updateParamObj);
            interfaceDefinition.AddMethodDescriptor(methodDescriptor);

            return interfaceDefinition;
        }


        public static void TestStaticUpdateMeSink(string[] args) {

            var session = Session.CreateSession(args[1], args[2], args[3]);
            var comStub = new ComServer(ProgId.ValueOf("TstMarsh.Test"), args[0], session);
            var unknown = comStub.CreateInstance();
            var ITest = unknown.QueryInterface("89D8C8BE-1E91-11D3-910F-00C04F9403C2"); // ITest

            // Create the Java Server class. This contains the instance to be called by the COM Server
            //
            var interfaceDefinition = RegisterInterface();
            if (_staticSinkJavaCoClass == null) {
                _staticSinkJavaCoClass = new LocalCoClass(interfaceDefinition, new SampleTestServerCallback());
            }
            var iStaticSink = ObjectFactory.BuildObject(session, _staticSinkJavaCoClass);

            var results = new object[1];
            // Create the session
            var javaCallback = new CallBuilder(true) {
                Opnum = 0
            };
            javaCallback.AddInParamAsComObject(iStaticSink);
            javaCallback.AddOutParamAsType(typeof(int)); // Long
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack about to call this...");
            results = ITest.Call(javaCallback); // <== same exception is thrown here as well
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack succeeded, session out =" + results[0]);
            var staticSession = (int)results[0];

            // set the refresh rate
            var rate = 4000;
            javaCallback.ReInit();
            javaCallback.Opnum = 4;
            javaCallback.AddInParamAsInt(staticSession, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            javaCallback.AddInParamAsInt(rate, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.SetSomethingInSomethingsRate about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.SetSomethingInSomethingsRate succeeded");

            // start the session
            javaCallback.ReInit();
            javaCallback.Opnum = 6;
            javaCallback.AddInParamAsInt(staticSession, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.StartSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.StartSomething succeeded");

            // stop the session
            Thread.Sleep(10000);
            javaCallback.ReInit();
            javaCallback.Opnum = 7;
            javaCallback.AddInParamAsInt(staticSession, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.StopSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.StopSomething succeeded");

            // destroy the session
            Thread.Sleep(1000);
            javaCallback.ReInit();
            javaCallback.Opnum = 1;
            javaCallback.AddInParamAsInt(staticSession, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.DestroySomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.DestroySomething succeeded");

            Session.DestroySession(session);
        }


        public static void TestSinkDebug(string[] args) {

            var session = Session.CreateSession(args[1], args[2], args[3]);
            var comStub = new ComServer(ProgId.ValueOf("TstMarsh.Test"), args[0], session);
            var unknown = comStub.CreateInstance();
            var ITest = unknown.QueryInterface("89D8C8BE-1E91-11D3-910F-00C04F9403C2"); // ITest

            // Create the Java Server class. This contains the instance to be called by the COM Server
            //
            var interfaceDefinition = RegisterInterface();
            if (_staticSinkJavaCoClass != null) {
                _staticSinkJavaCoClass = new LocalCoClass(interfaceDefinition, new SampleTestServerCallback());
            }

            var iStaticSink = ObjectFactory.BuildObject(session, _staticSinkJavaCoClass);

            var results = new object[1];
            // Create the session
            var javaCallback = new CallBuilder(true) {
                Opnum = 8
            };
            javaCallback.AddInParamAsComObject(iStaticSink);
            javaCallback.AddOutParamAsType(typeof(int)); // Long
            results = ITest.Call(javaCallback); // <== same exception is thrown here as well
            Console.WriteLine("ITest.DoSomethingAndGetSomethingBack succeeded, session out =" + results[0]);
            var staticSession = (int)results[0];

            Thread.Sleep(30000);

            javaCallback.ReInit();
            javaCallback.Opnum = 1;
            javaCallback.AddInParamAsInt(staticSession, InteropFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
            Console.WriteLine("ITest.UnDoSomething about to be called");
            results = ITest.Call(javaCallback);
            Console.WriteLine("ITest.UnDoSomething succeeded");

            Session.DestroySession(session);
        }

        public static void RunTest(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                Interop.UseAutoRegistration = true;
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

        internal static LocalCoClass _staticSinkJavaCoClass;

    }

}
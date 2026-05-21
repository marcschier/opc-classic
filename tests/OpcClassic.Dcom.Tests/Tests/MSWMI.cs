//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using SharpInterop.Automation;
    using SharpInterop.Common;
    using SharpInterop.Core;
    using System;

    public class MSWMI {

        private readonly ComServer _comStub;
        private readonly IComObject _comObject;
        private readonly IDispatch _dispatch;
        private readonly string _address;
        private readonly Session _session;

        public MSWMI(string address, string[] args) {
            _address = address;
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _session.UseSessionSecurity(true);
            _session.GlobalSocketTimeout = 5000;
            _comStub = new ComServer(ProgId.ValueOf("WbemScripting.SWbemLocator"), address, _session);
            var unknown = _comStub.CreateInstance();
            _comObject = unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); // ISWbemLocator
                                                                                         // This will obtain the dispatch interface
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_comObject.QueryInterface(Interfaces.IID_IDispatch));
        }



        public void PerformOp() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var results = _dispatch.CallMethodA("ConnectServer", new object[] { new ComString(_address), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), 0, Variant.CreateOPTIONAL_PARAM() });

            // using the dispatch results above you can use the "ConnectServer" api to retrieve a pointer to <see cref="IDispatch"/>
            // of ISWbemServices

            // OR
            // Make a direct call like below, in this case you would get back an interface pointer to ISWbemServices, NOT to it's IDispatch
            var callObject = new CallBuilder();
            callObject.AddInParamAsString(_address, InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsString("", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsInt(0);
            callObject.AddInParamAsPointer(null);
            callObject.Opnum = 0;
            callObject.AddOutParamAsType(typeof(IComObject));
            var wbemServices = ObjectFactory.NarrowObject((IComObject)_comObject.Call(callObject)[0]);
            wbemServices.InstanceLevelSocketTimeout = 1000;
            wbemServices.RegisterUnreferencedHandler(new UnreferencedHandler1());

            // Lets have a look at both.
            var wbemServices_dispatch = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            results = wbemServices_dispatch.CallMethodA("InstancesOf", new object[] { new ComString("Win32_Process"), 0, Variant.CreateOPTIONAL_PARAM() });
            var wbemObjectSet_dispatch = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            var variant = wbemObjectSet_dispatch.Get("_NewEnum");
            var object2 = variant.ObjectAsComObject;

            Console.WriteLine(object2.DispatchSupported);
            Console.WriteLine(object2.DispatchSupported);

            object2.RegisterUnreferencedHandler(new UnreferencedHandler2());

            var enumVARIANT = (IEnumVariant)ObjectFactory.NarrowObject(object2.QueryInterface(Interfaces.IID_IEnumVARIANT));

            // This will return back a dispatch of ISWbemObjectSet

            // OR
            // It returns back the pointer to ISWbemObjectSet
            callObject = new CallBuilder();
            callObject.AddInParamAsString("Win32_Process", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddInParamAsInt(0);
            callObject.AddInParamAsPointer(null);
            callObject.Opnum = 4;
            callObject.AddOutParamAsType(typeof(IComObject));
            var wbemObjectSet = ObjectFactory.NarrowObject((IComObject)wbemServices.Call(callObject)[0]);

            // okay seen enough of the other usage, lets just stick to disptach, it's lot simpler
            var Count = wbemObjectSet_dispatch.Get("Count");
            var count = Count.ObjectAsInt;
            for (var i = 0; i < count; i++) {
                var values = enumVARIANT.Next(1);
                var array = (ComArray)values[0];
                var arrayObj = (object[])array.ArrayInstance;
                for (var j = 0; j < arrayObj.Length; j++) {
                    var wbemObject_dispatch = (IDispatch)ObjectFactory.NarrowObject(((Variant)arrayObj[j]).ObjectAsComObject);
                    var variant2 = wbemObject_dispatch.CallMethodA("GetObjectText_", new object[] { 1 })[0];
                    Console.WriteLine(variant2.ObjectAsString.String);
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                }
            }
        }

        private class UnreferencedHandler1 : IUnreferenced {
            public void UnReferenced() => Console.WriteLine("wbemServices unreferenced... ");
        }

        private class UnreferencedHandler2 : IUnreferenced {
            public void UnReferenced() => Console.WriteLine("object2 unreferenced...");
        }

        private void Killme() => Session.DestroySession(_session);
        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }

                Interop.UseAutoRegistration = true;
                var test = new MSWMI(args[0], args);
                for (var i = 0; i < 100; i++) {
                    Console.WriteLine("Index i: " + i);
                    test.PerformOp();
                }
                test.Killme();
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
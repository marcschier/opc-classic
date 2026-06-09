// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    /// <summary>
    /// WMI example showing how to use a new logger implementation.
    /// </summary>
    public class MSWMI2 {

#pragma warning disable IDE0052 // Remove unread private members
        private readonly string _address;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly ComServer _comStub;
        private readonly IComObject _comObject;
        private readonly IDispatch _dispatch;
        private readonly Session _session;

        public MSWMI2(string address, string[] args) {
            _address = address;
            _session = Session.CreateSession(args[1], args[2], args[3]);
            //        session.useSessionSecurity(true);
            //        session.setGlobalSocketTimeout(5000);
            _comStub = new ComServer(Clsid.ValueOf("76a64158-cb41-11d1-8b02-00600806d9b6"), address, _session);
            var unknown = _comStub.CreateInstance();
            _comObject = unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); // ISWbemLocator
                                                                                         // This will obtain the dispatch interface
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_comObject.QueryInterface(Interfaces.IID_IDispatch));
        }



        public void PerformOp() {
            //        <see cref="IDispatch"/> securityDisp = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.narrowObject(dispatch.get("Security_").getObjectAsComObject());
            //        securityDisp.put("ImpersonationLevel", new <see cref="Variant"/>(3));
            var results = _dispatch.CallMethodA("ConnectServer", new object[] { Variant.CreateOPTIONAL_PARAM(), new ComString("ROOT\\CIMV2"), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), 0, Variant.CreateOPTIONAL_PARAM() });

            var wbemServices_dispatch = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            results = wbemServices_dispatch.CallMethodA("ExecQuery", new object[] { new ComString("select * from Win32_OperatingSystem where Primary=True"), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            var wbemObjectSet_dispatch = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            var variant = wbemObjectSet_dispatch.Get("_NewEnum");
            var object2 = variant.ObjectAsComObject;

            var enumVARIANT = (IEnumVariant)ObjectFactory.NarrowObject(object2.QueryInterface(Interfaces.IID_IEnumVARIANT));

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

        private void Killme() => Session.DestroySession(_session);

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }


                Interop.UseAutoRegistration = true;
                var test = new MSWMI2(args[0], args);
                for (var i = 0; i < 2; i++) {
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

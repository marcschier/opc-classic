// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    // StdCollection.VBCollection
    public class MSEnumVariant {

        private readonly ComServer _comServer;
        private readonly Session _session;
        private readonly IDispatch _dispatch;


        public MSEnumVariant(string address, string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("StdCollection.VBCollection"), address, _session);
            var @object = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(@object.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void PerformOp() {
            var i = 0;
            for (; i < 5; i++) {
                _dispatch.CallMethod("Add", new object[] { i, new ComString("Key-" + i) });
            }

            for (; i < 10; i++) {
                _dispatch.CallMethod("Add", new object[] { i, Variant.CreateOPTIONAL_PARAM() });
            }

            var variant = _dispatch.Get("_NewEnum");

            var object2 = variant.ObjectAsComObject;
            // var enumObject = (IComObject)object2.queryInterface(Interfaces.IID_IEnumVARIANT);

            var enumVARIANT = (IEnumVariant)ObjectFactory.NarrowObject(object2.QueryInterface(Interfaces.IID_IEnumVARIANT));

            for (i = 0; i < 10; i++) {
                var vals = enumVARIANT.Next(1);
                var array = (ComArray)vals[0];
                var arrayObj = (object[])array.ArrayInstance;
                for (var j = 0; j < arrayObj.Length; j++) {
                    Console.WriteLine(((Variant)arrayObj[j]).ObjectAsInt + "," + (int)vals[1]);
                }

            }

            enumVARIANT.Reset();
            var values = enumVARIANT.Next(5);
            enumVARIANT.Next(1);
            enumVARIANT.Skip(2);
            values = enumVARIANT.Next(1);
            var newenum = enumVARIANT.Clone();
            newenum.Reset();
            values = newenum.Next(10);

            Session.DestroySession(_session);
        }


        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                Interop.UseAutoRegistration = true;
                var enumVariant = new MSEnumVariant(args[0], args);
                enumVariant.PerformOp();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

    }

}
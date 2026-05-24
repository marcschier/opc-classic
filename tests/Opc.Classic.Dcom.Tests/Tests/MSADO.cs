// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    public class MSADO {

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;
        private readonly Session _session;

        public MSADO(string address, string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("ADODB.Connection"), address, _session);
        }



        public void PerformOp() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeInfo = _dispatch.GetTypeInfo(0);
            typeInfo.GetFuncDesc(0);

            _dispatch.CallMethod("Open", new object[] { new ComString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), -1 });

            var variant = _dispatch.CallMethodA("Execute", new object[] { new ComString("SELECT * FROM Products"), -1 });
            if (variant[0].IsNull) {
                Console.WriteLine("Recordset is empty.");
            }
            else {
                var resultSet = (IDispatch)ObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
                // variant = resultSet.get("EOF");
                while (!resultSet.Get("EOF").ObjectAsBoolean) {
                    var variant2 = resultSet.Get("Fields");
                    var fields = (IDispatch)ObjectFactory.NarrowObject(variant2.ObjectAsComObject);
                    var count = fields.Get("Count").ObjectAsInt;
                    for (var i = 0; i < count; i++) {
                        variant = fields.Get("Item", new object[] { i });
                        var field = (IDispatch)ObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
                        variant2 = field.Get("Value");
                        object val = null;
                        if (variant2.Type == VariantType.VT_BSTR) {
                            val = variant2.ObjectAsString.String;
                        }
                        if (variant2.Type == VariantType.VT_I4) {
                            val = variant2.ObjectAsInt;
                        }
                        Console.WriteLine(field.Get("Name").ObjectAsString.String + " = " + val + "[" + variant2.Type + "]");
                    }
                    resultSet.CallMethod("MoveNext");
                }


            }

            Session.DestroySession(_session);
        }

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                Interop.UseAutoRegistration = true;
                var test = new MSADO(args[0], args);
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
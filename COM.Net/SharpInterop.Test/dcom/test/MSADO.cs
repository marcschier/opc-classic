namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    public class MSADO {

        private readonly JIComServer _comServer;
        private IJIDispatch _dispatch;
        private IComObject _unknown;
        private readonly JISession _session;

        public MSADO(string address, string[] args) {
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            _comServer = new JIComServer(JIProgId.ValueOf("ADODB.Connection"), address, _session);
        }



        public void PerformOp() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeInfo = _dispatch.GetTypeInfo(0);
            typeInfo.GetFuncDesc(0);

            _dispatch.CallMethod("Open", new object[] { new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), -1 });

            var variant = _dispatch.CallMethodA("Execute", new object[] { new JIString("SELECT * FROM Products"), -1 });
            if (variant[0].IsNull) {
                Console.WriteLine("Recordset is empty.");
            }
            else {
                var resultSet = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
                // variant = resultSet.get("EOF");
                while (!resultSet.Get("EOF").ObjectAsBoolean) {
                    var variant2 = resultSet.Get("Fields");
                    var fields = (IJIDispatch)JIObjectFactory.NarrowObject(variant2.ObjectAsComObject);
                    var count = fields.Get("Count").ObjectAsInt;
                    for (var i = 0; i < count; i++) {
                        variant = fields.Get("Item", new object[] { i });
                        var field = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
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

            JISession.DestroySession(_session);
        }

        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                JISystem.UseAutoRegistration = true;
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
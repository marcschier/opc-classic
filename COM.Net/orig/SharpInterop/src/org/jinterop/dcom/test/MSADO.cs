using System;

namespace org.jinterop.dcom.test {



    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
    using IJITypeInfo = org.jinterop.dcom.impls.automation.IJITypeInfo;

    public class MSADO {

        private JIComServer ComServer = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;
        private JISession Session = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSADO(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSADO(string address, string[] args) {
            Session = JISession.CreateSession(args[1],args[2],args[3]);
            ComServer = new JIComServer(JIProgId.ValueOf("ADODB.Connection"),address,Session);
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {
            Unknown = ComServer.CreateInstance();
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
            IJITypeInfo typeInfo = Dispatch.GetTypeInfo(0);
            typeInfo.GetFuncDesc(0);

            Dispatch.CallMethod("Open",new object[]{ new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),new int?(-1) });

            JIVariant[] variant = Dispatch.CallMethodA("Execute",new object[]{ new JIString("SELECT * FROM Products"),new int?(-1) });
            if (variant[0].Null) {
                Console.WriteLine("Recordset is empty.");
            }
            else {
                IJIDispatch resultSet = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
                //variant = resultSet.get("EOF");
                while (!resultSet.Get("EOF").ObjectAsBoolean) {
                    JIVariant variant2 = resultSet.Get("Fields");
                    IJIDispatch fields = (IJIDispatch)JIObjectFactory.NarrowObject(variant2.ObjectAsComObject);
                    int count = fields.Get("Count").ObjectAsInt;
                    for (int i = 0;i < count;i++) {
                        variant = fields.Get("Item",new object[]{ new int?(i) });
                        IJIDispatch field = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
                        variant2 = field.Get("Value");
                        object val = null;
                        if (variant2.Type == JIVariant.VT_BSTR) {
                            val = variant2.ObjectAsString.String;
                        }
                        if (variant2.Type == JIVariant.VT_I4) {
                            val = new int?(variant2.ObjectAsInt);
                        }
                        Console.WriteLine(field.Get("Name").ObjectAsString.String + " = " + val + "[" + variant2.Type + "]");
                    }
                    resultSet.CallMethod("MoveNext");
                }


            }

            JISession.DestroySession(Session);
        }

        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    JISystem.AutoRegisteration = true;
                    MSADO test = new MSADO(args[0],args);
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
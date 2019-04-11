namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    /// <summary>
    /// WMI example showing how to use a new logger implementation.
    /// 
    /// @since 1.23
    /// 
    /// </summary>
    public class MSWMI2 {

#pragma warning disable IDE0052 // Remove unread private members
        private readonly string _address;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly JIComServer _comStub;
        private readonly IJIComObject _comObject;
        private readonly IJIDispatch _dispatch;
        private readonly JISession _session;
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public MSWMI2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSWMI2(string address, string[] args) {
            _address = address;
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            //		session.useSessionSecurity(true);
            //		session.setGlobalSocketTimeout(5000);
            _comStub = new JIComServer(JIClsid.ValueOf("76a64158-cb41-11d1-8b02-00600806d9b6"), address, _session);
            var unknown = _comStub.CreateInstance();
            _comObject = unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); //ISWbemLocator
                                                                                         //This will obtain the dispatch interface
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_comObject.QueryInterface(Interfaces.IID_IDispatch));
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {
            //		IJIDispatch securityDisp = (IJIDispatch)JIObjectFactory.narrowObject(dispatch.get("Security_").getObjectAsComObject());
            //		securityDisp.put("ImpersonationLevel", new JIVariant(3));
            var results = _dispatch.CallMethodA("ConnectServer", new object[] { JIVariant.CreateOPTIONAL_PARAM(), new JIString("ROOT\\CIMV2"), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), 0, JIVariant.CreateOPTIONAL_PARAM() });

            var wbemServices_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            results = wbemServices_dispatch.CallMethodA("ExecQuery", new object[] { new JIString("select * from Win32_OperatingSystem where Primary=True"), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            var wbemObjectSet_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);
            var variant = wbemObjectSet_dispatch.Get("_NewEnum");
            var object2 = variant.ObjectAsComObject;

            var enumVARIANT = (IJIEnumVariant)JIObjectFactory.NarrowObject(object2.QueryInterface(Interfaces.IID_IEnumVARIANT));

            var Count = wbemObjectSet_dispatch.Get("Count");
            var count = Count.ObjectAsInt;
            for (var i = 0; i < count; i++) {
                var values = enumVARIANT.Next(1);
                var array = (JIArray)values[0];
                var arrayObj = (object[])array.ArrayInstance;
                for (var j = 0; j < arrayObj.Length; j++) {
                    var wbemObject_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(((JIVariant)arrayObj[j]).ObjectAsComObject);
                    var variant2 = wbemObject_dispatch.CallMethodA("GetObjectText_", new object[] { 1 })[0];
                    Console.WriteLine(variant2.ObjectAsString.String);
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                }
            }


        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void killme() throws org.jinterop.dcom.common.JIException
        private void Killme() => JISession.DestroySession(_session);

        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }


                JISystem.UseAutoRegistration = true;
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
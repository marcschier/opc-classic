using System;

namespace org.jinterop.dcom.test {




    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIArray = org.jinterop.dcom.core.JIArray;
    using JIClsid = org.jinterop.dcom.core.JIClsid;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
    using IJIEnumVariant = org.jinterop.dcom.impls.automation.IJIEnumVariant;

    /// <summary>
    /// WMI example showing how to use a new logger implementation.
    /// 
    /// @since 1.23
    /// 
    /// </summary>
    public class MSWMI2 {

        private JIComServer ComStub = null;
        private IJIComObject ComObject = null;
        private IJIDispatch Dispatch = null;
        private string Address = null;
        private JISession Session = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWMI2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSWMI2(string address, string[] args) {
            this.Address = address;
            Session = JISession.CreateSession(args[1],args[2],args[3]);
    //        session.useSessionSecurity(true);
    //        session.setGlobalSocketTimeout(5000);
            ComStub = new JIComServer(JIClsid.ValueOf("76a64158-cb41-11d1-8b02-00600806d9b6"),address,Session);
            IJIComObject unknown = ComStub.CreateInstance();
            ComObject = (IJIComObject)unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); //ISWbemLocator
            //This will obtain the dispatch interface
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(ComObject.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {
    //        IJIDispatch securityDisp = (IJIDispatch)JIObjectFactory.narrowObject(dispatch.get("Security_").getObjectAsComObject());
    //        securityDisp.put("ImpersonationLevel", new JIVariant(3));
            JIVariant[] results = Dispatch.CallMethodA("ConnectServer",new object[]{ JIVariant.OPTIONAL_PARAM(),new JIString("ROOT\\CIMV2"),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),new int?(0),JIVariant.OPTIONAL_PARAM() });

            IJIDispatch wbemServices_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((results[0]).ObjectAsComObject);
            results = wbemServices_dispatch.CallMethodA("ExecQuery", new object[]{ new JIString("select * from Win32_OperatingSystem where Primary=True"), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            IJIDispatch wbemObjectSet_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((results[0]).ObjectAsComObject);
            JIVariant variant = wbemObjectSet_dispatch.Get("_NewEnum");
            IJIComObject object2 = variant.ObjectAsComObject;

            IJIEnumVariant enumVARIANT = (IJIEnumVariant)JIObjectFactory.NarrowObject(object2.QueryInterface(org.jinterop.dcom.impls.automation.IJIEnumVariant_Fields.IID));

            JIVariant Count = wbemObjectSet_dispatch.Get("Count");
            int count = Count.ObjectAsInt;
            for (int i = 0; i < count; i++) {
                object[] values = enumVARIANT.Next(1);
                JIArray array = (JIArray)values[0];
                object[] arrayObj = (object[])array.ArrayInstance;
                for (int j = 0; j < arrayObj.Length; j++) {
                    IJIDispatch wbemObject_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(((JIVariant)arrayObj[j]).ObjectAsComObject);
                    JIVariant variant2 = (JIVariant)(wbemObject_dispatch.CallMethodA("GetObjectText_",new object[]{ new int?(1) }))[0];
                    Console.WriteLine(variant2.ObjectAsString.String);
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                }
            }


        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void killme() throws org.jinterop.dcom.common.JIException
        private void Killme() {
            JISession.DestroySession(Session);
        }

        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }

                    JISystem.InBuiltLogHandler = false;
                    JISystem.Logger.Level = Level.FINEST;
                    JISystem.AutoRegisteration = true;
                    MSWMI2 test = new MSWMI2(args[0],args);
                    for (int i = 0 ; i < 2; i++) {
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
namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    //StdCollection.VBCollection
    public class MSEnumVariant {

        private readonly JIComServer _comServer;
        private readonly JISession _session;
        private readonly IJIDispatch _dispatch;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public MSEnumVariant(String address,String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSEnumVariant(string address, string[] args) {
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            _comServer = new JIComServer(JIProgId.ValueOf("StdCollection.VBCollection"), address, _session);
            var @object = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(@object.QueryInterface(Interfaces.IID_IDispatch));
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException
        public virtual void PerformOp() {
            var i = 0;
            for (; i < 5; i++) {
                _dispatch.CallMethod("Add", new object[] { i, new JIString("Key-" + i) });
            }

            for (; i < 10; i++) {
                _dispatch.CallMethod("Add", new object[] { i, JIVariant.CreateOPTIONAL_PARAM() });
            }

            var variant = _dispatch.Get("_NewEnum");

            var object2 = variant.ObjectAsComObject;
            //IJIComObject enumObject = (IJIComObject)object2.queryInterface(IJIEnumVARIANT.IID);

            var enumVARIANT = (IJIEnumVariant)JIObjectFactory.NarrowObject(object2.QueryInterface(Interfaces.IID_IEnumVARIANT));

            for (i = 0; i < 10; i++) {
                var vals = enumVARIANT.Next(1);
                var array = (JIArray)vals[0];
                var arrayObj = (object[])array.ArrayInstance;
                for (var j = 0; j < arrayObj.Length; j++) {
                    Console.WriteLine(((JIVariant)arrayObj[j]).ObjectAsInt + "," + (int)vals[1]);
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
            i = 0;

            JISession.DestroySession(_session);
        }


        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                JISystem.UseAutoRegistration = true;
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
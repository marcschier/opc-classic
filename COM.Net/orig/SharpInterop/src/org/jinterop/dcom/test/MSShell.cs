using System;

namespace org.jinterop.dcom.test {

    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIFlags = org.jinterop.dcom.core.JIFlags;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;

    public class MSShell {

        internal JISession Session = null;
        internal JIComServer ComServer = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSShell(String args[]) throws java.net.UnknownHostException, org.jinterop.dcom.common.JIException
        public MSShell(string[] args) {
            Session = JISession.CreateSession(args[1],args[2],args[3]);
            ComServer = new JIComServer(JIProgId.ValueOf("Shell.Application"),args[0],Session);
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void doStuff() throws org.jinterop.dcom.common.JIException
        public virtual void DoStuff() {
            //this will return a reference to the IUnknown of the Shell coclass.
            IJIComObject comUnknown = (IJIComObject)ComServer.CreateInstance();

            //now we query for the IShellDispatch interface
            IJIComObject shellDispatch = (IJIComObject)comUnknown.QueryInterface("D8F015C0-C278-11CE-A49E-444553540000");

            JICallBuilder callObject = new JICallBuilder();
    //        callObject.setOpnum(5);
    //        callObject.addInParamAsVariant(new JIVariant(new JIString("c:")),JIFlags.FLAG_NULL);
    //        Object result[] = shellDispatch.call(callObject);

    //        callObject.reInit();
    //        callObject.setOpnum(7);
    //        result = shellDispatch.call(callObject);

            callObject.ReInit();
            callObject.Opnum = 2;
            callObject.AddInParamAsVariant(new JIVariant(2),JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            object[] result = shellDispatch.Call(callObject);
            IJIComObject folder = JIObjectFactory.NarrowObject((IJIComObject)result[0]);

            callObject = new JICallBuilder();
            callObject.Opnum = 0;
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
            result = folder.Call(callObject);
            Console.WriteLine("Current Folder: " + result[0]);

            callObject.ReInit();
            callObject.Opnum = 1;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            result = folder.Call(callObject);
            IJIComObject test = JIObjectFactory.NarrowObject((IJIComObject)result[0]);

    //        Not implemented by shell
    //        callObject.reInit();
    //        callObject.setOpnum(2);
    //        callObject.addOutParamAsType(JIInterfacePointer.class,JIFlags.FLAG_NULL);
    //        result = folder.call(callObject);
    //        test = JIObjectFactory.createCOMInstance(shellDispatch,(JIInterfacePointer)result[0]);

            callObject.ReInit();
            callObject.Opnum = 3;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            result = folder.Call(callObject);
            test = JIObjectFactory.NarrowObject((IJIComObject)result[0]);

            callObject.ReInit();
            callObject.Opnum = 4;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            result = folder.Call(callObject);
            IJIComObject folderItems = JIObjectFactory.NarrowObject((IJIComObject)result[0]);

            callObject = new JICallBuilder();
            callObject.Opnum = 0;
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
            result = folderItems.Call(callObject);

            int count = (int)((int?)result[0]);

            for (int i = 0;i < count;i++) {
                callObject.ReInit();
                callObject.Opnum = 3;
                callObject.AddInParamAsVariant(new JIVariant(i),JIFlags.FLAG_NULL);
                callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
                result = folderItems.Call(callObject);
                IJIComObject folderItem = JIObjectFactory.NarrowObject((IJIComObject)result[0]);


                JICallBuilder callObject2 = new JICallBuilder();
                callObject2.Opnum = 2;
                callObject2.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
                result = folderItem.Call(callObject2);
                Console.WriteLine("Name of Object: " + result[0]);

                callObject2.ReInit();
                callObject2.Opnum = 4;
                callObject2.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
                result = folderItem.Call(callObject2);
                Console.WriteLine("Path of the Object: " + result[0]);


                callObject2.ReInit();
                callObject2 = new JICallBuilder();
                callObject2.Opnum = 9;
                //VARIANT_BOOL is Boolean
                callObject2.AddOutParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
                result = folderItem.Call(callObject2);

                bool isFileSystemObject = (bool)((bool?)result[0]);

                if (isFileSystemObject) {
                    Console.Write(" and is part of file system\n");
                }
                else {
                    Console.Write(" and is not part of file system\n");
                }

                callObject2.ReInit();
                callObject2 = new JICallBuilder();
                callObject2.Opnum = 13;
                callObject2.AddOutParamAsObject((typeof(int?)),JIFlags.FLAG_NULL);
                result = folderItem.Call(callObject2);
                Console.Write(" and size(in bytes) is: " + (int)((int?)result[0]) + "\n");

            }

        }



        public static void Main(string[] args) {

            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            JISystem.AutoRegisteration = true;
            try {
                MSShell shell = new MSShell(args);
                shell.DoStuff();
                JISession.DestroySession(shell.Session);
            }
            catch (UnknownHostException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (JIException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

    }

}
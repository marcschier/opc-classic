using System;
using System.Threading;

namespace org.jinterop.dcom.test {



    using JIException = org.jinterop.dcom.common.JIException;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class MSWord2 {

        private JIComServer ComStub = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWord2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSWord2(string address, string[] args) {
            JISession session = JISession.CreateSession(args[1],args[2],args[3]);
            ComStub = new JIComServer(JIProgId.ValueOf("Word.Application"),address,session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
        public virtual void StartWord() {
            Unknown = ComStub.CreateInstance();
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
        public virtual void ShowWord() {
            int dispId = Dispatch.GetIDsOfNames("Visible");
            JIVariant variant = new JIVariant(true);
            Dispatch.Put(dispId,variant);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {
            Console.WriteLine(((JIVariant)Dispatch.Get("Version")).ObjectAsString.String);
            Console.WriteLine(((JIVariant)Dispatch.Get("Path")).ObjectAsString.String);
            JIVariant variant = Dispatch.Get("Documents");
            //JIInterfacePointer ptr = variant.getObjectAsInterfacePointer();
            //IJIDispatch documents = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
            IJIDispatch documents = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            JIString filePath = new JIString("c:/temp/test.doc");
            JIVariant[] variant2 = documents.CallMethodA("open",new object[]{ filePath.VariantByRef,JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            //IJIDispatch document = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant2[0].getObjectAsInterfacePointer());
            IJIDispatch document = (IJIDispatch)JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = document.Get("Content");
            //IJIDispatch range = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant.getObjectAsInterfacePointer());
            IJIDispatch range = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            variant = range.Get("Find");
            IJIDispatch find = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            Thread.Sleep(2000);
            JIString findString = new JIString("ow");
            JIString replaceString = new JIString("igh");
            find.CallMethodA("Execute",new object[]{ findString.VariantByRef,JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),replaceString.VariantByRef,JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });
            Thread.Sleep(5000);

            Dispatch.CallMethod("Quit", new object[]{ new JIVariant(-1,true),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            JISession.DestroySession(Dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    MSWord2 test = new MSWord2(args[0],args);
                    test.StartWord();
                    test.ShowWord();
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
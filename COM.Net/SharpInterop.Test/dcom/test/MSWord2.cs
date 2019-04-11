namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class MSWord2 {

        private readonly JIComServer _comStub;
        private IJIDispatch _dispatch;
        private IComObject _unknown;


        public MSWord2(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);
            _comStub = new JIComServer(JIProgId.ValueOf("Word.Application"), address, session);
        }


        public void StartWord() {
            _unknown = _comStub.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowWord() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(true);
            _dispatch.Put(dispId, variant);
        }


        public void PerformOp() {
            Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
            Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);
            var variant = _dispatch.Get("Documents");
            // JIInterfacePointer ptr = variant.getObjectAsInterfacePointer();
            // IJIDispatch documents = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
            var documents = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            var filePath = new JIString("c:/temp/test.doc");
            var variant2 = documents.CallMethodA("open", new object[] { filePath.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            // IJIDispatch document = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant2[0].getObjectAsInterfacePointer());
            var document = (IJIDispatch)JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = document.Get("Content");
            // IJIDispatch range = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant.getObjectAsInterfacePointer());
            var range = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            variant = range.Get("Find");
            var find = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            Thread.Sleep(2000);
            var findString = new JIString("ow");
            var replaceString = new JIString("igh");
            find.CallMethodA("Execute", new object[] { findString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), replaceString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            Thread.Sleep(5000);

            _dispatch.CallMethod("Quit", new object[] { new JIVariant(-1, true), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            JISession.DestroySession(_dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MSWord2(args[0], args);
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
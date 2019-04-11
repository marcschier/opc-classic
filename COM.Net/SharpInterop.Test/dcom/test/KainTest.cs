
namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class KainTest {

        private readonly JIComServer _comServer;
        private readonly IJIDispatch _dispatch;
        private IJIComObject _unknown;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public KainTest(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public KainTest(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);
            _comServer = new JIComServer(JIProgId.ValueOf("Word.Application"), address, session);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
        public virtual void StartWord() {
            _unknown = _comServer.CreateInstance();
            var dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
        public virtual void ShowWord() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(true);
            _dispatch.Put(dispId, variant);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual void PerformOp() {
            var sDir = "c:\\tmp\\";
            var sInputDoc = sDir + "file_in.doc";
            var sOutputDoc = sDir + "file_out.doc";

            var sOldText = "[label:import:1]";
            var sNewText = "I am some horribly long sentence, so long that [insert something long here]";

            Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
            Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);

            var variant = _dispatch.Get("Documents");
            var documents = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            //String has to be a JIString.
            var filePath = new JIString(sInputDoc);
            //this "open" is of Word 2003
            var variant2 = documents.CallMethodA("open", new object[] { new JIVariant(filePath, true), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });

            var document = (IJIDispatch)JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = _dispatch.Get("Selection");
            var selection = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            variant = selection.Get("Find");
            var find = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            Thread.Sleep(2000);

            find.Put("Text", new JIVariant(new JIString(sOldText)));
            find.CallMethodA("Execute", new object[] { JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });

            Thread.Sleep(2000);

            selection.Put("Text", new JIVariant(new JIString(sNewText)));
            selection.CallMethodA("MoveDown", new object[] { JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            selection.Put("Text", new JIVariant(new JIString("\nSo we got the next line including BR.\n")));

            variant = selection.Get("Font");
            var font = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            font.Put("Bold", new JIVariant(1));
            font.Put("Italic", new JIVariant(1));
            font.Put("Underline", new JIVariant(0));

            variant = selection.Get("ParagraphFormat");
            var align = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            align.Put("Alignment", new JIVariant(3));

            Thread.Sleep(5000);

            var sImgFile = new JIString(sDir + "image.png");
            selection.CallMethodA("MoveDown", new object[] { JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            variant = selection.Get("InLineShapes");
            var image = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            image.CallMethodA("AddPicture", new object[] { new JIVariant(sImgFile), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });

            var sHyperlink = new JIString("http://www.google.com");
            selection.Put("Text", new JIVariant(new JIString("Text for the link to Google")));
            variant = selection.Get("Range");
            var range = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            variant = document.Get("Hyperlinks");
            var link = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            link.CallMethod("Add", new object[] { range, sHyperlink, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });

            variant = _dispatch.Get("WordBasic");
            var wordBasic = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            wordBasic.CallMethod("FileSaveAs", new object[] { new JIString(sOutputDoc) });

            _dispatch.CallMethod("Quit", new object[] { new JIVariant(-1, true), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            JISession.DestroySession(_dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new KainTest(args[0], args);
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
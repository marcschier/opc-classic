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

    public class KainTest {

        private JIComServer ComServer = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public KainTest(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public KainTest(string address, string[] args) {
            JISession session = JISession.CreateSession(args[1],args[2],args[3]);
            ComServer = new JIComServer(JIProgId.ValueOf("Word.Application"),address,session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
        public virtual void StartWord() {
            Unknown = ComServer.CreateInstance();
            IJIDispatch dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
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
             string sDir = "c:\\tmp\\";
             string sInputDoc = sDir + "file_in.doc";
             string sOutputDoc = sDir + "file_out.doc";

             string sOldText = "[label:import:1]";
             string sNewText = "I am some horribly long sentence, so long that [insert something long here]";
             bool tVisible = true;
             bool tSaveOnExit = false;

            Console.WriteLine(((JIVariant)Dispatch.Get("Version")).ObjectAsString.String);
            Console.WriteLine(((JIVariant)Dispatch.Get("Path")).ObjectAsString.String);

            JIVariant variant = Dispatch.Get("Documents");
            IJIDispatch documents = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            //String has to be a JIString.
            JIString filePath = new JIString(sInputDoc);
            //this "open" is of Word 2003
            JIVariant[] variant2 = documents.CallMethodA("open",new object[]{ new JIVariant(filePath,true),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });

            IJIDispatch document = (IJIDispatch)JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = Dispatch.Get("Selection");
            IJIDispatch selection = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            variant = selection.Get("Find");
            IJIDispatch find = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

            Thread.Sleep(2000);

            find.Put("Text",new JIVariant(new JIString(sOldText)));
            find.CallMethodA("Execute",new object[]{ JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });

            Thread.Sleep(2000);

            selection.Put("Text",new JIVariant(new JIString(sNewText)));
            selection.CallMethodA("MoveDown",new object[]{ JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            selection.Put("Text",new JIVariant(new JIString("\nSo we got the next line including BR.\n")));

            variant = selection.Get("Font");
            IJIDispatch font = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            font.Put("Bold",new JIVariant(1));
            font.Put("Italic",new JIVariant(1));
            font.Put("Underline",new JIVariant(0));

            variant = selection.Get("ParagraphFormat");
            IJIDispatch align = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            align.Put("Alignment",new JIVariant(3));

            Thread.Sleep(5000);

            JIString sImgFile = new JIString(sDir + "image.png");
            selection.CallMethodA("MoveDown",new object[]{ JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            variant = selection.Get("InLineShapes");
            IJIDispatch image = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            image.CallMethodA("AddPicture",new object[]{ new JIVariant(sImgFile),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });

            JIString sHyperlink = new JIString("http://www.google.com");
            selection.Put("Text",new JIVariant(new JIString("Text for the link to Google")));
            variant = selection.Get("Range");
            IJIDispatch range = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            variant = document.Get("Hyperlinks");
            IJIDispatch link = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            link.CallMethod("Add",new object[]{ range,sHyperlink,JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });

            variant = Dispatch.Get("WordBasic");
            IJIDispatch wordBasic = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            wordBasic.CallMethod("FileSaveAs",new object[]{ new JIString(sOutputDoc) });

            Dispatch.CallMethod("Quit", new object[]{ new JIVariant(-1,true),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            JISession.DestroySession(Dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    KainTest test = new KainTest(args[0],args);
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
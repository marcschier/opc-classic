// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Automation;
using System;
using System.Threading;

namespace Opc.Classic.Dcom.Test;

public class KainTest
{

    private readonly ComServer _comServer;
    private IDispatch _dispatch;
    private IComObject _unknown;

    public KainTest(string address, string[] args)
    {
        var session = Session.CreateSession(args[1], args[2], args[3]);
        _comServer = new ComServer(ProgId.ValueOf("Word.Application"), address, session);
    }

    public void StartWord()
    {
        _unknown = _comServer.CreateInstance();
        _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
    }

    public void ShowWord()
    {
        var dispId = _dispatch.GetIDsOfNames("Visible");
        var variant = new Variant(true);
        _dispatch.Put(dispId, variant);
    }

    public void PerformOp()
    {
        var sDir = "c:\\tmp\\";
        var sInputDoc = sDir + "file_in.doc";
        var sOutputDoc = sDir + "file_out.doc";

        var sOldText = "[label:import:1]";
        var sNewText = "I am some horribly long sentence, so long that [insert something long here]";

        Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
        Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);

        var variant = _dispatch.Get("Documents");
        var documents = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        // String has to be a <see cref="ComString"/>.
        var filePath = new ComString(sInputDoc);
        // this "open" is of Word 2003
        var variant2 = documents.CallMethodA("open", new object[] { new Variant(filePath, true), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });

        var document = (IDispatch)ObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
        variant = _dispatch.Get("Selection");
        var selection = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);

        variant = selection.Get("Find");
        var find = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);

        Thread.Sleep(2000);

        find.Put("Text", new Variant(new ComString(sOldText)));
        find.CallMethodA("Execute", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });

        Thread.Sleep(2000);

        selection.Put("Text", new Variant(new ComString(sNewText)));
        selection.CallMethodA("MoveDown", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
        selection.Put("Text", new Variant(new ComString("\nSo we got the next line including BR.\n")));

        variant = selection.Get("Font");
        var font = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        font.Put("Bold", new Variant(1));
        font.Put("Italic", new Variant(1));
        font.Put("Underline", new Variant(0));

        variant = selection.Get("ParagraphFormat");
        var align = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        align.Put("Alignment", new Variant(3));

        Thread.Sleep(5000);

        var sImgFile = new ComString(sDir + "image.png");
        selection.CallMethodA("MoveDown", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
        variant = selection.Get("InLineShapes");
        var image = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        image.CallMethodA("AddPicture", new object[] { new Variant(sImgFile), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });

        var sHyperlink = new ComString("http://www.google.com");
        selection.Put("Text", new Variant(new ComString("Text for the link to Google")));
        variant = selection.Get("Range");
        var range = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        variant = document.Get("Hyperlinks");
        var link = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        link.CallMethod("Add", new object[] { range, sHyperlink, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });

        variant = _dispatch.Get("WordBasic");
        var wordBasic = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
        wordBasic.CallMethod("FileSaveAs", new object[] { new ComString(sOutputDoc) });

        _dispatch.CallMethod("Quit", new object[] { new Variant(-1, true), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
        Session.DestroySession(_dispatch.AssociatedSession);
    }

    public static void RunTest(string[] args)
    {

        try
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            var test = new KainTest(args[0], args);
            test.StartWord();
            test.ShowWord();
            test.PerformOp();
        }
        catch (Exception e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }
    }
}

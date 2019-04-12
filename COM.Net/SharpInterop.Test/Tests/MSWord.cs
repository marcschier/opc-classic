//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using System;
    using System.Threading;
    using SharpInterop.Core;
    using SharpInterop.Automation;
    using SharpInterop;
    using SharpInterop.Common;

    public class MSWord
    {
        private readonly ComServer _comStub;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public MSWord(string address, string[] args)
        {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            session.UseSessionSecurity(true);
            _comStub = new ComServer(ProgId.ValueOf("Word.Application"), address, session);
        }


        public void StartWord()
        {
            _unknown = _comStub.CreateInstance();
            _dispatch = (IDispatch) ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowWord()
        {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);
        }


        public void PerformOp()
        {
            Interop.IsCoClassAutoCollection = true;

            Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
            Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);
            var variant = _dispatch.Get("Documents");

            Console.WriteLine("Open document...");
            var documents = (IDispatch) ObjectFactory.NarrowObject(variant.ObjectAsComObject);
            var filePath = new ComString("c:\\temp\\test.doc");
            var variant2 = documents.CallMethodA("open", new object[] {filePath, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(),Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM()});

            Console.WriteLine("doc opened");
            //10
            Sleep(10);

            Console.WriteLine("Get content...");
            var document = (IDispatch) ObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = document.Get("Content");
            var range = (IDispatch) ObjectFactory.NarrowObject(variant.ObjectAsComObject);

            //10
            Sleep(10);
            Console.WriteLine("Running find...");
            variant = range.Get("Find");
            var find = (IDispatch) ObjectFactory.NarrowObject(variant.ObjectAsComObject);

            //2
            Sleep(5);

            Console.WriteLine("Running execute...");
            var findString = new ComString("ow");
            var replaceString = new ComString("igh");
            find.CallMethodA("Execute", new object[] {findString.VariantByRef, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), replaceString.VariantByRef, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM()});

            //1
            Sleep(2);

            Console.WriteLine("Closing document...");
            document.CallMethod("Close");

        }

        private void Sleep(int minutes)
        {
            Console.WriteLine("Sleeping " + minutes + " minute(s)...");
            Thread.Sleep(minutes * 60 * 1000);
        }

        /// <exception cref="InteropException"> </exception>
        private void QuitAndDestroy()
        {
            Console.WriteLine("Quit...");
            _dispatch.CallMethod("Quit", new object[] {new Variant(-1, true), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM()});
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


                var test = new MSWord(args[0], args);
                test.StartWord();
                test.ShowWord();

    //            for (int i = 0; i < 10; i++) {
                    test.PerformOp();
    //            }

                test.QuitAndDestroy();

            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

    }

}
// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class MSWord2 {

        private readonly ComServer _comStub;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public MSWord2(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            _comStub = new ComServer(ProgId.ValueOf("Word.Application"), address, session);
        }


        public void StartWord() {
            _unknown = _comStub.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowWord() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);
        }


        public void PerformOp() {
            Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
            Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);
            var variant = _dispatch.Get("Documents");
            // <see cref="InterfacePointer"/> ptr = variant.getObjectAsInterfacePointer();
            // <see cref="IDispatch"/> documents = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,ptr);
            var documents = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
            var filePath = new ComString("c:/temp/test.doc");
            var variant2 = documents.CallMethodA("open", new object[] { filePath.VariantByRef, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            // <see cref="IDispatch"/> document = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,variant2[0].getObjectAsInterfacePointer());
            var document = (IDispatch)ObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
            variant = document.Get("Content");
            // <see cref="IDispatch"/> range = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,variant.getObjectAsInterfacePointer());
            var range = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);

            variant = range.Get("Find");
            var find = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);

            Thread.Sleep(2000);
            var findString = new ComString("ow");
            var replaceString = new ComString("igh");
            find.CallMethodA("Execute", new object[] { findString.VariantByRef, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), replaceString.VariantByRef, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            Thread.Sleep(5000);

            _dispatch.CallMethod("Quit", new object[] { new Variant(-1, true), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            Session.DestroySession(_dispatch.AssociatedSession);
        }

        public static void RunTest(string[] args) {
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

// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class MSPowerPoint {

        private readonly ComServer _comStub;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public MSPowerPoint(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            _comStub = new ComServer(Clsid.ValueOf("91493441-5A91-11CF-8700-00AA0060263B"), address, session);
        }


        public void StartPowerPoint() {
            _unknown = _comStub.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowPowerPoint() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(-1);
            _dispatch.Put(dispId, variant);
        }


        public void PerformOp() {
            // <see cref="Variant"/> variant = dispatch.get("Presentations");
            // <see cref="InterfacePointer"/> ptr = variant.getObjectAsInterfacePointer();
            // <see cref="IDispatch"/> presentations = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,ptr);
            var presentations = (IDispatch)ObjectFactory.NarrowObject(_dispatch.Get("Presentations").ObjectAsComObject);

            for (var i = 0; i < 2; i++) {
                var results = presentations.CallMethodA("Add", new object[] { Variant.CreateOPTIONAL_PARAM() });
                // variant = results[0];
                // ptr = variant.getObjectAsInterfacePointer();
                // <see cref="IDispatch"/> presentation = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,ptr);
                var presentation = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);
                // variant = presentation.get("Slides");
                // ptr = variant.getObjectAsInterfacePointer();
                // <see cref="IDispatch"/> slides = (<see cref="IDispatch"/>)<see cref="ObjectFactory"/>.createCOMInstance(unknown,ptr);
                var slides = (IDispatch)ObjectFactory.NarrowObject(presentation.Get("Slides").ObjectAsComObject);

                results = slides.CallMethodA("Add", new object[] { 1, 1 });
                // variant = results[0];
                // ptr = variant.getObjectAsInterfacePointer();
                var slide = (IDispatch)ObjectFactory.NarrowObject(results[0].ObjectAsComObject);

                // variant = slide.get("Shapes");
                // ptr = variant.getObjectAsInterfacePointer();
                var shapes = (IDispatch)ObjectFactory.NarrowObject(slide.Get("Shapes").ObjectAsComObject);

                // variant = shapes.get("Title");
                // ptr = variant.getObjectAsInterfacePointer();
                var shape = (IDispatch)ObjectFactory.NarrowObject(shapes.Get("Title").ObjectAsComObject);

                // variant = shape.get("TextFrame");
                // ptr = variant.getObjectAsInterfacePointer();
                var textframe = (IDispatch)ObjectFactory.NarrowObject(shape.Get("TextFrame").ObjectAsComObject);

                // variant = textframe.get("TextRange");
                // ptr = variant.getObjectAsInterfacePointer();
                var textrange = (IDispatch)ObjectFactory.NarrowObject(textframe.Get("TextRange").ObjectAsComObject);

                if (i == 0) {
                    textrange.Put("Text", new ComString("Presentation1").Variant);
                    presentation.CallMethod("SaveAs", new object[] { new ComString("C:\\temp\\presentation1.ppt").Variant, Variant.CreateOPTIONAL_PARAM(), -1 });
                    Thread.Sleep(3000);
                    presentation.CallMethod("Close");
                }
                else {
                    textrange.Put("Text", new ComString("Presentation2").Variant);
                    slides.CallMethod("InsertFromFile", new object[] { new ComString("C:\\temp\\presentation1.ppt"), 1, 1, 1 });
                    presentation.CallMethod("SaveAs", new object[] { new ComString("C:\\temp\\presentation2.ppt"), Variant.CreateOPTIONAL_PARAM(), -1 });
                    Thread.Sleep(3000);
                    presentation.CallMethod("Close");

                    _dispatch.CallMethod("Quit");
                }


            }

            Session.DestroySession(_dispatch.AssociatedSession);
        }

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MSPowerPoint(args[0], args);
                test.StartPowerPoint();
                test.ShowPowerPoint();
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

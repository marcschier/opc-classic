namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    public class MSPowerPoint2 {

        private readonly JIComServer _comStub;
        private IJIDispatch _dispatch;
        private IComObject _unknown;


        public MSPowerPoint2(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);
            _comStub = new JIComServer(JIProgId.ValueOf("PowerPoint.Application"), address, session);
        }


        public void StartPowerPoint() {
            _unknown = _comStub.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowPowerPoint() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(-1);
            _dispatch.Put(dispId, variant);
        }



        public IJIDispatch OpenPresentation(string fullEscapedPath) {
            var presentations = (IJIDispatch)JIObjectFactory.NarrowObject(_dispatch.Get("Presentations").ObjectAsComObject);
            var result = presentations.CallMethodA("Open", new object[] { new JIString(fullEscapedPath), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            return (IJIDispatch)JIObjectFactory.NarrowObject(result[0].ObjectAsComObject);
        }


        public IJIDispatch RunPresentation(IJIDispatch activePresentation) {
            var slideShowSettings = (IJIDispatch)JIObjectFactory.NarrowObject(activePresentation.Get("SlideShowSettings").ObjectAsComObject);
            Console.WriteLine("Running Slide show : " + activePresentation.Get("Name").ObjectAsString.String);
            var slideShowWindow = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowSettings.CallMethodA("Run").ObjectAsComObject);
            var slideShowView = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowWindow.Get("View").ObjectAsComObject);
            return slideShowView;
        }


        public void QuitPowerPoint() {
            _dispatch.CallMethod("Quit");
            JISession.DestroySession(_dispatch.AssociatedSession);
        }


        public void ClosePresentation(IJIDispatch presentation) => presentation.CallMethod("Close");


        public void SavePresentationAs(IJIDispatch presentation, string fullEscapedPath) => presentation.CallMethod("SaveAs", new object[] { new JIString(fullEscapedPath).Variant, JIVariant.CreateOPTIONAL_PARAM(), -1 });


        public void Goto_First_Slide(IJIDispatch view) => view.CallMethod("First");


        public void Goto_Last_Slide(IJIDispatch view) => view.CallMethod("Last");


        public void Do_Next_Action(IJIDispatch view) => view.CallMethod("Next");


        public void Do_Previous_Action(IJIDispatch view) => view.CallMethod("Previous");


        public void Goto_Numbered_Slide(IJIDispatch view, int index) => view.CallMethod("GotoSlide", new object[] { index, JIVariant.CreateOPTIONAL_PARAM() });



        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MSPowerPoint2(args[0], args);
                test.StartPowerPoint();
                test.ShowPowerPoint();

                Console.WriteLine("Welcome to PowerPoint Manager !");
                Console.WriteLine("Commands --> ");
                Console.WriteLine("'O' <path_to_ppt>               Open PPT, ex:- O c:\\temp\\j-Interop.ppt");
                Console.WriteLine("'C'                              Close PPT");
                Console.WriteLine("'N'                              Next Action");
                Console.WriteLine("'P'                              Previous Action");
                Console.WriteLine("'G' <slide number>              Goto Slide, ex:- G 3");
                Console.WriteLine("'F'                             First Slide");
                Console.WriteLine("'L'                             Last Slide");
                Console.WriteLine("'Q'                               Quit PowerPoint Manager");

                
                var inputreader = Console.In;
                const string commands = "OCNPGFLQ";
                IJIDispatch activePresentation = null;
                IJIDispatch view = null;
                var over = false;
                while (!over) {
                    var input = inputreader.ReadLine().Trim();
                    if (input.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                        continue;
                    }
                    var index = -1;
                    string command = null;

                    if (input.Length > 1) {
                        index = input.IndexOf(" ", StringComparison.Ordinal);
                        command = input.Substring(0, index);
                    }
                    else {
                        command = input;
                    }



                    switch (commands.IndexOf(command, StringComparison.Ordinal)) {
                        case 0:
                            var path = input.Substring(index++).Trim();
                            activePresentation = test.OpenPresentation(path);
                            view = test.RunPresentation(activePresentation);
                            break;
                        case 1:
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.ClosePresentation(activePresentation);
                                activePresentation = null;
                            }
                            break;
                        case 2:
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.Do_Next_Action(view);
                            }
                            break;
                        case 3:
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.Do_Previous_Action(view);
                            }
                            break;
                        case 4:
                            path = input.Substring(index++).Trim();
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.Goto_Numbered_Slide(view, Convert.ToInt32(path));
                            }

                            break;
                        case 5:
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.Goto_First_Slide(view);
                            }

                            break;
                        case 6:
                            if (activePresentation == null) {
                                Console.WriteLine("Please open a presentation first !");
                            }
                            else {
                                test.Goto_Last_Slide(view);
                            }

                            break;
                        case 7:
                            test.QuitPowerPoint();
                            over = true;
                            break;
                        default:
                            Console.WriteLine("Incorrect option !");
                            break;
                    }


                }

            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }





    }

}
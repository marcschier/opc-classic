using System;

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

    public class MSPowerPoint2 {

        private JIComServer ComStub = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSPowerPoint2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSPowerPoint2(string address, string[] args) {
            JISession session = JISession.CreateSession(args[1],args[2],args[3]);
            ComStub = new JIComServer(JIProgId.ValueOf("PowerPoint.Application"),address,session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startPowerPoint() throws org.jinterop.dcom.common.JIException
        public virtual void StartPowerPoint() {
            Unknown = ComStub.CreateInstance();
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showPowerPoint() throws org.jinterop.dcom.common.JIException
        public virtual void ShowPowerPoint() {
            int dispId = Dispatch.GetIDsOfNames("Visible");
            JIVariant variant = new JIVariant(-1);
            Dispatch.Put(dispId,variant);
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.impls.automation.IJIDispatch openPresentation(String fullEscapedPath) throws org.jinterop.dcom.common.JIException, InterruptedException
        public virtual IJIDispatch OpenPresentation(string fullEscapedPath) {
            IJIDispatch presentations = (IJIDispatch)JIObjectFactory.NarrowObject(Dispatch.Get("Presentations").ObjectAsComObject);
            JIVariant[] result = presentations.CallMethodA("Open",new object[]{ new JIString(fullEscapedPath),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
            return (IJIDispatch)JIObjectFactory.NarrowObject(result[0].ObjectAsComObject);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.impls.automation.IJIDispatch runPresentation(org.jinterop.dcom.impls.automation.IJIDispatch activePresentation) throws org.jinterop.dcom.common.JIException
        public virtual IJIDispatch RunPresentation(IJIDispatch activePresentation) {
            IJIDispatch slideShowSettings = (IJIDispatch)JIObjectFactory.NarrowObject(activePresentation.Get("SlideShowSettings").ObjectAsComObject);
            Console.WriteLine("Running Slide show : " + activePresentation.Get("Name").ObjectAsString.String);
            IJIDispatch slideShowWindow = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowSettings.CallMethodA("Run").ObjectAsComObject);
            IJIDispatch slideShowView = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowWindow.Get("View").ObjectAsComObject);
            return slideShowView;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quitPowerPoint() throws org.jinterop.dcom.common.JIException
        public virtual void QuitPowerPoint() {
            Dispatch.CallMethod("Quit");
            JISession.DestroySession(Dispatch.AssociatedSession);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closePresentation(org.jinterop.dcom.impls.automation.IJIDispatch presentation) throws org.jinterop.dcom.common.JIException
        public virtual void ClosePresentation(IJIDispatch presentation) {
            presentation.CallMethod("Close");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void savePresentationAs(org.jinterop.dcom.impls.automation.IJIDispatch presentation, String fullEscapedPath) throws org.jinterop.dcom.common.JIException
        public virtual void SavePresentationAs(IJIDispatch presentation, string fullEscapedPath) {
            presentation.CallMethod("SaveAs", new object[]{ (new JIString(fullEscapedPath)).Variant,JIVariant.OPTIONAL_PARAM(),new int?(-1) });
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_First_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
        public virtual void Goto_First_Slide(IJIDispatch view) {
            view.CallMethod("First");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_Last_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
        public virtual void Goto_Last_Slide(IJIDispatch view) {
            view.CallMethod("Last");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void do_Next_Action(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
        public virtual void Do_Next_Action(IJIDispatch view) {
            view.CallMethod("Next");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void do_Previous_Action(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
        public virtual void Do_Previous_Action(IJIDispatch view) {
            view.CallMethod("Previous");
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_Numbered_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view, int index) throws org.jinterop.dcom.common.JIException
        public virtual void Goto_Numbered_Slide(IJIDispatch view, int index) {
            view.CallMethod("GotoSlide", new object[]{ new int?(index), JIVariant.OPTIONAL_PARAM() });
        }



        public static void Main(string[] args) {

            try {
                    if (args.Length < 4) {
                        Console.WriteLine("Please provide address domain username password");
                        return;
                    }
                    MSPowerPoint2 test = new MSPowerPoint2(args[0],args);
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

                    System.IO.StreamReader inputreader = new System.IO.StreamReader(new BufferedInputStream(System.in));


                    const string commands = "OCNPGFLQ";
                    IJIDispatch activePresentation = null;
                    IJIDispatch view = null;
                    bool over = false;
                    while (!over) {
                        string input = inputreader.ReadLine().Trim();
                        if (input.Equals("", StringComparison.CurrentCultureIgnoreCase)) {
                            continue;
                        }
                        int index = -1;
                        string command = null;

                        if (input.Length > 1) {
                            index = input.IndexOf(" ", StringComparison.Ordinal);
                            command = input.Substring(0,index);
                        }
                        else {
                            command = input;
                        }



                        switch (commands.IndexOf(command, StringComparison.Ordinal)) {
                            case 0:
                                    string path = input.Substring(index++).Trim();
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
                                    test.Goto_Numbered_Slide(view,(int)Convert.ToInt32(path));
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
namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSPowerPoint2
	{

		private JIComServer comStub;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSPowerPoint2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSPowerPoint2(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comStub = new JIComServer(JIProgId.ValueOf("PowerPoint.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void startPowerPoint()
		{
			unknown = comStub.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(impls.automation.DispatchFlags.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void showPowerPoint()
		{
			var dispId = dispatch.GetIDsOfNames("Visible");
			var variant = new JIVariant(-1);
			dispatch.Put(dispId,variant);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.impls.automation.IJIDispatch openPresentation(String fullEscapedPath) throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual IJIDispatch openPresentation(string fullEscapedPath)
		{
			var presentations = (IJIDispatch)JIObjectFactory.NarrowObject(dispatch.Get("Presentations").ObjectAsComObject);
			var result = presentations.CallMethodA("Open",new object[]{new JIString(fullEscapedPath),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			return (IJIDispatch)JIObjectFactory.NarrowObject(result[0].ObjectAsComObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.impls.automation.IJIDispatch runPresentation(org.jinterop.dcom.impls.automation.IJIDispatch activePresentation) throws org.jinterop.dcom.common.JIException
		public virtual IJIDispatch runPresentation(IJIDispatch activePresentation)
		{
			var slideShowSettings = (IJIDispatch)JIObjectFactory.NarrowObject(activePresentation.Get("SlideShowSettings").ObjectAsComObject);
			Console.WriteLine("Running Slide show : " + activePresentation.Get("Name").ObjectAsString.String);
			var slideShowWindow = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowSettings.CallMethodA("Run").ObjectAsComObject);
			var slideShowView = (IJIDispatch)JIObjectFactory.NarrowObject(slideShowWindow.Get("View").ObjectAsComObject);
			return slideShowView;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void quitPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void quitPowerPoint()
		{
			dispatch.CallMethod("Quit");
			JISession.destroySession(dispatch.AssociatedSession);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void closePresentation(org.jinterop.dcom.impls.automation.IJIDispatch presentation) throws org.jinterop.dcom.common.JIException
		public virtual void closePresentation(IJIDispatch presentation)
		{
			presentation.CallMethod("Close");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void savePresentationAs(org.jinterop.dcom.impls.automation.IJIDispatch presentation, String fullEscapedPath) throws org.jinterop.dcom.common.JIException
		public virtual void savePresentationAs(IJIDispatch presentation, string fullEscapedPath)
		{
			presentation.CallMethod("SaveAs", new object[]{new JIString(fullEscapedPath).Variant,JIVariant.CreateOPTIONAL_PARAM(), -1 });
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_First_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
		public virtual void goto_First_Slide(IJIDispatch view)
		{
			view.CallMethod("First");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_Last_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
		public virtual void goto_Last_Slide(IJIDispatch view)
		{
			view.CallMethod("Last");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void do_Next_Action(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
		public virtual void do_Next_Action(IJIDispatch view)
		{
			view.CallMethod("Next");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void do_Previous_Action(org.jinterop.dcom.impls.automation.IJIDispatch view) throws org.jinterop.dcom.common.JIException
		public virtual void do_Previous_Action(IJIDispatch view)
		{
			view.CallMethod("Previous");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void goto_Numbered_Slide(org.jinterop.dcom.impls.automation.IJIDispatch view, int index) throws org.jinterop.dcom.common.JIException
		public virtual void goto_Numbered_Slide(IJIDispatch view, int index)
		{
			view.CallMethod("GotoSlide", new object[]{ index, JIVariant.CreateOPTIONAL_PARAM()});
		}



		public static void Main(string[] args)
		{

			try
			{
					if (args.Length < 4)
					{
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					var test = new MSPowerPoint2(args[0],args);
					test.startPowerPoint();
					test.showPowerPoint();

					Console.WriteLine("Welcome to PowerPoint Manager !");
					Console.WriteLine("Commands --> ");
					Console.WriteLine("'O' <path_to_ppt>               Open PPT, ex:- O c:\\temp\\j-Interop.ppt");
					Console.WriteLine("'C'  							Close PPT");
					Console.WriteLine("'N'  							Next Action");
					Console.WriteLine("'P'  							Previous Action");
					Console.WriteLine("'G' <slide number>              Goto Slide, ex:- G 3");
					Console.WriteLine("'F' 							First Slide");
					Console.WriteLine("'L' 							Last Slide");
					Console.WriteLine("'Q' 				  			Quit PowerPoint Manager");

					var inputreader = new System.IO.StreamReader(new BufferedInputStream(System.in));


					const string commands = "OCNPGFLQ";
					IJIDispatch activePresentation = null;
					IJIDispatch view = null;
					var over = false;
					while (!over)
					{
						var input = inputreader.ReadLine().Trim();
						if (input.Equals("", StringComparison.CurrentCultureIgnoreCase))
						{
							continue;
						}
						var index = -1;
						string command = null;

						if (input.Length > 1)
						{
							index = input.IndexOf(" ", StringComparison.Ordinal);
							command = input.Substring(0,index);
						}
						else
						{
							command = input;
						}



						switch (commands.IndexOf(command, StringComparison.Ordinal))
						{
							case 0:
									var path = input.Substring(index++).Trim();
									activePresentation = test.openPresentation(path);
									view = test.runPresentation(activePresentation);
								break;
							case 1:
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.closePresentation(activePresentation);
									activePresentation = null;
								}
								break;
							case 2:
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.do_Next_Action(view);
								}
								break;
							case 3:
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.do_Previous_Action(view);
								}
								break;
							case 4:
								path = input.Substring(index++).Trim();
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.goto_Numbered_Slide(view,(int)Convert.ToInt32(path));
								}

								break;
							case 5:
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.goto_First_Slide(view);
								}

								break;
							case 6:
								if (activePresentation == null)
								{
									Console.WriteLine("Please open a presentation first !");
								}
								else
								{
									test.goto_Last_Slide(view);
								}

								break;
							case 7:
								test.quitPowerPoint();
								over = true;
								break;
							default:
								Console.WriteLine("Incorrect option !");
							break;
						}


					}

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
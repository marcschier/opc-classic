namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JILocalCoClass = core.JILocalCoClass;
    using JILocalInterfaceDefinition = core.JILocalInterfaceDefinition;
    using JILocalMethodDescriptor = core.JILocalMethodDescriptor;
    using JILocalParamsDescriptor = core.JILocalParamsDescriptor;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSInternetExplorer
	{

		private JIComServer comServer;
		private JISession session;
		private IJIComObject ieObject;
		private IJIDispatch ieObjectDispatch;
		private string identifier;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSInternetExplorer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSInternetExplorer(string address, string[] args)
		{
			JISystem.MapHostNametoIP("locutus", "192.168.0.130");
			session = JISession.createSession(args[1],args[2],args[3]);
			session.useNTLMv2(true);
			session.useSessionSecurity(true);
			comServer = new JIComServer(JIProgId.ValueOf("InternetExplorer.Application"),address,session);
			ieObject = comServer.CreateInstance();
			var ieObjectWebBrowser2 = (IJIComObject)ieObject.QueryInterface("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E");
			ieObjectDispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)ieObject.QueryInterface(impls.automation.DispatchFlags.IID));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setVisible() throws org.jinterop.dcom.common.JIException
		private void setVisible()
		{

			var dispId = ieObjectDispatch.GetIDsOfNames("Visible");
			ieObjectDispatch.Put(dispId,new JIVariant(true));
			ieObjectDispatch.Put("AddressBar",new JIVariant(true));
			ieObjectDispatch.Put("MenuBar",new JIVariant(true));
			ieObjectDispatch.Put("ToolBar",new JIVariant(true));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void navigateToUrl(String url) throws org.jinterop.dcom.common.JIException
		private void navigateToUrl(string url)
		{
			//ieObjectDispatch.put("Top",new JIVariant(new Integer(600)));
			//ieObjectDispatch.put("Left",new JIVariant(new Integer(700)));
			ieObjectDispatch.CallMethod("Navigate2",new object[]{new JIString(url),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void attachCallBack() throws org.jinterop.dcom.common.JIException
		private void attachCallBack()
		{

		/// <summary>
		/// The JIJavaCOClass is a representation for a Java server class. It's there so that when we get to the next version of the library, I am able to support full bi-directional access. Currently, you can implement any IDL of an existing COM server using the JIJavaCOClass and
		/// pass it's interface pointer instead of the original COM server and it will work fine. Similar mechanism is exploited for call backs.In our case I had to implement DWebBrowserEvents interface.
		///
		/// IJavaCoClass javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("45B5FC0C-FAC2-42bd-923E-2B221A89E092"),DWebBrowserEvents2.class);
		///
		/// This definition create a Java component with an IID of 45B5FC0C-FAC2-42bd-923E-2B221A89E092...I just made this one up for uniquely classifying this class...you can equate this to a lib identifier of COM IDL. This is required if there are multilple interfaces being implemented in the same Java Class.
		/// If you have only one...you can put it's IID here. I just did not do it for showing the user a possiblity.
		///
		/// The JIJavaCOClass has the option of instantiating the DWebBrowserEvents.class or it could use another ctor to pass an already instantiated object. In latter scenario, the object would be used as target for the events instead of instantiating a new one from DWebBrowserEvents.class.
		/// Now that we have a Java server, we need to define the methods\events it will handle.
		///
		/// This is done using the Method descriptors which are themselves described using the Parameter Objects.
		///
		/// JILocalParamsDescriptor propertyChangeObject = new JILocalParamsDescriptor();
		///
		/// This creates a Parameter Object, capable of defining a IN or OUT type for a Method.
		///
		/// like:-
		/// propertyChangeObject.addInParamAsType(JIString.class,JIFlags.FLAG_NULL);
		///
		/// JILocalMethodDescriptor methodDescriptor = new JILocalMethodDescriptor("PropertyChange",0x70,propertyChangeObject);
		/// javaComponent.getInterfaceDefinition().addMethodDescriptor(methodDescriptor);
		///
		/// This declares a method descriptor. The first parameter in the ctor is the API name of the api to implement, the second one is it's OP number.
		/// This one can be obtained from the IDL\TypeLib. And the third param is the parameterObject describing the input\output types of this method.
		/// If you do not want to use this ctor, there is another, which sequentially increments the method numbers starting from 1.
		/// The calls below add a new interface IID to this Java server. It simply means that the server supports this interface definition.
		///
		/// ArrayList list = new ArrayList();
		/// list.add("34A715A0-6587-11D0-924A-0020AFC7AC4D");
		/// javaComponent.setSupportedEventInterfaces(list);
		///
		/// This will be the list of all COM interfaces which this Java class supports or implements.
		///
		/// The next call attaches the event handler (our JILocalCoClass) to the actual COM server for recieving events for the interface identified by the IID.
		/// There can be many such calls on the same COM server for different IIDs.
		/// identifier = JIObjectFactory.attachEventHandler(ieObject,"34A715A0-6587-11D0-924A-0020AFC7AC4D",JIInterfacePointer.getInterfacePointer(session,javaComponent));
		///
		/// Now whether you use IJIDispatch or not, events will work regardless of that. The COM object you have to use in the attachEventHandler is the COM Object on
		/// which you did the queryinterface for the IJIDispatch.
		///
		///
		/// </summary>
			var javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("34A715A0-6587-11D0-924A-0020AFC7AC4D"),typeof(DWebBrowserEvents2));

			var propertyChangeObject = new JILocalParamsDescriptor();
			propertyChangeObject.AddInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			var methodDescriptor = new JILocalMethodDescriptor("PropertyChange",0x70,propertyChangeObject);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


			var navigateObject = new JILocalParamsDescriptor();
			navigateObject.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("BeforeNavigate2",0xFA,navigateObject);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var StatusTextChange = new JILocalParamsDescriptor();
			StatusTextChange.AddInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("StatusTextChange",0x66,StatusTextChange);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var ProgressChange = new JILocalParamsDescriptor();
			ProgressChange.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			ProgressChange.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("ProgressChange",0x6c,ProgressChange);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var CommandStateChange = new JILocalParamsDescriptor();
			CommandStateChange.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			CommandStateChange.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("CommandStateChange",0x69,CommandStateChange);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var DownloadBegin = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("DownloadBegin",0x6a,DownloadBegin);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var DownloadComplete = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("DownloadComplete",0x68,DownloadComplete);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var TitleChange = new JILocalParamsDescriptor();
			TitleChange.AddInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("TitleChange",0x71,TitleChange);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var NewWindow2 = new JILocalParamsDescriptor();
			NewWindow2.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow2.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NewWindow2",0xfb,NewWindow2);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var NavigateComplete2 = new JILocalParamsDescriptor();
			NavigateComplete2.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			NavigateComplete2.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NavigateComplete2",0xfc,NavigateComplete2);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var DocumentComplete = new JILocalParamsDescriptor();
			DocumentComplete.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			DocumentComplete.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("DocumentComplete",0x103,DocumentComplete);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnQuit = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("OnQuit",0xfd,OnQuit);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnVisible = new JILocalParamsDescriptor();
			OnVisible.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnVisible",0xfe,OnVisible);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnToolBar = new JILocalParamsDescriptor();
			OnToolBar.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnToolBar",0xff,OnToolBar);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnMenuBar = new JILocalParamsDescriptor();
			OnMenuBar.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnMenuBar",0x100,OnMenuBar);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnStatusBar = new JILocalParamsDescriptor();
			OnStatusBar.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnStatusBar",0x101,OnStatusBar);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnFullScreen = new JILocalParamsDescriptor();
			OnFullScreen.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnFullScreen",0x102,OnFullScreen);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var OnTheaterMode = new JILocalParamsDescriptor();
			OnTheaterMode.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnTheaterMode",0x104,OnTheaterMode);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowSetResizable = new JILocalParamsDescriptor();
			WindowSetResizable.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetResizable",0x106,WindowSetResizable);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowSetLeft = new JILocalParamsDescriptor();
			WindowSetLeft.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetLeft",0x108,WindowSetLeft);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowSetTop = new JILocalParamsDescriptor();
			WindowSetTop.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetTop",0x109,WindowSetTop);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowSetWidth = new JILocalParamsDescriptor();
			WindowSetWidth.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetWidth",0x10a,WindowSetWidth);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowSetHeight = new JILocalParamsDescriptor();
			WindowSetHeight.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetHeight",0x10b,WindowSetHeight);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowClosing = new JILocalParamsDescriptor();
			WindowClosing.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			WindowClosing.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowClosing",0x107,WindowClosing);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var ClientToHostWindow = new JILocalParamsDescriptor();
			ClientToHostWindow.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			ClientToHostWindow.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("ClientToHostWindow",0x10c,ClientToHostWindow);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var SetSecureLockIcon = new JILocalParamsDescriptor();
			SetSecureLockIcon.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("SetSecureLockIcon",0x10d,SetSecureLockIcon);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var FileDownload = new JILocalParamsDescriptor();
			FileDownload.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			FileDownload.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("FileDownload",0x10e,FileDownload);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var NavigateError = new JILocalParamsDescriptor();
			NavigateError.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			NavigateError.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NavigateError",0x10f,NavigateError);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var NewWindow3 = new JILocalParamsDescriptor();
			NewWindow3.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow3.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow3.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			NewWindow3.AddInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			NewWindow3.AddInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NewWindow3",0x111,NewWindow3);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var PrintTemplateInstantiation = new JILocalParamsDescriptor();
			PrintTemplateInstantiation.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrintTemplateInstantiation",0xe1,PrintTemplateInstantiation);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var PrintTemplateTeardown = new JILocalParamsDescriptor();
			PrintTemplateTeardown.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrintTemplateTeardown",0xe2,PrintTemplateTeardown);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var SetPhishingFilterStatus = new JILocalParamsDescriptor();
			SetPhishingFilterStatus.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("SetPhishingFilterStatus",0x11A,SetPhishingFilterStatus);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var WindowStateChanged = new JILocalParamsDescriptor();
			WindowStateChanged.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			WindowStateChanged.AddInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowStateChanged",0x11B,WindowStateChanged);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


			var UpdatePageStatus = new JILocalParamsDescriptor();
			UpdatePageStatus.AddInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			UpdatePageStatus.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			UpdatePageStatus.AddInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("UpdatePageStatus",0xe3,UpdatePageStatus);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

			var PrivacyImpactedStateChange = new JILocalParamsDescriptor();
			PrivacyImpactedStateChange.AddInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrivacyImpactedStateChange",0x110,PrivacyImpactedStateChange);
			javaComponent.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


			var list = new ArrayList();
			list.Add("34A715A0-6587-11D0-924A-0020AFC7AC4D");
			list.Add("00020400-0000-0000-c000-000000000046");
			javaComponent.SupportedEventInterfaces = list;



			identifier = JIObjectFactory.AttachEventHandler(ieObject,"34A715A0-6587-11D0-924A-0020AFC7AC4D",JIObjectFactory.BuildObject(session,javaComponent));
			try
			{
				Thread.Sleep(5000);
			}
			catch (InterruptedException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			} //for call backs
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void detachCallBack() throws org.jinterop.dcom.common.JIException
		private void detachCallBack()
		{
			JIObjectFactory.DetachEventHandler(ieObject,identifier);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void quit() throws org.jinterop.dcom.common.JIException
		private void quit()
		{
			ieObjectDispatch.CallMethod("Quit");
			JISession.destroySession(ieObjectDispatch.AssociatedSession);
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

					 JISystem.InBuiltLogHandler = false;
					Logger l = Logger.getLogger("org.jinterop");
					l.Level = Level.INFO;
					var internetExplorer = new MSInternetExplorer(args[0],args);
					internetExplorer.setVisible();
					internetExplorer.attachCallBack();
					internetExplorer.navigateToUrl("http://www.sqlshark.com");
					Thread.Sleep(30000); //for call backs
					internetExplorer.detachCallBack();
					Thread.Sleep(5000); //wait for 5 secs
					internetExplorer.quit();
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
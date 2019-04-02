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
			JISystem.mapHostNametoIP("locutus", "192.168.0.130");
			session = JISession.createSession(args[1],args[2],args[3]);
			session.useNTLMv2(true);
			session.useSessionSecurity(true);
			comServer = new JIComServer(JIProgId.valueOf("InternetExplorer.Application"),address,session);
			ieObject = comServer.createInstance();
			var ieObjectWebBrowser2 = (IJIComObject)ieObject.queryInterface("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E");
			ieObjectDispatch = (IJIDispatch)JIObjectFactory.narrowObject((IJIComObject)ieObject.queryInterface(impls.automation.IJIDispatch_Fields.IID));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setVisible() throws org.jinterop.dcom.common.JIException
		private void setVisible()
		{

			var dispId = ieObjectDispatch.getIDsOfNames("Visible");
			ieObjectDispatch.put(dispId,new JIVariant(true));
			ieObjectDispatch.put("AddressBar",new JIVariant(true));
			ieObjectDispatch.put("MenuBar",new JIVariant(true));
			ieObjectDispatch.put("ToolBar",new JIVariant(true));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void navigateToUrl(String url) throws org.jinterop.dcom.common.JIException
		private void navigateToUrl(string url)
		{
			//ieObjectDispatch.put("Top",new JIVariant(new Integer(600)));
			//ieObjectDispatch.put("Left",new JIVariant(new Integer(700)));
			ieObjectDispatch.callMethod("Navigate2",new object[]{new JIString(url),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
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
			propertyChangeObject.addInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			var methodDescriptor = new JILocalMethodDescriptor("PropertyChange",0x70,propertyChangeObject);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);


			var navigateObject = new JILocalParamsDescriptor();
			navigateObject.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			navigateObject.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("BeforeNavigate2",0xFA,navigateObject);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var StatusTextChange = new JILocalParamsDescriptor();
			StatusTextChange.addInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("StatusTextChange",0x66,StatusTextChange);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var ProgressChange = new JILocalParamsDescriptor();
			ProgressChange.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			ProgressChange.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("ProgressChange",0x6c,ProgressChange);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var CommandStateChange = new JILocalParamsDescriptor();
			CommandStateChange.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			CommandStateChange.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("CommandStateChange",0x69,CommandStateChange);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var DownloadBegin = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("DownloadBegin",0x6a,DownloadBegin);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var DownloadComplete = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("DownloadComplete",0x68,DownloadComplete);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var TitleChange = new JILocalParamsDescriptor();
			TitleChange.addInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("TitleChange",0x71,TitleChange);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var NewWindow2 = new JILocalParamsDescriptor();
			NewWindow2.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow2.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NewWindow2",0xfb,NewWindow2);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var NavigateComplete2 = new JILocalParamsDescriptor();
			NavigateComplete2.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			NavigateComplete2.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NavigateComplete2",0xfc,NavigateComplete2);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var DocumentComplete = new JILocalParamsDescriptor();
			DocumentComplete.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			DocumentComplete.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("DocumentComplete",0x103,DocumentComplete);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnQuit = new JILocalParamsDescriptor();
			methodDescriptor = new JILocalMethodDescriptor("OnQuit",0xfd,OnQuit);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnVisible = new JILocalParamsDescriptor();
			OnVisible.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnVisible",0xfe,OnVisible);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnToolBar = new JILocalParamsDescriptor();
			OnToolBar.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnToolBar",0xff,OnToolBar);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnMenuBar = new JILocalParamsDescriptor();
			OnMenuBar.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnMenuBar",0x100,OnMenuBar);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnStatusBar = new JILocalParamsDescriptor();
			OnStatusBar.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnStatusBar",0x101,OnStatusBar);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnFullScreen = new JILocalParamsDescriptor();
			OnFullScreen.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnFullScreen",0x102,OnFullScreen);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var OnTheaterMode = new JILocalParamsDescriptor();
			OnTheaterMode.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("OnTheaterMode",0x104,OnTheaterMode);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowSetResizable = new JILocalParamsDescriptor();
			WindowSetResizable.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetResizable",0x106,WindowSetResizable);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowSetLeft = new JILocalParamsDescriptor();
			WindowSetLeft.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetLeft",0x108,WindowSetLeft);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowSetTop = new JILocalParamsDescriptor();
			WindowSetTop.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetTop",0x109,WindowSetTop);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowSetWidth = new JILocalParamsDescriptor();
			WindowSetWidth.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetWidth",0x10a,WindowSetWidth);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowSetHeight = new JILocalParamsDescriptor();
			WindowSetHeight.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowSetHeight",0x10b,WindowSetHeight);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowClosing = new JILocalParamsDescriptor();
			WindowClosing.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			WindowClosing.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowClosing",0x107,WindowClosing);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var ClientToHostWindow = new JILocalParamsDescriptor();
			ClientToHostWindow.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			ClientToHostWindow.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("ClientToHostWindow",0x10c,ClientToHostWindow);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var SetSecureLockIcon = new JILocalParamsDescriptor();
			SetSecureLockIcon.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("SetSecureLockIcon",0x10d,SetSecureLockIcon);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var FileDownload = new JILocalParamsDescriptor();
			FileDownload.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			FileDownload.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("FileDownload",0x10e,FileDownload);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var NavigateError = new JILocalParamsDescriptor();
			NavigateError.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			NavigateError.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NavigateError.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NavigateError",0x10f,NavigateError);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var NewWindow3 = new JILocalParamsDescriptor();
			NewWindow3.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow3.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			NewWindow3.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			NewWindow3.addInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			NewWindow3.addInParamAsType(typeof(JIString),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("NewWindow3",0x111,NewWindow3);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var PrintTemplateInstantiation = new JILocalParamsDescriptor();
			PrintTemplateInstantiation.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrintTemplateInstantiation",0xe1,PrintTemplateInstantiation);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var PrintTemplateTeardown = new JILocalParamsDescriptor();
			PrintTemplateTeardown.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrintTemplateTeardown",0xe2,PrintTemplateTeardown);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var SetPhishingFilterStatus = new JILocalParamsDescriptor();
			SetPhishingFilterStatus.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("SetPhishingFilterStatus",0x11A,SetPhishingFilterStatus);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var WindowStateChanged = new JILocalParamsDescriptor();
			WindowStateChanged.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			WindowStateChanged.addInParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("WindowStateChanged",0x11B,WindowStateChanged);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);


			var UpdatePageStatus = new JILocalParamsDescriptor();
			UpdatePageStatus.addInParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			UpdatePageStatus.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			UpdatePageStatus.addInParamAsType(typeof(JIVariant),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("UpdatePageStatus",0xe3,UpdatePageStatus);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);

			var PrivacyImpactedStateChange = new JILocalParamsDescriptor();
			PrivacyImpactedStateChange.addInParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
			methodDescriptor = new JILocalMethodDescriptor("PrivacyImpactedStateChange",0x110,PrivacyImpactedStateChange);
			javaComponent.InterfaceDefinition.addMethodDescriptor(methodDescriptor);


			var list = new ArrayList();
			list.Add("34A715A0-6587-11D0-924A-0020AFC7AC4D");
			list.Add("00020400-0000-0000-c000-000000000046");
			javaComponent.SupportedEventInterfaces = list;



			identifier = JIObjectFactory.attachEventHandler(ieObject,"34A715A0-6587-11D0-924A-0020AFC7AC4D",JIObjectFactory.buildObject(session,javaComponent));
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
			JIObjectFactory.detachEventHandler(ieObject,identifier);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void quit() throws org.jinterop.dcom.common.JIException
		private void quit()
		{
			ieObjectDispatch.callMethod("Quit");
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
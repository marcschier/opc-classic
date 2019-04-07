using System;

namespace org.jinterop.dcom.test {

	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;

	public class DWebBrowserEvents2 {


		public DWebBrowserEvents2() {

		}
	//	[id(0x00000070), helpstring("Fired when the PutProperty method has been called.")]
	//	 void PropertyChange([in] BSTR szProperty);
		public virtual void PropertyChange(JIString szProperty) {
			Console.WriteLine("PropertyChange -> " + szProperty.String);
		}


	//	[id(0x000000fa), helpstring("Fired before navigate occurs in the given WebBrowser (window or frameset element). The processing of this navigation may be modified.")]
	//	 void BeforeNavigate2(
	//	                 [in] IDispatch* pDisp, 
	//	                 [in] VARIANT* URL, 
	//	                 [in] VARIANT* Flags, 
	//	                 [in] VARIANT* TargetFrameName, 
	//	                 [in] VARIANT* PostData, 
	//	                 [in] VARIANT* Headers, 
	//	                 [in, out] VARIANT_BOOL* Cancel);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant BeforeNavigate2(org.jinterop.dcom.core.IJIComObject dispatch,org.jinterop.dcom.core.JIVariant URL,org.jinterop.dcom.core.JIVariant Flags,org.jinterop.dcom.core.JIVariant TargetFrameName, org.jinterop.dcom.core.JIVariant PostData, org.jinterop.dcom.core.JIVariant Headers, org.jinterop.dcom.core.JIVariant Cancel) throws org.jinterop.dcom.common.JIException
		public virtual JIVariant BeforeNavigate2(IJIComObject dispatch, JIVariant URL, JIVariant Flags, JIVariant TargetFrameName, JIVariant PostData, JIVariant Headers, JIVariant Cancel) {
			dispatch = JIObjectFactory.NarrowObject(dispatch);
			JIVariant realURL = URL;
			while (realURL.ByRefFlagSet) {
				realURL = realURL.ObjectAsVariant;
			}

			Console.WriteLine("BeforeNavigate2  -> " + realURL.ObjectAsString.String);

			//uncomment and return this to stop loading the page
			//JIVariant variant = new JIVariant(true,true);

			return Cancel;
		}

	   //[id(0x00000066), helpstring("Statusbar text changed.")]
		public virtual void StatusTextChange(JIString text) {
			Console.WriteLine("StatusTextChange -> " + text.String);
		}

		//[id(0x0000006c), helpstring("Fired when download progress is updated.")]
		public virtual void ProgressChange(int Progress, int ProgressMax) {
			Console.WriteLine("ProgressChange -> " + Progress + " , " + ProgressMax);
		}

		//[id(0x00000069), helpstring("The enabled state of a command changed.")]
		public virtual void CommandStateChange(int Command, bool Enable) {
			Console.WriteLine("CommandStateChange -> " + Command + " , " + Enable);
		}

		//    [id(0x0000006a), helpstring("Download of a page started.")]
		public virtual void DownloadBegin() {
			Console.WriteLine("DownloadBegin");
		}

		//    [id(0x00000068), helpstring("Download of page complete.")]
		public virtual void DownloadComplete() {
			Console.WriteLine("DownloadComplete");
		}

		//[id(0x00000071), helpstring("Document title changed.")]
		public virtual void TitleChange(JIString Text) {
			Console.WriteLine("TitleChange -> " + Text.String);
		}

	   //[id(0x000000fb), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant NewWindow2(org.jinterop.dcom.core.JIVariant ppDisp, org.jinterop.dcom.core.JIVariant Cancel) throws org.jinterop.dcom.common.JIException
		public virtual JIVariant NewWindow2(JIVariant ppDisp, JIVariant Cancel) {
			Console.WriteLine("NewWindow2 -> " + Cancel.ObjectAsBoolean);
			return Cancel;
		}

		//[id(0x000000fc), helpstring("Fired when the document being navigated to becomes visible and enters the navigation stack.")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void NavigateComplete2(org.jinterop.dcom.core.IJIComObject pDisp, org.jinterop.dcom.core.JIVariant URL) throws org.jinterop.dcom.common.JIException
		public virtual void NavigateComplete2(IJIComObject pDisp, JIVariant URL) {
			pDisp = JIObjectFactory.NarrowObject(pDisp);
			JIVariant realURL = URL;
			while (realURL.ByRefFlagSet) {
				realURL = realURL.ObjectAsVariant;
			}

			Console.WriteLine("NavigateComplete2 -> " + pDisp.InterfaceIdentifier + " , " + realURL.ObjectAsString.String);
		}

		//[id(0x00000103), helpstring("Fired when the document being navigated to reaches ReadyState_Complete.")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void DocumentComplete(org.jinterop.dcom.core.IJIComObject pDisp, org.jinterop.dcom.core.JIVariant URL) throws org.jinterop.dcom.common.JIException
		public virtual void DocumentComplete(IJIComObject pDisp, JIVariant URL) {
			Console.WriteLine("DocumentComplete -> " + pDisp.InterfaceIdentifier + " , " + URL);
		}

		//[id(0x000000fd), helpstring("Fired when application is quiting.")]
		public virtual void OnQuit() {
			Console.WriteLine("OnQuit -> ");
		}

		//[id(0x000000fe), helpstring("Fired when the window should be shown/hidden")]
		public virtual void OnVisible(bool Visible) {
			Console.WriteLine("OnVisible -> " + Visible);
		}

		//[id(0x000000ff), helpstring("Fired when the toolbar  should be shown/hidden")]
		public virtual void OnToolBar(bool ToolBar) {
			Console.WriteLine("OnToolBar -> " + ToolBar);
		}

		//[id(0x00000100), helpstring("Fired when the menubar should be shown/hidden")]
		public virtual void OnMenuBar(bool MenuBar) {
			Console.WriteLine("OnMenuBar -> " + MenuBar);
		}

		//[id(0x00000101), helpstring("Fired when the statusbar should be shown/hidden")]
		public virtual void OnStatusBar(bool StatusBar) {
			Console.WriteLine("OnStatusBar -> " + StatusBar);
		}

		//[id(0x00000102), helpstring("Fired when fullscreen mode should be on/off")]
		public virtual void OnFullScreen(bool FullScreen) {
			Console.WriteLine("OnFullScreen -> " + FullScreen);
		}

		//[id(0x00000104), helpstring("Fired when theater mode should be on/off")]
		public virtual void OnTheaterMode(bool TheaterMode) {
			Console.WriteLine("OnTheaterMode -> " + TheaterMode);
		}

		//[id(0x00000106), helpstring("Fired when the host window should allow/disallow resizing")]
		public virtual void WindowSetResizable(bool Resizable) {
			Console.WriteLine("OnResizable -> " + Resizable);
		}

		//[id(0x00000108), helpstring("Fired when the host window should change its Left coordinate")]
		public virtual void WindowSetLeft(int Left) {
			Console.WriteLine("WindowSetLeft - > " + Left);
		}

		//[id(0x00000109), helpstring("Fired when the host window should change its Top coordinate")]
		public virtual void WindowSetTop(int Top) {
			Console.WriteLine("WindowSetTop - > " + Top);
		}

		//[id(0x0000010a), helpstring("Fired when the host window should change its width")]
		public virtual void WindowSetWidth(int Width) {
			Console.WriteLine("WindowSetWidth - > " + Width);
		}

		//[id(0x0000010b), helpstring("Fired when the host window should change its height")]
		public virtual void WindowSetHeight(int Height) {
			Console.WriteLine("WindowSetHeight - > " + Height);
		}

		//[id(0x00000107), helpstring("Fired when the WebBrowser is about to be closed by script")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant WindowClosing(boolean IsChildWindow, org.jinterop.dcom.core.JIVariant Cancel) throws org.jinterop.dcom.common.JIException
		public virtual JIVariant WindowClosing(bool IsChildWindow, JIVariant Cancel) {
			Console.WriteLine("WindowClosing -> " + IsChildWindow + " , " + Cancel.ObjectAsBoolean);
			return Cancel;
		}

		//[id(0x0000010c), helpstring("Fired to request client sizes be converted to host window sizes")]
		public virtual int?[] ClientToHostWindow(int CX, int CY) {
			Console.WriteLine("ClientToHostWindow - > " + CX + " , " + CY);
			return new int?[] { new int?(CX),new int?(CY) };
		}

		//    [id(0x0000010d), helpstring("Fired to indicate the security level of the current web page contents")]
		public virtual void SetSecureLockIcon(int SecureLockIcon) {
			Console.WriteLine("SetSecureLockIcon - > " + SecureLockIcon);
		}

		//[id(0x0000010e), helpstring("Fired to indicate the File Download dialog is opening")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant FileDownload(boolean noIdeaWhat,org.jinterop.dcom.core.JIVariant Cancel) throws org.jinterop.dcom.common.JIException
		public virtual JIVariant FileDownload(bool noIdeaWhat, JIVariant Cancel) {
			Console.WriteLine("FileDownload - > " + Cancel);
			return Cancel;
		}

		//[id(0x0000010f), helpstring("Fired when a binding error occurs (window or frameset element).")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant NavigateError(org.jinterop.dcom.core.IJIComObject pDisp, org.jinterop.dcom.core.JIVariant URL, org.jinterop.dcom.core.JIVariant Frame, org.jinterop.dcom.core.JIVariant StatusCode, org.jinterop.dcom.core.JIVariant Cancel) throws org.jinterop.dcom.common.JIException
		public virtual JIVariant NavigateError(IJIComObject pDisp, JIVariant URL, JIVariant Frame, JIVariant StatusCode, JIVariant Cancel) {
			Console.WriteLine("NavigateError - > " + URL.ObjectAsString);
			return Cancel;
		}

	   //[id(0x000000e1), helpstring("Fired when a print template is instantiated.")]
	   public virtual void PrintTemplateInstantiation(IJIComObject pDisp) {
		   Console.WriteLine("PrintTemplateInstantiation - > " + pDisp.InterfaceIdentifier);
	   }

	   //[id(0x000000e2), helpstring("Fired when a print template destroyed.")]
	   public virtual void PrintTemplateTeardown(IJIComObject pDisp) {
		   Console.WriteLine("PrintTemplateTeardown - > " + pDisp.InterfaceIdentifier);
	   }

	   //[id(0x000000e3), helpstring("Fired when a page is spooled. When it is fired can be changed by a custom template.")]
	   public virtual void UpdatePageStatus(IJIComObject pDisp, JIVariant nPage, JIVariant fDone) {
		   Console.WriteLine("UpdatePageStatus - > " + pDisp.InterfaceIdentifier);
	   }

	   //[id(0x00000110), helpstring("Fired when the global privacy impacted state changes")]
	   public virtual void PrivacyImpactedStateChange(bool bImpacted) {
		   Console.WriteLine("PrivacyImpactedStateChange - > " + bImpacted);
	   }

	   //[id(0x00000111), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public org.jinterop.dcom.core.JIVariant NewWindow3(org.jinterop.dcom.core.JIVariant ppDisp, org.jinterop.dcom.core.JIVariant Cancel, int dwFlags, org.jinterop.dcom.core.JIString bstrUrlContext, org.jinterop.dcom.core.JIString bstrUrl) throws org.jinterop.dcom.common.JIException
	   public virtual JIVariant NewWindow3(JIVariant ppDisp, JIVariant Cancel, int dwFlags, JIString bstrUrlContext, JIString bstrUrl) {
		   Console.WriteLine("NewWindow3 - > " + ppDisp + " , " + Cancel.ObjectAsBoolean + " , " + bstrUrl.String);
		   return Cancel;
	   }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void SetPhishingFilterStatus(int PhishingFilterStatus) throws org.jinterop.dcom.common.JIException
	   public virtual void SetPhishingFilterStatus(int PhishingFilterStatus) {
		   Console.WriteLine("SetPhishingFilterStatus - > " + PhishingFilterStatus);
		   //return Cancel;
	   }

	   public virtual void WindowStateChanged(int dwWindowStateFlags, int dwValidFlagsMask) {
		   Console.WriteLine("WindowStateChanged - > " + dwWindowStateFlags + " , " + dwValidFlagsMask);
	   }



	}

}
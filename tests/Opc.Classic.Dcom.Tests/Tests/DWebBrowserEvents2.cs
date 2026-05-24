// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;
    using System;

    public class DWebBrowserEvents2 {

        //    [id(0x00000070), helpstring("Fired when the PutProperty method has been called.")]
        //     void PropertyChange([in] BSTR szProperty);
        public void PropertyChange(ComString szProperty) => Console.WriteLine("PropertyChange -> " + szProperty.String);


        //    [id(0x000000fa), helpstring("Fired before navigate occurs in the given WebBrowser (window or frameset element). The processing of this navigation may be modified.")]
        //     void BeforeNavigate2(
        //                     [in] IDispatch* pDisp,
        //                     [in] VARIANT* URL,
        //                     [in] VARIANT* Flags,
        //                     [in] VARIANT* TargetFrameName,
        //                     [in] VARIANT* PostData,
        //                     [in] VARIANT* Headers,
        //                     [in, out] VARIANT_BOOL* Cancel);


#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
        public Variant BeforeNavigate2(IComObject dispatch, Variant URL, Variant Flags, Variant TargetFrameName, Variant PostData, Variant Headers, Variant Cancel) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
            dispatch = ObjectFactory.NarrowObject(dispatch);
            var realURL = URL;
            while (realURL.IsByRef) {
                realURL = realURL.ObjectAsVariant;
            }

            Console.WriteLine("BeforeNavigate2  -> " + realURL.ObjectAsString.String);

            // uncomment and return this to stop loading the page
            // <see cref="Variant"/> variant = new <see cref="Variant"/>(true,true);

            return Cancel;
        }

        // [id(0x00000066), helpstring("Statusbar text changed.")]
        public void StatusTextChange(ComString text) => Console.WriteLine("StatusTextChange -> " + text.String);

        // [id(0x0000006c), helpstring("Fired when download progress is updated.")]
        public void ProgressChange(int Progress, int ProgressMax) => Console.WriteLine("ProgressChange -> " + Progress + ", " + ProgressMax);

        // [id(0x00000069), helpstring("The enabled state of a command changed.")]
        public void CommandStateChange(int Command, bool Enable) => Console.WriteLine("CommandStateChange -> " + Command + ", " + Enable);

        //    [id(0x0000006a), helpstring("Download of a page started.")]
        public void DownloadBegin() => Console.WriteLine("DownloadBegin");

        //    [id(0x00000068), helpstring("Download of page complete.")]
        public void DownloadComplete() => Console.WriteLine("DownloadComplete");

        // [id(0x00000071), helpstring("Document title changed.")]
        public void TitleChange(ComString Text) => Console.WriteLine("TitleChange -> " + Text.String);

        // [id(0x000000fb), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]

#pragma warning disable RECS0154 // Parameter is never used
        public Variant NewWindow2(Variant ppDisp, Variant Cancel) {
#pragma warning restore RECS0154 // Parameter is never used
            Console.WriteLine("NewWindow2 -> " + Cancel.ObjectAsBoolean);
            return Cancel;
        }

        // [id(0x000000fc), helpstring("Fired when the document being navigated to becomes visible and enters the navigation stack.")]
        public void NavigateComplete2(IComObject pDisp, Variant URL) {
            pDisp = ObjectFactory.NarrowObject(pDisp);
            var realURL = URL;
            while (realURL.IsByRef) {
                realURL = realURL.ObjectAsVariant;
            }

            Console.WriteLine("NavigateComplete2 -> " + pDisp.InterfaceIdentifier + ", " + realURL.ObjectAsString.String);
        }

        // [id(0x00000103), helpstring("Fired when the document being navigated to reaches ReadyState_Complete.")]
        public void DocumentComplete(IComObject pDisp, Variant URL) => Console.WriteLine("DocumentComplete -> " + pDisp.InterfaceIdentifier + ", " + URL);

        // [id(0x000000fd), helpstring("Fired when application is quiting.")]
        public void OnQuit() => Console.WriteLine("OnQuit -> ");

        // [id(0x000000fe), helpstring("Fired when the window should be shown/hidden")]
        public void OnVisible(bool Visible) => Console.WriteLine("OnVisible -> " + Visible);

        // [id(0x000000ff), helpstring("Fired when the toolbar  should be shown/hidden")]
        public void OnToolBar(bool ToolBar) => Console.WriteLine("OnToolBar -> " + ToolBar);

        // [id(0x00000100), helpstring("Fired when the menubar should be shown/hidden")]
        public void OnMenuBar(bool MenuBar) => Console.WriteLine("OnMenuBar -> " + MenuBar);

        // [id(0x00000101), helpstring("Fired when the statusbar should be shown/hidden")]
        public void OnStatusBar(bool StatusBar) => Console.WriteLine("OnStatusBar -> " + StatusBar);

        // [id(0x00000102), helpstring("Fired when fullscreen mode should be on/off")]
        public void OnFullScreen(bool FullScreen) => Console.WriteLine("OnFullScreen -> " + FullScreen);

        // [id(0x00000104), helpstring("Fired when theater mode should be on/off")]
        public void OnTheaterMode(bool TheaterMode) => Console.WriteLine("OnTheaterMode -> " + TheaterMode);

        // [id(0x00000106), helpstring("Fired when the host window should allow/disallow resizing")]
        public void WindowSetResizable(bool Resizable) => Console.WriteLine("OnResizable -> " + Resizable);

        // [id(0x00000108), helpstring("Fired when the host window should change its Left coordinate")]
        public void WindowSetLeft(int Left) => Console.WriteLine("WindowSetLeft - > " + Left);

        // [id(0x00000109), helpstring("Fired when the host window should change its Top coordinate")]
        public void WindowSetTop(int Top) => Console.WriteLine("WindowSetTop - > " + Top);

        // [id(0x0000010a), helpstring("Fired when the host window should change its width")]
        public void WindowSetWidth(int Width) => Console.WriteLine("WindowSetWidth - > " + Width);

        // [id(0x0000010b), helpstring("Fired when the host window should change its height")]
        public void WindowSetHeight(int Height) => Console.WriteLine("WindowSetHeight - > " + Height);

        // [id(0x00000107), helpstring("Fired when the WebBrowser is about to be closed by script")]
        public Variant WindowClosing(bool IsChildWindow, Variant Cancel) {
            Console.WriteLine("WindowClosing -> " + IsChildWindow + ", " + Cancel.ObjectAsBoolean);
            return Cancel;
        }

        // [id(0x0000010c), helpstring("Fired to request client sizes be converted to host window sizes")]
        public int[] ClientToHostWindow(int CX, int CY) {
            Console.WriteLine("ClientToHostWindow - > " + CX + ", " + CY);
            return new int[] { CX, CY };
        }

        // [id(0x0000010d), helpstring("Fired to indicate the security level of the current web page contents")]
        public void SetSecureLockIcon(int SecureLockIcon) => Console.WriteLine("SetSecureLockIcon - > " + SecureLockIcon);

        // [id(0x0000010e), helpstring("Fired to indicate the File Download dialog is opening")]
#pragma warning disable RECS0154 // Parameter is never used
        public Variant FileDownload(bool noIdeaWhat, Variant Cancel) {
#pragma warning restore RECS0154 // Parameter is never used
            Console.WriteLine("FileDownload - > " + Cancel);
            return Cancel;
        }

        // [id(0x0000010f), helpstring("Fired when a binding error occurs (window or frameset element).")]

#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
        public Variant NavigateError(IComObject pDisp, Variant URL, Variant Frame, Variant StatusCode, Variant Cancel) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
            Console.WriteLine("NavigateError - > " + URL.ObjectAsString);
            return Cancel;
        }

        // [id(0x000000e1), helpstring("Fired when a print template is instantiated.")]
        public void PrintTemplateInstantiation(IComObject pDisp) => Console.WriteLine("PrintTemplateInstantiation - > " + pDisp.InterfaceIdentifier);

        // [id(0x000000e2), helpstring("Fired when a print template destroyed.")]
        public void PrintTemplateTeardown(IComObject pDisp) => Console.WriteLine("PrintTemplateTeardown - > " + pDisp.InterfaceIdentifier);

        // [id(0x000000e3), helpstring("Fired when a page is spooled. When it is fired can be changed by a custom template.")]
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
        public void UpdatePageStatus(IComObject pDisp, Variant nPage, Variant fDone) => Console.WriteLine("UpdatePageStatus - > " + pDisp.InterfaceIdentifier);
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used

        // [id(0x00000110), helpstring("Fired when the global privacy impacted state changes")]
        public void PrivacyImpactedStateChange(bool bImpacted) => Console.WriteLine("PrivacyImpactedStateChange - > " + bImpacted);

        // [id(0x00000111), helpstring("A new, hidden, non-navigated WebBrowser window is needed.")]

#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable RECS0154 // Parameter is never used
        public Variant NewWindow3(Variant ppDisp, Variant Cancel, int dwFlags, ComString bstrUrlContext, ComString bstrUrl) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore RECS0154 // Parameter is never used
            Console.WriteLine("NewWindow3 - > " + ppDisp + ", " + Cancel.ObjectAsBoolean + ", " + bstrUrl.String);
            return Cancel;
        }


        public void SetPhishingFilterStatus(int PhishingFilterStatus) => Console.WriteLine("SetPhishingFilterStatus - > " + PhishingFilterStatus);// return Cancel;

        public void WindowStateChanged(int dwWindowStateFlags, int dwValidFlagsMask) => Console.WriteLine("WindowStateChanged - > " + dwWindowStateFlags + ", " + dwValidFlagsMask);

    }

}
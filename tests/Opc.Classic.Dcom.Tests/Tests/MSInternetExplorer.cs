// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Automation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Opc.Classic.Dcom.Test;

public class MSInternetExplorer
{

    private readonly ComServer _comServer;
    private readonly Session _session;
    private readonly IComObject _ieObject;
    private readonly IDispatch _ieObjectDispatch;
    private string _identifier;

    public MSInternetExplorer(string address, string[] args)
    {
        Interop.MapHostNametoIP("locutus", "192.168.0.130");
        _session = Session.CreateSession(args[1], args[2], args[3]);
        _session.UseNTLMv2(true);
        _session.UseSessionSecurity(true);
        _comServer = new ComServer(ProgId.ValueOf("InternetExplorer.Application"), address, _session);
        _ieObject = _comServer.CreateInstance();
        var ieObjectWebBrowser2 = _ieObject.QueryInterface("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E");
        _ieObjectDispatch = (IDispatch)ObjectFactory.NarrowObject(_ieObject.QueryInterface(Interfaces.IID_IDispatch));

    }

    private void SetVisible()
    {

        var dispId = _ieObjectDispatch.GetIDsOfNames("Visible");
        _ieObjectDispatch.Put(dispId, new Variant(true));
        _ieObjectDispatch.Put("AddressBar", new Variant(true));
        _ieObjectDispatch.Put("MenuBar", new Variant(true));
        _ieObjectDispatch.Put("ToolBar", new Variant(true));

    }

    private void NavigateToUrl(string url) =>
        // ieObjectDispatch.put("Top",new <see cref="Variant"/>(new Integer(600)));
        // ieObjectDispatch.put("Left",new <see cref="Variant"/>(new Integer(700)));
        _ieObjectDispatch.CallMethod("Navigate2", new object[] {
            new ComString(url), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });

    private void AttachCallBack()
    {

        /// <summary>
        /// The LocalCOClass is a representation for a server class. It's there so that when we get to the next version of the library, I am able to support full bi-directional access. Currently, you can implement any IDL of an existing COM server using the LocalCOClass and
        /// pass it's interface pointer instead of the original COM server and it will work fine. Similar mechanism is exploited for call backs.In our case I had to implement DWebBrowserEvents interface.
        ///
        /// var component = new <see cref="LocalCoClass"/>(new <see cref="LocalInterfaceDefinition"/>("45B5FC0C-FAC2-42bd-923E-2B221A89E092"),DWebBrowserEvents2.class);
        ///
        /// This definition create a Java component with an IID of 45B5FC0C-FAC2-42bd-923E-2B221A89E092...I just made this one up for uniquely classifying this class...you can equate this to a lib identifier of COM IDL. This is required if there are multilple interfaces being implemented in the same Java Class.
        /// If you have only one...you can put it's IID here. I just did not do it for showing the user a possiblity.
        ///
        /// The LocalCOClass has the option of instantiating the DWebBrowserEvents.class or it could use another ctor to pass an already instantiated object. In latter scenario, the object would be used as target for the events instead of instantiating a new one from DWebBrowserEvents.class.
        /// Now that we have a Java server, we need to define the methods\events it will handle.
        ///
        /// This is done using the Method descriptors which are themselves described using the Parameter Objects.
        ///
        /// var propertyChangeObject = new <see cref="LocalParamsDescriptor"/>();
        ///
        /// This creates a Parameter Object, capable of defining a IN or OUT type for a Method.
        ///
        /// like:
        /// propertyChangeObject.addInParamAsType(typeof(<see cref="ComString"/>));
        ///
        /// var methodDescriptor = new <see cref="LocalMethodDescriptor"/>("PropertyChange",0x70,propertyChangeObject);
        /// component.getInterfaceDefinition().addMethodDescriptor(methodDescriptor);
        ///
        /// This declares a method descriptor. The first parameter in the ctor is the API name of the api to implement, the second one is it's OP number.
        /// This one can be obtained from the IDL\TypeLib. And the third param is the parameterObject describing the input\output types of this method.
        /// If you do not want to use this ctor, there is another, which sequentially increments the method numbers starting from 1.
        /// The calls below add a new interface IID to this Java server. It simply means that the server supports this interface definition.
        ///
        /// List<string> list = new List<string>();
        /// list.add("34A715A0-6587-11D0-924A-0020AFC7AC4D");
        /// component.setSupportedEventInterfaces(list);
        ///
        /// This will be the list of all COM interfaces which this Java class supports or implements.
        ///
        /// The next call attaches the event handler (our <see cref="LocalCoClass"/>) to the actual COM server for recieving events for the interface identified by the IID.
        /// There can be many such calls on the same COM server for different IIDs.
        /// identifier = <see cref="ObjectFactory"/>.attachEventHandler(ieObject,"34A715A0-6587-11D0-924A-0020AFC7AC4D",<see cref="InterfacePointer"/>.getInterfacePointer(session,component));
        ///
        /// Now whether you use <see cref="IDispatch"/> or not, events will work regardless of that. The COM object you have to use in the attachEventHandler is the COM Object on
        /// which you did the queryinterface for the <see cref="IDispatch"/>.
        ///
        ///
        /// </summary>
        var component = new LocalCoClass(new LocalInterfaceDefinition("34A715A0-6587-11D0-924A-0020AFC7AC4D"), typeof(DWebBrowserEvents2));

        var propertyChangeObject = new LocalParamsDescriptor();
        propertyChangeObject.AddInParamAsType(typeof(ComString));
        var methodDescriptor = new LocalMethodDescriptor("PropertyChange", 0x70, propertyChangeObject);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


        var navigateObject = new LocalParamsDescriptor();
        navigateObject.AddInParamAsType(typeof(IComObject));
        navigateObject.AddInParamAsType(typeof(Variant));
        navigateObject.AddInParamAsType(typeof(Variant));
        navigateObject.AddInParamAsType(typeof(Variant));
        navigateObject.AddInParamAsType(typeof(Variant));
        navigateObject.AddInParamAsType(typeof(Variant));
        navigateObject.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("BeforeNavigate2", 0xFA, navigateObject);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var StatusTextChange = new LocalParamsDescriptor();
        StatusTextChange.AddInParamAsType(typeof(ComString));
        methodDescriptor = new LocalMethodDescriptor("StatusTextChange", 0x66, StatusTextChange);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var ProgressChange = new LocalParamsDescriptor();
        ProgressChange.AddInParamAsType(typeof(int));
        ProgressChange.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("ProgressChange", 0x6c, ProgressChange);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var CommandStateChange = new LocalParamsDescriptor();
        CommandStateChange.AddInParamAsType(typeof(int));
        CommandStateChange.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("CommandStateChange", 0x69, CommandStateChange);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var DownloadBegin = new LocalParamsDescriptor();
        methodDescriptor = new LocalMethodDescriptor("DownloadBegin", 0x6a, DownloadBegin);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var DownloadComplete = new LocalParamsDescriptor();
        methodDescriptor = new LocalMethodDescriptor("DownloadComplete", 0x68, DownloadComplete);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var TitleChange = new LocalParamsDescriptor();
        TitleChange.AddInParamAsType(typeof(ComString));
        methodDescriptor = new LocalMethodDescriptor("TitleChange", 0x71, TitleChange);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var NewWindow2 = new LocalParamsDescriptor();
        NewWindow2.AddInParamAsType(typeof(Variant));
        NewWindow2.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("NewWindow2", 0xfb, NewWindow2);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var NavigateComplete2 = new LocalParamsDescriptor();
        NavigateComplete2.AddInParamAsType(typeof(IComObject));
        NavigateComplete2.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("NavigateComplete2", 0xfc, NavigateComplete2);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var DocumentComplete = new LocalParamsDescriptor();
        DocumentComplete.AddInParamAsType(typeof(IComObject));
        DocumentComplete.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("DocumentComplete", 0x103, DocumentComplete);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnQuit = new LocalParamsDescriptor();
        methodDescriptor = new LocalMethodDescriptor("OnQuit", 0xfd, OnQuit);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnVisible = new LocalParamsDescriptor();
        OnVisible.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnVisible", 0xfe, OnVisible);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnToolBar = new LocalParamsDescriptor();
        OnToolBar.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnToolBar", 0xff, OnToolBar);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnMenuBar = new LocalParamsDescriptor();
        OnMenuBar.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnMenuBar", 0x100, OnMenuBar);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnStatusBar = new LocalParamsDescriptor();
        OnStatusBar.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnStatusBar", 0x101, OnStatusBar);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnFullScreen = new LocalParamsDescriptor();
        OnFullScreen.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnFullScreen", 0x102, OnFullScreen);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var OnTheaterMode = new LocalParamsDescriptor();
        OnTheaterMode.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("OnTheaterMode", 0x104, OnTheaterMode);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowSetResizable = new LocalParamsDescriptor();
        WindowSetResizable.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("WindowSetResizable", 0x106, WindowSetResizable);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowSetLeft = new LocalParamsDescriptor();
        WindowSetLeft.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("WindowSetLeft", 0x108, WindowSetLeft);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowSetTop = new LocalParamsDescriptor();
        WindowSetTop.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("WindowSetTop", 0x109, WindowSetTop);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowSetWidth = new LocalParamsDescriptor();
        WindowSetWidth.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("WindowSetWidth", 0x10a, WindowSetWidth);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowSetHeight = new LocalParamsDescriptor();
        WindowSetHeight.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("WindowSetHeight", 0x10b, WindowSetHeight);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowClosing = new LocalParamsDescriptor();
        WindowClosing.AddInParamAsType(typeof(bool));
        WindowClosing.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("WindowClosing", 0x107, WindowClosing);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var ClientToHostWindow = new LocalParamsDescriptor();
        ClientToHostWindow.AddInParamAsType(typeof(int));
        ClientToHostWindow.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("ClientToHostWindow", 0x10c, ClientToHostWindow);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var SetSecureLockIcon = new LocalParamsDescriptor();
        SetSecureLockIcon.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("SetSecureLockIcon", 0x10d, SetSecureLockIcon);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var FileDownload = new LocalParamsDescriptor();
        FileDownload.AddInParamAsType(typeof(bool));
        FileDownload.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("FileDownload", 0x10e, FileDownload);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var NavigateError = new LocalParamsDescriptor();
        NavigateError.AddInParamAsType(typeof(IComObject));
        NavigateError.AddInParamAsType(typeof(Variant));
        NavigateError.AddInParamAsType(typeof(Variant));
        NavigateError.AddInParamAsType(typeof(Variant));
        NavigateError.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("NavigateError", 0x10f, NavigateError);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var NewWindow3 = new LocalParamsDescriptor();
        NewWindow3.AddInParamAsType(typeof(Variant));
        NewWindow3.AddInParamAsType(typeof(Variant));
        NewWindow3.AddInParamAsType(typeof(int));
        NewWindow3.AddInParamAsType(typeof(ComString));
        NewWindow3.AddInParamAsType(typeof(ComString));
        methodDescriptor = new LocalMethodDescriptor("NewWindow3", 0x111, NewWindow3);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var PrintTemplateInstantiation = new LocalParamsDescriptor();
        PrintTemplateInstantiation.AddInParamAsType(typeof(IComObject));
        methodDescriptor = new LocalMethodDescriptor("PrintTemplateInstantiation", 0xe1, PrintTemplateInstantiation);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var PrintTemplateTeardown = new LocalParamsDescriptor();
        PrintTemplateTeardown.AddInParamAsType(typeof(IComObject));
        methodDescriptor = new LocalMethodDescriptor("PrintTemplateTeardown", 0xe2, PrintTemplateTeardown);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var SetPhishingFilterStatus = new LocalParamsDescriptor();
        SetPhishingFilterStatus.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("SetPhishingFilterStatus", 0x11A, SetPhishingFilterStatus);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var WindowStateChanged = new LocalParamsDescriptor();
        WindowStateChanged.AddInParamAsType(typeof(int));
        WindowStateChanged.AddInParamAsType(typeof(int));
        methodDescriptor = new LocalMethodDescriptor("WindowStateChanged", 0x11B, WindowStateChanged);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


        var UpdatePageStatus = new LocalParamsDescriptor();
        UpdatePageStatus.AddInParamAsType(typeof(IComObject));
        UpdatePageStatus.AddInParamAsType(typeof(Variant));
        UpdatePageStatus.AddInParamAsType(typeof(Variant));
        methodDescriptor = new LocalMethodDescriptor("UpdatePageStatus", 0xe3, UpdatePageStatus);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);

        var PrivacyImpactedStateChange = new LocalParamsDescriptor();
        PrivacyImpactedStateChange.AddInParamAsType(typeof(bool));
        methodDescriptor = new LocalMethodDescriptor("PrivacyImpactedStateChange", 0x110, PrivacyImpactedStateChange);
        component.InterfaceDefinition.AddMethodDescriptor(methodDescriptor);


        var list = new List<string> {
            "34A715A0-6587-11D0-924A-0020AFC7AC4D",
            Interfaces.IID_IDispatch
        };
        component.SupportedEventInterfaces = list;

        _identifier = ObjectFactory.AttachEventHandler(_ieObject, "34A715A0-6587-11D0-924A-0020AFC7AC4D", ObjectFactory.BuildObject(_session, component));
        Thread.Sleep(5000);
    }

    private void DetachCallBack() => ObjectFactory.DetachEventHandler(_ieObject, _identifier);


    private void Quit()
    {
        _ieObjectDispatch.CallMethod("Quit");
        Session.DestroySession(_ieObjectDispatch.AssociatedSession);
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


            var internetExplorer = new MSInternetExplorer(args[0], args);
            internetExplorer.SetVisible();
            internetExplorer.AttachCallBack();
            internetExplorer.NavigateToUrl("http://www.sqlshark.com");
            Thread.Sleep(30000); // for call backs
            internetExplorer.DetachCallBack();
            Thread.Sleep(5000); // wait for 5 secs
            internetExplorer.Quit();
        }
        catch (Exception e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }

    }


}

// SPDX-License-Identifier: MIT

using System;
using SharpCifs.Util.Sharpen;
using Opc.Classic.Dcom.Automation;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Test;

public class MSOutLookExpressContacts
{

    internal Session _session;
    internal ComServer _comServer;

    internal MSOutLookExpressContacts(string[] args)
    {
        _session = Session.CreateSession(args[1], args[2], args[3]);
        _comServer = new ComServer(ProgId.ValueOf("Outlook.Application"), args[0], _session);
    }

    internal void DoStuff()
    {
        var unknown = _comServer.CreateInstance();
        var application = unknown.QueryInterface("00063001-0000-0000-C000-000000000046");

        var callObject = new CallBuilder(!application.DispatchSupported)
        {
            Opnum = 12
        };
        callObject.AddInParamAsString("MAPI", InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
        callObject.AddOutParamAsType(typeof(IComObject));
        var res = application.Call(callObject);

        var @namespace = ObjectFactory.NarrowObject((IComObject)res[0]);
        callObject = new CallBuilder
        {
            Opnum = 16
        };
        callObject.AddOutParamAsType(typeof(IComObject));
        res = @namespace.Call(callObject);

        if (res[0] == null)
        {
            Console.WriteLine("user cancelled request");
            return;
        }

        var folder = ObjectFactory.NarrowObject((IComObject)res[0]);
        callObject = new CallBuilder
        {
            Opnum = 4
        };
        callObject.AddOutParamAsType(typeof(int));
        res = folder.Call(callObject);

        if ((int)res[0] != 2)
        {
            Console.WriteLine("Invalid folder selected, this is not a \"contact\" folder, please reselect..");
            return;
        }

        callObject.ReInit();
        callObject.Opnum = 10;
        callObject.AddOutParamAsType(typeof(IComObject));
        res = folder.Call(callObject);
        if (res[0] == null)
        {
            Console.WriteLine("Unable to get Contact Items.");
            return;
        }

        var items = ObjectFactory.NarrowObject((IComObject)res[0]);
        callObject = new CallBuilder
        {
            Opnum = 12
        };
        callObject.AddOutParamAsType(typeof(IComObject));
        res = items.Call(callObject);

        while (true)
        {
            if (res[0] == null)
            {
                break;
            }

            var contactItem = (IDispatch)ObjectFactory.NarrowObject((IComObject)res[0]);
            var res2 = contactItem.Get("FullName");
            //            callObject = new CallBuilder(contactItem.getIpid());
            //            callObject.setOpnum(124);
            //            callObject.addOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            //            res = contactItem.call(callObject);
            var details = res2.ObjectAsString.String;

            //            callObject.reInit();
            //            callObject.setOpnum(100);
            //            callObject.addOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
            //            res = contactItem.call(callObject);
            res2 = contactItem.Get("Email1Address");
            details = details + "<" + res2.ObjectAsString.String + ">";

            Console.WriteLine(details);

            callObject = new CallBuilder
            {
                Opnum = 14
            };
            callObject.AddOutParamAsType(typeof(IComObject));
            res = items.Call(callObject);
        }

    }

    public static void RunTest(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Please provide address domain username password");
            return;
        }
        Interop.UseAutoRegistration = true;
        try
        {
            var outlookMessages = new MSOutLookExpressContacts(args);
            outlookMessages.DoStuff();
            Session.DestroySession(outlookMessages._session);
        }
        catch (UnknownHostException e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }
        catch (InteropException e)
        {
            // TODO Auto-generated catch block
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }

    }

}

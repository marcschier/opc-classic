using System;

namespace org.jinterop.dcom.test {

    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIFlags = org.jinterop.dcom.core.JIFlags;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class MSOutLookExpressContacts {

        internal JISession Session = null;
        internal JIComServer ComServer = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSOutLookExpressContacts(String args[]) throws java.net.UnknownHostException, org.jinterop.dcom.common.JIException
        public MSOutLookExpressContacts(string[] args) {
            Session = JISession.CreateSession(args[1],args[2],args[3]);
            ComServer = new JIComServer(JIProgId.ValueOf("Outlook.Application"),args[0],Session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void doStuff() throws org.jinterop.dcom.common.JIException
        public virtual void DoStuff() {
            IJIComObject unknown = (IJIComObject)ComServer.CreateInstance();
            IJIComObject application = (IJIComObject)unknown.QueryInterface("00063001-0000-0000-C000-000000000046");

            JICallBuilder callObject = new JICallBuilder(!application.DispatchSupported);
            callObject.Opnum = 12;
            callObject.AddInParamAsString("MAPI", JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            object[] res = application.Call(callObject);

            IJIComObject @namespace = JIObjectFactory.NarrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder();
            callObject.Opnum = 16;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            res = @namespace.Call(callObject);

            if (res[0] == null) {
                Console.WriteLine("user cancelled request");
                return;
            }

            IJIComObject folder = JIObjectFactory.NarrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder();
            callObject.Opnum = 4;
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
            res = folder.Call(callObject);

            if ((int)((int?)res[0]) != 2) {
                Console.WriteLine("Invalid folder selected, this is not a \"contact\" folder , please reselect..");
                return;
            }

            callObject.ReInit();
            callObject.Opnum = 10;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            res = folder.Call(callObject);
            if (res[0] == null) {
                Console.WriteLine("Unable to get Contact Items.");
                return;
            }

            IJIComObject items = JIObjectFactory.NarrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder();
            callObject.Opnum = 12;
            callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
            res = items.Call(callObject);

            while (true) {
                if (res[0] == null) {
                    break;
                }

                string details = null;
                IJIDispatch contactItem = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)res[0]);
                JIVariant res2 = contactItem.Get("FullName");
    //            callObject = new JICallBuilder(contactItem.getIpid());
    //            callObject.setOpnum(124);
    //            callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
    //            res = contactItem.call(callObject);
                details = res2.ObjectAsString.String;

    //            callObject.reInit();
    //            callObject.setOpnum(100);
    //            callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
    //            res = contactItem.call(callObject);
                res2 = contactItem.Get("Email1Address");
                details = details + "<" + res2.ObjectAsString.String + ">";

                Console.WriteLine(details);

                callObject = new JICallBuilder();
                callObject.Opnum = 14;
                callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
                res = items.Call(callObject);
            }

        }

        public static void Main(string[] args) {
            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            JISystem.AutoRegisteration = true;
            try {
                MSOutLookExpressContacts outlookMessages = new MSOutLookExpressContacts(args);
                outlookMessages.DoStuff();
                JISession.DestroySession(outlookMessages.Session);
            }
            catch (UnknownHostException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (JIException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }

    }

}
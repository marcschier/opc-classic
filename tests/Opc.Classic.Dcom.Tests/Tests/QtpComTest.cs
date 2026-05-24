// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Automation;
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using System;

    public class QtpComTest {

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;
        private readonly Session _session;

        public QtpComTest(string address, string domain, string username, string password) {
            /*Let the  library do this for you. You can set the "AutoRegistration" flag in the
              Interop class. When the library encounters a "Class not registered" exception, it will
              perform all the registry changes if the autoRegistration flag is set. And then re-attempt
              loading the COM Server. Please have a look at MSSysInfo,MSWMI examples.*/

            Interop.UseAutoRegistration = true;
            _session = Session.CreateSession(domain, username, password);
            _comServer = new ComServer(ProgId.ValueOf("QuickTest.Application"), address, _session);
        }

        public void StartQTP() {
            Console.WriteLine(_comServer.Properties);
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }

        public void ShowQtp() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);
        }

        public void EnvQtp() {
            _dispatch.CallMethodA("Open", new object[] { new ComString(
                "C:\\Programme\\Mercury Interactive\\QuickTest Professional\\Tests\\Test1"),
                new Variant(false), new Variant(true) });
            var variant = _dispatch.Get("Test");
            var test = (IDispatch)ObjectFactory.NarrowObject(variant.ObjectAsComObject);
            Console.WriteLine(test.Get("Author"));
            // and this is the original session associated with dispatch.
            Session.DestroySession(_session);
        }


#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public static void RunTest(string[] args) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter

            // "localhost", "ctron", "mpitonia", "ChrisSarah1"
            // "VPC003", "automation", "automated_user", "@utom@tion"
            // "automationsvr01", "AUTOMATION", "Automated_User", "@utom@tion"
            try {
                var comQtp = new QtpComTest("localhost", "domain", "username", "password");
                comQtp.StartQTP();
                comQtp.ShowQtp();
                comQtp.EnvQtp();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
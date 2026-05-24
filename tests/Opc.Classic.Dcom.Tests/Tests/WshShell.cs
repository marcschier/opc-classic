// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class WshShell {
        private readonly Session _session;
        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="address"></param>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public WshShell(string address, string domain, string username, string password) {
            _session = Session.CreateSession(domain, username, password);
            _comServer = new ComServer(ProgId.ValueOf("WScript.Shell"), address, _session);
        }

        /// <summary>
        /// Start
        /// </summary>
        public void StartWScript() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var jv = _dispatch.Get("CurrentDirectory");
            Console.WriteLine(jv.ObjectAsString.String);
            var dispId = _dispatch.GetIDsOfNames("CurrentDirectory");
            Console.WriteLine(dispId);
            var variant = new Variant("C://WINDOWS");
            _dispatch.Put(dispId, variant);
            jv = _dispatch.Get("CurrentDirectory");
            Console.WriteLine(jv.ObjectAsString.String);

            Thread.Sleep(60 * 1000 * 3);
            // WshShell.Exec
            Console.WriteLine(_dispatch.CallMethodA("Exec", new object[] { new ComString("calc") })[0]);
            Thread.Sleep(60 * 1000 * 3);
            // WshShell.Run
            Console.WriteLine(_dispatch.CallMethodA("Run", new object[] { new ComString("notepad"), new Variant(10), Variant.CreateOPTIONAL_PARAM() })[0]);
        }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public static void RunTest(string[] args) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter
            try {
                Interop.UseAutoRegistration = true;
                var wScript = new WshShell("localhost", "domain", "username", "password");
                wScript.StartWScript();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class WshShell {
        private readonly JISession _session;
        private readonly JIComServer _comServer;
        private IJIDispatch _dispatch;
        private IJIComObject _unknown;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="address"></param>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public WshShell(string address, string domain, string username, string password) {
            _session = JISession.CreateSession(domain, username, password);
            _comServer = new JIComServer(JIProgId.ValueOf("WScript.Shell"), address, _session);
        }

        /// <summary>
        /// Start
        /// </summary>
        public virtual void StartWScript() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var jv = _dispatch.Get("CurrentDirectory");
            Console.WriteLine(jv.ObjectAsString.String);
            var dispId = _dispatch.GetIDsOfNames("CurrentDirectory");
            Console.WriteLine(dispId);
            var variant = new JIVariant("C://WINDOWS");
            _dispatch.Put(dispId, variant);
            jv = _dispatch.Get("CurrentDirectory");
            Console.WriteLine(jv.ObjectAsString.String);

            Thread.Sleep(60 * 1000 * 3);
            //WshShell.Exec
            Console.WriteLine(_dispatch.CallMethodA("Exec", new object[] { new JIString("calc") })[0]);
            Thread.Sleep(60 * 1000 * 3);
            //WshShell.Run
            Console.WriteLine(_dispatch.CallMethodA("Run", new object[] { new JIString("notepad"), new JIVariant(10), JIVariant.CreateOPTIONAL_PARAM() })[0]);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args) {
#pragma warning restore IDE0060 // Remove unused parameter
            try {
                JISystem.UseAutoRegistration = true;
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
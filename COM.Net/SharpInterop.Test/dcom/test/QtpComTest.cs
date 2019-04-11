namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.impls.automation;
    using System;
    using IComObject = core.IComObject;
    using IJIDispatch = impls.automation.IJIDispatch;
    using JIComServer = core.JIComServer;
    using JIObjectFactory = impls.JIObjectFactory;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JISystem = common.JISystem;
    using JIVariant = core.JIVariant;

    public class QtpComTest {

        private readonly JIComServer _comServer;

        private IJIDispatch _dispatch;

        private IComObject _unknown;

        private readonly JISession _session;






        public QtpComTest(string address, string domain, string username, string password) {


            /*Let the j-Interop library do this for you. You can set the "autoRegistration" flag in the

              JISystem class. When the library encounters a "Class not registered" exception, it will

              perform all the registry changes if the autoRegistration flag is set. And then re-attempt

              loading the COM Server. Please have a look at MSSysInfo,MSWMI examples.*/

            JISystem.UseAutoRegistration = true;

            _session = JISession.CreateSession(domain, username, password);

            _comServer = new JIComServer(JIProgId.ValueOf("QuickTest.Application"), address, _session);

            //                    session.setGlobalSocketTimeout(30000);

        }




        public void StartQTP() {

            Console.WriteLine(_comServer.Properties);

            _unknown = _comServer.CreateInstance();

            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));

            // System.out.println(((JIVariant)dispatch.get("Version")).getObjectAsString().getString());

        }




        public void ShowQtp() {

            var dispId = _dispatch.GetIDsOfNames("Visible");

            var variant = new JIVariant(true);

            _dispatch.Put(dispId, variant);

        }




        public void EnvQtp() {

            _dispatch.CallMethodA("Open", new object[] { new JIString("C:\\Programme\\Mercury Interactive\\QuickTest Professional\\Tests\\Test1"), new JIVariant(false), new JIVariant(true) });

            var variant = _dispatch.Get("Test");

            var test = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
            Console.WriteLine(test.Get("Author"));

            // and this is the original session associated with dispatch.
            JISession.DestroySession(_session);

        }





#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args) {
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
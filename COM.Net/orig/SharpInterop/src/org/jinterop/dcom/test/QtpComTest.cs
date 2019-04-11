using System;

namespace org.jinterop.dcom.test {


    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIVariant = org.jinterop.dcom.core.JIVariant;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    public class QtpComTest {

        private JIComServer ComServer = null;

        private IJIDispatch Dispatch = null;

        private IJIComObject Unknown = null;

        private JISession Session = null;





//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public QtpComTest(String address, String domain, String username, String password) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public QtpComTest(string address, string domain, string username, string password) {

                        JISystem.Logger.Level = Level.FINEST;

                        /*Let the j-Interop library do this for you. You can set the "autoRegistration" flag in the
    
                          JISystem class. When the library encounters a "Class not registered" exception, it will
    
                          perform all the registry changes if the autoRegistration flag is set. And then re-attempt
    
                          loading the COM Server. Please have a look at MSSysInfo,MSWMI examples.*/

                        JISystem.AutoRegisteration = true;

                        Session = JISession.CreateSession(domain,username,password);

                        ComServer = new JIComServer(JIProgId.ValueOf("QuickTest.Application"), address, Session);

    //                    session.setGlobalSocketTimeout(30000);

        }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startQTP() throws org.jinterop.dcom.common.JIException
        public virtual void StartQTP() {

                        Console.WriteLine(ComServer.Properties);

                        Unknown = ComServer.CreateInstance();

                        Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));

                        //System.out.println(((JIVariant)dispatch.get("Version")).getObjectAsString().getString());

        }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showQtp() throws org.jinterop.dcom.common.JIException
        public virtual void ShowQtp() {

                        int dispId = Dispatch.GetIDsOfNames("Visible");

                        JIVariant variant = new JIVariant(true);

                        Dispatch.Put(dispId,variant);

        }



//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void envQtp() throws org.jinterop.dcom.common.JIException
        public virtual void EnvQtp() {

                        Dispatch.CallMethodA("Open", new object[]{ new JIString("C:\\Programme\\Mercury Interactive\\QuickTest Professional\\Tests\\Test1"), new JIVariant(false), new JIVariant(true) });

                        JIVariant variant = Dispatch.Get("Test");

                        IJIDispatch test = (IJIDispatch)JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
                        Console.WriteLine(test.Get("Author"));

                        //and this is the original session associated with dispatch.
                        JISession.DestroySession(Session);

        }





        public static void Main(string[] args) {

                        //"localhost", "ctron", "mpitonia", "ChrisSarah1"

                        //"VPC003", "automation" , "automated_user", "@utom@tion"

                        //"automationsvr01", "AUTOMATION", "Automated_User", "@utom@tion"

                        try {

                                        QtpComTest comQtp = new QtpComTest("localhost", "domain", "username", "password");

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
namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class MSSysInfo {

        internal JISession _session;
        internal IJIComObject _sysInfoObject;
        internal IJIComObject _sysInfoServer;
        internal IJIDispatch _dispatch;
        internal string _identifier;
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: MSSysInfo(String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        internal MSSysInfo(string[] args) {
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            _session.UseSessionSecurity(true);
            var comServer = new JIComServer(JIProgId.ValueOf("SYSINFO.SysInfo"), args[0], _session);
            _sysInfoServer = comServer.CreateInstance();
            _sysInfoObject = _sysInfoServer.QueryInterface("6FBA474C-43AC-11CE-9A0E-00AA0062BB4C");
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_sysInfoObject.QueryInterface(Interfaces.IID_IDispatch));

        }
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void displayValues() throws org.jinterop.dcom.common.JIException
        internal virtual void DisplayValues() {
            Console.WriteLine("ACStatus: " + _dispatch.Get("ACStatus").ObjectAsShort);
            Console.WriteLine("BatteryFullTime: " + _dispatch.Get("BatteryFullTime").ObjectAsInt);
            Console.WriteLine("BatteryLifePercent: " + _dispatch.Get("BatteryLifePercent").ObjectAsShort);
            Console.WriteLine("BatteryLifeTime: " + _dispatch.Get("BatteryLifeTime").ObjectAsInt);
            Console.WriteLine("BatteryStatus: " + _dispatch.Get("BatteryStatus").ObjectAsShort);
            Console.WriteLine("OSVersion: " + _dispatch.Get("OSVersion").ObjectAsFloat);
            //dispatch.callMethod("AboutBox");

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void AttachEventListener() throws org.jinterop.dcom.common.JIException
        internal virtual void AttachEventListener() {
            //6FBA474D-43AC-11CE-9A0E-00AA0062BB4C

            var javaComponent = new JILocalCoClass(new JILocalInterfaceDefinition("6FBA474D-43AC-11CE-9A0E-00AA0062BB4C"), typeof(SysInfoEvents));
            javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("PowerStatusChanged", 8, null));
            javaComponent.InterfaceDefinition.AddMethodDescriptor(new JILocalMethodDescriptor("TimeChanged", 3, null));
            _identifier = JIObjectFactory.AttachEventHandler(_sysInfoServer, "6FBA474D-43AC-11CE-9A0E-00AA0062BB4C", JIObjectFactory.BuildObject(_session, javaComponent));
            Thread.Sleep(3000);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void DetachEventListener() throws org.jinterop.dcom.common.JIException
        internal virtual void DetachEventListener() {
            JIObjectFactory.DetachEventHandler(_sysInfoServer, _identifier);
            JISession.DestroySession(_dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                JISystem.UseAutoRegistration = true;
                var sysInfo = new MSSysInfo(args);
                sysInfo.DisplayValues();
                sysInfo.AttachEventListener();
                Thread.Sleep(20000); //now play around with power settings
                sysInfo.DetachEventListener();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }


    }


}
// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class MSSysInfo {

        internal Session _session;
        internal IComObject _sysInfoObject;
        internal IComObject _sysInfoServer;
        internal IDispatch _dispatch;
        internal string _identifier;

        internal MSSysInfo(string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _session.UseSessionSecurity(true);
            var comServer = new ComServer(ProgId.ValueOf("SYSINFO.SysInfo"), args[0], _session);
            _sysInfoServer = comServer.CreateInstance();
            _sysInfoObject = _sysInfoServer.QueryInterface("6FBA474C-43AC-11CE-9A0E-00AA0062BB4C");
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_sysInfoObject.QueryInterface(Interfaces.IID_IDispatch));

        }

        internal void DisplayValues() {
            Console.WriteLine("ACStatus: " + _dispatch.Get("ACStatus").ObjectAsShort);
            Console.WriteLine("BatteryFullTime: " + _dispatch.Get("BatteryFullTime").ObjectAsInt);
            Console.WriteLine("BatteryLifePercent: " + _dispatch.Get("BatteryLifePercent").ObjectAsShort);
            Console.WriteLine("BatteryLifeTime: " + _dispatch.Get("BatteryLifeTime").ObjectAsInt);
            Console.WriteLine("BatteryStatus: " + _dispatch.Get("BatteryStatus").ObjectAsShort);
            Console.WriteLine("OSVersion: " + _dispatch.Get("OSVersion").ObjectAsFloat);
            // dispatch.callMethod("AboutBox");

        }

        internal void AttachEventListener() {
            // 6FBA474D-43AC-11CE-9A0E-00AA0062BB4C

            var javaComponent = new LocalCoClass(new LocalInterfaceDefinition("6FBA474D-43AC-11CE-9A0E-00AA0062BB4C"), typeof(SysInfoEvents));
            javaComponent.InterfaceDefinition.AddMethodDescriptor(new LocalMethodDescriptor("PowerStatusChanged", 8, null));
            javaComponent.InterfaceDefinition.AddMethodDescriptor(new LocalMethodDescriptor("TimeChanged", 3, null));
            _identifier = ObjectFactory.AttachEventHandler(_sysInfoServer, "6FBA474D-43AC-11CE-9A0E-00AA0062BB4C", ObjectFactory.BuildObject(_session, javaComponent));
            Thread.Sleep(3000);
        }

        internal void DetachEventListener() {
            ObjectFactory.DetachEventHandler(_sysInfoServer, _identifier);
            Session.DestroySession(_dispatch.AssociatedSession);
        }

        public static void RunTest(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                Interop.UseAutoRegistration = true;
                var sysInfo = new MSSysInfo(args);
                sysInfo.DisplayValues();
                sysInfo.AttachEventListener();
                Thread.Sleep(20000); // now play around with power settings
                sysInfo.DetachEventListener();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}
// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class OPCEventSink {
        public const string OPC_IID = "6516885F-5783-11D1-84A0-00608CB8A7E9";
        private const string kLOCAL_CLASS_IID = "85360DFE-6249-47AB-BE2D-6D68AA325CE8";

        private readonly HashSet<object> _listeners;

        public OPCEventSink() => _listeners = new HashSet<object>();

        public void AddListener(IEventNotificationListener listener) {
            if (listener == null) {
                throw new System.NullReferenceException("The listener is null");
            }
            lock (_listeners) {
                _listeners.Add(listener);
            }
        }

        public void RemoveListener(IEventNotificationListener listener) {
            lock (_listeners) {
                _listeners.Remove(listener);
            }
        }

        /// <summary>
        /// This method is provided by the client to handle notifications
        /// from the OPCEventSubscription for events. This method can be
        /// called whether this is a refresh or standard event notification.
        /// </summary>
        /// <param name="clientSubscription"> The client handle for the
        /// subscription object sending the event notifications. </param>
        /// <param name="refresh">TRUE if this is a subscription refresh.
        /// </param>
        /// <param name="lastRefresh"> TRUE if this is the last subscription
        /// refresh in response to a specific invocation of the
        /// IOPCEventSubscriptionMgt::Refresh method. </param>
        /// <param name="count"> Number of event notifications. A value of
        /// zero indicates this is a keep-alive notification. </param>
        /// <param name="eventsArray">Array of event notifications</param>
        /// <returns>An EMPTY() array. </returns>
        /// <exception cref="InteropException"> </exception>
#pragma warning disable RECS0154 // Parameter is never used
        public object[] OnEvent(int clientSubscription, int refresh, int lastRefresh,
#pragma warning restore RECS0154 // Parameter is never used
            int count, ComArray eventsArray) {
            Struct[] events;
            if (count == 0) {
                events = new Struct[0];
            }
            else {
                events = (Struct[])eventsArray.ArrayInstance;
            }
            new RunnableAnonymousInnerClassHelper(this, events, "Opc event sink thread").Start();
            return new object[0];
        }

        private class RunnableAnonymousInnerClassHelper : Thread {
            private readonly OPCEventSink _outerInstance;

            private readonly Struct[] _events;

            public RunnableAnonymousInnerClassHelper(OPCEventSink outerInstance,
                Struct[] events, string name) : base(name) {
                _outerInstance = outerInstance;
                _events = events;
            }

            public override void Run() {
                IEventNotificationListener[] l;
                lock (_outerInstance._listeners) {
                    l = _outerInstance._listeners.Cast<IEventNotificationListener>().ToArray();
                }
                for (var i = 0; i < l.Length; i++) {
                    l[i].OnEvent(_events);
                }
            }
        }

        /// <summary>
        /// Create an out struct definition of this object that may be use in a call object
        /// @return
        ///        The OPC struct definition
        /// </summary>
        public static Struct FileTimeOutStruct() {
            var strukt = new Struct();
            try {
                strukt.AddMember(typeof(int)); // Low date time
                strukt.AddMember(typeof(int)); // High date time
                return strukt;
            }
            catch (InteropException) { // Can't occur
                throw new Exception("Add member error");
            }
        }

        /// <summary>
        /// Create an out struct definition of this object that may be use in a call object
        /// @return
        ///        The OPC struct definition
        /// </summary>
        private static Struct OutStruct() {
            var strukt = new Struct();
            try {
                strukt.AddMember(typeof(short));
                strukt.AddMember(typeof(short));
                strukt.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                strukt.AddMember(FileTimeOutStruct());
                strukt.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                strukt.AddMember(typeof(int));
                strukt.AddMember(typeof(int));
                strukt.AddMember(typeof(int));
                strukt.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                strukt.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                strukt.AddMember(typeof(short));
                strukt.AddMember(typeof(short));
                strukt.AddMember(typeof(int));
                strukt.AddMember(FileTimeOutStruct());
                strukt.AddMember(typeof(int));
                strukt.AddMember(typeof(int));
                strukt.AddMember(new ComPointer(new ComArray(typeof(Variant), null, 1, true)));
                strukt.AddMember(new ComPointer(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                return strukt;
            }
            catch (InteropException) { // Can't occur
                throw new Exception("Add member error");
            }
        }

        public static LocalCoClass GetCoClass(OPCEventSink instance) {
            // Define the onEvent method for this interface
            var oeParams = new LocalParamsDescriptor();
            oeParams.AddInParamAsType(typeof(int));
            oeParams.AddInParamAsType(typeof(int));
            oeParams.AddInParamAsType(typeof(int));
            oeParams.AddInParamAsType(typeof(int));
            oeParams.AddInParamAsObject(new ComArray(OutStruct(), null, 1, true));
            var oeMethod = new LocalMethodDescriptor("onEvent", 0, oeParams);
            // This identifies the OPCEventSink and not the interface
            var def = new LocalInterfaceDefinition(kLOCAL_CLASS_IID, false);
            def.AddMethodDescriptor(oeMethod);
            var coClass = (instance == null) ? new LocalCoClass(def, typeof(OPCEventSink)) : new LocalCoClass(def, instance);
            var list = new List<string> {
                // Supported interface
                OPC_IID
            };
            coClass.SupportedEventInterfaces = list;
            return coClass;
        }
    }

}
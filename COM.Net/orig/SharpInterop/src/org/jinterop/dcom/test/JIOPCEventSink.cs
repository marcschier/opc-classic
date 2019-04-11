using System;
using System.Collections.Generic;
using System.Threading;

namespace org.jinterop.dcom.test {


    using JIException = org.jinterop.dcom.common.JIException;
    using JIArray = org.jinterop.dcom.core.JIArray;
    using JIFlags = org.jinterop.dcom.core.JIFlags;
    using JILocalCoClass = org.jinterop.dcom.core.JILocalCoClass;
    using JILocalInterfaceDefinition = org.jinterop.dcom.core.JILocalInterfaceDefinition;
    using JILocalMethodDescriptor = org.jinterop.dcom.core.JILocalMethodDescriptor;
    using JILocalParamsDescriptor = org.jinterop.dcom.core.JILocalParamsDescriptor;
    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIStruct = org.jinterop.dcom.core.JIStruct;
    using JIVariant = org.jinterop.dcom.core.JIVariant;


    public class JIOPCEventSink {
        public const string OPC_IID = "6516885F-5783-11D1-84A0-00608CB8A7E9";
        private const string LOCAL_CLASS_IID = "85360DFE-6249-47AB-BE2D-6D68AA325CE8";

        private readonly HashSet Listeners;

        public JIOPCEventSink() {
            Listeners = new HashSet();
        }

        public virtual void AddListener(EventNotificationListener listener) {
            if (listener == null) {
                throw new System.NullReferenceException("The listener is null");
            }
            lock (Listeners) {
                Listeners.Add(listener);
            }
        }

        public virtual void RemoveListener(EventNotificationListener listener) {
            lock (Listeners) {
                Listeners.Remove(listener);
            }
        }

        /// <summary>
        /// This method is provided by the client to handle notifications from the OPCEventSubscription for events. This method can be
        /// called whether this is a refresh or standard event notification. </summary>
        /// <param name="clientSubscription">
        ///         The client handle for the subscription object sending the event notifications. </param>
        /// <param name="refresh">
        ///         TRUE if this is a subscription refresh. </param>
        /// <param name="lastRefresh">
        ///         TRUE if this is the last subscription refresh in response to a specific invocation of the IOPCEventSubscriptionMgt::Refresh method. </param>
        /// <param name="count">
        ///         Number of event notifications. A value of zero indicates this is a keep-alive notification. </param>
        /// <param name="events">
        ///         Array of event notifications
        /// @return
        ///         An EMPTY() array. </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] onEvent(final int clientSubscription,final int refresh,final int lastRefresh, int count, org.jinterop.dcom.core.JIArray eventsArray) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public virtual object[] OnEvent(int clientSubscription, int refresh, int lastRefresh, int count, JIArray eventsArray) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.jinterop.dcom.core.JIStruct[] events;
            JIStruct[] events;
            if (count == 0) {
                events = new JIStruct[0];
            }
            else {
                events = (JIStruct[])eventsArray.ArrayInstance;
            }
            (new Thread(new RunnableAnonymousInnerClassHelper(this, events),"Opc event sink thread")).Start();
            return new object[0];
        }

        private class RunnableAnonymousInnerClassHelper : Runnable {
            private readonly JIOPCEventSink OuterInstance;

            private JIStruct[] Events;

            public RunnableAnonymousInnerClassHelper(JIOPCEventSink outerInstance, JIStruct[] events) {
                this.OuterInstance = outerInstance;
                this.Events = events;
            }

            public virtual void Run() {
                EventNotificationListener[] l;
                lock (OuterInstance.Listeners) {
                    l = (EventNotificationListener[])OuterInstance.Listeners.toArray(new EventNotificationListener[OuterInstance.Listeners.Count]);
                }
                for (int i = 0; i < l.Length;i++) {
                    l[i].OnEvent(Events);
                }
            }
        }

        /// <summary>
        /// Create an out struct definition of this object that may be use in a call object
        /// @return
        ///         The OPC struct definition
        /// </summary>
        public static JIStruct FileTimeOutStruct() {
            JIStruct @struct = new JIStruct();
            try {
                @struct.AddMember(typeof(int?)); //Low date time
                @struct.AddMember(typeof(int?)); //High date time
                return @struct;
            }
            catch (JIException)
            { // Can't occur
                throw new Exception("Add member error");
            }
        }

        /// <summary>
        /// Create an out struct definition of this object that may be use in a call object
        /// @return
        ///         The OPC struct definition
        /// </summary>
        private static JIStruct OutStruct() {
            JIStruct @struct = new JIStruct();
            try {
                @struct.AddMember(typeof(short?));
                @struct.AddMember(typeof(short?));
                @struct.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                @struct.AddMember(FileTimeOutStruct());
                @struct.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                @struct.AddMember(typeof(int?));
                @struct.AddMember(typeof(int?));
                @struct.AddMember(typeof(int?));
                @struct.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                @struct.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                @struct.AddMember(typeof(short?));
                @struct.AddMember(typeof(short?));
                @struct.AddMember(typeof(int?));
                @struct.AddMember(FileTimeOutStruct());
                @struct.AddMember(typeof(int?));
                @struct.AddMember(typeof(int?));
                @struct.AddMember(new JIPointer(new JIArray(typeof(JIVariant),null,1,true)));
                @struct.AddMember(new JIPointer(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR)));
                return @struct;
            }
            catch (JIException)
            { // Can't occur
                throw new Exception("Add member error");
            }
        }

        public static JILocalCoClass GetCoClass(JIOPCEventSink instance) {
            //Define the onEvent method for this interface
            JILocalParamsDescriptor oeParams = new JILocalParamsDescriptor();
            oeParams.AddInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
            oeParams.AddInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
            oeParams.AddInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
            oeParams.AddInParamAsType(typeof(int?), JIFlags.FLAG_NULL);
            oeParams.AddInParamAsObject(new JIArray(OutStruct(),null,1,true), JIFlags.FLAG_NULL);
            JILocalMethodDescriptor oeMethod = new JILocalMethodDescriptor("onEvent",0,oeParams);
            //This identify the JIOPCEventSink and not the interface
            JILocalInterfaceDefinition def = new JILocalInterfaceDefinition(LOCAL_CLASS_IID,false);
            def.AddMethodDescriptor(oeMethod);
            JILocalCoClass coClass = (instance == null) ? new JILocalCoClass(def,typeof(JIOPCEventSink)) : new JILocalCoClass(def,instance);
            List<object> list = new List<object>();
            //Supported interface
            list.Add(OPC_IID);
            coClass.SupportedEventInterfaces = list;
            return coClass;
        }
    }

}
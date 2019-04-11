namespace org.jinterop.dcom.test {

    using JIStruct = org.jinterop.dcom.core.JIStruct;


    public interface EventNotificationListener {

        void OnEvent(JIStruct[] @event);

    }

}
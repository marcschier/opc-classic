namespace org.jinterop.dcom.test
{

    using JIStruct = core.JIStruct;


    public interface EventNotificationListener
    {

        void onEvent(JIStruct[] @event);

    }

}
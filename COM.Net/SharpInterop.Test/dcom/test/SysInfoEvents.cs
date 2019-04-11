
namespace org.jinterop.dcom.test {
    using System;

    public class SysInfoEvents {

        public void PowerStatusChanged() => Console.WriteLine("Called by COM -> PowerStatusChanged");

        public void TimeChanged() => Console.WriteLine("Called by COM -> TimeChanged");
    }
}

namespace org.jinterop.dcom.test {
    using System;

    public class SysInfoEvents {

        public virtual void PowerStatusChanged() => Console.WriteLine("Called by COM -> PowerStatusChanged");

        public virtual void TimeChanged() => Console.WriteLine("Called by COM -> TimeChanged");
    }
}
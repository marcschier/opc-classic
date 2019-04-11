using System;

namespace org.jinterop.dcom.test {

    public class SysInfoEvents {


            public SysInfoEvents() {

            }
            public virtual void PowerStatusChanged() {
                Console.WriteLine("Called by COM -> PowerStatusChanged");
            }

            public virtual void TimeChanged() {
                Console.WriteLine("Called by COM -> TimeChanged");
            }

    }

}
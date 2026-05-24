// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using System;

    public class SysInfoEvents {

        public void PowerStatusChanged() => Console.WriteLine("Called by COM -> PowerStatusChanged");

        public void TimeChanged() => Console.WriteLine("Called by COM -> TimeChanged");
    }
}
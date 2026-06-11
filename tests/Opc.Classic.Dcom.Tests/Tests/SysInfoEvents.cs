// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Test;

public class SysInfoEvents
{

    public void PowerStatusChanged() => Console.WriteLine("Called by COM -> PowerStatusChanged");

    public void TimeChanged() => Console.WriteLine("Called by COM -> TimeChanged");
}

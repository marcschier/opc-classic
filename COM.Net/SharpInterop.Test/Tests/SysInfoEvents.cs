//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using System;

    public class SysInfoEvents {

        public void PowerStatusChanged() => Console.WriteLine("Called by COM -> PowerStatusChanged");

        public void TimeChanged() => Console.WriteLine("Called by COM -> TimeChanged");
    }
}
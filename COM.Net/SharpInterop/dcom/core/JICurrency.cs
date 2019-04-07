//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using System;

    /// <summary>
    /// Definition from MSDN: encapsulates the CURRENCY data type used in Automation. CURRENCY is implemented
    /// as an 8-byte, two's-complement integer value scaled by 10,000. This gives a fixed-point number
    /// with 15 digits to the left of the decimal point and 4 digits to the right. The CURRENCY data type
    /// is extremely useful for calculations involving money, or for any fixed-point calculation where accuracy
    /// is important. It is one of the possible types for the VARIANT data type of Automation.
    /// If the absolute value of the fractional part is greater than 10,000, the appropriate adjustment
    /// is made to the units, as shown in the third of the following examples.
    /// Note that the units and fractional part are specified by signed long values. The fourth of the following
    /// examples shows what happens when the parameters have different signs.
    /// <code>
    /// COleCurrency curA;           // value: 0.0000
    /// curA.SetCurrency(4, 500);    // value: 4.0500
    /// curA.SetCurrency(2, 11000);  // value: 3.1000
    /// curA.SetCurrency(2, -50);    // value: 1.9950
    /// </code>
    /// </summary>
    public sealed class JICurrency {

        /// <summary>
        /// Returns the units value.
        /// </summary>
        public int Units { get; } = 0;

        /// <summary>
        /// Returns the fractionalUnits value.
        /// </summary>
        public int FractionalUnits { get; } = 0;

        /// <summary>
        /// Create currency
        /// </summary>
        /// <param name="value"></param>
        public JICurrency(string value) {
            if (value.StartsWith(".", StringComparison.Ordinal)) {
                value = "0" + value;
            }

            if (value.EndsWith(".", StringComparison.Ordinal)) {
                value += "0";
            }

            var str = value.Split("\\.", true);

            Units = int.Parse(str[0]);
            if (str.Length > 1) {
                FractionalUnits = int.Parse(str[1]);
            }
        }

        /// <summary>
        /// Create currency
        /// </summary>
        /// <param name="units"></param>
        /// <param name="fractionalUnits"></param>
        public JICurrency(int units, int fractionalUnits) {
            Units = units;
            FractionalUnits = fractionalUnits;
        }
    }
}
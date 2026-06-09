// SPDX-License-Identifier: MIT

using System;
using System.Globalization;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Definition from MSDN: encapsulates the CURRENCY data type used
/// in Automation. CURRENCY is implemented as an 8-byte, two's-complement
/// integer value scaled by 10,000. This gives a fixed-point number
/// with 15 digits to the left of the decimal point and 4 digits to the
/// right. The CURRENCY data type is extremely useful for calculations
/// involving money, or for any fixed-point calculation where accuracy
/// is important. It is one of the possible types for the VARIANT data
/// type of Automation.
/// If the absolute value of the fractional part is greater than 10,000,
/// the appropriate adjustment is made to the units, as shown in the third
/// of the following examples.
/// Note that the units and fractional part are specified by signed long
/// values. The fourth of the following examples shows what happens when
/// the parameters have different signs.
/// <code>
/// COleCurrency curA;           // value: 0.0000
/// curA.SetCurrency(4, 500);    // value: 4.0500
/// curA.SetCurrency(2, 11000);  // value: 3.1000
/// curA.SetCurrency(2, -50);    // value: 1.9950
/// </code>
/// </summary>
public sealed class Currency {

    /// <summary>
    /// Returns the units value.
    /// </summary>
    public int Units { get; }

    /// <summary>
    /// Returns the fractionalUnits value.
    /// </summary>
    public int FractionalUnits { get; }

    /// <summary>
    /// Create currency
    /// </summary>
    /// <param name="value"></param>
    public Currency(string value) {
        if (value.StartsWith('.')) {
            value = "0" + value;
        }

        if (value.EndsWith('.')) {
            value += "0";
        }

        var str = value.Split("\\.", true);

        Units = int.Parse(str[0], CultureInfo.InvariantCulture);
        if (str.Length > 1) {
            FractionalUnits = int.Parse(str[1], CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Create currency
    /// </summary>
    /// <param name="units"></param>
    /// <param name="fractionalUnits"></param>
    public Currency(int units, int fractionalUnits) {
        Units = units;
        FractionalUnits = fractionalUnits;
    }
}

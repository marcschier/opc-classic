using System;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {


	/// <summary>
	///Definition from MSDN: <i> encapsulates the CURRENCY data type used in Automation. CURRENCY is implemented
	/// as an 8-byte, two's-complement integer value scaled by 10,000. This gives a fixed-point number
	/// with 15 digits to the left of the decimal point and 4 digits to the right. The CURRENCY data type
	/// is extremely useful for calculations involving money, or for any fixed-point calculation where accuracy
	/// is important. It is one of the possible types for the VARIANT data type of Automation.<para>
	/// 
	/// for example :- <br>
	/// If the absolute value of the fractional part is greater than 10,000, the appropriate adjustment
	/// </para>
	/// is made to the units, as shown in the third of the following examples. <para>
	/// 
	/// Note that the units and fractional part are specified by signed long values. The fourth of the following
	/// </para>
	/// examples shows what happens when the parameters have different signs. <para>
	/// 
	/// COleCurrency curA;           // value: 0.0000 <br>
	/// curA.SetCurrency(4, 500);    // value: 4.0500 <br>
	/// curA.SetCurrency(2, 11000);  // value: 3.1000 <br>
	/// curA.SetCurrency(2, -50);    // value: 1.9950 <br>
	/// 
	/// </i>
	/// @since 1.0
	/// </para>
	/// </summary>
	public sealed class JICurrency {

		private int Units_Renamed = 0;
		private int FractionalUnits_Renamed = 0;

	//	private double value = 0;

		public JICurrency(string value) {
			if (value.StartsWith(".", StringComparison.Ordinal)) {
				value = "0" + value;
			}

			if (value.EndsWith(".", StringComparison.Ordinal)) {
				value = value + "0";
			}

			string[] str = value.Split("\\.", true);

			Units_Renamed = int.Parse(str[0]);
			if (str.Length > 1) {
				FractionalUnits_Renamed = int.Parse(str[1]);
			}

		}

		public JICurrency(int units, int fractionalUnits) {
			this.Units_Renamed = units;
			this.FractionalUnits_Renamed = fractionalUnits;
		}

		/// <summary>
		///Returns the units value. <br>
		/// 
		/// @return
		/// </summary>
		public int Units {
			get {
				return Units_Renamed;
			}
		}

		/// <summary>
		///Returns the fractionalUnits value. <br>
		/// 
		/// @return
		/// </summary>
		public int FractionalUnits {
			get {
				return FractionalUnits_Renamed;
			}
		}

	//	/**Returns the encapsulated value.
	//	 *
	//	 * @return
	//	 */
	//	public double getValue()
	//	{
	//		return value;
	//	}

	}

}
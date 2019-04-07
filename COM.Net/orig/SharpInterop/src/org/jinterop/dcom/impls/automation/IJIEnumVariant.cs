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

namespace org.jinterop.dcom.impls.automation {

	using JIException = org.jinterop.dcom.common.JIException;


	/// <summary>
	///<para> Represents the Windows COM <code>IEnumVARIANT</code> Interface. <p>
	///  Sample Usage:- <br>
	///  <code>
	///  //From MSEnumVariant example <br>
	///  JIVariant variant = dispatch.get("_NewEnum"); <br>
	/// IJIComObject object2 = variant.getObjectAsComObject();<br>
	/// IJIEnumVariant enumVARIANT = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.queryInterface(IJIEnumVariant.IID));
	/// <br>
	/// for (i = 0; i < 10; i++) <br>
	/// { <br>
	///		Object[] values = enumVARIANT.next(1); <br>
	///		JIArray array = (JIArray)values[0]; <br>
	///		Object[] arrayObj = (Object[])array.getArrayInstance(); <br>
	///		for (int j = 0; j < arrayObj.length; j++) <br>
	///		{ <br>
	///			System.out.println(((JIVariant)arrayObj[j]).getObjectAsInt() + "," + ((Integer)values[1]).intValue()); <br>
	///		} <br>
	/// } <br>
	/// 
	///  </code>
	/// 
	///  </para>
	/// @since 1.0
	/// 
	/// </summary>
	public interface IJIEnumVariant {

		/// <summary>
		/// IID representing the COM <code>IEnumVARIANT</code>.
		/// </summary>

		/// <summary>
		/// Definition from MSDN: <i>
		/// Attempts to get the next celt items in the enumeration sequence. If fewer than the requested number
		/// of elements remain in the sequence, Next returns only the remaining elements.
		/// </i>
		/// </summary>
		/// <param name="celt"> number of elements to be returned. </param>
		/// <returns> results </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] next(int celt) throws org.jinterop.dcom.common.JIException;
		object[] Next(int celt);

		/// <summary>
		/// Definition from MSDN: <i> Attempts to skip over the next celt elements in the enumeration sequence.
		/// </i>
		/// </summary>
		/// <param name="celt"> number of elements to skip. </param>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skip(int celt) throws org.jinterop.dcom.common.JIException;
		void Skip(int celt);

		/// <summary>
		///Definition from MSDN:
		/// <i>Resets the enumeration sequence to the beginning. There is no guarantee that exactly the same set of
		/// variants will be enumerated the second time as was enumerated the first time. Although an exact duplicate
		/// is desirable, the outcome depends on the collection being enumerated. You may find that it is impractical
		/// for some collections to maintain this condition (for example, an enumeration of the files in a directory).
		/// </i> </summary>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws org.jinterop.dcom.common.JIException;
		void Reset();

		/// <summary>
		/// Definition from MSDN: <i>
		/// Creates a copy of the current state of enumeration. Using this function, a particular point in the enumeration
		/// sequence can be recorded, and then returned to at a later time. The returned enumerator is of the same actual
		/// interface as the one that is being cloned. <para>
		/// There is no guarantee that exactly the same set of variants will be enumerated the second time as was
		/// enumerated the first. Although an exact duplicate is desirable, the outcome depends on the collection
		/// being enumerated. You may find that it is impractical for some collections to maintain this condition
		/// (for example, an enumeration of the files in a directory).
		/// </i>
		/// </para>
		/// </summary>
		/// <returns> reference to the clone. </returns>
		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJIEnumVariant Clone() throws org.jinterop.dcom.common.JIException;
		IJIEnumVariant Clone();


	}

	public static class IJIEnumVariant_Fields {
		public const string IID = "00020404-0000-0000-C000-000000000046";
	}

}
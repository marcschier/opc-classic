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
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIString = org.jinterop.dcom.core.JIString;

    /// <summary>
    /// Represents the Windows COM <code>ITypeLib</code> Interface. <para>
    /// Definition from MSDN: <i>
    /// The ITypeLib interface provides methods for accessing a library of type descriptions. This interface supports the following:
    /// Generalized containment for type information. ITypeLib allows iteration over the type descriptions contained
    /// </para>
    /// in the library. <para>
    /// Global functions and data. A type library can contain descriptions of a set of modules, each of which is the
    /// equivalent of a C or C++ source file that exports data and functions. The type library supports compiling references
    /// to the exported data and functions.
    /// </para>
    /// <para>General information, including a user-readable name for the library and help for the library as a whole.
    /// </i>
    /// <br>
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    public interface IJITypeLib : IJIComObject {

        /// <summary>
        /// IID representing the COM <code>ITypeLib</code>.
        /// </summary>

        /// <summary>
        ///Returns the number of type descriptions in the type library.
        /// 
        /// @return </summary>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoCount() throws org.jinterop.dcom.common.JIException;
        int TypeInfoCount { get; }

        /// <summary>
        ///Retrieves the specified type description in the library.
        /// </summary>
        /// <param name="index"> index of the ITypeInfo interface to be returned.
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfo(int index) throws org.jinterop.dcom.common.JIException;
        IJITypeInfo GetTypeInfo(int index);

        /// <summary>
        ///Retrieves the type of a type description.
        /// </summary>
        /// <param name="index"> ihe index of the type description within the type library.
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTypeInfoType(int index) throws org.jinterop.dcom.common.JIException;
        int GetTypeInfoType(int index);

        /// <summary>
        ///Retrieves the type description that corresponds to the specified GUID.
        /// </summary>
        /// <param name="uuid"> GUID of the type description.
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public IJITypeInfo getTypeInfoOfGuid(String uuid) throws org.jinterop.dcom.common.JIException;
        IJITypeInfo GetTypeInfoOfGuid(string uuid);

        /// <summary>
        /// Retrieves the structure that contains the library's attributes.
        /// </summary>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getLibAttr() throws org.jinterop.dcom.common.JIException;
        void GetLibAttr();

        /// <summary>
        ///Retrieves the library's documentation string, the complete Help file name and path, and the context
        /// identifier for the library Help topic in the Help file. <br>
        /// </summary>
        /// <param name="memberId">
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] getDocumentation(int memberId) throws org.jinterop.dcom.common.JIException;
        object[] GetDocumentation(int memberId);

        /// <summary>
        ///Finds occurrences of a type description in a type library. This may be used to quickly verify that a
        /// name exists in a type library. <br>
        /// </summary>
        /// <param name="nameBuf"> </param>
        /// <param name="hashValue"> </param>
        /// <param name="found">
        /// @return </param>
        /// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object[] findName(org.jinterop.dcom.core.JIString nameBuf,int hashValue,short found) throws org.jinterop.dcom.common.JIException;
        object[] FindName(JIString nameBuf, int hashValue, short found);
    }

    public static class IJITypeLib_Fields {
        public const string IID = "00020402-0000-0000-C000-000000000046";
    }

}
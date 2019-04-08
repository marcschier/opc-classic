// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;

    /// <summary>
    /// Represents the Windows COM <code>ITypeLib</code> Interface. 
    /// Definition from MSDN: <i>
    /// The ITypeLib interface provides methods for accessing a library of type descriptions. This interface supports the following:
    /// Generalized containment for type information. ITypeLib allows iteration over the type descriptions contained
    /// in the library.
    /// Global functions and data. A type library can contain descriptions of a set of modules, each of which is the
    /// equivalent of a C or C++ source file that exports data and functions. The type library supports compiling references
    /// to the exported data and functions.
    /// General information, including a user-readable name for the library and help for the library as a whole.
    /// </i>
    /// </summary>
    public interface IJITypeLib : IJIComObject {

        /// <summary>
        /// Returns the number of type descriptions in the type library.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        int TypeInfoCount { get; }

        /// <summary>
        /// Retrieves the specified type description in the library.
        /// </summary>
        /// <param name="index"> index of the ITypeInfo interface to be returned.
        /// </param>
        /// <exception cref="JIException"> </exception>
        IJITypeInfo GetTypeInfo(int index);

        /// <summary>
        /// Retrieves the type of a type description.
        /// </summary>
        /// <param name="index"> ihe index of the type description within the type library.
        /// </param>
        /// <exception cref="JIException"> </exception>
        int GetTypeInfoType(int index);

        /// <summary>
        /// Retrieves the type description that corresponds to the specified GUID.
        /// </summary>
        /// <param name="uuid"> GUID of the type description.</param>
        /// <exception cref="JIException"> </exception>
        IJITypeInfo GetTypeInfoOfGuid(string uuid);

        /// <summary>
        /// Retrieves the structure that contains the library's attributes.
        /// </summary>
        /// <exception cref="JIException"> </exception>
        void GetLibAttr();

        /// <summary>
        /// Retrieves the library's documentation string, the complete Help file name and path, and the context
        /// identifier for the library Help topic in the Help file. 
        /// </summary>
        /// <param name="memberId"></param>
        /// <exception cref="JIException"> </exception>
        object[] GetDocumentation(int memberId);

        /// <summary>
        ///Finds occurrences of a type description in a type library. This may be used to quickly verify that a
        /// name exists in a type library. 
        /// </summary>
        /// <param name="nameBuf"> </param>
        /// <param name="hashValue"> </param>
        /// <param name="found">
        /// </param>
        /// <exception cref="JIException"> </exception>
        object[] FindName(JIString nameBuf, int hashValue, short found);
    }
}
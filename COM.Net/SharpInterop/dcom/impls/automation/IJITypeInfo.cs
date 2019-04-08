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
    /// Represents the Windows COM <code>ITypeInfo</code> Interface. 
    /// ITypeInfo, an interface typically used for reading information
    /// about objects. For example, an object browser
    /// tool can use ITypeInfo to extract information about the
    /// characteristics and capabilities of objects from type
    /// libraries. Type information interfaces are intended to 
    /// describe the parts of the application that can be called
    /// by outside clients, rather than those that might be used 
    /// internally to build an application. 
    /// The ITypeInfo interface provides access to the following: 
    /// <UL>
    ///     <li>The set of function descriptions associated with the type. 
    ///     For interfaces, this contains the set of member
    ///     functions in the interface.</li>
    ///     <li> The set of data member descriptions associated with the 
    ///     type. For structures,
    ///     this contains the set of fields of the type. </li>
    ///     <li>The general attributes of the type, such as whether it 
    ///     describes a structure, an interface, and so on.</li>
    /// </UL>
    /// Please note that not all APIs of <code>ITypeInfo</code> have 
    /// been implemented. 
    /// </summary>
    public interface IJITypeInfo : IJIComObject {

        /// <summary>
        /// Retrieves a TypeAttr structure that contains the attributes 
        /// of the type description. 
        /// </summary>
        /// <exception cref="JIException"> </exception>
        TypeAttr TypeAttr { get; }

        /// <summary>
        /// Retrieves the containing type library and the index of the 
        /// type description within that type library. 
        /// </summary>
        /// <returns> Object[0] = IJITypeLib, Object[1] = Integer </returns>
        /// <exception cref="JIException"> </exception>
        object[] ContainingTypeLib { get; }

        /// <summary>
        /// Retrieves the FuncDesc structure that contains information 
        /// about a specified function.
        /// </summary>
        /// <param name="index"> index of the function whose description
        /// is to be returned. The index should be in the range
        /// of 0 to 1 less than the number of functions in this type.
        /// </param>
        /// <exception cref="JIException"> </exception>
        FuncDesc GetFuncDesc(int index);

        /// <summary>
        /// Retrieves the documentation string, the complete Help file 
        /// name and path, and the context ID for the Help
        /// topic for a specified type description. 
        /// </summary>
        /// <param name="memberId"> ID of the member whose documentation
        /// is to be returned. </param>
        /// <returns> Object[0] = JIString of BSTR type, Object[1]  = 
        /// JIString of BSTR type, Object[3] = JIString of BSTR type </returns>
        /// <exception cref="JIException"> </exception>
        object[] GetDocumentation(int memberId);

        /// <summary>
        /// Retrieves a description or specification of an entry point 
        /// for a function in a DLL. 
        /// </summary>
        /// <param name="memberId"> ID of the member function whose 
        /// DLL entry description is to be returned. </param>
        /// <param name="invKind"> Specifies the kind of member identified 
        /// by <code>memberId</code>. This is important for properties,
        /// because one memid can identify up to three separate functions. </param>
        /// <returns> Object[0] = JIString of BSTR type, 
        /// Object[1]  = JIString of BSTR type, Object[2] = Short </returns>
        /// <exception cref="JIException"> </exception>
        object[] GetDllEntry(int memberId, int invKind);

        /// <summary>
        /// Retrieves a VARDESC structure that describes the specified 
        /// variable. 
        /// </summary>
        /// <param name="index"> index of the variable whose description 
        /// is to be returned. The index should be in the range
        /// of 0 to 1 less than the number of variables in this type.
        /// </param>
        /// <exception cref="JIException"> </exception>
        VarDesc GetVarDesc(int index);

        /// <summary>
        /// Retrieves the variable with the specified member ID (or the name
        /// of the property or method and its parameters)
        /// that correspond to the specified function ID. 
        /// </summary>
        /// <param name="memberId"> ID of the member whose name (or names) 
        /// is to be returned. </param>
        /// <param name="maxNames"> Length of the passed-in array. </param>
        /// <returns> Object[0] = JIString[] of BSTR type, Object[1] = Integer
        /// </returns>
        /// <exception cref="JIException"> </exception>
        object[] GetNames(int memberId, int maxNames);

        /// <summary>
        /// If a type description describes a COM class, it retrieves the 
        /// type description of the implemented
        /// interface types. For an interface, getRefTypeOfImplType returns
        /// the type information for inherited interfaces, if any exist. 
        /// </summary>
        /// <param name="index"> index of the implemented type whose handle 
        /// is returned. The valid range is 0 to the
        /// cImplTypes field in the TypeAttr structure.
        /// </param>
        /// <exception cref="JIException"> </exception>
        int GetRefTypeOfImplType(int index);

        /// <summary>
        /// Retrieves the IMPLTYPEFLAGS enumeration for one implemented 
        /// interface or base interface in a type description. 
        /// </summary>
        /// <param name="index"> index of the implemented interface or base 
        /// interface for which to get the flags.
        /// </param>
        /// <exception cref="JIException"> </exception>
        int GetImplTypeFlags(int index);

        /// <summary>
        /// If a type description references other type descriptions, it
        /// retrieves the referenced type descriptions. 
        /// </summary>
        /// <param name="hrefType"> handle to the referenced type description 
        /// to be returned.
        /// </param>
        /// <exception cref="JIException"> </exception>
        IJITypeInfo GetRefTypeInfo(int hrefType);

        /// <summary>
        /// Creates a new instance of a type that describes a component 
        /// object class (coclass). 
        /// </summary>
        /// <param name="riid">
        /// </param>
        /// <exception cref="JIException"> </exception>
        IJIComObject CreateInstance(string riid);

        /// <summary>
        /// Retrieves marshaling information.
        /// </summary>
        /// <param name="memberId"> member ID that indicates which 
        /// marshaling information is needed.
        /// </param>
        /// <exception cref="JIException"> </exception>
        JIString GetMops(int memberId);
    }
}
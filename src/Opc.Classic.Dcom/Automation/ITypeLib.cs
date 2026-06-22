// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Represents the Windows COM <code>ITypeLib</code> Interface.
/// The ITypeLib interface provides methods for accessing a library
/// of type descriptions. This interface supports the following:
/// Generalized containment for type information. ITypeLib allows
/// iteration over the type descriptions contained in the library.
/// Global functions and data. A type library can contain
/// descriptions of a set of modules, each of which is the
/// equivalent of a C or C++ source file that exports data and
/// functions. The type library supports compiling references
/// to the exported data and functions.
/// </summary>
public interface ITypeLib : IComObject
{
    /// <summary>
    /// Returns the number of type descriptions in the type library.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    int TypeInfoCount { get; }

    /// <summary>
    /// Retrieves the specified type description in the library.
    /// </summary>
    /// <param name="index"> index of the ITypeInfo interface to
    /// be returned. </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    ITypeInfo GetTypeInfo(int index);

    /// <summary>
    /// Retrieves the type of a type description.
    /// </summary>
    /// <param name="index"> ihe index of the type description
    /// within the type library. </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    int GetTypeInfoType(int index);

    /// <summary>
    /// Retrieves the type description that corresponds to the
    /// specified GUID.
    /// </summary>
    /// <param name="uuid"> GUID of the type description.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    ITypeInfo GetTypeInfoOfGuid(string uuid);

    /// <summary>
    /// Retrieves the structure that contains the library's attributes.
    /// </summary>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    void GetLibAttr();

    /// <summary>
    /// Retrieves the library's documentation string, the complete
    /// Help file name and path, and the context identifier for the
    /// library Help topic in the Help file.
    /// </summary>
    /// <param name="memberId">Automation member identifier whose name should be resolved.</param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    object[] GetDocumentation(int memberId);

    /// <summary>
    /// Finds occurrences of a type description in a type library.
    /// This may be used to quickly verify that a name exists in a type
    /// library.
    /// </summary>
    /// <param name="nameBuf">Name used to identify the target server, member, or descriptor.</param>
    /// <param name="hashValue">Locale-specific Automation name hash used for lookup.</param>
    /// <param name="found">
    /// </param>
    /// <exception cref="InteropException">Thrown when the remote COM or DCOM operation reports a protocol or HRESULT failure.</exception>
    object[] FindName(ComString nameBuf, int hashValue, short found);
}

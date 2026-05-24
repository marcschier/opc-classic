// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Automation; 
/// <summary>
/// Represents the Windows COM <code>IEnumVARIANT</code> Interface.
/// Sample Usage:
/// <code>
/// // From MSEnumVariant example
/// <see cref="Variant"/> variant = dispatch.Get("_NewEnum");
/// <see cref="IComObject"/> object2 = variant.GetObjectAsComObject();
/// <see cref="IEnumVariant"/> enumVARIANT = (<see cref="IEnumVariant"/>)
///    <see cref="ObjectFactory"/>.NarrowObject(object2.QueryInterface(<see cref="IEnumVariant"/>.IID));
/// for (i = 0; i &lt; 10; i++)
/// {
///        object[] values = enumVARIANT.next(1);
///        ComArray array = (ComArray)values[0];
///        object[] arrayObj = (object[])array.GetArrayInstance();
///        for (int j = 0; j &lt; arrayObj.length; j++)
///        {
///            Console.WriteLine(((<see cref="Variant"/>)arrayObj[j]).GetObjectAsInt() + "," +
///                ((Integer)values[1]).intValue());
///        }
/// }
/// </code>
/// </summary>
public interface IEnumVariant {

    /// <summary>
    /// Attempts to get the next celt items in the enumeration
    /// sequence. If fewer than the requested number
    /// of elements remain in the sequence, Next returns only
    /// the remaining elements.
    /// </summary>
    /// <param name="celt"> number of elements to be returned. </param>
    /// <returns> results </returns>
    /// <exception cref="InteropException"> </exception>
    object[] Next(int celt);

    /// <summary>
    /// Attempts to skip over the next celt elements in the
    /// enumeration sequence.
    /// </summary>
    /// <param name="celt"> number of elements to skip. </param>
    /// <exception cref="InteropException"> </exception>
    void Skip(int celt);

    /// <summary>
    /// Resets the enumeration sequence to the beginning.
    /// There is no guarantee that exactly the same set of
    /// variants will be enumerated the second time as was
    /// enumerated the first time. Although an exact duplicate
    /// is desirable, the outcome depends on the collection being
    /// enumerated. You may find that it is impractical
    /// for some collections to maintain this condition (for
    /// example, an enumeration of the files in a directory).
    /// </summary>
    /// <exception cref="InteropException"> </exception>
    void Reset();

    /// <summary>
    /// Creates a copy of the current state of enumeration.
    /// Using this function, a particular point in the enumeration
    /// sequence can be recorded, and then returned to at a later
    /// time. The returned enumerator is of the same actual
    /// interface as the one that is being cloned.
    /// There is no guarantee that exactly the same set of variants
    /// will be enumerated the second time as was
    /// enumerated the first. Although an exact duplicate is
    /// desirable, the outcome depends on the collection
    /// being enumerated. You may find that it is impractical
    /// for some collections to maintain this condition
    /// (for example, an enumeration of the files in a directory).
    /// </summary>
    /// <returns> reference to the clone. </returns>
    /// <exception cref="InteropException"> </exception>
    IEnumVariant Clone();
}

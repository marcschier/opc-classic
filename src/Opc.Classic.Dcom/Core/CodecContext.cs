// SPDX-License-Identifier: MIT
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Codec context
/// </summary>
public class CodecContext {

    /// <summary>
    /// List of deferred pointers
    /// </summary>
    public IList<ComPointer> DefferedPointers { get; set; } = new List<ComPointer>();

    /// <summary>
    /// Special flags
    /// </summary>
    public int Flag { get; set; } = InteropFlags.FLAG_NULL;

    /// <summary>
    /// Current session
    /// </summary>
    public Session CurrentSession { get; internal set; }

    /// <summary>
    /// Com objects
    /// </summary>
    public IList<IComObject> ComObjects { get; set; } = new List<IComObject>();

    /// <summary>
    /// Decodes the deferred pointers
    /// </summary>
    /// <param name="ndr"></param>
    public void DecodeDeferredPointers(NdrCodec ndr) {
        var x = 0;
        var listOfDefferedPointers = DefferedPointers;
        var flags = Flag;
        while (x < listOfDefferedPointers.Count) {
            DefferedPointers = new List<ComPointer>();
            Flag = flags;

            var replacement = (ComPointer)MarshalUnMarshalHelper.Deserialize(
                ndr, listOfDefferedPointers[x], this);
            // this should replace the value in the original place.
            listOfDefferedPointers[x].ReplaceSelfWithNewPointer(replacement);
            x++;
            InsertRange(listOfDefferedPointers, x, DefferedPointers);
        }
    }

    /// <summary>
    /// Encodes the deferred pointers
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="flatten"></param>
    public void EncodeDeferredPointers(NdrCodec ndr, bool flatten = true) { // TODO: Understand flatten
        // The deferred pointers need to be completely serialized here. 
        // If they are also having nested deffered pointers then those pointers
        // should be "inserted" just after the current pointer itself.
        // change the logic below to send out a new list and insert that 
        // list after the current x.
        // consider the case when there is a Struct having a nested pointer to
        // another struct and this struct itself having a pointer.
        //
        // Inparams order: for 2 params.
        // int f,
        // Struct {
        //      int i;
        //      Struct *ptr;
        //      Struct *ptr2;
        //      int j;
        // }
        //
        // while serializing this struct the pointer 1 will get deffered 
        // and so will pointer 2. Now while writing the deffered pointers, 
        // we will find that the pointer 1 is pointing to a struct which has
        // another deffered pointer (pointer to another struct maybe)
        // in such case, the current logic will add the deffered pointer to 
        // the end of the listOfDefferedPointers list, effectively serializing it
        // after the pointer 2 referent. But that is what is against the rules
        // of DCERPC, in this case the referent of pointer 1 (struct with the
        // pointer to another struct)
        // should be serialized in place (following th rules of the struct 
        // serialization ofcourse) and should not go to the end of the list.
        var x = 0;
        var listOfDefferedPointers = DefferedPointers;
        while (x < listOfDefferedPointers.Count) {
            DefferedPointers = new List<ComPointer>();
            if (flatten) {
                var referent = listOfDefferedPointers[x].Referent;
                if (referent is Struct) {
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(Struct), referent, this);
                }
                else if (referent is ComString) {
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(ComString), referent, this);
                }
                else {
                    MarshalUnMarshalHelper.Serialize(ndr, typeof(ComArray), referent, this);
                }
            }
            else {
                MarshalUnMarshalHelper.Serialize(ndr, typeof(ComPointer),
                    listOfDefferedPointers[x], this);
            }
            x++; // incrementing index
            InsertRange(listOfDefferedPointers, x, DefferedPointers);
        }
    }

    private static void InsertRange<T>(IList<T> target, int index, IEnumerable<T> values) {
        foreach (var value in values) {
            target.Insert(index, value);
            index++;
        }
    }
}

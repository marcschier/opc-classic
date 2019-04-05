// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;
    using org.jinterop.dcom.common;
    using System.Collections;
    using System;

    /// <summary>
    /// Class representing a COM string. The Wide Char (<code>LPWSTR</code>) and the <code>BSTR</code> are
    /// both encoded by the server in "UTF-16LE". This encoding will be preserved by the library for all to
    /// and fro operations.
    /// </summary>
    [Serializable]
    public sealed class JIString {

        /// <summary>
        /// Represents <code>JIVariant</code> for this object,
        /// it is valid only if this object is a <code>BSTR</code>
        /// (<code>JIFlags.FLAG_REPRESENTATION_STRING_BSTR</code>) type.
        /// </summary>
        public readonly JIVariant Variant;

        /// <summary>
        /// Represents <code>JIVariant(byRef = true)</code> for this object,
        /// it is valid only if this object is a <code>BSTR</code>
        /// (<code>JIFlags.FLAG_REPRESENTATION_STRING_BSTR</code>) type.
        /// </summary>
        public readonly JIVariant VariantByRef;

        /// <summary>
        ///Creates an object of the specified type. Used while deserialiazing this object.
        /// </summary>
        /// <param name="type"> JIFlags string flags </param>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
        /// <exception cref="ArgumentException">
        /// if <code>type</code> is not a string flag.</exception>
        public JIString(int type) {
            Type = type;
            if (type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR || 
                type == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {
                _member = new JIPointer(typeof(string), true);
            }
            else if (type == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                _member = new JIPointer(typeof(string), false);
            }
            else {
                throw new ArgumentException(
                    JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_FLAG_ERROR));
            }
            Variant = null;
            VariantByRef = null;
            _member.Flags = type | JIFlags.FLAG_REPRESENTATION_VALID_STRING;
        }


        /// <summary>
        /// Creates a string object of a given <code>type</code>.
        /// </summary>
        /// <param name="str"> value encapsulated by this object. </param>
        /// <param name="type"> JIFlags string flags </param>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
        /// <exception cref="ArgumentException"> 
        /// if <code>type</code> is not a string flag. </exception>
        public JIString(string str, int type) {
            str = str ?? "";
            Type = type;
            if (type == JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR || 
                type == JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR) {
                _member = new JIPointer(str, true);
                Variant = null;
                VariantByRef = null;
            }
            else if (type == JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
                _member = new JIPointer(str, false);
                _member.setReferent(0x72657355); //"User" in LEndian.
                Variant = new JIVariant(this);
                VariantByRef = new JIVariant(this, true);
            }
            else {
                throw new ArgumentException(JISystem.getLocalizedMessage(JIErrorCodes.JI_UTIL_FLAG_ERROR));
            }
            _member.Flags = type | JIFlags.FLAG_REPRESENTATION_VALID_STRING;

        }

        /// <summary>
        /// Creates a object of the <code>BSTR</code> type.
        /// </summary>
        /// <param name="str"> value encapsulated by this object. </param>
        public JIString(string str) :
            this(str, JIFlags.FLAG_REPRESENTATION_STRING_BSTR) {
        }

        /// <summary>
        /// String encapsulated by this object. The encoding scheme 
        /// for <code>LPWSTR</code> and <code>BSTR</code> strings is "UTF-16LE".
        /// </summary>
        public string String => _member.getReferent()?.ToString();

        /// <summary>
        /// Type representing this object.
        /// </summary>
        /// <returns> JIFlags string flags </returns>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_BSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPCTSTR"> </seealso>
        /// <seealso cref="JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR"> </seealso>
        public int Type { get; } = JIFlags.FLAG_NULL;

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        internal void encode(NdrCodec ndr, IList defferedPointers, int FLAG) {
            JIMarshalUnMarshalHelper.serialize(ndr, _member.GetType(), _member, defferedPointers, Type | FLAG);
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="ndr"></param>
        /// <param name="defferedPointers"></param>
        /// <param name="FLAG"></param>
        /// <param name="additionalData"></param>
        /// <returns></returns>
        internal JIString decode(NdrCodec ndr, IList defferedPointers, int FLAG, IDictionary additionalData) {
            var newString = new JIString(Type) {
                _member = (JIPointer)JIMarshalUnMarshalHelper.deSerialize(ndr, _member, defferedPointers, Type | FLAG, additionalData)
            };
            return newString;
        }

        internal bool Deffered {
#pragma warning disable RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
            set {
#pragma warning restore RECS0029 // Warns about property or indexer setters and event adders or removers that do not use the value parameter
                //this condition is required so that only BSTRs are value and also since this member could be value and
                //setting it to true would spoil the logic
                // this is incorrect logic in the bug sent by Kevin , the ONEVENTSTRUCT consists of LPWSTRs which are value
                if (_member != null && !_member.Reference) {
                    ((JIPointer)_member).Deffered = true;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() {
            return _member == null ? "[null]" : "[Type: " + Type + " , " + _member + "]";
        }

        private JIPointer _member;
    }
}
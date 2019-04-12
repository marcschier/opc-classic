//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {

    /// <summary>
    /// Class representing various flags used in the system.
    /// </summary>
    public sealed class InteropFlags {

        /// <summary>
        /// FLAG representing nothing. Use this if no other flag is to be set.
        /// </summary>
        public const int FLAG_NULL = 0;

        /// <summary>
        /// FLAG representing a <code>BSTR</code> string.
        /// </summary>
        public const int FLAG_REPRESENTATION_STRING_BSTR = 1;

        /// <summary>
        /// FLAG representing a normal String.
        /// </summary>
        public const int FLAG_REPRESENTATION_STRING_LPCTSTR = 2;

        /// <summary>
        /// FLAG representing a Wide Char (16 bit characters)
        /// </summary>
        public const int FLAG_REPRESENTATION_STRING_LPWSTR = 4;

        /// <summary>
        /// flag representing an array
        /// </summary>
        public const int FLAG_REPRESENTATION_ARRAY = 8;

        /// <summary>
        /// flag representing that this is a pointer
        /// </summary>
        internal const int FLAG_REPRESENTATION_POINTER = 16;

        /// <summary>
        /// flag representing that this is a reference
        /// </summary>
        internal const int FLAG_REPRESENTATION_REFERENCE = 32;

        /// <summary>
        /// flag representing that this is a IDispatch invoke call
        /// </summary>
        public const int FLAG_REPRESENTATION_IDISPATCH_INVOKE = 64;

        /// <summary>
        /// flag representing that this is a IDispatch invoke call
        /// </summary>
        internal const int FLAG_REPRESENTATION_NESTED_POINTER = 128;

        /// <summary>
        /// Flag representing unsigned byte.
        /// </summary>
        public const int FLAG_REPRESENTATION_UNSIGNED_BYTE = 256;

        /// <summary>
        /// Flag representing unsigned short.
        /// </summary>
        public const int FLAG_REPRESENTATION_UNSIGNED_SHORT = 512;

        /// <summary>
        /// Flag representing unsigned integer.
        /// </summary>
        public const int FLAG_REPRESENTATION_UNSIGNED_INT = 1024;

        /// <summary>
        /// Flag representing integer of the type VT_INT.
        /// </summary>
        public const int FLAG_REPRESENTATION_VT_INT = 2048;

        /// <summary>
        /// Flag representing (unsigned) integer of the type VT_UINT.
        /// </summary>
        public const int FLAG_REPRESENTATION_VT_UINT = 4096;

        /// <summary>
        /// Flag representing <code>VARIANT_BOOL</code>, a <code>bool</code> is
        /// 2 bytes for a <code>VARIANT</code> and 1 byte for normal calls.
        /// Use this when setting array of <code>bool</code>s
        /// within <code>VARIANT</code>s.
        /// </summary>
        public const int FLAG_REPRESENTATION_VARIANT_BOOL = 8192;

        /// <summary>
        /// Represents an internal flag, which will disallow direct Strings
        /// from being marshalled or unmarshalled. Come via <see cref="ComString"/> only.
        /// </summary>
        internal const int FLAG_REPRESENTATION_VALID_STRING = 16384;

        /// <summary>
        /// Used from within <see cref="InterfacePointer"/> to use decode2 API.
        /// </summary>
        internal const int FLAG_REPRESENTATION_INTERFACEPTR_DECODE2 = 32768;

        /// <summary>
        /// Used in <see cref="Variant"/> when sending a IUnknown Pointer.
        /// This is also how COM runtime does it.
        /// A little strange to expect this behaviour since essentially all
        /// objects derieve from IUnknown so why replace the IID ?
        /// </summary>
        internal const int FLAG_REPRESENTATION_USE_IUNKNOWN_IID = 65536;

        /// <summary>
        /// Used in <see cref="Variant"/> when sending a IDispatch Pointer.
        /// This is also how COM runtime does it.
        /// </summary>
        internal const int FLAG_REPRESENTATION_USE_IDISPATCH_IID = 131072;

        /// <summary>
        /// Used in <see cref="Variant"/> to identify an ([out] IUnknown*) variable.
        /// </summary>
        internal const int FLAG_REPRESENTATION_IUNKNOWN_NULL_FOR_OUT = 262144;

        /// <summary>
        /// Used in <see cref="Variant"/> to identify an ([out] IDispatch*) variable.
        /// </summary>
        internal const int FLAG_REPRESENTATION_IDISPATCH_NULL_FOR_OUT = 524288;

        /// <summary>
        /// Used in <see cref="Variant"/> to send <see cref="InterfacePointer"/> as null.
        /// </summary>
        internal const int FLAG_REPRESENTATION_SET_INTERFACEPTR_NULL_FOR_VARIANT = 1048576;
    }
}
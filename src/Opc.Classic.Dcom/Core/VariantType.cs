//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using System;

namespace SharpInterop.Core; 
/// <summary>
/// Variant type
/// </summary>
[Flags]
public enum VariantType {
    /// <summary> id </summary>
    VT_EMPTY = 0,
    /// <summary> id </summary>
    VT_NULL = 1,
    /// <summary> id </summary>
    VT_I2 = 2,
    /// <summary> id </summary>
    VT_I4 = 3,
    /// <summary> id </summary>
    VT_R4 = 4,
    /// <summary> id </summary>
    VT_R8 = 5,
    /// <summary> id </summary>
    VT_CY = 6,
    /// <summary> id </summary>
    VT_DATE = 7,
    /// <summary> id </summary>
    VT_BSTR = 8,
    /// <summary> id </summary>
    VT_DISPATCH = 9,
    /// <summary> id </summary>
    VT_ERROR = 10,
    /// <summary> id </summary>
    VT_BOOL = 11,
    /// <summary> id </summary>
    VT_VARIANT = 12,
    /// <summary> id </summary>
    VT_UNKNOWN = 13,
    /// <summary> id </summary>
    VT_DECIMAL = 14,
    /// <summary> id </summary>
    VT_I1 = 16,
    /// <summary> id </summary>
    VT_UI1 = 17,
    /// <summary> id </summary>
    VT_UI2 = 18,
    /// <summary> id </summary>
    VT_UI4 = 19,
    /// <summary> id </summary>
    VT_I8 = 20,
    /// <summary> id </summary>
    VT_UI8 = 21,
    /// <summary> id </summary>
    VT_INT = 22,
    /// <summary> id </summary>
    VT_UINT = 23,
    /// <summary> id </summary>
    VT_VOID = 24,
    /// <summary> id </summary>
    VT_HRESULT = 25,
    /// <summary> id </summary>
    VT_PTR = 26,
    /// <summary> id </summary>
    VT_SAFEARRAY = 27,
    /// <summary> id </summary>
    VT_CARRAY = 28,
    /// <summary> id </summary>
    VT_USERDEFINED = 29,
    /// <summary> id </summary>
    VT_LPSTR = 30,
    /// <summary> id </summary>
    VT_LPWSTR = 31,
    /// <summary> id </summary>
    VT_FILETIME = 64,
    /// <summary> id </summary>
    VT_BLOB = 65,
    /// <summary> id </summary>
    VT_STREAM = 66,
    /// <summary> id </summary>
    VT_STORAGE = 67,
    /// <summary> id </summary>
    VT_STREAMED_OBJECT = 68,
    /// <summary> id </summary>
    VT_STORED_OBJECT = 69,
    /// <summary> id </summary>
    VT_BLOB_OBJECT = 70,
    /// <summary> id </summary>
    VT_CF = 71,
    /// <summary> id </summary>
    VT_CLSID = 72,

    /// <summary> id </summary>
    VT_VECTOR = 0x1000,
    /// <summary> id </summary>
    VT_ARRAY = 0x2000,
    /// <summary> id </summary>
    VT_BYREF = 0x4000,

    /// <summary> id </summary>
    VT_BYREF_VT_UI1 = VT_BYREF | VT_UI1, // 0x00004011,
    /// <summary> id </summary>
    VT_BYREF_VT_I2 = VT_BYREF | VT_I2, // 0x00004002,
    /// <summary> id </summary>
    VT_BYREF_VT_I4 = VT_BYREF | VT_I4, // 0x00004003,
    /// <summary> id </summary>
    VT_BYREF_VT_R4 = VT_BYREF | VT_R4, // 0x00004004,
    /// <summary> id </summary>
    VT_BYREF_VT_R8 = VT_BYREF | VT_R8, // 0x00004005,
    /// <summary> id </summary>
    VT_BYREF_VT_BOOL = VT_BYREF | VT_BOOL, // 0x0000400b,
    /// <summary> id </summary>
    VT_BYREF_VT_ERROR = VT_BYREF | VT_ERROR, // 0x0000400a,
    /// <summary> id </summary>
    VT_BYREF_VT_CY = VT_BYREF | VT_CY, // 0x00004006,
    /// <summary> id </summary>
    VT_BYREF_VT_DATE = VT_BYREF | VT_DATE, // 0x00004007,
    /// <summary> id </summary>
    VT_BYREF_VT_BSTR = VT_BYREF | VT_BSTR, // 0x00004008,
    /// <summary> id </summary>
    VT_BYREF_VT_UNKNOWN = VT_BYREF | VT_UNKNOWN, // 0x0000400d,
    /// <summary> id </summary>
    VT_BYREF_VT_DISPATCH = VT_BYREF | VT_DISPATCH, // 0x00004009,
    /// <summary> id </summary>
    VT_BYREF_VT_ARRAY = VT_BYREF | VT_ARRAY, // 0x00006000,
    /// <summary> id </summary>
    VT_BYREF_VT_VARIANT = VT_BYREF | VT_VARIANT, // 0x0000400c,
    /// <summary> id </summary>
    VT_BYREF_VT_DECIMAL = VT_BYREF | VT_DECIMAL, // 0x0000400e,
    /// <summary> id </summary>
    VT_BYREF_VT_I1 = VT_BYREF | VT_I1, // 0x00004010,
    /// <summary> id </summary>
    VT_BYREF_VT_UI2 = VT_BYREF | VT_UI2, // 0x00004012,
    /// <summary> id </summary>
    VT_BYREF_VT_UI4 = VT_BYREF | VT_UI4, // 0x00004013,
    /// <summary> id </summary>
    VT_BYREF_VT_I8 = VT_BYREF | VT_I8, // 0x00004014,
    /// <summary> id </summary>
    VT_BYREF_VT_UI8 = VT_BYREF | VT_UI8, // 0x00004015,
    /// <summary> id </summary>
    VT_BYREF_VT_INT = VT_BYREF | VT_INT, // 0x00004016,
    /// <summary> id </summary>
    VT_BYREF_VT_UINT = VT_BYREF | VT_UINT, // 0x00004017,

    /// <summary> id </summary>
    VT_RESERVED = 0x8000,

    /// <summary> id </summary>
    VT_ILLEGAL = 0xffff,
    /// <summary> id </summary>
    VT_ILLEGALMASKED = 0xfff,
    /// <summary> id </summary>
    VT_TYPEMASK = 0xfff
}

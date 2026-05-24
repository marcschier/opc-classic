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
/// Extent array
/// </summary>
[Serializable]
internal sealed class OrpcExtentArray {

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="size"></param>
    /// <param name="data"></param>
    internal OrpcExtentArray(string guid, int size, byte[] data) {
        GUID = guid;
        SizeOfData = size;
        _data = data;
    }

    /// <summary>
    /// Guid
    /// </summary>
    public string GUID { get; }

    /// <summary>
    /// Size
    /// </summary>
    public int SizeOfData { get; }

    /// <summary>
    /// Data
    /// </summary>
    public byte[] Data {
        get {
            var newData = new byte[_data.Length];
            for (var i = 0; i < _data.Length; i++) {
                newData[i] = _data[i];
            }
            return newData;
        }
    }

    private readonly byte[] _data;
}

// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Extent array
/// </summary>
[Serializable]
internal sealed class OrpcExtentArray
{
    /// <summary>
    /// Create
    /// </summary>
    /// <param name="guid">GUID value encoded in the RPC or COM descriptor.</param>
    /// <param name="size">Number of bytes or elements represented by the field.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    internal OrpcExtentArray(string guid, int size, byte[] data)
    {
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
    public byte[] Data
    {
        get
        {
            var newData = new byte[_data.Length];
            for (var i = 0; i < _data.Length; i++)
            {
                newData[i] = _data[i];
            }
            return newData;
        }
    }

    private readonly byte[] _data;
}

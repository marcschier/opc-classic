// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Rpc; 
/// <summary>
/// Generic buffer
/// </summary>
public class Buffer {

    /// <summary>
    /// Length
    /// </summary>
    public int Length {
        get => _length;
        set {
            _length = value;
            if (value > _buffer.Length) {
                Grow(value);
            }
        }
    }


    /// <summary>
    /// Current index
    /// </summary>
    public int Index {
        get => _index;
        set {
            _index = value;
            if (value > _length) {
                _length = value;
            }
            if (_length > _buffer.Length) {
                Grow(_length);
            }
        }
    }

    /// <summary>
    /// Current buffer capacity
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Increment
    /// </summary>
    public int CapacityIncrement { get; set; }

    /// <summary>
    /// Get buffer
    /// Set new buffer
    /// </summary>
    public byte[] Buf {
        get => _buffer;
        set {
            _buffer = value ?? Array.Empty<byte>();
            _index = 0;
            _length = 0;
        }
    }

    /// <summary>
    /// Create buffer
    /// </summary>
    public Buffer() :
        this(null, 0) {
    }

    /// <summary>
    /// Create buffer
    /// </summary>
    /// <param name="capacityIncrement"></param>
    public Buffer(int capacityIncrement) :
        this(null, capacityIncrement) {
    }

    /// <summary>
    /// Create buffer
    /// </summary>
    /// <param name="buffer"></param>
    public Buffer(byte[] buffer) :
        this(buffer, 0) {
    }

    /// <summary>
    /// Create buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="capacityIncrement"></param>
    public Buffer(byte[] buffer, int capacityIncrement) {
        Buf = buffer;
        CapacityIncrement = capacityIncrement;
    }

    /// <summary>
    /// Copy
    /// </summary>
    /// <returns></returns>
    public byte[] Copy() {
        var copy_Renamed = new byte[_length];
        Array.Copy(_buffer, 0, copy_Renamed, 0, _length);
        return copy_Renamed;
    }

    /// <summary>
    /// Reset
    /// </summary>
    public void Reset() {
        _length = 0;
        _index = 0;
    }

    /// <summary>
    /// Get new index
    /// </summary>
    /// <param name="advance"></param>
    /// <returns></returns>
    public int GetIndex(int advance) {
        try {
            return _index;
        }
        finally {
            _index += advance;
            if (_index > _length) {
                _length = _index;
            }
            if (_length > _buffer.Length) {
                Grow(_length);
            }
        }
    }

    /// <summary>
    /// Align
    /// </summary>
    /// <param name="boundary"></param>
    /// <returns></returns>
    public int Align(int boundary) {
        var align_Renamed = _index % boundary;
        if (align_Renamed == 0) {
            return 0;
        }
        Advance(align_Renamed = boundary - align_Renamed);
        return align_Renamed;
    }

    /// <summary>
    /// Align
    /// </summary>
    /// <param name="boundary"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int Align(int boundary, byte value) {
        var align_Renamed = _index % boundary;
        if (align_Renamed == 0) {
            return 0;
        }
        Advance(align_Renamed = boundary - align_Renamed, value);
        return align_Renamed;
    }

    /// <summary>
    /// Advance
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public int Advance(int step) {
        _index += step;
        if (_index > _length) {
            _length = _index;
        }
        if (_length > _buffer.Length) {
            Grow(_length);
        }
        return _index;
    }

    /// <summary>
    /// Advance
    /// </summary>
    /// <param name="step"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int Advance(int step, byte value) {
        for (var finish = _index + step; _index < finish; _index++) {
            _buffer[_index] = value;
        }
        if (_index > _length) {
            _length = _index;
        }
        if (_length > _buffer.Length) {
            Grow(_length);
        }
        return _index;
    }

    /// <summary>
    /// Grow buffer
    /// </summary>
    /// <param name="length"></param>
    private void Grow(int length) {
        if (CapacityIncrement <= 0) {
            throw new InvalidOperationException("Buffer cannot grow when CapacityIncrement is not positive.");
        }
        var newLength = _buffer.Length;
        while (newLength < length) {
            newLength += CapacityIncrement;
        }
        var newBuffer = new byte[newLength];
        Array.Copy(_buffer, 0, newBuffer, 0, _buffer.Length);
        _buffer = newBuffer;
    }

    private byte[] _buffer;
    private int _index;
    private int _length;
}

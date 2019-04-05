//-------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2014 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to simulate the java.nio.ByteBuffer class in C#.
//
//	Instances are only obtainable via the static 'allocate' method.
//
//	Some methods are not available:
//		All methods which create shared views of the buffer such as: array,
//		asCharBuffer, asDoubleBuffer, asFloatBuffer, asIntBuffer, asLongBuffer,
//		asReadOnlyBuffer, asShortBuffer, duplicate, slice, & wrap.
//
//		Other methods such as: mark, reset, isReadOnly, order, compareTo,
//		arrayOffset, & the limit setter method.
//-------------------------------------------------------------------------------------------
public class ByteBuffer
{
	//'Mode' is only used to determine whether to return data length or capacity from the 'limit' method:
	private enum Mode
	{
		Read,
		Write
	}
	private Mode mode;

	private System.IO.MemoryStream stream;
	private System.IO.BinaryReader reader;
	private System.IO.BinaryWriter writer;

	private ByteBuffer()
	{
		stream = new System.IO.MemoryStream();
		reader = new System.IO.BinaryReader(stream);
		writer = new System.IO.BinaryWriter(stream);
	}

	~ByteBuffer()
	{
		reader.Close();
		writer.Close();
		stream.Close();
		stream.Dispose();
	}

	public static ByteBuffer allocate(int capacity)
	{
		var buffer = new ByteBuffer();
		buffer.stream.Capacity = capacity;
		buffer.mode = Mode.Write;
		return buffer;
	}

	public static ByteBuffer allocateDirect(int capacity)
	{
		//this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
		return allocate(capacity);
	}

	public int capacity()
	{
		return stream.Capacity;
	}

	public ByteBuffer flip()
	{
		mode = Mode.Read;
		stream.SetLength(stream.Position);
		stream.Position = 0;
		return this;
	}

	public ByteBuffer clear()
	{
		mode = Mode.Write;
		stream.Position = 0;
		return this;
	}

	public ByteBuffer compact()
	{
		mode = Mode.Write;
		var newStream = new System.IO.MemoryStream(stream.Capacity);
		stream.CopyTo(newStream);
		stream = newStream;
		return this;
	}

	public ByteBuffer rewind()
	{
		stream.Position = 0;
		return this;
	}

	public long limit()
	{
        if (mode == Mode.Write)
            return stream.Capacity;
        return stream.Length;
    }

	public long position()
	{
		return stream.Position;
	}

	public ByteBuffer position(long newPosition)
	{
		stream.Position = newPosition;
		return this;
	}

	public long remaining()
	{
		return limit() - position();
	}

	public bool hasRemaining()
	{
		return remaining() > 0;
	}

	public int get()
	{
		return stream.ReadByte();
	}

	public ByteBuffer get(byte[] dst, int offset, int length)
	{
		stream.Read(dst, offset, length);
		return this;
	}

	public ByteBuffer put(byte b)
	{
		stream.WriteByte(b);
		return this;
	}

	public ByteBuffer put(byte[] src, int offset, int length)
	{
		stream.Write(src, offset, length);
		return this;
	}

	public bool Equals(ByteBuffer other)
	{
        if (other != null && remaining() == other.remaining()) {
            var thisOriginalPosition = position();
            var otherOriginalPosition = other.position();

            var differenceFound = false;
            while (stream.Position < stream.Length) {
                if (get() != other.get()) {
                    differenceFound = true;
                    break;
                }
            }

            position(thisOriginalPosition);
            other.position(otherOriginalPosition);

            return !differenceFound;
        }
        return false;
    }

	//methods using the internal BinaryReader:
	public char getChar()
	{
		return reader.ReadChar();
	}
	public char getChar(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadChar();
		stream.Position = originalPosition;
		return value;
	}
	public double getDouble()
	{
		return reader.ReadDouble();
	}
	public double getDouble(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadDouble();
		stream.Position = originalPosition;
		return value;
	}
	public float getFloat()
	{
		return reader.ReadSingle();
	}
	public float getFloat(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadSingle();
		stream.Position = originalPosition;
		return value;
	}
	public int getInt()
	{
		return reader.ReadInt32();
	}
	public int getInt(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadInt32();
		stream.Position = originalPosition;
		return value;
	}
	public long getLong()
	{
		return reader.ReadInt64();
	}
	public long getLong(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadInt64();
		stream.Position = originalPosition;
		return value;
	}
	public short getShort()
	{
		return reader.ReadInt16();
	}
	public short getShort(int index)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		var value = reader.ReadInt16();
		stream.Position = originalPosition;
		return value;
	}

	//methods using the internal BinaryWriter:
	public ByteBuffer putChar(char value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putChar(int index, char value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer putDouble(double value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putDouble(int index, double value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer putFloat(float value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putFloat(int index, float value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer putInt(int value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putInt(int index, int value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer putLong(long value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putLong(int index, long value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
	public ByteBuffer putShort(short value)
	{
		writer.Write(value);
		return this;
	}
	public ByteBuffer putShort(int index, short value)
	{
		var originalPosition = stream.Position;
		stream.Position = index;
		writer.Write(value);
		stream.Position = originalPosition;
		return this;
	}
}
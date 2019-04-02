using System;

// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace rpc
{

	public class Buffer
	{

		public const int NO_INCREMENT = 0;

		private sbyte[] buffer;

		private int capacityIncrement;

		private int index;

		private int length;

		public Buffer() : this(null, NO_INCREMENT)
		{
		}

		public Buffer(int capacityIncrement) : this(null, capacityIncrement)
		{
		}

		public Buffer(sbyte[] buffer) : this(buffer, NO_INCREMENT)
		{
		}

		public Buffer(sbyte[] buffer, int capacityIncrement)
		{
			setBuffer(buffer);
			CapacityIncrement = capacityIncrement;
		}

        public virtual int Capacity => buffer.Length;

        public virtual int CapacityIncrement {
            get => capacityIncrement;
            set => capacityIncrement = value;
        }


        public virtual sbyte[] getBuffer()
		{
			return buffer;
		}

		public virtual void setBuffer(sbyte[] buffer)
		{
			this.buffer = buffer ?? (new sbyte[0]);
			index = 0;
			length = 0;
		}

		public virtual int Length {
            get => length;
            set {
                length = value;
                if (value > buffer.Length) {
                    grow(value);
                }
            }
        }


        public virtual sbyte[] copy()
		{
			var copy_Renamed = new sbyte[length];
			Array.Copy(buffer, 0, copy_Renamed, 0, length);
			return copy_Renamed;
		}

		public virtual void reset()
		{
			length = 0;
			index = 0;
		}

		public virtual int Index {
            get => index;
            set {
                index = value;
                if (value > length) {
                    length = value;
                }
                if (length > buffer.Length) {
                    grow(length);
                }
            }
        }

        public virtual int getIndex(int advance)
		{
			try
			{
				return index;
			}
			finally
			{
				index += advance_Renamed;
				if (index > length)
				{
					length = index;
				}
				if (length > buffer.Length)
				{
					grow(length);
				}
			}
		}


		public virtual int align(int boundary)
		{
			var align_Renamed = index % boundary;
			if (align_Renamed == 0)
			{
				return 0;
			}
			advance(align_Renamed = boundary - align_Renamed);
			return align_Renamed;
		}

		public virtual int align(int boundary, sbyte value)
		{
			var align_Renamed = index % boundary;
			if (align_Renamed == 0)
			{
				return 0;
			}
			advance(align_Renamed = boundary - align_Renamed, value);
			return align_Renamed;
		}

		public virtual int advance(int step)
		{
			index += step;
			if (index > length)
			{
				length = index;
			}
			if (length > buffer.Length)
			{
				grow(length);
			}
			return index;
		}

		public virtual int advance(int step, sbyte value)
		{
			for (var finish = index + step; index < finish; index++)
			{
				buffer[index] = value;
			}
			if (index > length)
			{
				length = index;
			}
			if (length > buffer.Length)
			{
				grow(length);
			}
			return index;
		}

		private void grow(int length)
		{
			if (capacityIncrement <= 0)
			{
				throw new IndexOutOfRangeException();
			}
			var newLength = buffer.Length;
			while (newLength < length)
			{
				newLength += capacityIncrement;
			}
			var newBuffer = new sbyte[newLength];
			Array.Copy(buffer, 0, newBuffer, 0, buffer.Length);
			buffer = newBuffer;
		}

	}

}
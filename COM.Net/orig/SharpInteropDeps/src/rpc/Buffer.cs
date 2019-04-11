using System;

/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc {

    public class Buffer {

        public const int NO_INCREMENT = 0;

        private sbyte[] Buffer_Renamed;

        private int CapacityIncrement_Renamed = NO_INCREMENT;

        private int Index_Renamed = 0;

        private int Length_Renamed;

        public Buffer() : this(null, NO_INCREMENT) {
        }

        public Buffer(int capacityIncrement) : this(null, capacityIncrement) {
        }

        public Buffer(sbyte[] buffer) : this(buffer, NO_INCREMENT) {
        }

        public Buffer(sbyte[] buffer, int capacityIncrement) {
            SetBuffer(buffer);
            CapacityIncrement = capacityIncrement;
        }

        public virtual int Capacity {
            get {
                return Buffer_Renamed.Length;
            }
        }

        public virtual int CapacityIncrement {
            get {
                return CapacityIncrement_Renamed;
            }
            set {
                this.CapacityIncrement_Renamed = value;
            }
        }


        public virtual sbyte[] GetBuffer() {
            return Buffer_Renamed;
        }

        public virtual void SetBuffer(sbyte[] buffer) {
            this.Buffer_Renamed = (buffer != null) ? buffer : new sbyte[0];
            this.Index_Renamed = 0;
            this.Length_Renamed = 0;
        }

        public virtual int Length {
            get {
                return Length_Renamed;
            }
            set {
                this.Length_Renamed = value;
                if (value > Buffer_Renamed.Length) {
                    Grow(value);
                }
            }
        }


        public virtual sbyte[] Copy() {
            sbyte[] copy = new sbyte[Length_Renamed];
            Array.Copy(Buffer_Renamed, 0, copy, 0, Length_Renamed);
            return copy;
        }

        public virtual void Reset() {
            Length_Renamed = 0;
            Index_Renamed = 0;
        }

        public virtual int Index {
            get {
                return Index_Renamed;
            }
            set {
                this.Index_Renamed = value;
                if (value > Length_Renamed) {
                    Length_Renamed = value;
                }
                if (Length_Renamed > Buffer_Renamed.Length) {
                    Grow(Length_Renamed);
                }
            }
        }

        public virtual int GetIndex(int advance) {
            try {
                return Index_Renamed;
            }
            finally {
                Index_Renamed += advance;
                if (Index_Renamed > Length_Renamed) {
                    Length_Renamed = Index_Renamed;
                }
                if (Length_Renamed > Buffer_Renamed.Length) {
                    Grow(Length_Renamed);
                }
            }
        }


        public virtual int Align(int boundary) {
            int align = Index_Renamed % boundary;
            if (align == 0) {
                return 0;
            }
            Advance(align = boundary - align);
            return align;
        }

        public virtual int Align(int boundary, sbyte value) {
            int align = Index_Renamed % boundary;
            if (align == 0) {
                return 0;
            }
            Advance(align = boundary - align, value);
            return align;
        }

        public virtual int Advance(int step) {
            Index_Renamed += step;
            if (Index_Renamed > Length_Renamed) {
                Length_Renamed = Index_Renamed;
            }
            if (Length_Renamed > Buffer_Renamed.Length) {
                Grow(Length_Renamed);
            }
            return Index_Renamed;
        }

        public virtual int Advance(int step, sbyte value) {
            for (int finish = Index_Renamed + step; Index_Renamed < finish; Index_Renamed++) {
                Buffer_Renamed[Index_Renamed] = value;
            }
            if (Index_Renamed > Length_Renamed) {
                Length_Renamed = Index_Renamed;
            }
            if (Length_Renamed > Buffer_Renamed.Length) {
                Grow(Length_Renamed);
            }
            return Index_Renamed;
        }

        private void Grow(int length) {
            if (CapacityIncrement_Renamed <= 0) {
                throw new System.IndexOutOfRangeException();
            }
            int newLength = Buffer_Renamed.Length;
            while (newLength < length) {
                newLength += CapacityIncrement_Renamed;
            }
            sbyte[] newBuffer = new sbyte[newLength];
            Array.Copy(Buffer_Renamed, 0, newBuffer, 0, Buffer_Renamed.Length);
            Buffer_Renamed = newBuffer;
        }

    }

}
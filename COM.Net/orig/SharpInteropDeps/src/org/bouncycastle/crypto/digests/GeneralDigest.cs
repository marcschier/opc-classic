using System;

namespace org.bouncycastle.crypto.digests {

    using Memoable = org.bouncycastle.util.Memoable;

    /// <summary>
    /// base implementation of MD4 family style digest as outlined in
    /// "Handbook of Applied Cryptography", pages 344 - 347.
    /// </summary>
    public abstract class GeneralDigest : ExtendedDigest, Memoable {
        public abstract void Reset(Memoable other);
        public abstract Memoable Copy();
        public abstract int DoFinal(sbyte[] @out, int outOff);
        public abstract int DigestSize { get; }
        public abstract string AlgorithmName { get; }
        private const int BYTE_LENGTH = 64;
        private sbyte[] XBuf;
        private int XBufOff;

        private long ByteCount;

        /// <summary>
        /// Standard constructor
        /// </summary>
        public GeneralDigest() {
            XBuf = new sbyte[4];
            XBufOff = 0;
        }

        /// <summary>
        /// Copy constructor.  We are using copy constructors in place
        /// of the Object.clone() interface as this interface is not
        /// supported by J2ME.
        /// </summary>
        public GeneralDigest(GeneralDigest t) {
            XBuf = new sbyte[t.XBuf.Length];

            CopyIn(t);
        }

        public virtual void CopyIn(GeneralDigest t) {
            Array.Copy(t.XBuf, 0, XBuf, 0, t.XBuf.Length);

            XBufOff = t.XBufOff;
            ByteCount = t.ByteCount;
        }

        public virtual void Update(sbyte @in) {
            XBuf[XBufOff++] = @in;

            if (XBufOff == XBuf.Length) {
                ProcessWord(XBuf, 0);
                XBufOff = 0;
            }

            ByteCount++;
        }

        public virtual void Update(sbyte[] @in, int inOff, int len) {
            //
            // fill the current word
            //
            while ((XBufOff != 0) && (len > 0)) {
                Update(@in[inOff]);

                inOff++;
                len--;
            }

            //
            // process whole words.
            //
            while (len > XBuf.Length) {
                ProcessWord(@in, inOff);

                inOff += XBuf.Length;
                len -= XBuf.Length;
                ByteCount += XBuf.Length;
            }

            //
            // load in the remainder.
            //
            while (len > 0) {
                Update(@in[inOff]);

                inOff++;
                len--;
            }
        }

        public virtual void Finish() {
            long bitLength = (ByteCount << 3);

            //
            // add the pad bytes.
            //
            Update(unchecked((sbyte)128));

            while (XBufOff != 0) {
                Update((sbyte)0);
            }

            ProcessLength(bitLength);

            ProcessBlock();
        }

        public virtual void Reset() {
            ByteCount = 0;

            XBufOff = 0;
            for (int i = 0; i < XBuf.Length; i++) {
                XBuf[i] = 0;
            }
        }

        public virtual int ByteLength {
            get {
                return BYTE_LENGTH;
            }
        }

        public abstract void ProcessWord(sbyte[] @in, int inOff);

        public abstract void ProcessLength(long bitLength);

        public abstract void ProcessBlock();
    }

}
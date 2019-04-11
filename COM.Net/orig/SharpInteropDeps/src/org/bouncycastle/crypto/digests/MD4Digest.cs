using System;

namespace org.bouncycastle.crypto.digests {


    using Memoable = org.bouncycastle.util.Memoable;

    /// <summary>
    /// implementation of MD4 as RFC 1320 by R. Rivest, MIT Laboratory for
    /// Computer Science and RSA Data Security, Inc.
    /// <para>
    /// <b>NOTE</b>: This algorithm is only included for backwards compatability
    /// with legacy applications, it's not secure, don't use it for anything new!
    /// </para>
    /// </summary>
    public class MD4Digest : GeneralDigest {
        private const int DIGEST_LENGTH = 16;

        private int H1, H2, H3, H4; // IV's

        private int[] X = new int[16];
        private int XOff;

        /// <summary>
        /// Standard constructor
        /// </summary>
        public MD4Digest() {
            Reset();
        }

        /// <summary>
        /// Copy constructor.  This will copy the state of the provided
        /// message digest.
        /// </summary>
        public MD4Digest(MD4Digest t) : base(t) {

            CopyIn(t);
        }

        private void CopyIn(MD4Digest t) {
            base.CopyIn(t);

            H1 = t.H1;
            H2 = t.H2;
            H3 = t.H3;
            H4 = t.H4;

            Array.Copy(t.X, 0, X, 0, t.X.Length);
            XOff = t.XOff;
        }

        public override string AlgorithmName {
            get {
                return "MD4";
            }
        }

        public override int DigestSize {
            get {
                return DIGEST_LENGTH;
            }
        }

        public override void ProcessWord(sbyte[] @in, int inOff) {
            X[XOff++] = (@in[inOff] & 0xff) | ((@in[inOff + 1] & 0xff) << 8) | ((@in[inOff + 2] & 0xff) << 16) | ((@in[inOff + 3] & 0xff) << 24);

            if (XOff == 16) {
                ProcessBlock();
            }
        }

        public override void ProcessLength(long bitLength) {
            if (XOff > 14) {
                ProcessBlock();
            }

            X[14] = unchecked((int)(bitLength & 0xffffffff));
            X[15] = (int)((long)((ulong)bitLength >> 32));
        }

        private void UnpackWord(int word, sbyte[] @out, int outOff) {
            @out[outOff] = (sbyte)word;
            @out[outOff + 1] = (sbyte)((int)((uint)word >> 8));
            @out[outOff + 2] = (sbyte)((int)((uint)word >> 16));
            @out[outOff + 3] = (sbyte)((int)((uint)word >> 24));
        }

        public override int DoFinal(sbyte[] @out, int outOff) {
            Finish();

            UnpackWord(H1, @out, outOff);
            UnpackWord(H2, @out, outOff + 4);
            UnpackWord(H3, @out, outOff + 8);
            UnpackWord(H4, @out, outOff + 12);

            Reset();

            return DIGEST_LENGTH;
        }

        /// <summary>
        /// reset the chaining variables to the IV values.
        /// </summary>
        public override void Reset() {
            base.Reset();

            H1 = 0x67452301;
            H2 = unchecked((int)0xefcdab89);
            H3 = unchecked((int)0x98badcfe);
            H4 = 0x10325476;

            XOff = 0;

            for (int i = 0; i != X.Length; i++) {
                X[i] = 0;
            }
        }

        //
        // round 1 left rotates
        //
        private const int S11 = 3;
        private const int S12 = 7;
        private const int S13 = 11;
        private const int S14 = 19;

        //
        // round 2 left rotates
        //
        private const int S21 = 3;
        private const int S22 = 5;
        private const int S23 = 9;
        private const int S24 = 13;

        //
        // round 3 left rotates
        //
        private const int S31 = 3;
        private const int S32 = 9;
        private const int S33 = 11;
        private const int S34 = 15;

        /*
         * rotate int x left n bits.
         */
        private int RotateLeft(int x, int n) {
            return (x << n) | ((int)((uint)x >> (32 - n)));
        }

        /*
         * F, G, H and I are the basic MD4 functions.
         */
        private int F(int u, int v, int w) {
            return (u & v) | (~u & w);
        }

        private int G(int u, int v, int w) {
            return (u & v) | (u & w) | (v & w);
        }

        private int H(int u, int v, int w) {
            return u ^ v ^ w;
        }

        public override void ProcessBlock() {
            int a = H1;
            int b = H2;
            int c = H3;
            int d = H4;

            //
            // Round 1 - F cycle, 16 times.
            //
            a = RotateLeft(a + F(b, c, d) + X[0], S11);
            d = RotateLeft(d + F(a, b, c) + X[1], S12);
            c = RotateLeft(c + F(d, a, b) + X[2], S13);
            b = RotateLeft(b + F(c, d, a) + X[3], S14);
            a = RotateLeft(a + F(b, c, d) + X[4], S11);
            d = RotateLeft(d + F(a, b, c) + X[5], S12);
            c = RotateLeft(c + F(d, a, b) + X[6], S13);
            b = RotateLeft(b + F(c, d, a) + X[7], S14);
            a = RotateLeft(a + F(b, c, d) + X[8], S11);
            d = RotateLeft(d + F(a, b, c) + X[9], S12);
            c = RotateLeft(c + F(d, a, b) + X[10], S13);
            b = RotateLeft(b + F(c, d, a) + X[11], S14);
            a = RotateLeft(a + F(b, c, d) + X[12], S11);
            d = RotateLeft(d + F(a, b, c) + X[13], S12);
            c = RotateLeft(c + F(d, a, b) + X[14], S13);
            b = RotateLeft(b + F(c, d, a) + X[15], S14);

            //
            // Round 2 - G cycle, 16 times.
            //
            a = RotateLeft(a + G(b, c, d) + X[0] + 0x5a827999, S21);
            d = RotateLeft(d + G(a, b, c) + X[4] + 0x5a827999, S22);
            c = RotateLeft(c + G(d, a, b) + X[8] + 0x5a827999, S23);
            b = RotateLeft(b + G(c, d, a) + X[12] + 0x5a827999, S24);
            a = RotateLeft(a + G(b, c, d) + X[1] + 0x5a827999, S21);
            d = RotateLeft(d + G(a, b, c) + X[5] + 0x5a827999, S22);
            c = RotateLeft(c + G(d, a, b) + X[9] + 0x5a827999, S23);
            b = RotateLeft(b + G(c, d, a) + X[13] + 0x5a827999, S24);
            a = RotateLeft(a + G(b, c, d) + X[2] + 0x5a827999, S21);
            d = RotateLeft(d + G(a, b, c) + X[6] + 0x5a827999, S22);
            c = RotateLeft(c + G(d, a, b) + X[10] + 0x5a827999, S23);
            b = RotateLeft(b + G(c, d, a) + X[14] + 0x5a827999, S24);
            a = RotateLeft(a + G(b, c, d) + X[3] + 0x5a827999, S21);
            d = RotateLeft(d + G(a, b, c) + X[7] + 0x5a827999, S22);
            c = RotateLeft(c + G(d, a, b) + X[11] + 0x5a827999, S23);
            b = RotateLeft(b + G(c, d, a) + X[15] + 0x5a827999, S24);

            //
            // Round 3 - H cycle, 16 times.
            //
            a = RotateLeft(a + H(b, c, d) + X[0] + 0x6ed9eba1, S31);
            d = RotateLeft(d + H(a, b, c) + X[8] + 0x6ed9eba1, S32);
            c = RotateLeft(c + H(d, a, b) + X[4] + 0x6ed9eba1, S33);
            b = RotateLeft(b + H(c, d, a) + X[12] + 0x6ed9eba1, S34);
            a = RotateLeft(a + H(b, c, d) + X[2] + 0x6ed9eba1, S31);
            d = RotateLeft(d + H(a, b, c) + X[10] + 0x6ed9eba1, S32);
            c = RotateLeft(c + H(d, a, b) + X[6] + 0x6ed9eba1, S33);
            b = RotateLeft(b + H(c, d, a) + X[14] + 0x6ed9eba1, S34);
            a = RotateLeft(a + H(b, c, d) + X[1] + 0x6ed9eba1, S31);
            d = RotateLeft(d + H(a, b, c) + X[9] + 0x6ed9eba1, S32);
            c = RotateLeft(c + H(d, a, b) + X[5] + 0x6ed9eba1, S33);
            b = RotateLeft(b + H(c, d, a) + X[13] + 0x6ed9eba1, S34);
            a = RotateLeft(a + H(b, c, d) + X[3] + 0x6ed9eba1, S31);
            d = RotateLeft(d + H(a, b, c) + X[11] + 0x6ed9eba1, S32);
            c = RotateLeft(c + H(d, a, b) + X[7] + 0x6ed9eba1, S33);
            b = RotateLeft(b + H(c, d, a) + X[15] + 0x6ed9eba1, S34);

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;

            //
            // reset the offset and clean out the word buffer.
            //
            XOff = 0;
            for (int i = 0; i != X.Length; i++) {
                X[i] = 0;
            }
        }

        public override Memoable Copy() {
            return new MD4Digest(this);
        }

        public override void Reset(Memoable other) {
            MD4Digest d = (MD4Digest)other;

            CopyIn(d);
        }
    }

}
using System;

namespace org.bouncycastle.crypto.digests
{

	using Memoable = util.Memoable;

	/// <summary>
	/// base implementation of MD4 family style digest as outlined in
	/// "Handbook of Applied Cryptography", pages 344 - 347.
	/// </summary>
	public abstract class GeneralDigest : ExtendedDigest, Memoable
	{
		public abstract void reset(Memoable other);
		public abstract Memoable copy();
		public abstract int doFinal(sbyte[] @out, int outOff);
		public abstract int DigestSize {get;}
		public abstract string AlgorithmName {get;}
		private const int BYTE_LENGTH = 64;
		private sbyte[] xBuf;
		private int xBufOff;

		private long byteCount;

		/// <summary>
		/// Standard constructor
		/// </summary>
		protected internal GeneralDigest()
		{
			xBuf = new sbyte[4];
			xBufOff = 0;
		}

		/// <summary>
		/// Copy constructor.  We are using copy constructors in place
		/// of the Object.clone() interface as this interface is not
		/// supported by J2ME.
		/// </summary>
		protected internal GeneralDigest(GeneralDigest t)
		{
			xBuf = new sbyte[t.xBuf.Length];

			copyIn(t);
		}

		protected internal virtual void copyIn(GeneralDigest t)
		{
			Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);

			xBufOff = t.xBufOff;
			byteCount = t.byteCount;
		}

		public virtual void update(sbyte @in)
		{
			xBuf[xBufOff++] = @in;

			if (xBufOff == xBuf.Length)
			{
				processWord(xBuf, 0);
				xBufOff = 0;
			}

			byteCount++;
		}

		public virtual void update(sbyte[] @in, int inOff, int len)
		{
			//
			// fill the current word
			//
			while ((xBufOff != 0) && (len > 0))
			{
				update(@in[inOff]);

				inOff++;
				len--;
			}

			//
			// process whole words.
			//
			while (len > xBuf.Length)
			{
				processWord(@in, inOff);

				inOff += xBuf.Length;
				len -= xBuf.Length;
				byteCount += xBuf.Length;
			}

			//
			// load in the remainder.
			//
			while (len > 0)
			{
				update(@in[inOff]);

				inOff++;
				len--;
			}
		}

		public virtual void finish()
		{
			var bitLength = byteCount << 3;

			//
			// add the pad bytes.
			//
			update(unchecked((sbyte)128));

			while (xBufOff != 0)
			{
				update((sbyte)0);
			}

			processLength(bitLength);

			processBlock();
		}

		public virtual void reset()
		{
			byteCount = 0;

			xBufOff = 0;
			for (var i = 0; i < xBuf.Length; i++)
			{
				xBuf[i] = 0;
			}
		}

        public virtual int ByteLength => BYTE_LENGTH;

        protected internal abstract void processWord(sbyte[] @in, int inOff);

		protected internal abstract void processLength(long bitLength);

		protected internal abstract void processBlock();
	}

}
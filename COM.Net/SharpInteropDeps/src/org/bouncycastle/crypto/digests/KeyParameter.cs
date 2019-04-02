using System;

namespace org.bouncycastle.crypto.digests
{

	public class KeyParameter : CipherParameters
	{
		private readonly sbyte[] key;

		public KeyParameter(sbyte[] key) : this(key, 0, key.Length)
		{
		}

		public KeyParameter(sbyte[] key, int keyOff, int keyLen)
		{
			this.key = new sbyte[keyLen];

			Array.Copy(key, keyOff, this.key, 0, keyLen);
		}

        public virtual sbyte[] Key => key;
    }

}
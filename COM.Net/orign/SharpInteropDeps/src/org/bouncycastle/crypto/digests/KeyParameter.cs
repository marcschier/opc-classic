using System;

namespace org.bouncycastle.crypto.digests {

	public class KeyParameter : CipherParameters {
		private sbyte[] Key_Renamed;

		public KeyParameter(sbyte[] key) : this(key, 0, key.Length) {
		}

		public KeyParameter(sbyte[] key, int keyOff, int keyLen) {
			this.Key_Renamed = new sbyte[keyLen];

			Array.Copy(key, keyOff, this.Key_Renamed, 0, keyLen);
		}

		public virtual sbyte[] Key {
			get {
				return Key_Renamed;
			}
		}
	}

}
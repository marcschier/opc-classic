namespace org.bouncycastle.crypto.engines {

	using KeyParameter = org.bouncycastle.crypto.@params.KeyParameter;

	public class RC4Engine : StreamCipher {
		private const int STATE_LENGTH = 256;

		/*
		 * variables to hold the state of the RC4 engine
		 * during encryption and decryption
		 */

		private sbyte[] EngineState = null;
		private int x = 0;
		private int y = 0;
		private sbyte[] WorkingKey = null;

		/// <summary>
		/// initialise a RC4 cipher.
		/// </summary>
		/// <param name="forEncryption"> whether or not we are for encryption. </param>
		/// <param name="params"> the parameters required to set up the cipher. </param>
		/// <exception cref="IllegalArgumentException"> if the params argument is
		/// inappropriate. </exception>
		public virtual void Init(bool forEncryption, CipherParameters @params) {
			if (@params is KeyParameter) {
				/* 
				 * RC4 encryption and decryption is completely
				 * symmetrical, so the 'forEncryption' is 
				 * irrelevant.
				 */
				WorkingKey = ((KeyParameter)@params).Key;
				Key = WorkingKey;

				return;
			}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new System.ArgumentException("invalid parameter passed to RC4 init - " + @params.GetType().FullName);
		}

		public virtual string AlgorithmName {
			get {
				return "RC4";
			}
		}

		public virtual sbyte ReturnByte(sbyte @in) {
			x = (x + 1) & 0xff;
			y = (EngineState[x] + y) & 0xff;

			// swap
			sbyte tmp = EngineState[x];
			EngineState[x] = EngineState[y];
			EngineState[y] = tmp;

			// xor
			return (sbyte)(@in ^ EngineState[(EngineState[x] + EngineState[y]) & 0xff]);
		}

		public virtual void ProcessBytes(sbyte[] @in, int inOff, int len, sbyte[] @out, int outOff) {
			if ((inOff + len) > @in.Length) {
				throw new DataLengthException("input buffer too short");
			}

			if ((outOff + len) > @out.Length) {
				throw new OutputLengthException("output buffer too short");
			}

			for (int i = 0; i < len ; i++) {
				x = (x + 1) & 0xff;
				y = (EngineState[x] + y) & 0xff;

				// swap
				sbyte tmp = EngineState[x];
				EngineState[x] = EngineState[y];
				EngineState[y] = tmp;

				// xor
				@out[i + outOff] = (sbyte)(@in[i + inOff] ^ EngineState[(EngineState[x] + EngineState[y]) & 0xff]);
			}
		}

		public virtual void Reset() {
			Key = WorkingKey;
		}

		// Private implementation

		private sbyte[] Key {
			set {
				WorkingKey = value;
    
				// System.out.println("the key length is ; "+ workingKey.length);
    
				x = 0;
				y = 0;
    
				if (EngineState == null) {
					EngineState = new sbyte[STATE_LENGTH];
				}
    
				// reset the state of the engine
				for (int i = 0; i < STATE_LENGTH; i++) {
					EngineState[i] = (sbyte)i;
				}
    
				int i1 = 0;
				int i2 = 0;
    
				for (int i = 0; i < STATE_LENGTH; i++) {
					i2 = ((value[i1] & 0xff) + EngineState[i] + i2) & 0xff;
					// do the byte-swap inline
					sbyte tmp = EngineState[i];
					EngineState[i] = EngineState[i2];
					EngineState[i2] = tmp;
					i1 = (i1 + 1) % value.Length;
				}
			}
		}
	}

}
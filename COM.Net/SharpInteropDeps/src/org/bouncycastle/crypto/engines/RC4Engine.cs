namespace org.bouncycastle.crypto.engines {

    using KeyParameter = @params.KeyParameter;

    /// <summary>
    /// Rc4 engine
    /// </summary>
    public class RC4Engine : StreamCipher {
        private const int STATE_LENGTH = 256;

        /*
		 * variables to hold the state of the RC4 engine
		 * during encryption and decryption
		 */

        private sbyte[] engineState;
        private int x;
        private int y;
        private sbyte[] workingKey;

        /// <summary>
        /// initialise a RC4 cipher.
        /// </summary>
        /// <param name="forEncryption"> whether or not we are for encryption. </param>
        /// <param name="params"> the parameters required to set up the cipher. </param>
        /// <exception cref="System.ArgumentException"> if the params argument is
        /// inappropriate. </exception>
        public virtual void init(bool forEncryption, CipherParameters @params) {
            if (@params is KeyParameter) {
                /* 
				 * RC4 encryption and decryption is completely
				 * symmetrical, so the 'forEncryption' is 
				 * irrelevant.
				 */
                workingKey = ((KeyParameter)@params).Key;
                Key = workingKey;

                return;
            }

            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            throw new System.ArgumentException("invalid parameter passed to RC4 init - " + @params.GetType().FullName);
        }

        /// <summary>
        /// Alg name
        /// </summary>
        public virtual string AlgorithmName => "RC4";

        /// <summary>
        /// Returns byte
        /// </summary>
        /// <param name="in"></param>
        /// <returns></returns>
        public virtual sbyte returnByte(sbyte @in) {
            x = (x + 1) & 0xff;
            y = (engineState[x] + y) & 0xff;

            // swap
            var tmp = engineState[x];
            engineState[x] = engineState[y];
            engineState[y] = tmp;

            // xor
            return (sbyte)(@in ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
        }

        /// <summary>
        /// Process byte
        /// </summary>
        /// <param name="in"></param>
        /// <param name="inOff"></param>
        /// <param name="len"></param>
        /// <param name="out"></param>
        /// <param name="outOff"></param>
		public virtual void processBytes(sbyte[] @in, int inOff, int len, sbyte[] @out, int outOff) {
            if ((inOff + len) > @in.Length) {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + len) > @out.Length) {
                throw new OutputLengthException("output buffer too short");
            }

            for (var i = 0; i < len; i++) {
                x = (x + 1) & 0xff;
                y = (engineState[x] + y) & 0xff;

                // swap
                var tmp = engineState[x];
                engineState[x] = engineState[y];
                engineState[y] = tmp;

                // xor
                @out[i + outOff] = (sbyte)(@in[i + inOff] ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
            }
        }

        /// <summary>
        /// Reset
        /// </summary>
		public virtual void reset() {
            Key = workingKey;
        }

        // Private implementation

        /// <summary>
        /// Key
        /// </summary>
        private sbyte[] Key {
            set {
                workingKey = value;

                // System.out.println("the key length is ; "+ workingKey.length);

                x = 0;
                y = 0;

                if (engineState == null) {
                    engineState = new sbyte[STATE_LENGTH];
                }

                // reset the state of the engine
                for (var i = 0; i < STATE_LENGTH; i++) {
                    engineState[i] = (sbyte)i;
                }

                var i1 = 0;
                var i2 = 0;

                for (var i = 0; i < STATE_LENGTH; i++) {
                    i2 = ((value[i1] & 0xff) + engineState[i] + i2) & 0xff;
                    // do the byte-swap inline
                    var tmp = engineState[i];
                    engineState[i] = engineState[i2];
                    engineState[i2] = tmp;
                    i1 = (i1 + 1) % value.Length;
                }
            }
        }
    }
}
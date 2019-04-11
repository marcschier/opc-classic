namespace org.bouncycastle.crypto {

    /// <summary>
    /// the interface stream ciphers conform to.
    /// </summary>
    public interface StreamCipher {
        /// <summary>
        /// Initialise the cipher.
        /// </summary>
        /// <param name="forEncryption"> if true the cipher is initialised for
        ///  encryption, if false for decryption. </param>
        /// <param name="params"> the key and other data required by the cipher. </param>
        /// <exception cref="IllegalArgumentException"> if the params argument is
        /// inappropriate. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void init(boolean forEncryption, CipherParameters params) throws IllegalArgumentException;
        void Init(bool forEncryption, CipherParameters @params);

        /// <summary>
        /// Return the name of the algorithm the cipher implements.
        /// </summary>
        /// <returns> the name of the algorithm the cipher implements. </returns>
        string AlgorithmName { get; }

        /// <summary>
        /// encrypt/decrypt a single byte returning the result.
        /// </summary>
        /// <param name="in"> the byte to be processed. </param>
        /// <returns> the result of processing the input byte. </returns>
        sbyte ReturnByte(sbyte @in);

        /// <summary>
        /// process a block of bytes from in putting the result into out.
        /// </summary>
        /// <param name="in"> the input byte array. </param>
        /// <param name="inOff"> the offset into the in array where the data to be processed starts. </param>
        /// <param name="len"> the number of bytes to be processed. </param>
        /// <param name="out"> the output buffer the processed bytes go into. </param>
        /// <param name="outOff"> the offset into the output byte array the processed data starts at. </param>
        /// <exception cref="DataLengthException"> if the output buffer is too small. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processBytes(byte[] in, int inOff, int len, byte[] out, int outOff) throws DataLengthException;
        void ProcessBytes(sbyte[] @in, int inOff, int len, sbyte[] @out, int outOff);

        /// <summary>
        /// reset the cipher. This leaves it in the same state
        /// it was at after the last init (if there was one).
        /// </summary>
        void Reset();
    }

}
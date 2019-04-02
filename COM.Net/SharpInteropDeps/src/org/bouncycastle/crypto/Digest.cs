namespace org.bouncycastle.crypto
{

	/// <summary>
	/// interface that a message digest conforms to.
	/// </summary>
	public interface Digest
	{
		/// <summary>
		/// return the algorithm name
		/// </summary>
		/// <returns> the algorithm name </returns>
		string AlgorithmName {get;}

		/// <summary>
		/// return the size, in bytes, of the digest produced by this message digest.
		/// </summary>
		/// <returns> the size, in bytes, of the digest produced by this message digest. </returns>
		int DigestSize {get;}

		/// <summary>
		/// update the message digest with a single byte.
		/// </summary>
		/// <param name="in"> the input byte to be entered. </param>
		void update(sbyte @in);

		/// <summary>
		/// update the message digest with a block of bytes.
		/// </summary>
		/// <param name="in"> the byte array containing the data. </param>
		/// <param name="inOff"> the offset into the byte array where the data starts. </param>
		/// <param name="len"> the length of the data. </param>
		void update(sbyte[] @in, int inOff, int len);

		/// <summary>
		/// close the digest, producing the final digest value. The doFinal
		/// call leaves the digest reset.
		/// </summary>
		/// <param name="out"> the array the digest is to be copied into. </param>
		/// <param name="outOff"> the offset into the out array the digest is to start at. </param>
		int doFinal(sbyte[] @out, int outOff);

		/// <summary>
		/// reset the digest back to it's initial state.
		/// </summary>
		void reset();
	}

}
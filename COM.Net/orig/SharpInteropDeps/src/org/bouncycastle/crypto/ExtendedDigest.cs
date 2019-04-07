namespace org.bouncycastle.crypto {

	public interface ExtendedDigest : Digest {
		/// <summary>
		/// Return the size in bytes of the internal buffer the digest applies it's compression
		/// function to.
		/// </summary>
		/// <returns> byte length of the digests internal buffer. </returns>
		int ByteLength { get; }
	}

}
namespace org.bouncycastle.crypto
{

	/// <summary>
	/// this exception is thrown if a buffer that is meant to have output
	/// copied into it turns out to be too short, or if we've been given 
	/// insufficient input. In general this exception will get thrown rather
	/// than an ArrayOutOfBounds exception.
	/// </summary>
	public class DataLengthException : RuntimeCryptoException
	{
		/// <summary>
		/// base constructor.
		/// </summary>
		public DataLengthException()
		{
		}

		/// <summary>
		/// create a DataLengthException with the given message.
		/// </summary>
		/// <param name="message"> the message to be carried with the exception. </param>
		public DataLengthException(string message) : base(message)
		{
		}
	}

}
using System;

namespace org.bouncycastle.crypto {

	/// <summary>
	/// the foundation class for the exceptions thrown by the crypto packages.
	/// </summary>
	public class RuntimeCryptoException : Exception {
		/// <summary>
		/// base constructor.
		/// </summary>
		public RuntimeCryptoException() {
		}

		/// <summary>
		/// create a RuntimeCryptoException with the given message.
		/// </summary>
		/// <param name="message"> the message to be carried with the exception. </param>
		public RuntimeCryptoException(string message) : base(message) {
		}
	}

}
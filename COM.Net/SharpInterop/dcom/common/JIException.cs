//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.common {
    using System;

    /// <summary>
    /// Exception class for the framework. Developers are expected to catch
    /// or re-throw these exceptions and not create one themselves.
    /// </summary>
    public class JIException : Exception {

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public JIException(int errorCode, string message) :
            this(errorCode, message, null) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        public JIException(JIErrorCodes errorCode, string message) : 
            this (errorCode, message, null) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="errorCode"></param>
        public JIException(int errorCode) :
            this(errorCode, (Exception)null) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="errorCode"></param>
        public JIException(JIErrorCodes errorCode) :
            this(errorCode, (Exception)null) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        public JIException(int errorCode, Exception cause) :
            this(errorCode, null, cause) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        public JIException(JIErrorCodes errorCode, Exception cause) :
            this(errorCode, null, cause) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        public JIException(JIRuntimeException exception) :
            this(exception.HResult, null, exception) {
        }

        /// <summary>
        /// Create exception
        /// </summary>
        public JIException(int errorCode, string message, Exception cause) :
            base(message, cause) {
            ErrorCode = (JIErrorCodes)errorCode;
            _message = message;
        }

        /// <summary>
        /// Create exception
        /// </summary>
        public JIException(JIErrorCodes errorCode, string message, Exception cause) :
            base(message, cause) {
            ErrorCode = errorCode;
            _message = message;
        }

        /// <summary>
        /// Returns the localized error messages.
        /// </summary>
        public override string Message =>
            _message ?? (_message = JISystem.GetLocalizedMessage(ErrorCode));

        /// <summary>
        /// Returns the error code associated with this exception. Please refer
        /// <code>JIErrorCodes</code> for a complete list of errors.
        /// </summary>
        /// <returns> int representing the error code. </returns>
        public JIErrorCodes ErrorCode { get; } = (JIErrorCodes)(-1);

        private string _message;
    }
}
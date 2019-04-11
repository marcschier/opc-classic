using System;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.common {


    /// <summary>
    /// Exception class for the framework. Developers are expected to catch or re-throw these exceptions. 
    /// and not create one themselves.
    /// 
    /// @since 1.0
    /// </summary>

    public class JIException : Exception {


        private const long SerialVersionUID = 8648697261032503931L;
        private string Message_Renamed = null;
        private int ErrorCode_Renamed = -1;
        //US English messages sent by server , this is used especially during IDispatch exceptions
        //when the server returns an error.
        /// <summary>
        /// @exclude
        /// </summary>
        public JIException(int errorCode, string message) : this(errorCode,message,null) {
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public JIException(int errorCode) : this(errorCode,(Exception)null) {
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public JIException(int errorCode, Exception cause) : this(errorCode,null,cause) {
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public JIException(JIRuntimeException exception) : this(exception.HResult,null,exception) {
        }

        /// <summary>
        /// @exclude
        /// </summary>
        public JIException(int errorCode, string message, Exception cause) {
            base.initCause(cause);
            this.ErrorCode_Renamed = errorCode;
            this.Message_Renamed = message;
        }

        /// <summary>
        /// Returns the localized error messages.
        /// 
        /// @return
        /// </summary>
        public virtual string Message {
            get {
                return Message_Renamed == null ? Message_Renamed = InitMessageFromBundle() : Message_Renamed;
            }
        }

        private string InitMessageFromBundle() {
            return (Message_Renamed = JISystem.GetLocalizedMessage(ErrorCode_Renamed));
        }

        /// <summary>
        /// Returns the error code associated with this exception. Please refer 
        /// <code>JIErrorCodes</code> for a complete list of errors.
        /// </summary>
        /// <returns> int representing the error code. </returns>
        public virtual int ErrorCode {
            get {
                return ErrorCode_Renamed;
            }
        }
    }

}
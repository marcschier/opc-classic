
// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace rpc {
    using rpc.core;

    /// <summary>
    /// Presentation exception
    /// </summary>
    public class PresentationException : BindException {

        /// <summary>
        /// Create default
        /// </summary>
        public PresentationException() {
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="message"></param>
        public PresentationException(string message) :
            base(message) {
        }

        /// <summary>
        /// Create presentation exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public PresentationException(string message, PresentationResult result) :
            base(ToString(message, result)) {
        }

        /// <summary>
        /// Create message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static string ToString(string message, PresentationResult result) {
            if (result == null) {
                return message;
            }
            return !string.IsNullOrEmpty(message) ? message +
                " (" + result + ")" : result.ToString();
        }
    }
}
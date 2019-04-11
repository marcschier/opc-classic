//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.common;

    /// <summary>
    /// Class for signifying Automation related exceptions.
    /// </summary>
    public sealed class JIAutomationException : JIException {

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="e"></param>
        public JIAutomationException(JIException e) :
            base(e.ErrorCode, e.Message, e.InnerException) {
        }

        /// <summary>
        /// Exception information
        /// </summary>
        internal JIExcepInfo ExcepInfo {
            set {
                _excepInfo.ErrorCode = value.ErrorCode;
                _excepInfo.ExcepDesc = value.ExcepDesc;
                _excepInfo.HelpFilePath = value.HelpFilePath;
                _excepInfo.ExcepSource = value.ExcepSource;
            }
            get => _excepInfo;
        }

        private readonly JIExcepInfo _excepInfo = new JIExcepInfo();
    }
}
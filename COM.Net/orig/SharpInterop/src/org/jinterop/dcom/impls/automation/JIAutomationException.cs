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

namespace org.jinterop.dcom.impls.automation {

    using JIException = org.jinterop.dcom.common.JIException;

    /// <summary>
    /// Class for signifying Automation related exceptions.
    /// 
    /// @since 2.01
    /// </summary>
    public sealed class JIAutomationException : JIException {

        public JIAutomationException(JIException e) : base(e.ErrorCode,e.Message,e.InnerException) {
        }

        private JIExcepInfo ExcepInfo_Renamed = new JIExcepInfo();

        public JIExcepInfo ExcepInfo {
            set {
                this.ExcepInfo_Renamed.ErrorCode_Renamed = value.ErrorCode_Renamed;
                this.ExcepInfo_Renamed.ExcepDesc_Renamed = value.ExcepDesc_Renamed;
                this.ExcepInfo_Renamed.ExcepHelpfile = value.ExcepHelpfile;
                this.ExcepInfo_Renamed.ExcepSource_Renamed = value.ExcepSource_Renamed;
            }
            get {
                return ExcepInfo_Renamed;
            }
        }

        /// 
        private const long SerialVersionUID = 6969766293190131365L;

    }

}
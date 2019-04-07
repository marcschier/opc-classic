// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation
{

	using JIException = common.JIException;

	/// <summary>
	/// Class for signifying Automation related exceptions.
	/// 
	/// @since 2.01
	/// </summary>
	public sealed class JIAutomationException : JIException
	{

		public JIAutomationException(JIException e) : base(e.ErrorCode,e.Message,e.InnerException)
		{
		}

		private JIExcepInfo excepInfo = new JIExcepInfo();

		internal JIExcepInfo ExcepInfo {
            set {
                excepInfo.errorCode = value.errorCode;
                excepInfo.excepDesc = value.excepDesc;
                excepInfo.excepHelpfile = value.excepHelpfile;
                excepInfo.excepSource = value.excepSource;
            }
            get => excepInfo;
        }

        /// 
        private const long serialVersionUID = 6969766293190131365L;

	}

}
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

	/// <summary>
	/// Exposes error code , exception source, exception description and help file path
	/// for an unsuccessful <seealso cref="IJIDispatch"/> operation.
	/// 
	/// @since 2.0
	/// </summary>
	public sealed class JIExcepInfo
	{
		internal string excepSource;
		internal string excepDesc;
		internal string excepHelpfile;

		internal int errorCode = -1;
		internal JIExcepInfo()
		{
		}

		internal void clearAll()
		{
			errorCode = -1;
			excepSource = null;
			excepDesc = null;
			excepHelpfile = null;
		}

        /// <summary>
        /// An error code identifying the error.
        /// 
        /// @return
        /// </summary>
        public int ErrorCode => errorCode;

        /// <summary>
        /// A textual, human-readable name of the source of the exception. Typically, this is an
        /// application name.
        /// 
        /// @return
        /// </summary>
        public string ExcepSource => excepSource;

        /// <summary>
        /// A textual, human-readable description of the error intended for the customer. If no
        /// description is available it returns <code>null</code>.
        /// 
        /// @return
        /// </summary>
        public string ExcepDesc => excepDesc;

        /// <summary>
        /// The fully qualified drive, path, and file name of a Help file that has more information
        /// about the error. If no Help is available it returns <code>null</code>.
        /// 
        /// @return
        /// </summary>
        public string HelpFilePath => excepHelpfile;
    }

}
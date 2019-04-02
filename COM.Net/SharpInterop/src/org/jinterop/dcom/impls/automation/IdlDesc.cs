// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {

    using JIPointer = core.JIPointer;
    using JIStruct = core.JIStruct;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	public sealed class IdlDesc
	{

		private const long serialVersionUID = 7130410752801712935L;
		public const short IDLFLAG_NONE = ParamDesc.PARAMFLAG_NONE;
		public const short IDLFLAG_FIN = ParamDesc.PARAMFLAG_FIN;
		public const short IDLFLAG_FOUT = ParamDesc.PARAMFLAG_FOUT;
		public const short IDLFLAG_FLCID = ParamDesc.PARAMFLAG_FLCID;
		public const short IDLFLAG_FRETVAL = ParamDesc.PARAMFLAG_FRETVAL;


		public readonly JIPointer dwReserved;
		public readonly short wIDLFlags;

		internal IdlDesc(JIStruct values)
		{
			if (values == null)
			{
				dwReserved = null;
				wIDLFlags = -1;
				return;
			}
			dwReserved = (JIPointer)values.getMember(0);
			wIDLFlags = (short)(short?)values.getMember(1);
		}

	}

}
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.core;
    using System;

    /// <summary>
    /// IDL description
    /// </summary>
    [Serializable]
    public sealed class IdlDesc {

        public const short IDLFLAG_NONE = ParamDesc.PARAMFLAG_NONE;
        public const short IDLFLAG_FIN = ParamDesc.PARAMFLAG_FIN;
        public const short IDLFLAG_FOUT = ParamDesc.PARAMFLAG_FOUT;
        public const short IDLFLAG_FLCID = ParamDesc.PARAMFLAG_FLCID;
        public const short IDLFLAG_FRETVAL = ParamDesc.PARAMFLAG_FRETVAL;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly JIPointer dwReserved;
        public readonly short wIDLFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        internal IdlDesc(JIStruct values) {
            if (values == null) {
                dwReserved = null;
                wIDLFlags = -1;
                return;
            }
            dwReserved = (JIPointer)values.GetMember(0);
            wIDLFlags = (short)(short?)values.GetMember(1);
        }
    }
}
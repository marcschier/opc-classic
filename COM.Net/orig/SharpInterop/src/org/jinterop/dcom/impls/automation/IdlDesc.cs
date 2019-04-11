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

namespace org.jinterop.dcom.impls.automation {

    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIStruct = org.jinterop.dcom.core.JIStruct;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
    public sealed class IdlDesc {

        private const long SerialVersionUID = 7130410752801712935L;
        public const short IDLFLAG_NONE = ParamDesc.PARAMFLAG_NONE;
        public const short IDLFLAG_FIN = ParamDesc.PARAMFLAG_FIN;
        public const short IDLFLAG_FOUT = ParamDesc.PARAMFLAG_FOUT;
        public const short IDLFLAG_FLCID = ParamDesc.PARAMFLAG_FLCID;
        public const short IDLFLAG_FRETVAL = ParamDesc.PARAMFLAG_FRETVAL;


        public readonly JIPointer DwReserved;
        public readonly short WIDLFlags;

        public IdlDesc(JIStruct values) {
            if (values == null) {
                DwReserved = null;
                WIDLFlags = -1;
                return;
            }
            DwReserved = (JIPointer)values.GetMember(0);
            WIDLFlags = (short)((short?)values.GetMember(1));
        }

    }

}
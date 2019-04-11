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

    using JIException = org.jinterop.dcom.common.JIException;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;

    /// <summary>
    /// Creates automation related objects. Internal Factory , to be used only by the framework.
    /// 
    /// 
    /// @exclude
    /// @since 1.25
    /// </summary>
    public sealed class Internal_JIAutomationFactory {


        /// <summary>
        /// IID of <code>IDispatch [IJIDispatch]</code>.
        /// </summary>
        public const string IID_IDispatch = IJIDispatch_Fields.IID;
        /// <summary>
        /// IID of <code>ITypeInfo [IJITypeInfo]</code>.
        /// </summary>
        public const string IID_ITypeInfo = IJITypeInfo_Fields.IID;
        /// <summary>
        /// IID of <code>ITypeLib [IJITypeLib]</code>.
        /// </summary>
        public const string IID_ITypeLib = IJITypeLib_Fields.IID;

        /// <summary>
        /// IID of <code>IEnumVARIANT [IJIEnumVARIANT]</code>.
        /// </summary>
        public const string IID_IEnumVariant = IJIEnumVariant_Fields.IID;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static org.jinterop.dcom.core.IJIComObject narrowObject(final org.jinterop.dcom.core.IJIComObject comObject) throws org.jinterop.dcom.common.JIException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public static IJIComObject NarrowObject(IJIComObject comObject) {
            IJIComObject retval = comObject;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String IID = comObject.getInterfaceIdentifier();
            string IID = comObject.InterfaceIdentifier;
            if (IID.Equals(IID_IDispatch, StringComparison.CurrentCultureIgnoreCase)) { // && iUnknown.isIDispatchSupported())
                retval = new JIDispatchImpl((IJIComObject)retval);
            }
            else {
            if (IID.Equals(IID_ITypeInfo, StringComparison.CurrentCultureIgnoreCase)) { // && iUnknown.isIDispatchSupported())
                retval = new JITypeInfoImpl((IJIComObject)retval);
            }
            else {
            if (IID.Equals(IID_ITypeLib, StringComparison.CurrentCultureIgnoreCase)) { // && iUnknown.isIDispatchSupported())
                retval = new JITypeLibImpl((IJIComObject)retval);
            }
            else {
            if (IID.Equals(IID_IEnumVariant, StringComparison.CurrentCultureIgnoreCase)) { // && iUnknown.isIDispatchSupported())
                retval = new JIEnumVARIANTImpl((IJIComObject)retval);
            }
            }
            }
            }

            return retval;
        }




    }

}
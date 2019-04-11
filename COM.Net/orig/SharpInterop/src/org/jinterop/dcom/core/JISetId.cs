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

namespace org.jinterop.dcom.core {


    [Serializable]
    internal sealed class JISetId {


        private const long SerialVersionUID = -3819165506317998524L;
        internal sbyte[] Setid = null;

        public JISetId(sbyte[] setid) {
            this.Setid = setid;
        }

        public sbyte[] SetID {
            get {
                return Setid;
            }
        }

        public override int GetHashCode() {
            int result = 1;
            //from SUN
            for (int i = 0;i < Setid.Length;i++) {
                result = 31 * result + Setid[i];
            }
            return result;
            //return Arrays.hashCode(setid);
        }

         public override bool Equals(object obj) {
             if (!(obj is JISetId)) {
                return false;
             }

             return Arrays.Equals(Setid,((JISetId)obj).SetID);
         }

    }

}
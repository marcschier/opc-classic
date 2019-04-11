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


    using JISystem = org.jinterop.dcom.common.JISystem;

    [Serializable]
    internal sealed class JIObjectId {

        private const long SerialVersionUID = -4335536047242439700L;
        private readonly sbyte[] Oid;
        private int RefcountofIPID = 0;
        private long LastPingTime = DateTimeHelperClass.CurrentUnixTimeMillis();
        internal readonly bool Dontping;

        public int IPIDRefCount {
            get {
                return RefcountofIPID;
            }
        }

        public bool HasExpired() {
            //8 minutes interval...giving COM Client some grace period.
            if ((DateTimeHelperClass.CurrentUnixTimeMillis() - LastPingTime) > 8 * 60 * 1000) {
                return true;
            }
            else {
    //            lastPingTime = System.currentTimeMillis();
                return false;
            }
        }

        public void UpdateLastPingTime() {
            LastPingTime = DateTimeHelperClass.CurrentUnixTimeMillis();
        }

        public void SetIPIDRefCountTo0() {
            RefcountofIPID = 0;
        }

        public void DecrementIPIDRefCountBy1() {
            RefcountofIPID--;
        }

        public void IncrementIPIDRefCountBy1() {
            RefcountofIPID++;
        }

        public JIObjectId(sbyte[] oid, bool dontping) {
            this.Oid = oid;
            this.Dontping = dontping;
            if (dontping) {
                if (JISystem.Logger.isLoggable(Level.INFO)) {
                    JISystem.Logger.info("DONT PING is true for OID: " + ToString());
                }
            }
        }

        public sbyte[] OID {
            get {
                return Oid;
            }
        }

        public override int GetHashCode() {
            int result = 1;
            //from SUN
            for (int i = 0;i < Oid.Length;i++) {
                result = 31 * result + Oid[i];
            }
            return result;

            //return Arrays.hashCode(oid);
        }

        public override bool Equals(object obj) {
             if (!(obj is JIObjectId)) {
                 return false;
             }

             return Arrays.Equals(Oid,((JIObjectId)obj).OID);
        }

        public override string ToString() {
               ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
               jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), Oid, 0, Oid.Length);
               return "{ IPID ref count is " + RefcountofIPID + " } and OID in bytes[] " + byteArrayOutputStream.ToString() + " , hasExpired " + HasExpired() + " } ";
        }

    //    void addIpid(String IPID)
    //    {
    //        listOfIpids.add(IPID);
    //    }
    //
    //    List getIpidList()
    //    {
    //        return listOfIpids;
    //    }
    }

}
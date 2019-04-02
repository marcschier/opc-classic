// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 
namespace org.jinterop.dcom.core {
    using Serilog;
    using System;
    using System.Linq;

    [Serializable]
    internal sealed class JIObjectId {

        /// <summary>
        /// Ref count
        /// </summary>
        internal int IPIDRefCount { get; private set; }

        /// <summary>
        /// Object id
        /// </summary>
        internal sbyte[] OID { get; }

        /// <summary>
        /// Returns whether object expired
        /// </summary>
        /// <returns></returns>
        internal bool hasExpired() {
            // TODO: Make configurable
            // 8 minutes interval...giving COM Client some grace period.
            if ((DateTimeHelperClass.CurrentUnixTimeMillis() - _lastPingTime) > 8 * 60 * 1000) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Create object id
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="dontping"></param>
        internal JIObjectId(sbyte[] oid, bool dontping) {
            OID = oid;
            _dontping = dontping;
            if (dontping) {
                Log.Logger.Information("DONT PING is true for OID: " + ToString());
            }
        }

        /// <summary>
        /// Update last ping
        /// </summary>
        internal void updateLastPingTime() {
            _lastPingTime = DateTimeHelperClass.CurrentUnixTimeMillis();
        }

        /// <summary>
        /// Reset ref count
        /// </summary>
        internal void setIPIDRefCountTo0() {
            IPIDRefCount = 0;
        }

        /// <summary>
        /// Decrement ref count
        /// </summary>
        internal void decrementIPIDRefCountBy1() {
            IPIDRefCount--;
        }

        /// <summary>
        /// Increment ref count
        /// </summary>
        internal void incrementIPIDRefCountBy1() {
            IPIDRefCount++;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            var result = 1;
            for (var i = 0; i < OID.Length; i++) {
                result = 31 * result + OID[i];
            }
            return result;
        }

        /// <inheritdoc/>
		public override bool Equals(object obj) {
            if (!(obj is JIObjectId other)) {
                return false;
            }

            return OID.SequenceEqual(other.OID);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return "{ IPID ref count is " + IPIDRefCount + " } and OID in bytes[] " +
                OID + " , hasExpired " + hasExpired() + " } ";
        }

        internal readonly bool _dontping;
        private long _lastPingTime = DateTimeHelperClass.CurrentUnixTimeMillis();
    }
}
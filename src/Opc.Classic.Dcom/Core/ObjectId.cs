// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Common;
using System;
using System.Linq;

namespace Opc.Classic.Dcom.Core; 
[Serializable]
internal sealed class ObjectId {

    /// <summary>
    /// Ref count
    /// </summary>
    internal int IPIDRefCount { get; private set; }

    /// <summary>
    /// Object id
    /// </summary>
    internal byte[] OID { get; }

    /// <summary>
    /// Do not ping
    /// </summary>
    internal bool Dontping { get; }

    /// <summary>
    /// Returns whether object expired
    /// </summary>
    /// <returns></returns>
    internal bool HasExpired() {
        if ((DcomTimings.UtcNow - _lastPingTime) > DcomTimings.ObjectExpiryPeriod) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Create object id
    /// </summary>
    /// <param name="oid"></param>
    /// <param name="dontping"></param>
    internal ObjectId(byte[] oid, bool dontping) {
        OID = oid;
        Dontping = dontping;
        if (dontping) {
            Log.Logger.Information("DONT PING is true for OID: " + ToString());
        }
    }

    /// <summary>
    /// Update last ping
    /// </summary>
    internal void UpdateLastPingTime() =>
        _lastPingTime = DcomTimings.UtcNow;

    /// <summary>
    /// Reset ref count
    /// </summary>
    internal void SetIPIDRefCountTo0() => IPIDRefCount = 0;

    /// <summary>
    /// Decrement ref count
    /// </summary>
    internal void DecrementIPIDRefCountBy1() => IPIDRefCount--;

    /// <summary>
    /// Increment ref count
    /// </summary>
    internal void IncrementIPIDRefCountBy1() => IPIDRefCount++;

    /// <inheritdoc/>
    public override int GetHashCode() {
        var result = 1;
        for (var i = 0; i < OID.Length; i++) {
            result = (31 * result) + OID[i];
        }
        return result;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) {
        if (!(obj is ObjectId other)) {
            return false;
        }

        return OID.SequenceEqual(other.OID);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        "{ IPID ref count is " + IPIDRefCount + " } and OID in bytes[] " +
            OID + ", hasExpired " + HasExpired() + " } ";

    private DateTimeOffset _lastPingTime = DcomTimings.UtcNow;
}

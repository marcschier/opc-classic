// SPDX-License-Identifier: MIT

using System;
using System.Linq;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Oxid
/// </summary>
[Serializable]
internal sealed class Oxid {

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create
    /// </summary>
    /// <param name="oxid"></param>
    internal Oxid(byte[] oxid) => _oxid = oxid;
#pragma warning restore RECS0154 // Parameter is never used

    /// <summary>
    /// Oxid
    /// </summary>
    internal byte[] OXID => _oxid;

    /// <inheritdoc/>
    public override int GetHashCode() {
        var result = 1;
        // from SUN
        for (var i = 0; i < OXID.Length; i++) {
            result = (31 * result) + OXID[i];
        }
        return result;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) {
        if (!(obj is Oxid other)) {
            return false;
        }
        return _oxid.SequenceEqual(other.OXID);
    }

    internal byte[] _oxid;
}

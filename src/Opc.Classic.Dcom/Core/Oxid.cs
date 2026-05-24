//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using System;
using System.Linq;

namespace SharpInterop.Core; 
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
